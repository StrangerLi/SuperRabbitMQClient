using Newtonsoft.Json;
using System;
using System.Text;
using ZDZN.DataPlatform.Common.LogHelper;
using ZDZN.DataPlatform.WMS.Adapter.Model.MCS;
using ZDZN.DataPlatform.WMS.Adapter.RabbitMQClinet.Receiver;
using ZDZN.DataPlatform.WMS.RabbitMQClient;

namespace SmartCar.WMS.Server.RabbitMQ.Command
{
    /// <summary>
    /// ZZSM_WH1_WMS_CMD
    /// </summary>
    public class ZZSM_WH1_WMS_CMD : RabbitMQCommandBase
    {
        /// <summary>
        /// 命令名：默认为当前类名
        /// </summary>
        public override string Name
        {
            get
            {
                return "ZZSM.WH1.WMS.CMD";
            }
        }
        /// <summary>
        /// 获取当前类的完全限定名
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return base.ToString();
        }

        /// <summary>
        /// 队列消息接收事件
        /// </summary>
        /// <param name="Parameter"></param>
        /// <returns></returns>
        public override bool Event_MessageReceive(byte[] Parameter)
        {
            try
            {
                //反序列化结构体
                var ModelBase = JsonConvert.DeserializeObject<BaseMsg>(Encoding.UTF8.GetString(Parameter));

                //拼接Receiver_CommandName
                var Receiver_CommandName = ModelBase != null ? ModelBase.DA_Verb ?? "" + ModelBase.DA_Noun ?? "" : "";
                if (string.IsNullOrEmpty(Receiver_CommandName))
                {
                    var Result = ReceiverInvoker.GetInstance().Invoke(this, Receiver_CommandName, Parameter);
                    RabbitMQInvoker.GetInstance().SendMessage("ExchangeName", "RouteKey", Encoding.UTF8.GetBytes(Result));
                }
                else
                {
                    //记录日志，未能成功解析请求内容
                }
            }
            catch (Exception ex)
            {
                LogFactory.GetLogger().LogMessage("RabbitMQ", LogLevel.Debug & LogLevel.Error, ex);
            }
            return true;
        }
    }
}
