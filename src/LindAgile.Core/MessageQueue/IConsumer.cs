using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LindAgile.Core.MessageQueue
{
    /// <summary>
    /// 消费者
    /// </summary>
    public interface IConsumer
    {
        /// <summary>
        /// 消费消息
        /// topic用在配置文件里定义
        /// </summary>
        /// <param name="callback">回调</param>
        void Subscribe<TMessage>(Action<TMessage> callback) where TMessage : class, new();

        /// <summary>
        /// 消费消息
        /// </summary>
        /// <param name="topic">指定topic</param>
        /// <param name="callback">回调</param>
        void Subscribe<TMessage>(string topic, Action<TMessage> callback) where TMessage : class, new();
    }
}
