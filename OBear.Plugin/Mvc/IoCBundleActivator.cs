// -----------------------------------------------------------------------
//  <copyright file="IoCBundleActivator.cs" company="OBear开源团队">
//      Copyright (c) 2014 OBear. All rights reserved.
//  </copyright>
//  <last-editor>郭明锋</last-editor>
//  <last-date>2014-07-16 18:18</last-date>
// -----------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using UIShell.OSGi;
using UIShell.OSGi.Configuration.BundleManifest;
using UIShell.OSGi.Core.Service;
using UIShell.OSGi.Loader;


namespace OBear.Plugin.Mvc
{
    /// <summary>
    /// IoC插件激活器基类
    /// </summary>
    public abstract class IoCBundleActivator : IBundleActivator
    {
        private readonly ConcurrentDictionary<long, Assembly[]> _registerHostory;

        protected IoCBundleActivator()
        {
            _registerHostory = new ConcurrentDictionary<long, Assembly[]>();
        }

        /// <summary>
        /// 获取 IoC插件最终整合并返回的IoC实例创建者
        /// </summary>
        public static IServiceResolver ServiceResolver { get; private set; }

        /// <summary>
        /// 设置 宿主程序的程序集
        /// </summary>
        public static Func<Assembly[]> HostingAssemblyFunc { private get; set; }

        public void Start(IBundleContext context)
        {
            BundleRuntime runtime = BundleRuntime.Instance;
            context.BundleStateChanged += context_BundleStateChanged;

            //注册宿主程序
            if (HostingAssemblyFunc != null)
            {
                Assembly[] hostingAssemblies = HostingAssemblyFunc();
                RegisterAssemblies(hostingAssemblies);
            }

            //注册激活的插件的依赖项
            foreach (IBundle bundle in context.Framework.Bundles)
            {
                if (bundle.State == BundleState.Active)
                {
                    RegisterBundle(bundle);
                }
            }

            if (runtime.State == BundleRuntimeState.Starting)
            {
                context.FrameworkStateChanged += context_FrameworkStateChanged;
            }
            else
            {
                Complate();
            }
        }

        private void RegisterBundle(IBundle bundle)
        {
            IRuntimeService service = BundleRuntime.Instance.GetFirstOrDefaultService<IRuntimeService>();
            Assembly[] assemblies = service.LoadBundleAssembly(bundle.SymbolicName).ToArray();
            if (assemblies.Length == 0)
            {
                return;
            }
            RegisterAssemblies(assemblies);
            _registerHostory[bundle.BundleID] = assemblies;
        }

        private void RegisterAssemblies(Assembly[] assemblies)
        {
            if (assemblies == null || assemblies.Length  == 0)
            {
                return;
            }
            RegisterController(assemblies);
            RegisterDependencies(assemblies);
        }

        private void Complate()
        {
            IServiceResolver resolver = ComplateAndGetControllerResolver();
            ServiceResolver = resolver;
        }

        /// <summary>
        /// 重写以实现注册程序集中的MVC控制器类型
        /// </summary>
        /// <param name="assemblies">程序集</param>
        protected abstract void RegisterController(Assembly[] assemblies);

        /// <summary>
        /// 重写以实现注册程序集中的<see cref="OBear.Core.IDependency"/>接口的实现类型及其他映射类型
        /// </summary>
        /// <param name="assemblies">程序集</param>
        protected abstract void RegisterDependencies(Assembly[] assemblies);

        /// <summary>
        /// 重写以完成IoC窗口的组装工作，并创建<see cref="IServiceResolver"/>类型实例
        /// </summary>
        /// <returns></returns>
        protected abstract IServiceResolver ComplateAndGetControllerResolver();

        /// <summary>
        /// 重写以完成插件程序集的卸载工作
        /// </summary>
        protected abstract IServiceResolver UnRegisterBundleAndGetControllerResolver(Assembly[] assemblies);

        private void context_BundleStateChanged(object sender, BundleStateChangedEventArgs e)
        {
            BundleRuntime runtime = BundleRuntime.Instance;
            BundleData bundleData = runtime.GetFirstOrDefaultService<IBundleInstallerService>()
                .GetBundleDataByName(e.Bundle.SymbolicName);
            if (bundleData == null)
            {
                return;
            }

            bool needLoad = e.CurrentState == BundleState.Active;
            if (needLoad)
            {
                RegisterBundle(e.Bundle);
            }
            else if (e.CurrentState == BundleState.Stopping)
            {
                //如果插件正在停止，就不需要更新ContainerBuilder了，因为这个服务即将不可用。
                if (BundleRuntime.Instance.State == BundleRuntimeState.Stopping)
                {
                    return;
                }
                Assembly[] assemblies;
                if (_registerHostory.TryGetValue(e.Bundle.BundleID, out assemblies))
                {
                    IServiceResolver newResolver = UnRegisterBundleAndGetControllerResolver(assemblies);
                    ServiceResolver = newResolver;
                }
            }
        }

        private void context_FrameworkStateChanged(object sender, FrameworkEventArgs e)
        {
            if (e.EventType == FrameworkEventType.Started)
            {
                Complate();
            }
        }

        public void Stop(IBundleContext context)
        {
            context.BundleStateChanged -= context_BundleStateChanged;
        }
    }
}