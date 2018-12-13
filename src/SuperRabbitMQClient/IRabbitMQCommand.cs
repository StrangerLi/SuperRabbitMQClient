using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace SuperRabbitMQClient
{
 public   interface IRabbitMQCommand
    {
        /// <summary>
        /// 队列名
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 当前队列接收器，允许的最大未确认消息量
        /// </summary>
        uint PrefetchSize { get; }

        /// <summary>
        /// 当前队列接收器，允许的最大并发消息量
        /// </summary>
        ushort PrefetchCount { get; }

        /// <summary>
        /// 是否自动进行消息确认
        /// </summary>
        bool AutoAck { get; }

        /// <summary>
        /// 队列接收到消息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="channel"></param>
        /// <param name="e"></param>
        void Queue_Received(object sender, IModel channel, BasicDeliverEventArgs e);

        /// <summary>
        /// 获取完全限定名
        /// </summary>
        /// <returns></returns>
        string ToString();
    }
}
