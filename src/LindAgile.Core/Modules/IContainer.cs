using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LindAgile.Core.Modules
{
    /// <summary>
    /// 容器里的行为
    /// </summary>
    public interface IContainer
    {
        /// <summary>
        /// 注册类型，泛型
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImplementer"></typeparam>
        /// <param name="serviceName"></param>
        /// <param name="life"></param>
        void Register<TService, TImplementer>(string serviceName = null, LifeCycle life = LifeCycle.Singleton)
            where TService : class
            where TImplementer : class, TService;

        /// <summary>
        /// 注册对象,添加别名
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImplementer"></typeparam>
        /// <param name="serviceName"></param>
        /// <param name="service"></param>
        /// <param name="implement"></param>
        /// <param name="life"></param>
        void Register(Type service, Type implement, string serviceName = null, LifeCycle life = LifeCycle.Singleton);

        /// <summary>
        /// 注册一个泛型对象
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImplementer"></typeparam>
        /// <param name="instance"></param>
        /// <param name="serviceName"></param>
        void RegisterGeneric(Type service, Type implement, LifeCycle life = LifeCycle.Singleton);

        /// <summary>
        /// 从容器中取了对象
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <returns></returns>
        TService Resolve<TService>() where TService : class;

        /// <summary>
        /// 从容器中取了对象
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        object Resolve(Type service);

        /// <summary>
        /// 从容器中取了对象
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        object Resolve(Type service, params object[] param);

        /// <summary>
        /// 从容器中取了对象,别名方式
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        TService ResolveNamed<TService>(string serviceName) where TService : class;

        /// <summary>
        /// 从容器中取了对象
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <returns></returns>
        TService Resolve<TService>(params object[] param) where TService : class;

        /// <summary>
        /// 从容器中取了对象,别名方式
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        TService ResolveNamed<TService>(string serviceName, params object[] param) where TService : class;


    }
}
