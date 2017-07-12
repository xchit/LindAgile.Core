using LindAgile.Core.Domain;
using LindAgile.Core.Logger;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LindAgile.Core.IRepositories
{
    /// <summary>
    /// 沙箱数据库
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class MockRepository<TEntity> : IRepository<TEntity>
        where TEntity : Entity
    {
        #region Fields
        /// <summary>
        /// 库
        /// </summary>
        readonly static ConcurrentDictionary<string, List<TEntity>> db = new ConcurrentDictionary<string, List<TEntity>>();
        /// <summary>
        /// 表
        /// </summary>
        List<TEntity> tbl;
        /// <summary>
        /// 日志
        /// </summary>
        ILogger _logger;
        #endregion

        public MockRepository(ILogger logger = null)
        {
            _logger = logger ?? new EmptyLogger();
            var tblName = typeof(TEntity).Name;
            if (!db.ContainsKey(tblName))
            {
                db.TryAdd(tblName, new List<TEntity>());
            }

            db.TryGetValue(tblName, out tbl);
        }
        #region IRepository<TEntity> 成员

        public TEntity Find(params object[] id)
        {
            return db[typeof(TEntity).Name].Find(i => i.Id.Equals(id[0]));
        }

        public IQueryable<TEntity> GetModel()
        {
            return db[typeof(TEntity).Name].AsQueryable();
        }

        public void SetDataContext(object db)
        {
            throw new NotImplementedException();
        }

        public void Insert(TEntity item)
        {
            tbl.Add(item);
        }

        public void Update(TEntity item)
        {
            tbl.Remove(item);
            tbl.Add(item);
        }

        public void Delete(TEntity item)
        {
            tbl.Remove(item);
        }

        #endregion
    }
}
