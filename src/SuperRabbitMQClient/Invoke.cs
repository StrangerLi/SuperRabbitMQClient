using SuperRabbitMQClient.AssemblyHelper;
using SuperRabbitMQClient.Command;
using SuperRabbitMQClient.Command.ExampleClient1;
using SuperRabbitMQClient.RabbitMQClient;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SuperRabbitMQClient
{
    public class Invoke
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public Invoke()
        {

        }
        public void LoadClient()
        {
            ////置空当前命令集合
            //Commands = null;

            //设置集合用于存储程序集
            var commandAssemblies = new List<Assembly>();
            //向程序集中添加元素
            commandAssemblies.Add(GetType().Assembly);
            //定义返回接过
            var outputCommands = new ConcurrentDictionary<string, IRabbitMQClient>();
            //遍历程序集 集合中的元素，添加类的实例到命令中
            foreach (var assembly in commandAssemblies)
            {
                foreach (var item in assembly.GetImplementedObjectsByInterface<IRabbitMQClient>())
                {
                    if (!outputCommands.TryAdd(item.Name, item))
                    {
                        //记录日志，输出命令添加失败
                        outputCommands.Clear();
                        return;
                    }
                }
            }
            //Commands = outputCommands;
        }
    }
}
