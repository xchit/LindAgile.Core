using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace LindAgile.Core.GlobalConfig.Models
{
    /// <summary>
    /// 配置信息实体
    /// </summary>
    public class ConfigModel
    {
        public ConfigModel()
        {
            Caching = new Caching();
            Logger = new Logger();
            MongoDB = new MongoDB();
            Redis = new Redis();

        }

        /// <summary>
        /// 缓存相关配置
        /// </summary>
        public Caching Caching { get; set; }
        /// <summary>
        /// 日志相关
        /// </summary>
        public Logger Logger { get; set; }

        /// <summary>
        /// MongoDB相关
        /// </summary>
        public MongoDB MongoDB { get; set; }
        /// <summary>
        /// redis相关
        /// </summary>
        public Redis Redis { get; set; }
    }

    /// <summary>
    /// 缓存Caching(Redis,RunTime)
    /// </summary>
    public class Caching
    {
        #region 缓存Caching(Redis,RunTime)
        /// <summary>
        /// 缓存提供者:RuntimeCache,RedisCache
        /// </summary>
        [DisplayName("缓存提供者:RuntimeCache,RedisCache")]
        public string Provider { get; set; }
        /// <summary>
        /// 缓存过期时间(minutes)
        /// </summary>
        [DisplayName("缓存过期时间(minutes)")]
        public int ExpireMinutes { get; set; }
        #endregion
    }
    /// <summary>
    /// 日志相关
    /// 日志Logger(File,Log4net,MongoDB)
    /// </summary>
    public class Logger
    {
        #region 日志Logger(File,Log4net,MongoDB)
        /// <summary>
        /// 日志实现方式：File,Log4net,MongoDB
        /// </summary>
        [DisplayName("日志实现方式：File,Log4net,MongoDB")]
        public string Type { get; set; }
        /// <summary>
        /// 日志级别：DEBUG|INFO|WARN|ERROR|FATAL|OFF
        /// </summary>
        [DisplayName("日志级别：DEBUG|INFO|WARN|ERROR|FATAL|OFF")]
        public string Level { get; set; }
        /// <summary>
        /// 日志记录的项目名称
        /// </summary>
        [DisplayName("日志记录的项目名称")]
        public string ProjectName { get; set; }
        #endregion
    }

    /// <summary>
    /// Redis相关配置
    /// </summary>
    public class Redis
    {
        #region Redis
        /// <summary>
        /// redis缓存的连接串
        /// var conn = ConnectionMultiplexer.Connect("contoso5.redis.cache.windows.net,password=...");
        /// </summary>
        [DisplayName("StackExchange.redis缓存的连接串")]
        public string Host { get; set; }
        [DisplayName("StackExchange.redis代理模式（可选0:无，1：TW")]
        public int Proxy { get; set; }
        [DisplayName("是否为sentinel模式(可选0:连接普通redis，1：连接Sentinel)")]
        public int IsSentinel { get; set; }
        [DisplayName("Sentinel服务名称)")]
        public string ServiceName { get; set; }
        [DisplayName("Sentinel模式下Redis数据服务器的密码)")]
        public string AuthPassword { get; set; }
        #endregion
    }
    /// <summary>
    /// MongoDB相关配置
    /// </summary>
    public class MongoDB
    {
        #region MongoDB
        /// <summary>
        /// Mongo连接串，支持多路由localhost:27017,localhost:27018,localhost:27018
        /// </summary>
        [DisplayName("Mongo连接串，支持多路由localhost:27017,localhost:27018,localhost:27018")]
        public string Host { get; set; }
        /// <summary>
        /// Mongo-数据库名称
        /// </summary>
        [DisplayName("Mongo-数据库名称")]
        public string DbName { get; set; }
        /// <summary>
        /// Mongo-登陆名
        /// </summary>
        [DisplayName("Mongo-登陆名")]
        public string UserName { get; set; }
        /// <summary>
        /// Mongo-密码
        /// </summary>
        [DisplayName("Mongo-密码")]
        public string Password { get; set; }
        #endregion
    }

}
