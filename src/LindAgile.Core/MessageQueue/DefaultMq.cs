using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LindAgile.Core.MessageQueue
{
    /// <summary>
    /// 默认的消息队列
    /// </summary>
    public class DefaultMq : IProducer, IConsumer
    {
        static ConcurrentDictionary<string, Queue<string>> queueList = new ConcurrentDictionary<string, Queue<string>>();

        string _topic;
        public DefaultMq(string consumerTopic)
        {
            _topic = consumerTopic;
        }
        public DefaultMq() : this("test")
        {

        }
        public void Subscribe(Action<string> callback)
        {
            Queue<string> _queue;
            queueList.TryGetValue(_topic, out _queue);
            var message = _queue.Dequeue();
            callback(message);
        }

        public void Subscribe(string topic, Action<string> callback)
        {
            Queue<string> _queue;
            queueList.TryGetValue(topic, out _queue);
            var message = _queue.Dequeue();
            callback(message);
        }

        public void Publish<TMessage>(string topic, TMessage message)
        {
            Queue<string> _queue;
            if (!queueList.TryGetValue(topic, out _queue))
                _queue = new Queue<string>();
            _queue.Enqueue(JsonConvert.SerializeObject(message));
            queueList.TryAdd(topic, _queue);
        }

        public void Publish<TMessage>(TMessage message)
        {
            Publish<TMessage>(_topic, message);
        }

        public void Subscribe<TMessage>(Action<TMessage> callback) where TMessage : class, new()
        {
            Subscribe<TMessage>(_topic, callback);
        }

        public void Subscribe<TMessage>(string topic, Action<TMessage> callback) where TMessage : class, new()
        {
            Queue<string> _queue;
            queueList.TryGetValue(topic, out _queue);
            var message = JsonConvert.DeserializeObject<TMessage>(_queue.Dequeue());
            callback(message);
        }
    }
}
