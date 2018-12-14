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

        /// <summary>
        /// 客户端列表
        /// </summary>
        public ConcurrentDictionary<string, IRabbitMQClient> Clients { get; } = new ConcurrentDictionary<string, IRabbitMQClient>();

        /// <summary>
        /// 加载客户端
        /// </summary>
        private void LoadClient()
        {
            //设置集合用于存储程序集
            var commandAssemblies = new List<Assembly>();
            //向程序集中添加元素
            commandAssemblies.Add(GetType().Assembly);
            //遍历程序集 集合中的元素，添加类的实例到命令中
            foreach (var assembly in commandAssemblies)
            {
                foreach (var item in assembly.GetImplementedObjectsByInterface<IRabbitMQClient>())
                {
                    if (Clients.ContainsKey(item.Name))
                    {
                        continue;
                    }
                    Clients.TryAdd(item.Name, item);
                }
            }
        }

        /// <summary>
        /// 开启连接
        /// <para>将自动加载当前程序集中，所有继承IRabbitMQClient的类。</para>
        /// <para>根据IRabbitMQClient.Name区分不同的实例，加载时，将跳过重复实例/para>
        /// </summary>
        public void Start()
        {
            LoadClient();

            if (Clients.Count > 0)
            {
                //打开Client链接
                foreach (var item in Clients)
                {
                    if (!item.Value.IsOpen)
                    {
                        item.Value.Open();
                    }
                }
                //开启队列监听
                foreach (var item in Clients)
                {
                    if (!item.Value.IsListening)
                    {
                        item.Value.StartQueueListen();
                    }
                }
            }
        }


        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="ExchangeName"></param>
        /// <param name="RouteKey"></param>
        /// <param name="model"></param>
        public virtual bool SendMessage(string ClientName, string ExchangeName, string RouteKey, byte[] bytes)
        {
            if (Clients.ContainsKey(ClientName))
            {
                return Clients[ClientName].SendMessage(ExchangeName, RouteKey, bytes);
            }
            return false;
        }
        /// <summary>
        /// 发送发消息
        /// </summary>
        /// <param name="ExchangeName"></param>
        /// <param name="RouteKey"></param>
        /// <param name="model"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        public virtual bool SendMessage(string ClientName, string ExchangeName, string RouteKey, byte[] bytes, int offset, int count)
        {
            if (Clients.ContainsKey(ClientName))
            {
                return Clients[ClientName].SendMessage(ExchangeName, RouteKey, bytes, offset, count);
            }
            return false;
        }
        /// <summary>
        /// 发送消息
        /// <para>对Message使用UTF-8编码格式进行编码</para>
        /// </summary>
        /// <param name="ExchangeName"></param>
        /// <param name="RouteKey"></param>
        /// <param name="Message"></param>
        public virtual bool SendMessage(string ClientName, string ExchangeName, string RouteKey, string Message)
        {
            if (Clients.ContainsKey(ClientName))
            {
                return Clients[ClientName].SendMessage(ExchangeName, RouteKey, Message);
            }
            return false;
        }
        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="ExchangeName"></param>
        /// <param name="RouteKey"></param>
        /// <param name="Message"></param>
        /// <param name="encoding"></param>
        public virtual bool SendMessage(string ClientName, string ExchangeName, string RouteKey, string Message, Encoding encoding)
        {
            if (Clients.ContainsKey(ClientName))
            {
                return Clients[ClientName].SendMessage(ExchangeName, RouteKey, Message, encoding);
            }
            return false;
        }
    }
}
