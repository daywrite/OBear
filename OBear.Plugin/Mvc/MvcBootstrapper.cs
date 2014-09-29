// -----------------------------------------------------------------------
//  <copyright file="AbstractBuilder.cs" company="OBear开源团队">
//      Copyright (c) OBear 2014. All rights reserved.
//  </copyright>
//  <last-editor>郭明锋</last-editor>
//  <last-date>2014:06:28 18:00</last-date>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Compilation;
using System.Web.Configuration;
using System.Web.Hosting;
using System.Web.Mvc;

using UIShell.OSGi;
using UIShell.OSGi.Configuration.BundleManifest;
using UIShell.OSGi.Core.Service;
using UIShell.OSGi.Loader;
using UIShell.OSGi.Logging;
using UIShell.OSGi.Utility;


namespace OBear.Plugin.Mvc
{
    public class MvcBootstrapper : IBundleRuntimeMvcHost
    {
        public const int StateTimeOut = 90;
        private const string WebConfigWebPath = "~/web.config";
        private const string RefreshHtmlWebPath = "~/refresh.html";
        private const string HostRestartWebPath = "~/bin/HostRestart";
        private static BundleRuntime _bundleRuntime;
        private static IList<Assembly> _topLevelReferencedAssemblies;
        private static readonly Dictionary<BundleData, ICollection<Assembly>> RegisteredBunldeCache = new Dictionary<BundleData, ICollection<Assembly>>();
        private static readonly ReaderWriterLock RefAssembliesLock = new ReaderWriterLock();
        private static readonly ReaderWriterLock CacheLock = new ReaderWriterLock();
        private static Timer _appStartingTimeout;

        private static string _webConfigPhysicalPath;
        private static string _refreshHtmlPhysicalPath;
        private static string _hostRestartPhysicalPath;

        private LogLevel LogLevel
        {
            get
            {
                string level = WebConfigurationManager.AppSettings["LogLevel"];
                if (!string.IsNullOrEmpty(level))
                {
                    try
                    {
                        object result = Enum.Parse(typeof(LogLevel), level);
                        if (result != null)
                        {
                            return (LogLevel)result;
                        }
                    }
                    catch
                    {
                        return LogLevel.Debug;
                    }
                }
                return LogLevel.Debug;
            }
        }

        private int MaxLogFileSize
        {
            get
            {
                string size = WebConfigurationManager.AppSettings["MaxLogFileSize"];
                if (!string.IsNullOrEmpty(size))
                {
                    try
                    {
                        return int.Parse(size);
                    }
                    catch
                    {
                        return 10;
                    }
                }

                return 10;
            }
        }

        private bool CreateNewLogFileOnMaxSize
        {
            get
            {
                string createNew = WebConfigurationManager.AppSettings["CreateNewLogFileOnMaxSize"];
                if (!string.IsNullOrEmpty(createNew))
                {
                    try
                    {
                        return bool.Parse(createNew);
                    }
                    catch
                    {
                        return false;
                    }
                }

                return false;
            }
        }

        /// <summary>
        /// Bundle运行时实例。
        /// </summary>
        public BundleRuntime BundleRuntime { get { return _bundleRuntime; } private set { _bundleRuntime = value; } }

        protected virtual BundleRuntime CreateBundleRuntime()
        {
            return new BundleRuntime();
        }

