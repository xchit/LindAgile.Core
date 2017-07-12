using Autofac;
using LindAgile.Core.Logger;
using LindAgile.Core.Utils;
using System;
using System.Linq;
using System.Reflection;

namespace LindAgile.Core.LindPlugins
{
    /// <summary>
    /// 可插拔组件的管理者
    /// Author:Lind
    /// 依赖于Autofac
    /// </summary>
    public class PluginManager
    {
        private static ILogger logger = new LindLogger();
        /// <summary>
        /// 插件容器辅助字段
        /// </summary>
        private static IContainer _container = null;
        /// <summary>
        /// 互斥锁
        /// </summary>
        private static object lockObj = new object();
        /// <summary>
        ///初始化应用程序的插件
        /// </summary>
        public static void Init()
        {

            lock (lockObj)
            {
                if (_container == null)
                {
                    lock (lockObj)
                    {
                        try
                        {
                            var builder = new ContainerBuilder();
                            //装载的插件都是公共类型
                            var typeList = AssemblyHelper.GetTypesByInterfaces(typeof(IPlugins));
                            foreach (var item in typeList)
                            {
                                TypeInfo typeInfo = IntrospectionExtensions.GetTypeInfo(item);
                                foreach (var sub in typeInfo.GetInterfaces())
                                {
                                    builder.RegisterType(item).Named(item.FullName, sub);

                                    logger.Logger_Info(
                                        item.FullName.PadRight(50, '-') + sub.FullName);
                                }
                            }
                            _container = builder.Build();
                        }
                        catch (Exception ex)
                        {
                            throw new ArgumentException("PluginManager依赖于autofac包包..." + ex.Message);
                        }

                    }
                }
            }

        }

        /// <summary>
        /// 从容器中取出某个类型，遍历执行某个动作
        /// 功能：可以快速1个接口下所有实现遍历出来，进行回调操作
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action"></param>
        public static void ForAction<T>(Action<T> action)
        {
            var dll = Utils.AssemblyHelper.GetTypesByInterfaces(typeof(T));
            foreach (var item in dll)
            {
                var obj = PluginManager.Resolve<T>(item.FullName);
                //回调方法
                action(obj);
            }
        }
        /// <summary>
        /// 从插件容器里返回对象
        /// </summary>
        /// <param name="serviceName">对象全名</param>
        /// <param name="serviceType">接口类型</param>
        /// <returns></returns>
        public static object Resolve(string serviceName, Type serviceType)
        {
            var obj = _container.ResolveNamed(serviceName, serviceType);
            return obj;
        }
        /// <summary>
        /// 从插件容器里返回对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        public static TService Resolve<TService>(string serviceName)
        {
            return (TService)Resolve(serviceName, typeof(TService));
        }
        /// <summary>
        /// 从插件容器里返回对象,第一个实现它的类型
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <returns></returns>
        public static TService Resolve<TService>()
        {
            var types = AssemblyHelper.GetTypesByInterfaces(typeof(TService));
            return (TService)Resolve(types.FirstOrDefault().FullName, typeof(TService));
        }

    }
}
