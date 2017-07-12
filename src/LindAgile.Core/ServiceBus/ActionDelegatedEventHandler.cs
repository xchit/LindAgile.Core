using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LindAgile.Core.ServiceBus
{
    /// <summary>
    /// 代理事件处理器[伪装处理程序,灵活加载事件]
    /// 使用了装饰器模式，将Action<TEventBase>委托“装饰”成IEventHandler<TEventBase>
    /// 本对象不支持自动化注册SubscribeAll
    /// </summary>
    /// <typeparam name="TEvent"></typeparam>
    [Serializable]
    public class ActionDelegatedEventHandler<TEvent> : IBusHandler<TEvent>
        where TEvent : IBusData
    {

        #region Private Fields
        private readonly Action<TEvent> eventHandlerDelegate;
        #endregion

        #region Ctor
        /// <summary>
        /// 初始化一个新的<c>ActionDelegatedDomainEventHandler{TEvent}</c>实例。
        /// </summary>
        /// <param name="eventHandlerDelegate">用于当前领域事件处理器所代理的事件处理委托。</param>
        public ActionDelegatedEventHandler(Action<TEvent> eventHandlerDelegate)
        {
            this.eventHandlerDelegate = eventHandlerDelegate;
        }
        #endregion

        #region IEventHandler<TEvent> Members
        /// <summary>
        /// 处理给定的事件。
        /// 事实上，它是调用你传递过来的委托的实例，注意，这块设计的很巧妙
        /// </summary>
        /// <param name="evnt">需要处理的事件。</param>
        public void Handle(TEvent evnt)
        {
            this.eventHandlerDelegate(evnt);
        }

        #endregion

        /// <summary>
        /// 获取一个<see cref="Boolean"/>值，该值表示当前对象是否与给定的类型相同的另一对象相等。
        /// </summary>
        /// <param name="other">需要比较的与当前对象类型相同的另一对象。</param>
        /// <returns>如果两者相等，则返回true，否则返回false。</returns>
        public override bool Equals(object other)
        {
            if (ReferenceEquals(this, other))
                return true;
            if ((object)other == (object)null)
                return false;
            ActionDelegatedEventHandler<TEvent> otherDelegate = other as ActionDelegatedEventHandler<TEvent>;
            if ((object)otherDelegate == (object)null)
                return false;
            // 使用Delegate.Equals方法判定两个委托是否是代理的同一方法。
            return Delegate.Equals(this.eventHandlerDelegate, otherDelegate.eventHandlerDelegate);
        }

        public override int GetHashCode()
        {
            return this.eventHandlerDelegate.GetHashCode();
        }
    }
}
