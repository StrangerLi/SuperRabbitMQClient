using SuperRabbitMQClient.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperRabbitMQClient.RabbitMQClient
{
    public class ExampleClient2 : RabbitMQClientBase<Command.ExampleClient2.AbstractCommand>
    {
        public override string IPAddress => base.IPAddress;
        public override string VirtualHost => base.VirtualHost;
        public override string UserName => base.UserName;
        public override string PassWd => base.PassWd;
    }
}
