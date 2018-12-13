using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SuperRabbitMQClient.AssemblyHelper
{
    /// <summary>
    /// 反射实现扩展类（根据接口/基类获取类的实例）
    /// </summary>
    public static class AssemblyHelper
    {
        /// <summary>
        /// 扩展方法，获取某程序集中继承与某接口/基类的所有类的实例
        /// </summary>
        /// <typeparam name="TInterface"></typeparam>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static IEnumerable<TInterface> GetImplementedObjectsByInterface<TInterface>(this Assembly assembly)
        {
            return GetImplementedObjectsByInterface<TInterface>(assembly, typeof(TInterface));
        }

        /// <summary>
        /// 获取某程序集中继承与某类型的所有类的实例
        /// </summary>
        /// <typeparam name="TInterface"></typeparam>
        /// <param name="assembly"></param>
        /// <param name="targetType"></param>
        /// <returns></returns>
        public static IEnumerable<TInterface> GetImplementedObjectsByInterface<TInterface>(this Assembly assembly, Type targetType)
        {
            Type[] arrType = assembly.GetExportedTypes();

            var result = new List<TInterface>();

            for (int i = 0; i < arrType.Length; i++)
            {
                var currentImplementType = arrType[i];

                if (currentImplementType.IsAbstract)
                    continue;

                if (!targetType.IsAssignableFrom(currentImplementType))
                    continue;

                result.Add((TInterface)Activator.CreateInstance(currentImplementType));
            }

            return result;
        }
    }
}
