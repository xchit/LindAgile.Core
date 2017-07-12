using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LindAgile.Core.Caching
{
    /// <summary>
    /// 缓存所需要的方法
    /// </summary>
    public interface ICache
    {
        /// <summary>
        /// 数据加入缓存，并使用全局配置的过期时间
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="obj">数据</param>
        void Put(string key, object obj);
        /// <summary>
        /// 数据加入缓存，并指定过期时间（分钟）
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="obj">数据</param>
        /// <param name="expireMinutes">过期时间</param>
        void Put(string key, object obj, int expireMinutes);
        /// <summary>
        /// 拿出缓存数据
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        object Get(string key);
        /// <summary>
        /// 手动删除缓存数据
        /// </summary>
        /// <param name="key"></param>
        void Delete(string key);
    }
}