        public void StartBundleRuntime()
        {
            FileLogUtility.SetLogLevel(LogLevel);
            FileLogUtility.SetMaxFileSizeByMB(MaxLogFileSize);
            FileLogUtility.SetCreateNewFileOnMaxSize(CreateNewLogFileOnMaxSize);
            FileLogUtility.Debug("WebSite is starting.");
            PropertyInfo buildManagerProp = typeof(BuildManager).GetProperty("TheBuildManager",
                BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.GetProperty);
            if (buildManagerProp != null)
            {
                BuildManager buildManager = buildManagerProp.GetValue(null, null) as BuildManager;
                if (buildManager != null)
                {
                    PropertyInfo toplevelAssembliesProp = typeof(BuildManager).GetProperty("TopLevelReferencedAssemblies",
                        BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty);
                    if (toplevelAssembliesProp != null)
                    {
                        _topLevelReferencedAssemblies = toplevelAssembliesProp.GetValue(buildManager, null) as IList<Assembly>;
                    }
                }
            }
            if (_topLevelReferencedAssemblies == null)
            {
                throw new Exception("Retrieve top level referenced assembiles of BuildManager failed.");
            }

            AddPreDefinedRefAssemblies();
            // Set SQLCE compact before BundleRuntime starting.
            AppDomain.CurrentDomain.SetData("SQLServerCompactEditionUnderWebHosting", true);
            InitPhysicalPaths();

            BundleRuntime = CreateBundleRuntime();
            BundleRuntime.Instance.Framework.EventManager.AddBundleEventListener(BundleEventListener, true);
            FileLogUtility.Debug("Framework is starting.");

            StateTimeoutToRestart(BundleRuntimeState.Started);
            BundleRuntime.Start();
            FileLogUtility.Debug("Framework is started.");
            FileLogUtility.Debug("WebSite is started.");

            ControllerBuilder.Current.SetControllerFactory(new BundleRuntimeControllerFactory());

            RegisterGlobalFilters(GlobalFilters.Filters);
        }

        public virtual void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        private void BundleEventListener(object sender, BundleStateChangedEventArgs args)
        {
            // This is in another domain, thus the HttpContext.Current is always null.
            // Just comment it.
            //if (HttpContext.Current == null)
            //{
            //    return;
            //}

            //check if this bundle still exist.
            BundleData bundleData = BundleRuntime.Instance.GetFirstOrDefaultService<IBundleInstallerService>()
                .GetBundleDataByName(args.Bundle.SymbolicName);
            if (bundleData == null)
            {
                return;
            }
            bool needLoad = (args.CurrentState == BundleState.Active);

            if (needLoad)
            {
                //already registered its assemblies
                using (ReaderWriterLockHelper.CreateReaderLock(CacheLock))
                {
                    if (RegisteredBunldeCache.ContainsKey(bundleData))
                    {
                        return;
                    }
                    //register bundle assemblies to BuildManager.
                    ICollection<Assembly> assemblies = AddReferencedAssemblies(bundleData.SymbolicName);
                    FileLogUtility.Debug(
                        string.Format("Add the assemblies of bundle '{0}' to top level referenced assemblies since the bundle is active.",
                            args.Bundle.SymbolicName));
                    if (assemblies != null && assemblies.Count > 0)
                    {
                        using (ReaderWriterLockHelper.CreateWriterLock(CacheLock))
                        {
                            RegisteredBunldeCache[bundleData] = assemblies;
                        }
                    }
                }
            }
            else if (args.CurrentState == BundleState.Stopping)
            {
                //unregister when stopping.
                using (ReaderWriterLockHelper.CreateReaderLock(CacheLock))
                {
                    if (RegisteredBunldeCache.ContainsKey(bundleData))
                    {
                        RemoveReferencedAssemblies(RegisteredBunldeCache[bundleData]);
                        //remove from cache.
                        using (ReaderWriterLockHelper.CreateWriterLock(CacheLock))
                        {
                            RegisteredBunldeCache.Remove(bundleData);
                        }
                        FileLogUtility.Debug(
                            string.Format("Remove the assemblies of bundle '{0}' from top level referenced assemblies since the bundle is stopping.",
                                args.Bundle.SymbolicName));
                    }
                }
            }
        }

