using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

using Autofac;
using Autofac.Core;
using Autofac.Integration.Mvc;

using OBear.Core;
using OBear.Core.Data;
//using OSharp.Core.Data.Entity;
using OBear.Plugin.Mvc;

using UIShell.OSGi;

namespace OBear.Plugin.IoC.Autofac
{
    public class BundleActivator : IoCBundleActivator, IBundleActivator
    {
        private readonly ContainerBuilder _builder;

        public BundleActivator()
        {
            _builder = new ContainerBuilder();
            //_builder.RegisterGeneric(typeof(Repository<,>)).As(typeof(IRepository<,>));
        }

        protected override void RegisterController(Assembly[] assemblies)
        {
            if (assemblies == null || assemblies.Length == 0)
            {
                return;
            }
            lock (_builder)
            {
                IContainer container = BundleRuntime.Instance.GetFirstOrDefaultService<IContainer>();
                if (container == null)
                {
                    _builder.RegisterControllers(assemblies);
                }
                else
                {
                    ContainerBuilder anotherBuilder = new ContainerBuilder();
                    anotherBuilder.RegisterControllers(assemblies);
                    anotherBuilder.Update(container);
                }
            }
        }

        protected override void RegisterDependencies(Assembly[] assemblies)
        {
            if (assemblies == null || assemblies.Length == 0)
            {
                return;
            }
            lock (_builder)
            {
                IContainer container = BundleRuntime.Instance.GetFirstOrDefaultService<IContainer>();
                if (container == null)
                {
                    RegisterDependencies(_builder, assemblies);
                }
                else
                {
                    ContainerBuilder anotherBuilder = new ContainerBuilder();
                    RegisterDependencies(anotherBuilder, assemblies);
                    anotherBuilder.Update(container);
                }
            }
        }

        protected override IServiceResolver ComplateAndGetControllerResolver()
        {
            BundleRuntime runtime = BundleRuntime.Instance;
            IContainer container = runtime.GetFirstOrDefaultService<IContainer>();
            if (container == null)
            {
                lock (_builder)
                {
                    container = _builder.Build();
                    runtime.AddService<IContainer>(container);
                }
            }
            runtime.AddService<IContainer>(container);
            //注入创建者
            IServiceResolver resolver = new ServiceResolver(container);
            return resolver;
        }

        protected override IServiceResolver UnRegisterBundleAndGetControllerResolver(Assembly[] assemblies)
        {
            if (assemblies == null || assemblies.Length == 0)
            {
                return null;
            }

            IContainer container = BundleRuntime.Instance.GetFirstOrDefaultService<IContainer>();
            if (container == null)
            {
                return null;
            }
            ContainerBuilder newBuilder = new ContainerBuilder();
            IEnumerable<IComponentRegistration> components = container.ComponentRegistry.Registrations
                .Where(cr => !assemblies.Contains(cr.Activator.LimitType.Assembly));
            foreach (IComponentRegistration component in components)
            {
                newBuilder.RegisterComponent(component);
            }

            foreach (IRegistrationSource source in container.ComponentRegistry.Sources)
            {
                newBuilder.RegisterSource(source);
            }
            IContainer newContainer = newBuilder.Build();
            BundleRuntime.Instance.RemoveService<IContainer>(container);
            BundleRuntime.Instance.AddService<IContainer>(newContainer);
            return new ServiceResolver(newContainer);
        }


        private static void RegisterDependencies(ContainerBuilder builder, Assembly[] assemblies)
        {
            if (assemblies == null || assemblies.Length == 0)
            {
                return;
            }
            Type baseType = typeof(IDependency);
            builder.RegisterAssemblyTypes(assemblies).Where(type => baseType.IsAssignableFrom(type) && !type.IsAbstract)
                .AsImplementedInterfaces().AsSelf();
        }
    }
}
