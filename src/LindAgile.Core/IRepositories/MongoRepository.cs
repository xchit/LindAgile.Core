using LindAgile.Core.GlobalConfig;
using LindAgile.Core.Logger;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace LindAgile.Core.IRepositories
{
    /// <summary>
    /// Mongodb实现数据持久化
    /// Author:Lind.zhang
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class MongoRepository<TEntity> : IRepository<TEntity>
      where TEntity : class
    {

        #region Fields & consts
        /// <summary>
        /// 创建数据库链接
        /// </summary>
        private MongoClient _server;
        /// <summary>
        /// 获得数据库
        /// </summary>
        private IMongoDatabase _database;
        /// <summary>
        /// 操作的集合（数据表）
        /// </summary>
        private IMongoCollection<TEntity> _table;
        /// <summary>
        /// 实体键，对应MongoDB的_id
        /// </summary>
        private const string EntityKey = "Id";

        /// <summary>
        /// 服务器地址和端口
        /// </summary>
        private static readonly string _connectionStringHost = ConfigManager.Config.MongoDB.Host;
        /// <summary>
        /// 数据库名称
        /// </summary>
        private static readonly string _dbName = ConfigManager.Config.MongoDB.DbName;
        /// <summary>
        /// 用户名
        /// </summary>
        private static readonly string _userName = ConfigManager.Config.MongoDB.UserName;
        /// <summary>
        /// 密码
        /// </summary>
        private static readonly string _password = ConfigManager.Config.MongoDB.Password;

        ILogger _logger;
        #endregion

        #region Constructors
        public MongoRepository(ILogger logger = null)
        {
            _server = new MongoClient(ConnectionString());
            _database = _server.GetDatabase(_dbName);
            _table = _database.GetCollection<TEntity>(typeof(TEntity).Name);
            _logger = logger ?? new EmptyLogger();
        }

        #endregion

        #region Private Methods
        /// <summary>
        /// 组织Mongo连接串
        /// </summary>
        /// <returns></returns>
        private static string ConnectionString()
        {
            var database = _dbName;
            var userName = _userName;
            var password = _password;
            var authentication = string.Empty;
            var host = string.Empty;
            if (!string.IsNullOrWhiteSpace(userName))
            {
                authentication = string.Concat(userName, ':', password, '@');
            }
            database = database ?? "Test";
            if (string.IsNullOrWhiteSpace(_connectionStringHost))
            {
                throw new ArgumentNullException("请配置MongoDB_Host节点");
            }
            //mongodb://[username:password@]host1[:port1][,host2[:port2],…[,hostN[:portN]]][/[database][?options]]
            return string.Format("mongodb://{0}{1}/{2}", authentication, _connectionStringHost, database);
        }

        /// <summary>
        /// 构建Mongo的查询表达式，通过一个匿名对象
        /// 最多支持三层嵌套
        /// </summary>
        /// <typeparam name="U"></typeparam>
        /// <param name="template"></param>
        /// <returns></returns>
        private BsonDocumentFilterDefinition<TEntity> GeneratorMongoQuery<U>(U template)
        {
            var qType = typeof(U);
            var outter = new BsonDocument();
            var simpleQuery = new BsonDocument();
            foreach (var item in qType.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                if (item.PropertyType.IsByRef && item.PropertyType != typeof(string))
                {
                    //复杂类型，导航属性，类对象和集合对象 
                    foreach (var sub in item.PropertyType.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                    {

                        if (sub.PropertyType.IsByRef && sub.PropertyType != typeof(string))
                        {
                            foreach (var subInner in sub.PropertyType.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                            {
                                if (subInner.PropertyType.IsByRef && subInner.PropertyType != typeof(string))
                                {
                                    foreach (var subItemInner in subInner.PropertyType.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                                    {
                                        simpleQuery.Add(new BsonElement(item.Name + "." + sub.Name + "." + subInner.Name + "." + subItemInner.Name,
                                           BsonValue.Create(subItemInner.GetValue(subInner.GetValue(sub.GetValue(item.GetValue(template)))))));
                                    }

                                }
                                else
                                {
                                    simpleQuery.Add(new BsonElement(item.Name + "." + sub.Name + "." + subInner.Name,
                                        BsonValue.Create(subInner.GetValue(sub.GetValue(item.GetValue(template))))));
                                }
                            }
                        }
                        else
                        {
                            simpleQuery.Add(new BsonElement(item.Name + "." + sub.Name,
                               BsonValue.Create(sub.GetValue(item.GetValue(template)))
                           ));

                        }


                    }
                }
                else
                {
                    //简单类型,ValueType和string
                    simpleQuery.Add(new BsonElement(item.Name,
                      BsonValue.Create(item.GetValue(template))
                        ));
                }
            }
            return new BsonDocumentFilterDefinition<TEntity>(simpleQuery);
        }

        /// <summary>
        /// 递归构建Update操作串
        /// 注意：如果子属性为集合，只更新原集，不能添加push和删除pull集合元素
        /// </summary>
        /// <param name="fieldList">要更新的字段列表</param>
        /// <param name="property">当前属性</param>
        /// <param name="propertyValue">当前属性值</param>
        /// <param name="item">原对象</param>
        /// <param name="father">父属性</param>
        private void GenerateRecursion(
                 List<UpdateDefinition<TEntity>> fieldList,
                 PropertyInfo property,
                 object propertyValue,
                 TEntity item,
                 string father)
        {
            //复杂类型
            if (property.PropertyType.IsByRef && property.PropertyType != typeof(string) && propertyValue != null)
            {
                //集合
                if (typeof(IList).IsAssignableFrom(propertyValue.GetType()))
                {
                    foreach (var sub in property.PropertyType.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                    {
                        if (sub.PropertyType.IsByRef && sub.PropertyType != typeof(string))
                        {
                            var arr = propertyValue as IList;
                            if (arr != null && arr.Count > 0)
                            {
                                for (int index = 0; index < arr.Count; index++)
                                {
                                    foreach (var subInner in sub.PropertyType.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                                    {
                                        if (string.IsNullOrWhiteSpace(father))
                                            GenerateRecursion(fieldList, subInner, subInner.GetValue(arr[index]), item, property.Name + "." + index);
                                        else
                                            GenerateRecursion(fieldList, subInner, subInner.GetValue(arr[index]), item, father + "." + property.Name + "." + index);
                                    }
                                }
                            }
                        }
                    }
                }
                //实体
                else
                {
                    foreach (var sub in property.PropertyType.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                    {
                        if (string.IsNullOrWhiteSpace(father))
                            GenerateRecursion(fieldList, sub, sub.GetValue(propertyValue), item, property.Name);
                        else
                            GenerateRecursion(fieldList, sub, sub.GetValue(propertyValue), item, father + "." + property.Name);
                    }
                }
            }
            //简单类型
            else
            {
                if (property.Name != EntityKey)//更新集中不能有实体键_id
                {
                    if (string.IsNullOrWhiteSpace(father))
                        fieldList.Add(Builders<TEntity>.Update.Set(property.Name, propertyValue));
                    else
                        fieldList.Add(Builders<TEntity>.Update.Set(father + "." + property.Name, propertyValue));

                }
            }
        }

        /// <summary>
        /// 版本二：递归构建Update操作串
        /// 主要功能：实现List子属性的push操作
        ///           更新时，添加了unset动作，将需要更新的元素先移除，再更新
        /// </summary>
        /// <param name="fieldList"></param>
        /// <param name="property"></param>
        /// <param name="propertyValue"></param>
        /// <param name="item"></param>
        /// <param name="father"></param>
        private void GenerateRecursionSet(
                  List<UpdateDefinition<TEntity>> fieldList,
                  PropertyInfo property,
                  object propertyValue,
                  TEntity item,
                  string father,
                  FilterDefinition<TEntity> filter
           )
        {
            //复杂类型
            if (property.PropertyType.IsByRef && property.PropertyType != typeof(string) && propertyValue != null)
            {
                //对于复杂类型的更新：移除属性，避免为赋值为NULL类型复杂字段无法set的问题
                _table.UpdateOne(filter, Builders<TEntity>.Update.Unset(string.IsNullOrWhiteSpace(father) ? property.Name : father + "." + property.Name));

                //集合
                if (typeof(IList).IsAssignableFrom(propertyValue.GetType()))
                {
                    var arr = propertyValue as IList;
                    if (arr != null && arr.Count > 0)
                    {
                        fieldList.Add(Builders<TEntity>.Update.Set(string.IsNullOrWhiteSpace(father) ? property.Name : father + "." + property.Name, arr));
                    }
                }
                //实体
                else
                {
                    foreach (var sub in property.PropertyType.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                    {
                        GenerateRecursionSet(fieldList, sub, sub.GetValue(propertyValue), item, string.IsNullOrWhiteSpace(father) ? property.Name : father + "." + property.Name, filter);
                    }
                }
            }
            //简单类型
            else
            {
                if (property.Name != EntityKey)//更新集中不能有实体键_id
                {
                    fieldList.Add(Builders<TEntity>.Update.Set(string.IsNullOrWhiteSpace(father) ? property.Name : father + "." + property.Name, propertyValue));
                }
            }
        }


        /// <summary>
        /// 构建Mongo的更新表达式
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private List<UpdateDefinition<TEntity>> GeneratorMongoUpdate(TEntity item, FilterDefinition<TEntity> filter)
        {
            var fieldList = new List<UpdateDefinition<TEntity>>();

            var properties = typeof(TEntity).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(i => i.GetCustomAttribute(typeof(MongoDB.Bson.Serialization.Attributes.BsonIgnoreAttribute)) == null);
            foreach (var property in properties)
            {
                GenerateRecursionSet(fieldList, property, property.GetValue(item), item, string.Empty, filter);
            }
            return fieldList;
        }

        /// <summary>
        /// 按需要更新的构建者
        /// 递归构建Update操作串
        /// </summary>
        /// <param name="fieldList"></param>
        /// <param name="property"></param>
        /// <param name="propertyValue"></param>
        /// <param name="item"></param>
        /// <param name="fatherValue"></param>
        /// <param name="father"></param>
        private void GenerateRecursionExpress(
          List<UpdateDefinition<TEntity>> fieldList,
          PropertyInfo property,
          object propertyValue,
          TEntity item,
          object fatherValue,
          string father)
        {
            //复杂类型
            if (property.PropertyType.IsByRef && property.PropertyType != typeof(string) && propertyValue != null)
            {
                //集合
                if (typeof(IList).IsAssignableFrom(propertyValue.GetType()))
                {
                    var modifyIndex = 0;//要更新的记录索引
                    foreach (var sub in property.PropertyType.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                    {
                        if (sub.PropertyType.IsByRef && sub.PropertyType != typeof(string))
                        {
                            var arr = propertyValue as IList;
                            if (arr != null && arr.Count > 0)
                            {

                                var oldValue = property.GetValue(fatherValue ?? item) as IList;
                                if (oldValue != null)
                                {
                                    for (int index = 0; index < arr.Count; index++)
                                    {
                                        for (modifyIndex = 0; modifyIndex < oldValue.Count; modifyIndex++)
                                            if (sub.PropertyType.GetProperty(EntityKey).GetValue(oldValue[modifyIndex]).ToString()
                                                == sub.PropertyType.GetProperty(EntityKey).GetValue(arr[index]).ToString())//比较_id是否相等
                                                break;
                                        foreach (var subInner in sub.PropertyType.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                                        {
                                            if (string.IsNullOrWhiteSpace(father))
                                                GenerateRecursionExpress(fieldList, subInner, subInner.GetValue(arr[index]), item, arr[index], property.Name + "." + modifyIndex);
                                            else
                                                GenerateRecursionExpress(fieldList, subInner, subInner.GetValue(arr[index]), item, arr[index], father + "." + property.Name + "." + modifyIndex);
                                        }
                                    }
                                }

                            }
                        }
                    }
                }
                //实体
                else
                {
                    foreach (var sub in property.PropertyType.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                    {

                        if (string.IsNullOrWhiteSpace(father))
                            GenerateRecursionExpress(fieldList, sub, sub.GetValue(propertyValue), item, property.GetValue(fatherValue), property.Name);
                        else
                            GenerateRecursionExpress(fieldList, sub, sub.GetValue(propertyValue), item, property.GetValue(fatherValue), father + "." + property.Name);
                    }
                }
            }
            //简单类型
            else
            {
                if (property.Name != EntityKey)//更新集中不能有实体键_id
                {
                    if (string.IsNullOrWhiteSpace(father))
                        fieldList.Add(Builders<TEntity>.Update.Set(property.Name, propertyValue));
                    else
                        fieldList.Add(Builders<TEntity>.Update.Set(father + "." + property.Name, propertyValue));
                }
            }
        }

        /// <summary>
        /// 等待Task执行完成后再返回
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        private Task ForWait(Func<Task> func)
        {
            var t = func();
            t.Wait();
            return t;
        }
        #endregion

        #region IRepository<TEntity> 成员

        public TEntity Find(params object[] id)
        {
            var condition = Builders<TEntity>.Filter.Eq("_id", id[0].ToString());
            return _table.Find(condition).FirstOrDefaultAsync().Result;
        }

        public IQueryable<TEntity> GetModel()
        {
            return _table.AsQueryable();
        }

        public void SetDataContext(object db)
        {
            throw new NotImplementedException();
        }

        public void Insert(TEntity item)
        {
            _table.InsertOne(item);
        }

        public void Update(TEntity item)
        {
            var query = Builders<TEntity>.Filter.Eq("_id", new ObjectId(typeof(TEntity).GetProperty(EntityKey)
                                                           .GetValue(item)
                                                           .ToString()));
            _table.UpdateOne(
              query,
              Builders<TEntity>.Update.Combine(GeneratorMongoUpdate(item, query)));
        }

        public void Delete(TEntity item)
        {
            var query = Builders<TEntity>.Filter.Eq("_id", new ObjectId(typeof(TEntity).GetProperty(EntityKey)
                                                            .GetValue(item)
                                                            .ToString()));
            _table.DeleteOne(query);
        }

        public void Insert(IEnumerable<TEntity> item)
        {
            if (item != null && item.Any())
            {
                var list = new List<WriteModel<TEntity>>();
                foreach (var iitem in item)
                {
                    list.Add(new InsertOneModel<TEntity>(iitem));
                }
                _table.BulkWrite(list);
            }
        }

        public void Update(IEnumerable<TEntity> item)
        {
            if (item != null && item.Any())
            {
                var list = new List<WriteModel<TEntity>>();
                foreach (var iitem in item)
                {
                    var query = Builders<TEntity>.Filter.Eq("_id", new ObjectId(typeof(TEntity).GetProperty(EntityKey)
                                                           .GetValue(iitem)
                                                           .ToString()));
                    list.Add(new UpdateOneModel<TEntity>(query, Builders<TEntity>.Update.Combine(GeneratorMongoUpdate(iitem as TEntity, query))));
                }
                _table.BulkWrite(list);
            }
        }

        public void Delete(IEnumerable<TEntity> item)
        {
            if (item != null && item.Any())
            {
                var list = new List<WriteModel<TEntity>>();
                foreach (var iitem in item)
                {
                    var query = Builders<TEntity>.Filter.Eq("_id", new ObjectId(typeof(TEntity).GetProperty(EntityKey)
                                                           .GetValue(iitem)
                                                           .ToString()));
                    list.Add(new DeleteOneModel<TEntity>(query));
                }
                _table.BulkWrite(list);
            }
        }

        #endregion
    }
}