        /// <summary>
        /// 添加预定义的引用程序集。目前只有UIShell.OSGi和UIShell.OSGi.WebExtension。
        /// </summary>
        protected virtual void AddPreDefinedRefAssemblies()
        {
            bool webExtAssemblyAdded = false, osgiAssemblyAdded = false;

            foreach (Assembly assembly in TopLevelReferencedAssemblies)
            {
                if (assembly.FullName.Contains("UIShell.OSGi.WebExtension,"))
                {
                    webExtAssemblyAdded = true;
                }

                if (assembly.FullName.Contains("UIShell.OSGi,"))
                {
                    osgiAssemblyAdded = true;
                }

                if (webExtAssemblyAdded && osgiAssemblyAdded)
                {
                    break;
                }
            }

            if (!osgiAssemblyAdded)
            {
                AddReferencedAssembly(typeof(BundleRuntime).Assembly);
            }

            if (!webExtAssemblyAdded)
            {
                AddReferencedAssembly(GetType().Assembly);
            }
        }

        private void InitPhysicalPaths()
        {
            _webConfigPhysicalPath = HostingEnvironment.MapPath(WebConfigWebPath);
            _refreshHtmlPhysicalPath = HostingEnvironment.MapPath(RefreshHtmlWebPath);
            _hostRestartPhysicalPath = HostingEnvironment.MapPath(HostRestartWebPath);
        }

        /// <summary>
        /// Restart the website if the bundle runtime state is still in specified state.
        /// </summary>
        /// <param name="expectedState">Specified state.</param>
        private void StateTimeoutToRestart(BundleRuntimeState expectedState)
        {
            _appStartingTimeout = new Timer(
                state =>
                {
                    _appStartingTimeout.Dispose();
                    if (BundleRuntime.State != (BundleRuntimeState)state)
                    {
                        // If website is not started within 90 seconds, it will be restarted immediately.
                        FileLogUtility.Warn(string.Format("Fail to start/stop framework. BundleRuntime state is not in '{0}'.", state));
                        RestartAppDomain();
                    }
                    else
                    {
                        // If website is not started within 90 seconds, it will be restarted immediately.
                        FileLogUtility.Inform(string.Format("Dectect that the framework is in '{0}' state.", state));
                    }
                },
                expectedState,
                StateTimeOut * 1000,
                -1);
        }

        /// <summary>
        /// 当应用停止时，停止Bundle运行时。
        /// </summary>
        /// <param name="sender">Sender。</param>
        /// <param name="e">事件参数。</param>
        protected virtual void Application_End(object sender, EventArgs e)
        {
            FileLogUtility.Debug("Framework is stopping.");

            StateTimeoutToRestart(BundleRuntimeState.Stopped);
            BundleRuntime.Stop();
            FileLogUtility.Debug("Framework is stopped.");
            FileLogUtility.Debug("WebSite is stopped.");
        }

        #region IBundleRuntimeMvcHost Members

        public ICollection<Assembly> TopLevelReferencedAssemblies { get { return _topLevelReferencedAssemblies; } }

        /// <summary>
        /// 将插件本地程序集添加到ASP.NET页面预编译引用程序集列表。这个方法一般是内部使用。
        /// </summary>
        /// <param name="bundleSymbolicName">插件唯一名称。</param>
        /// <returns>返回插件所有本地程序集。</returns>
        public virtual ICollection<Assembly> AddReferencedAssemblies(string bundleSymbolicName)
        {
            //Check if this bundle still exist or not.
            BundleData bundleData = BundleRuntime.Instance.GetFirstOrDefaultService<IBundleInstallerService>().GetBundleDataByName(bundleSymbolicName);
            if (bundleData == null)
            {
                FileLogUtility.Debug(string.Format("Bundle '{0}' does not exist when trying to add its assemblies to referenced assemblies.",
                    bundleSymbolicName));
                return new List<Assembly>();
            }

            using (ReaderWriterLockHelper.CreateReaderLock(CacheLock))
            {
                //already registered its assembiles
                ICollection<Assembly> registeredItems;
                if (RegisteredBunldeCache.TryGetValue(bundleData, out registeredItems))
                {
                    return registeredItems;
                }

                IServiceManager serviceContainer = BundleRuntime.Framework.ServiceContainer;
                IRuntimeService service = serviceContainer.GetFirstOrDefaultService<IRuntimeService>();
                List<Assembly> assemlbies = service.LoadBundleAssembly(bundleSymbolicName);
                FileLogUtility.Debug(string.Format("Add the assemblies of bundle '{0}' to top level referenced assemblies.", bundleSymbolicName));
                assemlbies.ForEach(AddReferencedAssembly);
                //cache the assemblies
                using (ReaderWriterLockHelper.CreateWriterLock(CacheLock))
                {
                    RegisteredBunldeCache[bundleData] = assemlbies;
                }

                return assemlbies;
            }
        }

