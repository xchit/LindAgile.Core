using System;
namespace LindAgile.Core.ServiceBus
{
    /// <summary>
    /// 事件总线，生产者接口
    /// </summary>
    public interface IBus
    {
        /// <summary>
        ///  发布事件，支持异步事件
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <param name="event"></param>
        void Publish<TEvent>(TEvent @event) where TEvent : class, IBusData;
        /// <summary>
        ///  发布事件
        /// event参数为关键字,所以加了@符
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <param name="event"></param>
        /// <param name="callback"></param>
        /// <param name="timeout"></param>
        void Publish<TEvent>(TEvent @event, Action<TEvent, bool, Exception> callback, TimeSpan? timeout = null) where TEvent : class, IBusData;
        /// <summary>
        ///  订阅事件列表
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <param name="eventHandler"></param>
        void Subscribe<TEvent>(IBusHandler<TEvent> eventHandler) where TEvent : class, IBusData;
        /// <summary>
        /// 订阅事件实体
        /// 装饰模式
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <param name="eventHandlerFunc"></param>
        void Subscribe<TEvent>(Action<TEvent> eventHandlerFunc) where TEvent : class, IBusData;

        /// <summary>
        /// 取消订阅
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <param name="eventHandler"></param>
        void Unsubscribe<TEvent>(IBusHandler<TEvent> eventHandler) where TEvent : class, IBusData;
        /// <summary>
        /// 取消订阅
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <param name="eventHandlerFunc"></param>
        void Unsubscribe<TEvent>(Action<TEvent> eventHandlerFunc) where TEvent : class, IBusData;

        /// <summary>
        /// 取消订阅全部事件
        /// </summary>
        void UnsubscribeAll();
        /// <summary>
        /// 取消订阅全部事件
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        void UnsubscribeAll<TEvent>() where TEvent : class, IBusData;
        /// <summary>
        /// 订阅全部事件，实现了IEventHandler的类型
        /// </summary>
        void SubscribeAll();
    }
}
