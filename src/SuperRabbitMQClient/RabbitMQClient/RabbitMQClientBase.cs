using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SuperRabbitMQClient.AssemblyHelper;
using SuperRabbitMQClient.Command;

namespace SuperRabbitMQClient.RabbitMQClient
{
    public abstract  class RabbitMQClientBase<TRabbitMQCommand> : IRabbitMQClient
          where TRabbitMQCommand : IRabbitMQCommand
    {
        public RabbitMQClientBase()
        {
            if (!IsInit)
            {
                this.ConnectionFactory = new ConnectionFactory()
                {
                    HostName = IPAddress,
                    Port = Port,
                    VirtualHost = VirtualHost,
                    UserName = UserName,
                    Password = PassWd,
                    AutomaticRecoveryEnabled = AutomaticRecoveryEnable
                };
            }
        }
        /// <summary>
        /// 客户端名称
        /// <para>该名称需在作用域内唯一</para>
        /// </summary>
        public virtual string Name
        {
            get
            {
                return this.GetType().Name;
            }
        }

        #region 配置项
        /// <summary>
        /// 主机Ip地址
        /// </summary>
        public virtual string IPAddress { get; } = "localhost";
        /// <summary>
        /// 连接端口号
        /// </summary>
        public virtual int Port { get; } = 5672;
        /// <summary>
        /// 连接域
        /// </summary>
        public virtual string VirtualHost { get; } = "/";
        /// <summary>
        /// 连接用户名
        /// </summary>
        public virtual string UserName { get; } = "guest";
        /// <summary>
        /// 连接密码
        /// </summary>
        public virtual string PassWd { get; } = "guest";
        /// <summary>
        /// 是否自动重连
        /// </summary>
        public virtual bool AutomaticRecoveryEnable { get; } = true;

        #region 废弃的代码
        ///// <summary>
        ///// 设置主机IP地址
        ///// </summary>
        ///// <param name="IPAddress"></param>
        ///// <returns></returns>
        //public virtual bool SetIPaddress(string IPAddress)
        //{
        //    if (string.IsNullOrEmpty(IPAddress))
        //    {
        //        this.IPAddress = IPAddress;
        //        return true;
        //    }
        //    return false;
        //}
        ///// <summary>
        ///// 设置连接域
        ///// </summary>
        ///// <param name="VirtualHost"></param>
        ///// <returns></returns>
        //public virtual bool SetVirtualHost(string VirtualHost)
        //{
        //    if (string.IsNullOrEmpty(VirtualHost))
        //    {
        //        this.VirtualHost = VirtualHost;
        //        return true;
        //    }
        //    return false;
        //}
        ///// <summary>
        ///// 设置连接用户名
        ///// </summary>
        ///// <param name="UserName"></param>
        ///// <returns></returns>
        //public virtual bool SetUserName(string UserName)
        //{
        //    if (string.IsNullOrEmpty(UserName))
        //    {
        //        this.UserName = UserName;
        //        return true;
        //    }
        //    return false;
        //}
        ///// <summary>
        ///// 设置连接密码
        ///// </summary>
        ///// <param name="PassWd"></param>
        ///// <returns></returns>
        //public virtual bool SetPassWd(string PassWd)
        //{
        //    if (string.IsNullOrEmpty(PassWd))
        //    {
        //        this.PassWd = PassWd;
        //        return true;
        //    }
        //    return false;
        //}
        ///// <summary>
        ///// 设置端口号
        ///// </summary>
        ///// <param name="Port"></param>
        ///// <returns></returns>
        //public virtual bool SetPort(int Port)
        //{
        //    if (Port>0&& Port<65535)
        //    {
        //        this.Port = Port;
        //        return true;
        //    }
        //    return false;
        //}
        ///// <summary>
        ///// 设置自动重连
        ///// </summary>
        ///// <param name="AutomaticRecoveryEnable"></param>
        ///// <returns></returns>
        //public virtual bool SetAutomaticRecoveryEnable(bool AutomaticRecoveryEnable)
        //{
        //    this.AutomaticRecoveryEnable = AutomaticRecoveryEnable;
        //    return true;
        //}
        #endregion

        #endregion

        #region RabbitMQ状态
        /// <summary>
        /// 是否已经完成初始化
        /// </summary>
        public virtual bool IsInit
        {
            get
            {
                return ConnectionFactory != null ? true : false;
            }
        }
        /// <summary>
        /// 连接是否开启
        /// </summary>
        public virtual bool IsOpen
        {
            get
            {
                if (IsInit)
                {
                    if (Connection != null)
                    {
                        return Connection.IsOpen;
                    }
                }
                return false;
            }
        }
        /// <summary>
        /// 监听是否开启
        /// </summary>
        public virtual bool IsListening
        {
            get
            {
                if (IsOpen)
                {
                    if (Commands != null && Commands.Count > 0)
                    {
                        return true;
                    }
                }
                return false;
            }
        }
        #endregion


        #region RabbitMQ对象
        /// <summary>
        /// 连接工厂
        /// </summary>
        public IConnectionFactory ConnectionFactory { get; protected set; }
        /// <summary>
        /// RabbitMQ连接
        /// </summary>
        public IConnection Connection { get; protected set; }
        /// <summary>
        /// 监听信道
        /// </summary>
        public ConcurrentDictionary<string, IModel> Models { get; protected set; }
        /// <summary>
        /// 命令成员实例集合
        /// </summary>
        public ConcurrentDictionary<string, IRabbitMQCommand> Commands { get; protected set; }

        #endregion

        /// <summary>
        /// 打开RabbitMQClient连接
        /// </summary>
        /// <returns></returns>
        public virtual void Open()
        {
            if (IsInit)
            {
                if (!IsOpen)
                {
                    if (Connection != null)
                    {
                        Connection.Close();
                        Connection.Dispose();
                        Connection = null;
                    }
                    Connection = ConnectionFactory.CreateConnection();
                }
            }
        }
        /// <summary>
        /// 关闭RabbitMQClient连接
        /// </summary>
        public virtual void Close()
        {
            if (IsOpen)
            {
                if (IsListening)
                {
                    CloseQueueListen();
                }
                Connection.Close();
                Connection.Dispose();
                Connection = null;
            }
        }

        /// <summary>
        /// 开启监听队列
        /// </summary>
        /// <returns></returns>
        public virtual void StartQueueListen()
        {
            if (IsOpen)
            {
                if (!IsListening)
                {
                    //校验命令列表
                    if (Commands == null || Commands.Count == 0)
                    {
                        //尝试加载命令
                        LoadCommand();
                    }

                    //复校验信道，确保信道清空
                    if (Models != null && Models.Count > 0)
                    {
                        foreach (var item in Models)
                        {
                            if (!item.Value.IsClosed)
                            {
                                item.Value.Close();
                                item.Value.Dispose();
                            }
                        }
                        Models.Clear();
                        Models = null;
                    }

                    var m_Models = new ConcurrentDictionary<string, IModel>();
                    foreach (var Command in Commands)
                    {
                        if (m_Models.ContainsKey(Command.Key))
                        {
                            continue;
                        }

                        //创建信道监听队列
                        var Temp_Channel = Connection.CreateModel();
                        //设置当前信道的QOS
                        Temp_Channel.BasicQos(Command.Value.PrefetchSize, Command.Value.PrefetchCount, false);


                        //声明消费者
                        var consumer = new EventingBasicConsumer(Temp_Channel);
                        //绑定信息接收事件
                        consumer.Received += (sender, e) => Command.Value.Queue_Received(sender, Temp_Channel, e);

                        //将信道与消费者绑定
                        Temp_Channel.BasicConsume(Command.Key, Command.Value.AutoAck, consumer);

                        //将信道添加到字典
                        m_Models.TryAdd(Command.Key, Temp_Channel);
                    }
                    this.Models = m_Models;
                }
            }
        }
        /// <summary>
        /// 关闭队列监听
        /// </summary>
        /// <returns></returns>
        public virtual void CloseQueueListen()
        {
            if (IsListening)
            {
                //关闭所有信道
                foreach (var item in Models)
                {
                    if (!item.Value.IsClosed)
                    {
                        item.Value.Close();
                        item.Value.Dispose();
                    }
                }
                //清空信道集合
                Models.Clear();
                Models = null;
                //清空命令列表集合
                Commands.Clear();
                Commands = null;
            }
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="ExchangeName"></param>
        /// <param name="RouteKey"></param>
        /// <param name="model"></param>
        public virtual bool SendMessage(string ExchangeName, string RouteKey, byte[] bytes)
        {
            //当ExchangeName为Null或者Empty时，方法返回
            //ExchangeName不允许为空
            if (string.IsNullOrEmpty(ExchangeName))
            {
                return false;
            }
            //要发送的字节数组不允许为空
            if (bytes == null || bytes.Length == 0)
            {
                return false;
            }
            if (IsOpen)
            {
                using (var Temp_Model = Connection.CreateModel())
                {
                    Temp_Model.BasicPublish(ExchangeName, RouteKey, null, bytes);
                    return true;
                }
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
        public virtual bool SendMessage(string ExchangeName, string RouteKey, byte[] bytes, int offset, int count)
        {
            using (var temp_steam = new MemoryStream())
            {
                temp_steam.Write(bytes, offset, count);
                return SendMessage(ExchangeName, RouteKey, temp_steam.GetBuffer());
            }
        }
        /// <summary>
        /// 发送消息
        /// <para>对Message使用UTF-8编码格式进行编码</para>
        /// </summary>
        /// <param name="ExchangeName"></param>
        /// <param name="RouteKey"></param>
        /// <param name="Message"></param>
        public virtual bool SendMessage(string ExchangeName, string RouteKey, string Message)
        {
            return SendMessage(ExchangeName,RouteKey,Encoding.UTF8.GetBytes(Message));
        }
        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="ExchangeName"></param>
        /// <param name="RouteKey"></param>
        /// <param name="Message"></param>
        /// <param name="encoding"></param>
        public virtual bool SendMessage(string ExchangeName, string RouteKey, string Message, Encoding encoding)
        {
            if (encoding == null)
            {
                return false ;
            }
            return SendMessage(ExchangeName, RouteKey, encoding.GetBytes(Message));
        }
        /// <summary>
        /// 加载命令集合
        /// </summary>
        public void LoadCommand()
        {
            //置空当前命令集合
            Commands = null;

            //设置集合用于存储程序集
            var commandAssemblies = new List<Assembly>();
            //向程序集中添加元素
            commandAssemblies.Add(GetType().Assembly);
            //定义返回接过
            var outputCommands = new ConcurrentDictionary<string, IRabbitMQCommand>();
            //遍历程序集 集合中的元素，添加类的实例到命令中
            foreach (var assembly in commandAssemblies)
            {
                foreach (var item in assembly.GetImplementedObjectsByInterface<TRabbitMQCommand>())
                {
                    if (!outputCommands.TryAdd(item.Name, item))
                    {
                        //记录日志，输出命令添加失败
                        outputCommands.Clear();
                        return;
                    }
                }
            }
            Commands = outputCommands;
        }

        #region IDisposable Support
        private bool disposedValue = false; // 要检测冗余调用

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)。
                    this.Close();
                }

                // TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                // TODO: 将大型字段设置为 null。

                disposedValue = true;
            }
        }

        // TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
        ~RabbitMQClientBase()
        {
            // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
            Dispose(false);
        }

        // 添加此代码以正确实现可处置模式。
        public void Dispose()
        {
            // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
            Dispose(true);
            // TODO: 如果在以上内容中替代了终结器，则取消注释以下行。
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
