using Newtonsoft.Json;
using Pilipa.Core.MessageQueue;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pilipa.Core.Adapter
{
    /// <summary>
    /// RabbitMq发布者
    /// </summary>
    public class RabbitMqPublisher : IProducer
    {
        private string _uri;
        private readonly string exchangeName = "";
        private readonly IConnection connection;
        private readonly IModel channel;
        private static object lockObj = new object();
        /// <summary>
        /// 初始化
        /// 子类去实现相关的rabbit地址,端口和授权
        /// </summary>
        /// <param name="uri">消息服务器地址</param>
        /// <param name="userName">用户名</param>
        /// <param name="password">密码</param>
        /// <param name="exchangeName">交换器,为空表示分发模式,否则为广播模式</param>
        public RabbitMqPublisher(string uri = "amqp://localhost:5672", string userName = "", string password = "", string exchangeName = "test")
        {

            _uri = uri;
            var factory = new ConnectionFactory()
            {
                Uri = _uri
            };
            if (!string.IsNullOrWhiteSpace(exchangeName))
                this.exchangeName = exchangeName;
            if (!string.IsNullOrWhiteSpace(userName))
                factory.UserName = userName;
            if (!string.IsNullOrWhiteSpace(userName))
                factory.Password = password;
            connection = factory.CreateConnection();
            this.channel = connection.CreateModel();

        }

        /// <summary>
        /// 将消息推送到服务器
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        /// <param name="topic"></param>
        /// <param name="message"></param>
        public void Publish<TMessage>(string topic, TMessage message)
        {
            channel.QueueDeclare(queue: topic,//队列名
                                     durable: false,//是否持久化
                                     exclusive: false,//排它性
                                     autoDelete: false,//一旦客户端连接断开则自动删除queue
                                     arguments: null);//如果安装了队列优先级插件则可以设置优先级

            var json =JsonConvert.SerializeObject(message);
            var bytes = Encoding.UTF8.GetBytes(json);
            Console.WriteLine("向服务器{0}推消息", _uri);
            channel.BasicPublish(exchange: this.exchangeName, routingKey: topic, basicProperties: null, body: bytes);
        }

        /// <summary>
        /// 广播消息,需要在初始化时为exchangeName赋值
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        /// <param name="message"></param>
        public void Publish<TMessage>(TMessage message)
        {
            const string ROUTING_KEY = "";
            channel.ExchangeDeclare(this.exchangeName, "fanout");//广播
            var json = JsonConvert.SerializeObject(message);
            var bytes = Encoding.UTF8.GetBytes(json);
            channel.BasicPublish(this.exchangeName, ROUTING_KEY, null, bytes);//不需要指定routing key，设置了fanout,指了也没有用.
            Console.WriteLine(DateTime.Now + " 向服务器{0}推消息", _uri);
        }


    }


    /// <summary>
    /// RabbitMq消息消费者
    /// </summary>
    public class RabbitMqSubscriber : IConsumer
    {
        private readonly string exchangeName;
        private string queueName = "default";
        private readonly IConnection connection;
        private readonly IModel channel;

        /// <summary>
        /// 初始化消费者
        /// </summary>
        /// <param name="uri">消息服务器地址</param>
        /// <param name="queueName">队列名</param>
        /// <param name="userName">用户</param>
        /// <param name="password">密码</param>
        /// <param name="exchangeName">交换机,有值表示广播模式</param>
        public RabbitMqSubscriber(string uri= "amqp://localhost:5672", string userName = "", string password = "", string exchangeName = "")
        {
            var factory = new ConnectionFactory() { Uri = uri };
            if (!string.IsNullOrWhiteSpace(exchangeName))
                this.exchangeName = exchangeName;
            if (!string.IsNullOrWhiteSpace(userName))
                factory.UserName = userName;
            if (!string.IsNullOrWhiteSpace(password))
                factory.Password = password;
            this.connection = factory.CreateConnection();
            this.channel = connection.CreateModel();
        }
        /// <summary>
        ///  触发消费行为
        /// </summary>
        /// <param name="callback">回调方法</param>
        public void Subscribe<TMessage>(Action<TMessage> callback) where TMessage : class, new()
        {
            if (string.IsNullOrWhiteSpace(exchangeName))//发发模式
            {
                channel.QueueDeclare(
                    queue: this.queueName,
                    durable: false,//持久化
                    exclusive: false, //独占,只能被一个consumer使用
                    autoDelete: false,//自己删除,在最后一个consumer完成后删除它
                    arguments: null);
            }
            else
            {
                //广播模式
                channel.ExchangeDeclare(this.exchangeName, "fanout");//广播
                QueueDeclareOk queueOk = channel.QueueDeclare();//每当Consumer连接时，我们需要一个新的，空的queue,如果在声明queue时不指定,那么RabbitMQ会随机为我们选择这个名字
                string queueName = queueOk.QueueName;//得到RabbitMQ帮我们取了名字
                channel.QueueBind(queueName, this.exchangeName, string.Empty);//不需要指定routing key，设置了fanout,指了也没有用.
            }
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (sender, e) =>
            {
                var body = e.Body;
                var json = Encoding.UTF8.GetString(body);
                callback(JsonConvert.DeserializeObject<TMessage>(json));
                channel.BasicAck(e.DeliveryTag, multiple: false);
            };
            channel.BasicConsume(queue: this.queueName,
                                 noAck: false,
                                 consumer: consumer);
            Console.WriteLine(" [*] Waiting for messages." + "To exit press CTRL+C");

        }

        public void Subscribe<TMessage>(string topic, Action<TMessage> callback) where TMessage : class, new()
        {
            this.queueName = topic;
            Subscribe<TMessage>(callback);
        }
    }

}
