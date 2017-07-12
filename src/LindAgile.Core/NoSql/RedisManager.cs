using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StackExchange.Redis;
using LindAgile.Core.Logger;

namespace LindAgile.Core.NoSql
{
    /// <summary>
    /// StackExchange.Redis管理者
    /// 添加了tw和sentionel的机制(sentionel不支持密码)
    /// 相关appsetting配置:redisProxy(0和1),redisIssentinel(0和1),redisServiceName(服务名称)
    /// 注意：这个客户端没有连接池的概念，需要注意生产redis的数量
    /// 由于支持的集群环境，所以不能做成单例
    /// </summary>
    public class RedisManager
    {
        /// <summary>
        /// 锁对象
        /// </summary>
        private static object _locker = new object();
        /// <summary>
        /// StackExchange.Redis对象
        /// </summary>
        private static ConnectionMultiplexer _redis;

        /// <summary>
        /// 代理类型
        /// None = 0,
        /// Twemproxy = 1,
        /// </summary>
        private static int proxy
        {
            get
            {
                return GlobalConfig.ConfigManager.Config.Redis.Proxy;
            }
        }
        /// <summary>
        /// 是否为sentinel服务器
        /// yes=1,
        /// no=0,
        /// </summary>
        private static int issentinel
        {
            get
            {
                return GlobalConfig.ConfigManager.Config.Redis.IsSentinel;
            }
        }
        /// <summary>
        /// sentinel模式下,redis主数据服务器的密码
        /// </summary>
        private static string authPassword
        {
            get
            {
                return GlobalConfig.ConfigManager.Config.Redis.AuthPassword;
            }
        }
        /// <summary>
        /// serverName
        /// </summary>
        private static string serviceName
        {
            get
            {
                return GlobalConfig.ConfigManager.Config.Redis.ServiceName;
            }
        }

        /// <summary>
        /// 得到StackExchange.Redis单例对象
        /// </summary>
        public static ConnectionMultiplexer Instance
        {
            get
            {
                if (_redis == null || !_redis.IsConnected)
                {
                    lock (_locker)
                    {
                        if (_redis == null || !_redis.IsConnected)
                        {

                            _redis = GetManager();
                            return _redis;
                        }
                    }
                }

                return _redis;
            }
        }

        /// <summary>
        /// 构建链接
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        private static ConnectionMultiplexer GetManager()
        {
            return GetCurrentRedis();
        }

        /// <summary>
        /// 每个连接串用逗号分开,sentinel不支持密码
        /// </summary>
        /// <returns></returns>
        static ConnectionMultiplexer GetCurrentRedis()
        {
            var connectionString = GlobalConfig.ConfigManager.Config.Redis.Host;
            ConnectionMultiplexer conn;
            var option = new ConfigurationOptions();
            option.Proxy = (Proxy)proxy;//代理模式,目前支持TW


            //sentinel模式下自动连接主redis
            if (issentinel == 1)
            {
                var configArr = connectionString.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                configArr.ToList().ForEach(i =>
                {
                    Modules.ModuleManager.Resolve<ILogger>().Logger_Debug("join redis sentinel host:" + i);
                    option.EndPoints.Add(i);
                });

                option.TieBreaker = "";//这行在sentinel模式必须加上
                option.CommandMap = CommandMap.Sentinel;
                conn = ConnectionMultiplexer.Connect(option);

                for (int i = 0; i < option.EndPoints.Count; i++)
                {

                    try
                    {
                        connectionString = conn.GetServer(option.EndPoints[i]).SentinelGetMasterAddressByName(serviceName).ToString();
                        Console.WriteLine("当前主master[{0}]:{1}", i, connectionString);
                        break;
                    }
                    catch (RedisConnectionException ex)//超时
                    {
                        Modules.ModuleManager.Resolve<ILogger>().Logger_Debug("GetCurrentRedis().RedisConnectionException" + ex.Message);
                        continue;
                    }
                    catch (Exception ex)
                    {
                        Modules.ModuleManager.Resolve<ILogger>().Logger_Debug("GetCurrentRedis().Exception" + ex.Message);
                        continue;
                    }
                }

            }

            return ConnectionMultiplexer.Connect(connectionString + ",password=" + authPassword);
        }

    }

}