        /// <summary>
        /// 将程序集从ASP.NET页面预编译引用程序集列表中删除。
        /// </summary>
        /// <param name="assemblies">程序集列表。</param>
        public virtual void RemoveReferencedAssemblies(ICollection<Assembly> assemblies)
        {
            if (assemblies != null)
            {
                foreach (Assembly assembly in assemblies)
                {
                    RemoveReferencedAssemlby(assembly);
                }
            }
        }

        /// <summary>
        /// 将Assembly添加到ASP.NET页面预编译引用程序集列表。这个方法一般是内部使用。
        /// </summary>
        /// <param name="assembly">程序集对象。</param>
        public void AddReferencedAssembly(Assembly assembly)
        {
            // TODO：在以下场景会出现一些问题，场景描述为：WebHost程序引用了
            // 插件Service的程序集，这个程序集在编译时会拷贝到bin目录下。
            //
            // 此时，该程序集在CLR路径中可以加载到，但是这个程序集
            // 又被服务插件添加到TopReferencedAssemblies，此时在编译时会提示
            // 一个重复程序集异常。
            // 
            // 如果，我们在这里面进行判断是否CLR可以加载到时，又会有另一个异常，
            // 即WebHost的程序集是由CLR加载的，但是服务插件的程序集是由Assembly.LoadFile
            // 加载的，因此服务插件注册的服务接口是使用Assembly.LoadFile来定义，
            // 因此在WebHost用CLR加载的同一个程序集的接口去访问服务则无法访问到。
            //
            //Assembly asmInClrLoader = null;
            //try
            //{
            //    asmInClrLoader = Assembly.Load(assembly.FullName);
            //}
            //catch { }
            //if (asmInClrLoader != null)
            //{
            //    FileLogUtility.Warn(string.Format("There is a assembly '{0}' can be loaded by CLR loader and it is ignored to add to top level assembly reference.", assembly.FullName));
            //    return;
            //}

            // TODO: 性能优化
            using (ReaderWriterLockHelper.CreateWriterLock(RefAssembliesLock))
            {
                if (TopLevelReferencedAssemblies.Any(tempAssembly => tempAssembly.FullName.Equals(assembly.FullName)))
                {
                    return;
                }

                TopLevelReferencedAssemblies.Add(assembly);
            }

            FileLogUtility.Debug(string.Format("Add assembly '{0} to top level referenced assemblies.'", assembly.FullName));
        }

        /// <summary>
        /// 将程序集从ASP.NET页面预编译引用程序集列表中删除。
        /// </summary>
        /// <param name="assembly">程序集对象。</param>
        public void RemoveReferencedAssemlby(Assembly assembly)
        {
            using (ReaderWriterLockHelper.CreateWriterLock(RefAssembliesLock))
            {
                TopLevelReferencedAssemblies.Remove(assembly);
            }

            FileLogUtility.Debug(string.Format("Remove assembly '{0} from top level referenced assemblies.'", assembly.FullName));
        }

        public virtual void RestartAppDomain()
        {
            RestartAppDomain(WriteRebootMessage);
        }

