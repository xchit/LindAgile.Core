using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper.Contrib.Extensions;
namespace LindAgile.Core.IRepositories
{
    /// <summary>
    /// 使用Dapper实现的仓储
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class DapperRepository<TEntity> :
        IRepository<TEntity>
        where TEntity : class
    {
        IDbConnection conn;
        string connString;
        string tableName;
        public DapperRepository(string connString)
        {
            this.connString = connString;
            conn = new SqlConnection(connString);
            tableName = typeof(TEntity).Name;

        }

        #region IRepository<TEntity> 成员

        public TEntity Find(params object[] id)
        {
            return conn.Get<TEntity>(id);
        }

        public IQueryable<TEntity> GetModel()
        {
            return conn.GetAll<TEntity>().AsQueryable();
        }

        public void SetDataContext(object db)
        {
            throw new NotImplementedException();
        }

        public void Insert(TEntity item)
        {
            conn.Insert(item);
        }

        public void Update(TEntity item)
        {
            conn.Update(item);
        }

        public void Delete(TEntity item)
        {
            conn.Delete(item);
        }

        #endregion

    }
}
