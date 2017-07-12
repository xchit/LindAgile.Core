using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace LindAgile.Core.Caching
{
    /// <summary>
    /// 运行时缓存，基于服务端内存存储
    /// </summary>
    public class RuntimeCache : ICache
    {
        readonly static Dictionary<string, object> httpRuntimeCache = new Dictionary<string, object>();

        #region ICache 成员

        public void Put(string key, object obj)
        {
            this.Put(key, obj);
        }

        public void Put(string key, object obj, int expireMinutes)
        {
            httpRuntimeCache.Add(key, obj);
        }

        public object Get(string key)
        {
            return httpRuntimeCache[key];
        }

        public void Delete(string key)
        {
            httpRuntimeCache.Remove(key);
        }

        #endregion
    }
}
