using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperRabbitMQClient.Test
{
   public class RabbitMQClientPool: RabbitMQClientPoolBase
    {
        #region 定义此类为单例模式(双重锁检查)

        /// <summary>
        /// 定义实例化线程锁
        /// </summary>
        private static readonly object _lock = new object();

        /// <summary>
        /// 本类的静态实例
        /// </summary>
        private volatile static RabbitMQClientPool Instance = null;

        /// <summary>
        /// 构造函数
        /// </summary>
        private RabbitMQClientPool()
        {
        }

        /// <summary>
        /// 获取类的实例
        /// 启用双重锁检查
        /// 避免冗余调用
        /// </summary>
        /// <returns>类的实例</returns>
        public static RabbitMQClientPool GetInstance()
        {
            if (Instance == null)
            {
                lock (_lock)
                {
                    if (Instance == null)
                    {
                        Instance = new RabbitMQClientPool();
                    }
                }
            }
            return Instance;
        }
        #endregion
    }
}
