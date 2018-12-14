using SuperRabbitMQClient.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperRabbitMQClient.RabbitMQClient
{
    public class ExampleClient1 : RabbitMQClientBase<Command.ExampleClient1.AbstractCommand>
    {
        public override string IPAddress => zz;
        public override string VirtualHost => base.VirtualHost;
        public override string UserName => base.UserName;
        public override string PassWd => base.PassWd;
    }
}
