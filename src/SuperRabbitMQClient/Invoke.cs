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
        #region 定义此类为单例模式(双重锁检查)

        /// <summary>
        /// 定义实例化线程锁
        /// </summary>
        private static readonly object _lock = new object();

        /// <summary>
        /// 本类的静态实例
        /// </summary>
        private volatile static Invoke Instance = null;

        /// <summary>
        /// 构造函数
        /// </summary>
        private Invoke()
        {
        }

        /// <summary>
        /// 获取类的实例
        /// 启用双重锁检查
        /// 避免冗余调用
        /// </summary>
        /// <returns>类的实例</returns>
        public static Invoke GetInstance()
        {
            if (Instance == null)
            {
                lock (_lock)
                {
                    if (Instance == null)
                    {
                        Instance = new Invoke();
                    }
                }
            }
            return Instance;
        }
        #endregion
        public ConcurrentDictionary<string, IRabbitMQClient> Clients { get; } = new ConcurrentDictionary<string, IRabbitMQClient>();

        /// <summary>
        /// 加载客户端
        /// </summary>
        private void LoadClient()
        {
            //复校验当前队列
            if (Clients.Count > 0)
            {
                Clients.Clear();

            }
            //设置集合用于存储程序集
            var commandAssemblies = new List<Assembly>();
            //向程序集中添加元素
            commandAssemblies.Add(GetType().Assembly);
            //遍历程序集 集合中的元素，添加类的实例到命令中
            foreach (var assembly in commandAssemblies)
            {
                foreach (var item in assembly.GetImplementedObjectsByInterface<IRabbitMQClient>())
                {
                    Clients.TryAdd(item.Name, item);
                }
            }
        }

        public bool Start()
        {
            LoadClient();

            if (Clients.Count > 0)
            {
                foreach (var item in Clients)
                {
                    item.Value.Open();
                    item.Value.StartQueueListen();
                }
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