        /// <summary>
        /// 重启ASP.NET应用域，包括BundleRuntime。当卸载一个模块或者更新一个模块时，需要重新启动应用域，因为旧的程序集会一直保留在BundleRuntime所在应用域直到应用域重启。
        /// </summary>
        public virtual void RestartAppDomain(WriteHtmlContentAfterReboot writeHtmlContent)
        {
            FileLogUtility.Debug("Restarting the website by write bin forder or web config.");
            bool success = TryWriteBinFolder() || TryWriteWebConfig();

            if (!success)
            {
                throw new BundleException(
                    string.Format("UIShell.OSGi needs to be restarted due to bundle uninstalling or updating, but was unable to do so.\r\n" +
                        "To prevent this issue in the future, a change to the web server configuration is required:\r\n" +
                        "- run the application in a full trust environment, or\r\n" +
                        "- give the application write access to the '{0}' folder, or\r\n" +
                        "- give the application write access to the '{1}' file.",
                        _hostRestartPhysicalPath,
                        _webConfigPhysicalPath));
            }

            // If setting up extensions/modules requires an AppDomain restart, it's very unlikely the
            // current request can be processed correctly.  So, we redirect to the same URL, so that the
            // new request will come to the newly started AppDomain.
            HttpContext httpContext = HttpContext.Current;
            if (httpContext != null)
            {
                // Don't redirect posts...
                if (httpContext.Request.RequestType == "GET")
                {
                    httpContext.Response.Redirect(httpContext.Request.Url.ToString(), true /*endResponse*/);
                }
                else
                {
                    string refreshHtmlFullPath = _refreshHtmlPhysicalPath;
                    try
                    {
                        // AppStore will create a refresh.html with different content.
                        if (File.Exists(refreshHtmlFullPath))
                        {
                            File.Delete(refreshHtmlFullPath);
                        }
                        using (StreamWriter sw = File.CreateText(refreshHtmlFullPath))
                        {
                            if (writeHtmlContent != null)
                            {
                                writeHtmlContent(sw);
                            }
                        }
                    }
                    catch
                    {
                        throw new BundleException(
                            string.Format("UIShell.OSGi needs to be restarted due to bundle uninstalling or updating, but was unable to do so.\r\n" +
                                "To prevent this issue in the future, a change to the web server configuration is required:\r\n" +
                                "- give the application create/write access to the '{0}' file.",
                                refreshHtmlFullPath));
                    }
                    httpContext.Response.WriteFile(_refreshHtmlPhysicalPath);
                    httpContext.Response.End();
                }

                FileLogUtility.Debug("Restart website successfully.");
            }
        }

        private void WriteRebootMessage(StreamWriter sw)
        {
            HttpContext httpContext = HttpContext.Current;
            if (httpContext != null)
            {
                string redirectUrl = httpContext.Request.Url.ToString();

                sw.Write("<HTML><HEAD> <meta http-equiv=\"content-type\" content=\"text/html;charset=utf-8\"> <TITLE>页面跳转中……</TITLE> " +
                    "<META HTTP-EQUIV=\"refresh\" content=\"15; url={0}?Message=操作已成功，请按 F5 刷新浏览器或退出系统后重新登录!\"> </HEAD>",
                    redirectUrl);
                sw.Write("<style>body{TEXT-ALIGN:center;} .center{MARGIN-RIGHT:auto;MARGIN-LEFT:auto;margin-top:200px;height:200px;width:400px;" +
                    "vertical-align:middle;line-height:40px;}</style>");
                sw.Write(string.Format("<BODY><div class=\"center\"><p>操作已成功，15 秒后将返回操作页面！请稍候……</p><p></p></div></BODY></HTML>"));
            }
        }

        private bool TryWriteWebConfig()
        {
            try
            {
                // In medium trust, "UnloadAppDomain" is not supported. Touch web.config
                // to force an AppDomain restart.
                File.SetLastWriteTimeUtc(_webConfigPhysicalPath, DateTime.UtcNow);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool TryWriteBinFolder()
        {
            try
            {
                string binMarker = HostingEnvironment.MapPath(_hostRestartPhysicalPath);
                if (binMarker != null)
                {
                    Directory.CreateDirectory(binMarker);

                    using (StreamWriter stream = File.CreateText(Path.Combine(binMarker, "marker.txt")))
                    {
                        stream.WriteLine("Restart on '{0}'", DateTime.UtcNow);
                        stream.Flush();
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion
    }
}