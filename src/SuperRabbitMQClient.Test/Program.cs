using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperRabbitMQClient.Test
{
    public static class Program
    {
        public static void Main()
        {
            RabbitMQClientPoolBase invoked = RabbitMQClientPool.GetInstance();
            invoked.Start();
            Console.ReadLine();
        }
    }
}
