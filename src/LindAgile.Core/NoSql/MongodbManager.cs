using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace LindAgile.Core.NoSql
{
    /// <summary>
    /// Mongodb驱动
    /// </summary>
    public class MongodbManager<TEntity> where TEntity : class
    {
        #region MongoDB配置
        /// <summary>
        /// 服务器地址和端口
        /// </summary>
        private static readonly string _connectionStringHost = GlobalConfig.ConfigManager.Config.MongoDB.Host;
        /// <summary>
        /// 数据库名称
        /// </summary>
        private static readonly string _dbName = GlobalConfig.ConfigManager.Config.MongoDB.DbName;
        /// <summary>
        /// 用户名
        /// </summary>
        private static readonly string _userName = GlobalConfig.ConfigManager.Config.MongoDB.UserName;
        /// <summary>
        /// 密码
        /// </summary>
        private static readonly string _password = GlobalConfig.ConfigManager.Config.MongoDB.Password;


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
            return string.Format("mongodb://{0}{1}/{2}", authentication, _connectionStringHost, database);
        }
        #endregion

        static IMongoCollection<TEntity> mongoRepository;
        static object lockObj = new object();
        /// <summary>
        /// MongoDB使用者
        /// </summary>
        public static IMongoCollection<TEntity> Instance
        {
            get
            {
                if (mongoRepository == null)
                {
                    lock (lockObj)
                    {
                        if (mongoRepository == null)
                        {
                            var svrSettings = MongoUrl.Create(ConnectionString());
                            var server = new MongoClient(svrSettings);
                            var database = server.GetDatabase(_dbName);
                            mongoRepository = database.GetCollection<TEntity>(typeof(TEntity).Name);
                        }
                    }
                }
                return mongoRepository;
            }
        }
        /// <summary>
        /// 实例集合BsonDocument，一般用于Aggregate中
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public static IMongoCollection<BsonDocument> Collection
        {
            get
            {
                var svrSettings = MongoUrl.Create(ConnectionString());
                var server = new MongoClient(svrSettings);
                var database = server.GetDatabase(_dbName);
                var mongoRepository = database.GetCollection<BsonDocument>(typeof(TEntity).Name);
                return mongoRepository;
            }
        }
    }
}
