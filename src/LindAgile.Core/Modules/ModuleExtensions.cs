using LindAgile.Core.Caching;
using LindAgile.Core.Adapter;
using LindAgile.Core.IRepositories;
using LindAgile.Core.Logger;
using LindAgile.Core.MessageQueue;
using LindAgile.Core.ServiceBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LindAgile.Core.Modules
{
    /// <summary>
    /// function:module design
    /// author:lind
    /// </summary>
    public static class ModuleExtensions
    {
        #region 容器注入
        /// <summary>
        /// 注册一个默认的容器，接口IContainer，用来存储对象与接口的映射关系
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static ModuleManager UseDefaultContainer(this ModuleManager configuration)
        {
            configuration.SetContainer(new DefaultContainer());
            return configuration;
        }

        /// <summary>
        /// 注册一个autofac容器，接口IContainer
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static ModuleManager UseAutofac(this ModuleManager configuration)
        {
            configuration.SetContainer(new AutofacContainer());
            return configuration;
        }
        #endregion

        #region 日志组件
        /// 注册一个Lind框架的日志组件，接口ILogger
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static ModuleManager UseLindLogger(this ModuleManager configuration)
        {
            Logger.LoggerFactory.SetCurrent(new LindLogger());
            return configuration;
        }

        /// <summary>
        /// 注册一个Mongodb日志组件，接口ILogger
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static ModuleManager UseMongoLogger(this ModuleManager configuration)
        {
            Logger.LoggerFactory.SetCurrent(new MongoLogger());
            return configuration;
        }
        #endregion

        #region 数据仓储
        /// <summary>
        /// 注册一个EF数据仓储，接口IRepository
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static ModuleManager UseEfRepository(this ModuleManager configuration)
        {
            configuration.RegisterGenericModule(
                typeof(IRepositories.IRepository<>),
                typeof(IRepositories.EFRepository<>));
            return configuration;
        }


        /// <summary>
        /// 注册一个mock内存数据仓储，接口IRepository
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static ModuleManager UseMockRepository(this ModuleManager configuration)
        {
            configuration.RegisterGenericModule(typeof(IRepositories.IRepository<>), typeof(IRepositories.MockRepository<>));
            return configuration;
        }

        /// <summary>
        /// 注册一个Dapper数据仓储，接口IRepository
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static ModuleManager UseDapperRepository(this ModuleManager configuration)
        {
            configuration.RegisterGenericModule(typeof(IRepositories.IRepository<>), typeof(IRepositories.DapperRepository<>));
            return configuration;
        }

        /// <summary>
        /// 注册一个Mongodb数据仓储，接口IRepository
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static ModuleManager UseMongoDbRepository(this ModuleManager configuration)
        {
            configuration.RegisterGenericModule(typeof(IRepositories.IRepository<>), typeof(IRepositories.MongoRepository<>));
            return configuration;
        }
        #endregion

        #region 消息队列
        /// <summary>
        /// 注册一个默认的消息队列组件，接口IProducer和IConsumer
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static ModuleManager UseDefaultMq(this ModuleManager configuration)
        {
            configuration.RegisterModule<IProducer, DefaultMq>();
            configuration.RegisterModule<IConsumer, DefaultMq>();
            return configuration;
        }
        /// <summary>
        /// 注册一个rabbitMq消息队列组件，接口IProducer和IConsumer
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static ModuleManager UseRabbitMq(this ModuleManager configuration)
        {
            configuration.RegisterModule<IProducer, RabbitMqPublisher>();
            configuration.RegisterModule<IConsumer, RabbitMqSubscriber>();
            return configuration;
        }
        #endregion

        #region 服务总线
        /// <summary>
        /// 注册一个服务总线，基于内存，接口IBus
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static ModuleManager UseESBRunTime(this ModuleManager configuration)
        {
            configuration.RegisterModule<IBus, MemoryBus>();
            return configuration;
        }
        /// <summary>
        /// 注册一个服务总线，基于redis，接口IBus
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static ModuleManager UseESBRedis(this ModuleManager configuration)
        {
            configuration.RegisterModule<IBus, RedisBus>();
            return configuration;
        }
        /// <summary>
        /// 注册一个服务总线，基于ioc容器，接口IBus
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static ModuleManager UseESBIoC(this ModuleManager configuration)
        {
            configuration.RegisterModule<IBus, IoCBus>();
            return configuration;
        }
        #endregion

        #region 缓存组件
        /// <summary>
        /// 使用内存作为缓存
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static ModuleManager UseDefaultCache(this ModuleManager configuration)
        {
            configuration.RegisterModule<ICache, RuntimeCache>();
            return configuration;
        }

        /// <summary>
        /// 使用redis作为缓存
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static ModuleManager UseRedisCache(this ModuleManager configuration)
        {
            configuration.RegisterModule<ICache, RedisCache>();
            return configuration;
        }
        #endregion
    }
}
