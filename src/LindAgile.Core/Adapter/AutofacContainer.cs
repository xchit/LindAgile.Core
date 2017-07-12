using Autofac;
using Autofac.Core;
using LindAgile.Core.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LindAgile.Core.Adapter
{
    /// <summary>
    /// autofac的失陪器
    /// </summary>
    public class AutofacContainer : Modules.IContainer
    {
        private readonly Autofac.IContainer _container;

        /// <summary>
        /// 新容器
        /// </summary>
        public AutofacContainer()
        {
            _container = new ContainerBuilder().Build();
        }
        /// <summary>
        /// 已有容器
        /// </summary>
        /// <param name="containerBuilder"></param>
        public AutofacContainer(ContainerBuilder containerBuilder)
        {
            _container = containerBuilder.Build();
        }

        /// <summary>Represents the inner autofac container.
        /// </summary>
        public Autofac.IContainer Container
        {
            get
            {
                return _container;
            }
        }

        public void Register<TService, TImplementer>(string serviceName = null, LifeCycle life = LifeCycle.Singleton)
            where TService : class
            where TImplementer : class, TService
        {
            var builder = new ContainerBuilder();
            var registrationBuilder = builder.RegisterType<TImplementer>().As<TService>();
            if (serviceName != null)
            {
                registrationBuilder.Named<TService>(serviceName);
            }
            if (life == LifeCycle.Singleton)
            {
                registrationBuilder.SingleInstance();
            }
            builder.Update(_container);
        }



        public void RegisterGeneric(Type service, Type implement, LifeCycle life = LifeCycle.Singleton)

        {
            var builder = new ContainerBuilder();
            var registrationBuilder = builder.RegisterGeneric(implement).As(service);

            if (life == LifeCycle.Singleton)
            {
                registrationBuilder.SingleInstance();
            }
            builder.Update(_container);
        }

        public TService Resolve<TService>() where TService : class
        {
            return _container.Resolve<TService>();
        }

        public TService ResolveNamed<TService>(string serviceName) where TService : class
        {
            return _container.ResolveNamed<TService>(serviceName);
        }
        public TService Resolve<TService>(params object[] param) where TService : class
        {
            return _container.Resolve<TService>(Array.ConvertAll(param, i => new TypedParameter(i.GetType(), i)));
        }

        public TService ResolveNamed<TService>(string serviceName, params object[] param) where TService : class
        {
            return _container.ResolveNamed<TService>(serviceName, Array.ConvertAll(param, i => new TypedParameter(i.GetType(), i)));
        }
        public object Resolve(Type service)
        {
            return _container.Resolve(service);
        }
        public object Resolve(Type service, params object[] param)
        {
            return _container.Resolve(service, Array.ConvertAll(param, i => new TypedParameter(i.GetType(), i)));
        }

        public void Register(Type service, Type implement, string serviceName = null, LifeCycle life = LifeCycle.Singleton)
        {
            var builder = new ContainerBuilder();
            var registrationBuilder = builder.RegisterType(implement).As(service).Named(serviceName, service);

            if (life == LifeCycle.Singleton)
            {
                registrationBuilder.SingleInstance();
            }
            builder.Update(_container);


        }
    }

    public class Array
    {
        public static TOutput[] ConvertAll<TInput, TOutput>(TInput[] array, Func<TInput, TOutput> converter)
        {
            List<TOutput> outs = new List<TOutput>();
            foreach (var item in array)
            {
                outs.Add(converter(item));
            }
            return outs.ToArray();
        }
    }
}
