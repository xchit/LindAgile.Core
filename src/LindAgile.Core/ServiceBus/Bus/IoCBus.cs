using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using LindAgile.Core.Utils;
using System.Reflection;
using LindAgile.Core.Modules;
using LindAgile.Core.Adapter;
using LindAgile.Core.NoSql;
using LindAgile.Core.Caching;

namespace LindAgile.Core.ServiceBus
{
    /// <summary>
    /// 通过依赖注入的方式实现事件总线
    /// </summary>
    internal class IoCBus : IBus
    {
        /// <summary>
        /// redis key
        /// </summary>
        const string ESBKEY = "IoCESBBus";
        /// <summary>
        /// cache事件字典
        /// </summary>
        ICache cache = ModuleManager.Resolve<ICache>();
        /// <summary>
        /// 模式锁
        /// </summary>
        private static object _objLock = new object();
        /// <summary>
        /// 对于事件数据的存储，目前采用内存字典
        /// </summary>
        private readonly IContainer container = new AutofacContainer();

        #region IEventBus 成员

        #region 事件订阅 & 取消订阅，可以扩展
        /// <summary>
        /// 订阅事件列表
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <param name="eventHandler"></param>
        public void Subscribe<TEvent>(IBusHandler<TEvent> eventHandler)
            where TEvent : class, IBusData
        {
            var eventKey = typeof(TEvent).Name;
            var key = typeof(TEvent).Name + "_" + eventHandler.GetType().Name;
            Dictionary<string, List<string>> keyDic = new Dictionary<string, List<string>>();
            if (keyDic.ContainsKey(eventKey))
            {
                var oldEvent = keyDic[eventKey];
                oldEvent.Add(key);
            }
            else
            {
                var newEvent = new List<string>();
                newEvent.Add(key);
                keyDic.Add(eventKey, newEvent);
            }

            container.Register(typeof(IBusHandler<TEvent>), eventHandler.GetType(), key);
            //redis存储事件与处理程序的映射关系
            foreach (var hash in keyDic)
            {
                RedisManager.Instance.GetDatabase().HashSet(
                    ESBKEY,
                    hash.Key.ToString(),
                    JsonConvert.SerializeObject(hash.Value));
            }


        }
        /// <summary>
        /// 订阅事件实体
        /// 装饰模式
        /// </summary>
        /// <param name="type"></param>
        /// <param name="subTypeList"></param>
        public void Subscribe<TEvent>(Action<TEvent> eventHandlerFunc)
            where TEvent : class, IBusData
        {
            Subscribe<TEvent>(new ActionDelegatedEventHandler<TEvent>(eventHandlerFunc));
        }

        /// <summary>
        /// 取消订阅事件
        /// </summary>
        /// <param name="type"></param>
        /// <param name="subType"></param>
        public void Unsubscribe<TEvent>(IBusHandler<TEvent> eventHandler)
            where TEvent : class, IBusData
        {
            throw new NotImplementedException("本方法不被实现");
        }

        public void Unsubscribe<TEvent>(Action<TEvent> eventHandlerFunc)
            where TEvent : class, IBusData
        {
            Unsubscribe<TEvent>(new ActionDelegatedEventHandler<TEvent>(eventHandlerFunc));
        }
        /// <summary>
        /// 取消指定事件的所有订阅
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <param name="eventHandlerFunc"></param>
        public void UnsubscribeAll<TEvent>()
        where TEvent : class, IBusData
        {
            throw new NotImplementedException("本方法不被实现");
        }
        /// <summary>
        /// 取消所有事件的所有订阅
        /// </summary>
        public void UnsubscribeAll()
        {
            throw new NotImplementedException("本方法不被实现");
        }
        #endregion

        #region 事件发布
        /// <summary>
        /// 发布事件，支持异步事件
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <param name="evnt"></para m>
        public void Publish<TEvent>(TEvent @event)
           where TEvent : class, IBusData
        {
            var keyArr = JsonConvert.DeserializeObject<List<string>>(RedisManager.Instance.GetDatabase().HashGet(ESBKEY, typeof(TEvent).Name));
            foreach (var key in keyArr)
            {
                var item = container.ResolveNamed<IBusHandler<TEvent>>(key);
                item.Handle(@event);
            }

        }
        /// <summary>
        /// 发布事件
        /// event参数为关键字,所以加了@符
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <param name="event"></param>
        /// <param name="callback"></param>
        /// <param name="timeout"></param>
        public void Publish<TEvent>(TEvent @event, Action<TEvent, bool, Exception> callback, TimeSpan? timeout = null)
           where TEvent : class, IBusData
        {
            Publish(@event);
            callback(@event, true, null);
        }

        #endregion

        #region 订阅所有

        /// <summary>
        /// 全局统一注册所有事件处理程序，实现了IEventHandlers的
        /// 目前只支持注册显示实现IEventHandlers的处理程序，不支持匿名处理程序
        /// </summary>
        public void SubscribeAll()
        {
            var types = AssemblyHelper.GetTypesByInterfaces(typeof(IBusHandler<>));
            Dictionary<string, List<string>> keyDic = new Dictionary<string, List<string>>();
            foreach (var item in types)
            {
                if (!item.IsGenericParameter)
                {

                    TypeInfo typeInfo = IntrospectionExtensions.GetTypeInfo(item);

                    foreach (var t in typeInfo.GetMethods().Where(i => i.Name == "Handle"))
                    {
                        //ioc name key
                        var eventKey = t.GetParameters().First().ParameterType.Name;
                        var key = t.GetParameters().First().ParameterType.Name + "_" + item.Name;
                        //eventhandler
                        var inter = typeof(IBusHandler<>).MakeGenericType(t.GetParameters().First().ParameterType);
                        container.Register(inter, item, key);

                        if (keyDic.ContainsKey(eventKey))
                        {
                            var oldEvent = keyDic[eventKey];
                            oldEvent.Add(key);
                        }
                        else
                        {
                            var newEvent = new List<string>();
                            newEvent.Add(key);
                            keyDic.Add(eventKey, newEvent);
                        }
                    }
                }
                //redis存储事件与处理程序的映射关系
                foreach (var hash in keyDic)
                    RedisManager.Instance.GetDatabase().HashSet(
                        ESBKEY,
                        hash.Key.ToString(),
                        JsonConvert.SerializeObject(hash.Value));

            }

        }

        #endregion

        #endregion
    }
}
