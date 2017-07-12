using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LindAgile.Core.MessageQueue
{
    /// <summary>
    /// 生产者
    /// </summary>
    public interface IProducer
    {
        /// <summary>
        /// 生产消息
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="message"></param>
        void Publish<TMessage>(string topic, TMessage message);

        /// <summary>
        /// 生产消息
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="message"></param>
        void Publish<TMessage>(TMessage message);
    }
}
