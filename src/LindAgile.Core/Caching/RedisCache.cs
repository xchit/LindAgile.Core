using LindAgile.Core.NoSql;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LindAgile.Core.Caching
{
    /// <summary>
    /// 使用redis
    /// </summary>
    public class RedisCache : ICache
    {
        const string RedisCacheKey = "Redis_Cache::";
        #region ICache 成员
        string getKey(string key)
        {
            return RedisCacheKey + key;
        }
        public void Put(string key, object obj)
        {
            this.Put(key, obj);
        }

        public void Put(string key, object obj, int expireMinutes)
        {
            RedisManager.Instance.GetDatabase().KeyExpire(getKey(key), DateTime.Now.AddMinutes(expireMinutes));
            RedisManager.Instance.GetDatabase().StringSet(getKey(key), JsonConvert.SerializeObject(obj));
        }

        public object Get(string key)
        {
            if (RedisManager.Instance.GetDatabase().StringGet(getKey(key)).HasValue)
                return JsonConvert.DeserializeObject(RedisManager.Instance.GetDatabase().StringGet(getKey(key)).ToString());
            else
                return null;
        }

        public void Delete(string key)
        {
            NoSql.RedisManager.Instance.GetDatabase().KeyDelete(getKey(key));
        }

        #endregion
    }


}
