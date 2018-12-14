using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace SuperRabbitMQClient.Command.ExampleClient1
{
    /// <summary>
    /// ZZSM_WH1_WMS_CMD
    /// </summary>
    public class DemoCommand : AbstractCommand
    {
        /// <summary>
        /// 命令名：默认为当前类名
        /// </summary>
        public override string Name
        {
            get
            {
                return "DemoCommand";
            }
        }

        /// <summary>
        /// 消息接收-触发
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="channel"></param>
        /// <param name="e"></param>
        public override void Queue_Received(object sender, IModel channel, BasicDeliverEventArgs e)
        {
            //throw new NotImplementedException();
            Console.WriteLine("Client1");
        }

        /// <summary>
        /// 获取当前类的完全限定名
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return base.ToString();
        }


    }
}
