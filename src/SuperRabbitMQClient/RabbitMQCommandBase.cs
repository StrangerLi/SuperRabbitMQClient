using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperRabbitMQClient
{
    /// <summary>
    /// 任务处理Command基类
    /// </summary>
    public abstract class RabbitMQCommandBase : IRabbitMQCommand
    {
        /// <summary>
        /// 默认命令名
        /// </summary>
        public virtual string Name
        {
            get
            {
                return GetType().Name;
            }
        }

        /// <summary>
        /// 当前队列接收器，允许的最大未确认消息量
        /// </summary>
        public uint PrefetchSize
        {
            get
            {
                return 0;
            }
        }

        /// <summary>
        /// 当前队列接收器，允许的最大并发消息量
        /// </summary>
        public ushort PrefetchCount
        {
            get
            {
                return 1;
            }
        }

        /// <summary>
        /// 是否自动进行消息确认
        /// </summary>
        public bool AutoAck
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// 默认处理方法
        /// </summary>
        /// <param name="Parameter"></param>
        /// <returns></returns>
        public abstract bool Event_MessageReceive(byte[] Parameter);

        /// <summary>
        /// 消息队列接收事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="channel"></param>
        /// <param name="e"></param>
        public void Queue_Received(object sender, IModel channel, BasicDeliverEventArgs e)
        {
            //Console.WriteLine("接到消息e.RoutingKey:{0}", e.RoutingKey);
            var m_sender = sender is IBasicConsumer ? sender as IBasicConsumer : null;
            if (m_sender != null)
            {
                try
                {
                    Event_MessageReceive(e.Body);
                }
                catch (Exception ex)
                {
                    LogFactory.GetLogger().LogMessage("RabbitMQ", LogLevel.Debug & LogLevel.Error, ex);
                }
            }
            //手动应答RabbitMQ服务器
            //channel.BasicAck(e.DeliveryTag, false);
        }

        /// <summary>
        /// 获取本类的完全限定名
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return GetType().AssemblyQualifiedName;
        }
    }
}
