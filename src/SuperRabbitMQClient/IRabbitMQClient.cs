using RabbitMQ.Client;
using System;
using System.Collections.Concurrent;
using System.Text;

namespace SuperRabbitMQClient
{
    public interface IRabbitMQClient: IDisposable
    {
        /// <summary>
        /// 客户端名称
        /// <para>该名称需在作用域内唯一</para>
        /// </summary>
        string Name { get; }

        #region 配置项
        /// <summary>
        /// 主机Ip地址
        /// </summary>
        string IPAddress { get; }
        /// <summary>
        /// 连接域
        /// </summary>
        string VirtualHost { get; }
        /// <summary>
        /// 连接用户名
        /// </summary>
        string UserName { get; }
        /// <summary>
        /// 连接密码
        /// </summary>
        string PassWd { get; }
        /// <summary>
        /// 连接端口号
        /// </summary>
        int Port { get; }
        /// <summary>
        /// 是否自动重连
        /// </summary>
        bool AutomaticRecoveryEnable { get; }

        #region 废弃的代码
        ///// <summary>
        ///// 设置主机IP地址
        ///// </summary>
        ///// <param name="IPaddress"></param>
        ///// <returns></returns>
        //bool SetIPaddress(string IPaddress);
        ///// <summary>
        ///// 设置连接域
        ///// </summary>
        ///// <param name="VirtualHost"></param>
        ///// <returns></returns>
        //bool SetVirtualHost(string VirtualHost);
        ///// <summary>
        ///// 设置连接用户名
        ///// </summary>
        ///// <param name="UserName"></param>
        ///// <returns></returns>
        //bool SetUserName(string UserName);
        ///// <summary>
        ///// 设置连接密码
        ///// </summary>
        ///// <param name="PassWd"></param>
        ///// <returns></returns>
        //bool SetPassWd(string PassWd);
        ///// <summary>
        ///// 设置端口号
        ///// </summary>
        ///// <param name="Port"></param>
        ///// <returns></returns>
        //bool SetPort(int Port);
        ///// <summary>
        ///// 设置自动重连
        ///// </summary>
        ///// <param name="AutomaticRecoveryEnable"></param>
        ///// <returns></returns>
        //bool SetAutomaticRecoveryEnable(bool AutomaticRecoveryEnable);
        #endregion

        #endregion

        #region RabbitMQ状态
        /// <summary>
        /// 是否已经完成初始化
        /// </summary>
        bool IsInit { get; }
        /// <summary>
        /// 连接是否开启
        /// </summary>
        bool IsOpen { get; }
        /// <summary>
        /// 监听是否开启
        /// </summary>
        bool IsListening { get; }
        #endregion


        #region RabbitMQ对象
        /// <summary>
        /// 连接工厂
        /// </summary>
        IConnectionFactory ConnectionFactory { get; }
        /// <summary>
        /// RabbitMQ连接
        /// </summary>
        IConnection Connection { get; }
        /// <summary>
        /// 监听信道
        /// </summary>
        ConcurrentDictionary<string, IModel> Models { get; }
        /// <summary>
        /// 命令成员实例集合
        /// </summary>
        ConcurrentDictionary<string, IRabbitMQCommand> Commands { get; }
        #endregion

        /// <summary>
        /// 打开RabbitMQClient连接
        /// </summary>
        /// <returns></returns>
        void Open();
        /// <summary>
        /// 关闭RabbitMQClient连接
        /// </summary>
        void Close();

        /// <summary>
        /// 开启监听队列
        /// </summary>
        /// <returns></returns>
        void StartQueueListen();
        /// <summary>
        /// 关闭队列监听
        /// </summary>
        /// <returns></returns>
        void CloseQueueListen();

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="ExchangeName"></param>
        /// <param name="RouteKey"></param>
        /// <param name="model"></param>
        void SendMessage(string ExchangeName, string RouteKey, byte[] model);
        /// <summary>
        /// 发送发消息
        /// </summary>
        /// <param name="ExchangeName"></param>
        /// <param name="RouteKey"></param>
        /// <param name="model"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        void SendMessage(string ExchangeName, string RouteKey, byte[] model, int offset, int count);
        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="ExchangeName"></param>
        /// <param name="RouteKey"></param>
        /// <param name="Message"></param>
        void SendMessage(string ExchangeName, string RouteKey, string Message);
        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="ExchangeName"></param>
        /// <param name="RouteKey"></param>
        /// <param name="Message"></param>
        /// <param name="encoding"></param>
        void SendMessage(string ExchangeName, string RouteKey, string Message, Encoding encoding);
    }
}