using SuperRabbitMQClient.Command;
using SuperRabbitMQClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SuperRabbitMQClient.Test.Command;
using SuperRabbitMQClient.RabbitMQClient;

namespace SuperRabbitMQClient.Test.RabbitMQClient
{
    public class ZJLY_VH_ESB : RabbitMQClientBase<AbstractCommand>
    {
        public override string Name => "ZJLY_VH_ESB";
        public override string IPAddress => base.IPAddress;
        public override string VirtualHost => base.VirtualHost;
        public override string UserName => base.UserName;
        public override string PassWd => base.PassWd;
    }
}
