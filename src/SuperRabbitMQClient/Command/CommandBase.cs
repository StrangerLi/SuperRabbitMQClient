using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace SuperRabbitMQClient.Command
{
    public abstract class CommandBase : IRabbitMQCommand
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
        public virtual uint PrefetchSize
        {
            get
            {
                return 0;
            }
        }

        /// <summary>
        /// 当前队列接收器，允许的最大并发消息量
        /// </summary>
        public virtual ushort PrefetchCount
        {
            get
            {
                return 1;
            }
        }

        /// <summary>
        /// 是否自动进行消息确认
        /// </summary>
        public virtual bool AutoAck
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// 消息队列接收事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="channel"></param>
        /// <param name="e"></param>
        public abstract void Queue_Received(object sender, IModel channel, BasicDeliverEventArgs e);

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
