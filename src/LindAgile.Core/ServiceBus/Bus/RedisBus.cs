using Newtonsoft.Json;
using LindAgile.Core.GlobalConfig;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Loader;
using LindAgile.Core.Utils;
using System.Reflection;

namespace LindAgile.Core.ServiceBus
{
    internal class RedisBus : IBus
    {
        /// <summary>
        /// 模式锁
        /// </summary>
        private static object _objLock = new object();
        /// <summary>
        /// 对于事件数据的存储，目前采用内存字典
        /// </summary>
        private readonly IDatabase _redisClient = NoSql.RedisManager.Instance.GetDatabase();
        /// <summary>
        /// redis事件总线的Key
        /// </summary>
        private string redisKey = "ServiceBus";

        /// <summary>
        /// 得到当前redis-eventbus-key
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <returns></returns>
        private string GetCurrentRedisKey<TEvent>()
        {
            return redisKey + "_" + typeof(TEvent).FullName;
        }
        /// <summary>
        ///得到非泛型版本的值
        /// </summary>
        /// <param name="tEvent"></param>
        /// <returns></returns>
        private string GetCurrentRedisKey(Type tEvent)
        {
            return redisKey + "_" + tEvent.FullName;
        }

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
            lock (_objLock)
            {
                var newVal = JsonConvert.SerializeObject(eventHandler);
                _redisClient.SetAdd(GetCurrentRedisKey<TEvent>(), newVal);
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

            lock (_objLock)
            {


                _redisClient.SetRemove(GetCurrentRedisKey<TEvent>(), JsonConvert.SerializeObject((eventHandler)));
            }

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
            _redisClient.KeyDelete(GetCurrentRedisKey<TEvent>());

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

            lock (_objLock)
            {
                var _eventHandlers = _redisClient.SetMembers(GetCurrentRedisKey<TEvent>());
                if (@event == null)
                    throw new ArgumentNullException("event");

                if (_eventHandlers.Count() > 0)
                {
                    foreach (var handler in _eventHandlers)
                    {
                        var eventHandler = JsonConvert.DeserializeObject(handler) as IBusHandler<TEvent>;//显示处理程序
                        if (eventHandler == null)//匿名处理程序
                            eventHandler = JsonConvert.DeserializeObject(handler) as ActionDelegatedEventHandler<TEvent>;
                        if (eventHandler != null)//非正常处理程序
                        {
                            if (eventHandler.GetType().GetGenericTypeDefinition() == typeof(HandlesAsynchronouslyAttribute))
                            {
                                Task.Factory.StartNew((o) => eventHandler.Handle((TEvent)o), @event);
                            }
                            else
                            {
                                eventHandler.Handle(@event);
                            }
                        }
                    }

                }
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

            lock (_objLock)
            {
                var _eventHandlers = _redisClient.SetMembers(GetCurrentRedisKey<TEvent>());
                if (@event == null)
                    throw new ArgumentNullException("event");
                var eventType = @event.GetType();
                if (_eventHandlers.Count() > 0)
                {

                    List<Task> tasks = new List<Task>();

                    foreach (var handler in _eventHandlers)
                    {

                        var eventHandler = JsonConvert.DeserializeObject(handler) as IBusHandler<TEvent>;//显示处理程序
                        if (eventHandler == null)//匿名处理程序
                            eventHandler = JsonConvert.DeserializeObject(handler) as ActionDelegatedEventHandler<TEvent>;
                        if (eventHandler != null)//非正常处理程序
                        {


                            if (eventHandler.GetType().GetGenericTypeDefinition() == typeof(HandlesAsynchronouslyAttribute))
                            {
                                tasks.Add(Task.Factory.StartNew((o) => eventHandler.Handle((TEvent)o), @event));
                            }
                            else
                            {
                                eventHandler.Handle(@event);
                            }
                        }
                        if (tasks.Count > 0)
                        {
                            if (timeout == null)
                                Task.WaitAll(tasks.ToArray());
                            else
                                Task.WaitAll(tasks.ToArray(), timeout.Value);
                        }
                        callback(@event, true, null);

                    }
                }
            }
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

            foreach (var item in types)
            {
                if (!item.IsGenericParameter)
                {
                    var eventHandler = Activator.CreateInstance(item);
                    TypeInfo typeInfo = IntrospectionExtensions.GetTypeInfo(item);

                    foreach (var t in typeInfo.GetMethods().Where(i => i.Name == "Handle"))
                    {
                        var newVal = JsonConvert.SerializeObject(eventHandler);
                        _redisClient.SetAdd(GetCurrentRedisKey(t.GetParameters().First().ParameterType), newVal);
                    }
                }

            }

        }

        #endregion

        #endregion
    }
}
