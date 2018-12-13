using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace SuperRabbitMQClient
{
    /// <summary>
    /// RabbitMQ操作类
    /// </summary>
    public class RabbitMQInvoker
    {
        #region 定义此类为单例模式(双重锁检查)

        /// <summary>
        /// 定义实例化线程锁
        /// </summary>
        private static readonly object _lock = new object();

        /// <summary>
        /// 本类的静态实例
        /// </summary>
        private volatile static RabbitMQInvoker Instance = null;

        /// <summary>
        /// 构造函数
        /// </summary>
        private RabbitMQInvoker()
        {
            m_Channels = new ConcurrentDictionary<string, IModel>();
        }

        /// <summary>
        /// 获取类的实例
        /// 启用双重锁检查
        /// 避免冗余调用
        /// </summary>
        /// <returns>类的实例</returns>
        public static RabbitMQInvoker GetInstance()
        {
            if (Instance == null)
            {
                lock (_lock)
                {
                    if (Instance == null)
                    {
                        Instance = new RabbitMQInvoker();
                    }
                }
            }
            return Instance;
        }
        #endregion

        #region 本类全局静态变量


        #endregion

        #region 本类全局变量

        /// <summary>
        /// RabbitMQ连接配置文件
        /// </summary>
        private IConnectionFactory myConnectionFactory = null;

        /// <summary>
        /// 连接对象
        /// </summary>
        private IConnection myConnection = null;

        /// <summary>
        /// 监听信道
        /// </summary>
        private ConcurrentDictionary<string, IModel> m_Channels;

        /// <summary>
        /// 命令成员实例集合
        /// </summary>
        private ConcurrentDictionary<string, IRabbitMQCommand> m_Commands;

        #endregion

        #region 初始化方法

        /// <summary>
        /// 定义初始化线程锁
        /// </summary>
        private static readonly object Init_lock = new object();

        /// <summary>
        /// 标记类是否完成初始化
        /// </summary>
        private bool IsInit = false;

        /// <summary>
        /// 初始化方法
        /// </summary>
        /// <param name="ConnectionFactory">链接配置文件</param>
        /// <returns></returns>
        public bool Init(IConnectionFactory ConnectionFactory)
        {
            if (!IsInit)
            {
                lock (Init_lock)
                {
                    if (!IsInit)
                    {
                        try
                        {

                            //加载命令列表
                            if (!LoadCommand())
                            {
                                m_Commands = null;
                                IsInit = false;
                                return IsInit;
                            }

                            //开启链接
                            var OutConnnection = ConnectionFactory.CreateConnection();

                            //判断链接是否开启
                            if (!OutConnnection.IsOpen)
                            {
                                IsInit = false;
                                return IsInit;
                            }

                            //修改引用，将其标记为全局元素，防止GC回收
                            this.myConnectionFactory = ConnectionFactory;
                            this.myConnection = OutConnnection;

                        }
                        catch (Exception ex)
                        {
                            IsInit = false;
                            LogFactory.GetLogger().LogMessage("RabbitMQ", LogLevel.Debug & LogLevel.Error, ex);
                        }
                    }
                }
            }
            return IsInit;
        }

        /// <summary>
        /// 初始化方法
        /// </summary>
        /// <param name="IPAddress">RabbitMQ服务地址</param>
        /// <param name="Port">RabbitMQ服务端口号</param>
        /// <param name="VirtualHost">RabbitMQ虚拟域</param>
        /// <param name="UserName">链接用户名</param>
        /// <param name="PassWd">链接密码</param>
        /// <param name="AutomaticRecoveryEnabled">是否自动重连，默认为True</param>
        /// <returns></returns>
        public bool Init(string IPAddress, string VirtualHost, string UserName, string PassWd, int Port=5672, bool AutomaticRecoveryEnabled = true)
        {
            if (!IsInit)
            {
                lock (Init_lock)
                {
                    if (!IsInit)
                    {
                        try
                        {
                            //加载命令列表
                            if (!LoadCommand())
                            {
                                m_Commands = null;
                                IsInit = false;
                                return IsInit;
                            }
                            //初始化链接配置文件
                            var OutConnectionFactory = new ConnectionFactory()
                            {
                                VirtualHost = VirtualHost,
                                Port = Port,
                                HostName = IPAddress,
                                UserName = UserName,
                                Password = PassWd,
                                AutomaticRecoveryEnabled = AutomaticRecoveryEnabled
                            };
                            //开启链接
                            var OutConnnection = OutConnectionFactory.CreateConnection();

                            //判断链接是否开启
                            if (!OutConnnection.IsOpen)
                            {
                                IsInit = false;
                                return IsInit;
                            }

                            //修改引用，将其标记为全局元素，防止GC回收
                            this.myConnectionFactory = OutConnectionFactory;
                            this.myConnection = OutConnnection;
                            IsInit = true;
                        }
                        catch (Exception ex)
                        {
                            LogFactory.GetLogger().LogMessage("RabbitMQ", LogLevel.Debug & LogLevel.Error, ex);
                        }
                    }
                }
            }
            return IsInit;
        }

        /// <summary>
        /// 加载命令
        /// </summary>
        /// <returns></returns>
        private bool LoadCommand()
        {
            //清空当前命令队列
            m_Commands = null;

            //设置集合用于存储程序集
            var commandAssemblies = new List<Assembly>();
            //向程序集中添加元素
            commandAssemblies.Add(GetType().Assembly);
            //定义返回接过
            var outputCommands = new ConcurrentDictionary<string, IRabbitMQCommand>();
            //遍历程序集 集合中的元素，添加类的实例到命令中
            foreach (var assembly in commandAssemblies)
            {
                try
                {
                    foreach (var item in assembly.GetImplementedObjectsByInterface<RabbitMQCommandBase>())
                    {
                        if (!outputCommands.TryAdd(item.Name, item))
                        {
                            //记录日志，输出命令添加失败
                            m_Commands.Clear();
                            return false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogFactory.GetLogger().LogMessage("RabbitMQ", LogLevel.Debug & LogLevel.Error, ex);
                    return false;
                }
            }
            m_Commands = outputCommands;
            return true;
        }
        #endregion

        #region RabbitMQ监听队列
        /// <summary>
        /// 开启消息队列监听，重复调用此方法，会导致历史队列监听线程被释放
        /// </summary>
        /// <returns></returns>
        public bool StartQueueListen()
        {
            try
            {
                //判断系统是否已经完成初始化
                if (!IsInit)
                {
                    return false;
                }

                //判断传入要监听的队列是否为空
                if (m_Commands == null || m_Commands.Keys.Count == 0)
                {
                    return false;
                }

                //判断链接是否已经建立(打开)
                if (myConnection == null || !myConnection.IsOpen)
                {
                    return false;
                }

                //清理当前监听信道
                foreach (var item in this.m_Channels.Values)
                {
                    //关闭信道
                    item.Close();
                    //清理信道
                    item.Dispose();
                }

                var m_Channels = new ConcurrentDictionary<string, IModel>();
                foreach (KeyValuePair<string, IRabbitMQCommand> Command in m_Commands)
                {
                    if (m_Channels.ContainsKey(Command.Key))
                    {
                        continue;
                    }

                    //创建信道监听队列
                    var Temp_Channel = myConnection.CreateModel();
                    //设置该信道的所有消费者，每次只消费一条信息，在没有ACK回应前，不接受下一条信息
                    Temp_Channel.BasicQos(Command.Value.PrefetchSize, Command.Value.PrefetchCount, false);
                    //声明消费者事件
                    var consumer = new EventingBasicConsumer(Temp_Channel);
                    //绑定信息接收事件
                    //如果将noack设置为false，消费时一定记得发送ack如：model.BasicAck(e.DeliveryTag, false);
                    Temp_Channel.BasicConsume(Command.Key, Command.Value.AutoAck, consumer);
                    //绑定接收消息事件
                    consumer.Received += (sender, e) => Command.Value.Queue_Received(sender, Temp_Channel, e);
                    //将信道添加到字典
                    m_Channels.TryAdd(Command.Key, Temp_Channel);
                }

                this.m_Channels = m_Channels;
                return true;
            }
            catch (Exception ex)
            {
                LogFactory.GetLogger().LogMessage("RabbitMQ", LogLevel.Debug & LogLevel.Error, ex);
                return false;
            }
        }

        #endregion

        #region RabbitMQ发送队列
        /// <summary>
        /// RabbitMQ消息发送
        /// </summary>
        public void SendMessage(string ExchangeName, string RouteKey, byte[] model)
        {
            using (var Channel = this.myConnection.CreateModel())
            {
                Channel.BasicPublish(ExchangeName, RouteKey, null, model);
            }
        }
        /// <summary>
        /// RabbitMQ消息发送
        /// </summary>
        public void SendMessage<TModel>(string ExchangeName, string RouteKey, TModel model) where TModel : ModelBase
        {
            SendMessage(ExchangeName, RouteKey, model.Serialize2JsonByte());
        }
        #endregion
    }
}