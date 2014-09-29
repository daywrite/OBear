// -----------------------------------------------------------------------
//  <copyright file="AbstractBuilder.cs" company="OBear开源团队">
//      Copyright (c) 2014 OBear. All rights reserved.
//  </copyright>
//  <last-editor>郭明锋</last-editor>
//  <last-date>2014-07-13 17:55</last-date>
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
using System.Web.UI;

using OBear.Plugin.Mvc;

using UIShell.OSGi;
using UIShell.OSGi.Configuration.BundleManifest;
using UIShell.OSGi.Core.Service;
using UIShell.OSGi.Loader;
using UIShell.OSGi.Logging;
using UIShell.OSGi.Utility;


namespace OBear.Plugin.Web
{
    /// <summary>
    /// BundleHttpApplication是一个宿主了BundleRuntime的HttpApplication，它实现了IBundleRuntimeHttpHost接口。
    /// 我们可以在ASP.NET的页面中来获取该实例，从而来获取IBundleRuntimeHttpHost接口的实例。这样也可以
    /// 获得BundleRuntime实例，并使用它提供的方法来从插件中正确加载一个用户控件。如果你仅需要获取BundleRuntime
    /// 实例的话，也可以通过BundleRuntime.Instance这个静态属性。
    /// </summary>
    /// <example>
    /// <para>在基于UIOSP的Web应用程序，您需要使Global类继承于BundleHttpApplication，而不是HttpApplication。</para>
    /// <code>
    /// using System;
    /// using System.Collections.Generic;
    /// using System.Linq;
    /// using System.Web;
    /// using System.Web.Security;
    /// using System.Web.SessionState;
    /// using UIShell.OSGi.WebExtension;
    ///
    /// namespace MovieStore
    /// {
    ///     public class Global : BundleHttpApplication
    ///     {
    ///     }
    /// }
    /// </code>
    /// </example>
    public class BundleHttpApplication : HttpApplication, IBundleRuntimeHttpHost
    {
        public const int STATE_TIME_OUT = 90;
        private static BundleRuntime _bundleRuntime;
        private static Timer _appStartingTimeout;
        private LogLevel LogLevel
        {
            get
            {
                string value = WebConfigurationManager.AppSettings["LogLevel"];
                if (!string.IsNullOrEmpty(value))
                {
                    LogLevel result;
                    try
                    {
                        object obj = Enum.Parse(typeof(LogLevel), value);
                        if (obj == null)
                        {
                            return LogLevel.Debug;
                        }
                        result = (LogLevel)obj;
                    }
                    catch
                    {
                        return LogLevel.Debug;
                    }
                    return result;
                }
                return LogLevel.Debug;
            }
        }
        private int MaxLogFileSize
        {
            get
            {
                string text = WebConfigurationManager.AppSettings["MaxLogFileSize"];
                if (!string.IsNullOrEmpty(text))
                {
                    int result;
                    try
                    {
                        result = int.Parse(text);
                    }
                    catch
                    {
                        return 10;
                    }
                    return result;
                }
                return 10;
            }
        }
        private bool CreateNewLogFileOnMaxSize
        {
            get
            {
                string value = WebConfigurationManager.AppSettings["CreateNewLogFileOnMaxSize"];
                if (!string.IsNullOrEmpty(value))
                {
                    bool result;
                    try
                    {
                        result = bool.Parse(value);
                    }
                    catch
                    {
                        return false;
                    }
                    return result;
                }
                return false;
            }
        }
        private string LogName
        {
            get
            {
                string value = WebConfigurationManager.AppSettings["LogName"];
                if (!string.IsNullOrEmpty(value))
                {
                    return LogName;
                }
                return "logs\\OSGi.NET-log.txt";
            }
        }
        private string LogLocation
        {
            get
            {
                string text = WebConfigurationManager.AppSettings["LogLocation"];
                if (!string.IsNullOrEmpty(text))
                {
                    return text;
                }
                return AppDomain.CurrentDomain.BaseDirectory;
            }
        }
        /// <summary>
        /// Bundle运行时实例。
        /// </summary>
        public BundleRuntime BundleRuntime { get { return _bundleRuntime; } private set { _bundleRuntime = value; } }
        /// <summary>
        /// ASP.NET页面预编译时引用的程序集。
        /// </summary>
        public virtual ICollection<Assembly> TopLevelReferencedAssemblies { get { return BundleRuntimeHttpHostHelper.TopLevelReferencedAssemblies; } }

        /// <summary>
        /// 将插件本地程序集添加到ASP.NET页面预编译引用程序集列表。这个方法一般是内部使用。
        /// </summary>
        /// <param name="bundleSymbolicName">插件唯一名称。</param>
        /// <returns>返回插件所有本地程序集。</returns>
        public virtual ICollection<Assembly> AddReferencedAssemblies(string bundleSymbolicName)
        {
            return BundleRuntimeHttpHostHelper.AddReferencedAssemblies(bundleSymbolicName);
        }

        /// <summary>
        /// 将程序集从ASP.NET页面预编译引用程序集列表中删除。
        /// </summary>
        /// <param name="assemblies">程序集列表。</param>
        public virtual void RemoveReferencedAssemblies(ICollection<Assembly> assemblies)
        {
            BundleRuntimeHttpHostHelper.RemoveReferencedAssemblies(assemblies);
        }

        /// <summary>
        /// 将Assembly添加到ASP.NET页面预编译引用程序集列表。这个方法一般是内部使用。
        /// </summary>
        /// <param name="assembly">程序集对象。</param>
        public void AddReferencedAssembly(Assembly assembly)
        {
            BundleRuntimeHttpHostHelper.AddReferencedAssembly(assembly);
        }

        /// <summary>
        /// 将程序集从ASP.NET页面预编译引用程序集列表中删除。
        /// </summary>
        /// <param name="assembly">程序集对象。</param>
        public void RemoveReferencedAssemlby(Assembly assembly)
        {
            BundleRuntimeHttpHostHelper.RemoveReferencedAssemlby(assembly);
        }

        /// <summary>
        /// 从一个插件中加载一个ASP.NET用户控件。
        /// </summary>
        /// <param name="controlLoader">用户控件加载器，一般为Page类的实例。</param>
        /// <param name="path">用户控件路径。</param>
        /// <param name="bundleSymbolicName">插件唯一名称。</param>
        /// <returns>用户控件</returns>
        /// <example>
        /// <para>以下代码用于从一个ASP.NET页面Default.aspx.cs的Page_Load中加载一个用户控件。</para>
        /// <code>
        /// using System;
        /// using System.Collections.Generic;
        /// using System.Web;
        /// using System.Web.UI;
        /// using System.Web.UI.WebControls;
        /// using UIShell.OSGi.Core.Service;
        /// using UIShell.OSGi.WebExtension;
        /// using System.Xml;
        /// using System.Reflection;
        ///
        /// namespace UIShell.OSGi.WebShell
        /// {
        ///     public partial class _Default : System.Web.UI.Page
        ///     {
        ///         protected void Page_Load(object sender, EventArgs e)
        ///         {
        ///             IBundleRuntimeHttpHost bundleRuntimeHttpHost = (IBundleRuntimeHttpHost)Context.ApplicationInstance;
        ///             BundleRuntime runtime = bundleRuntimeHttpHost.BundleRuntime;
        ///             PlaceHolder1.Controls.Add(bundleRuntimeHttpHost.LoadControlFromBundle(this, "~/Plugins/CommonsPlugin/UserControl1.ascx", "CommonsPlugin"));
        ///         }
        ///     }
        /// }
        /// </code>
        /// </example>
        public virtual Control LoadControlFromBundle(TemplateControl controlLoader, string path, string bundleSymbolicName)
        {
            return BundleRuntimeHttpHostHelper.LoadControlFromBundle(controlLoader, path, bundleSymbolicName);
        }

        /// <summary>
        /// 重启ASP.NET应用域，包括BundleRuntime。当卸载一个模块或者更新一个模块时，需要重新启动应用域，因为旧的程序集会一直保留在BundleRuntime所在应用域直到应用域重启。
        /// </summary>
        public virtual void RestartAppDomain()
        {
            BundleRuntimeHttpHostHelper.RestartAppDomain();
        }

        /// <summary>
        /// 重启ASP.NET应用域，包括BundleRuntime。当卸载一个模块或者更新一个模块时，需要重新启动应用域，因为旧的程序集会一直保留在BundleRuntime所在应用域直到应用域重启。
        /// </summary>
        public virtual void RestartAppDomain(WriteHtmlContentAfterReboot writeHtmlContent)
        {
            BundleRuntimeHttpHostHelper.RestartAppDomain(writeHtmlContent);
        }

        /// <summary>
        /// 创建插件运行时<see cref="BundleRuntime"/>实例
        /// </summary>
        /// <returns></returns>
        protected virtual BundleRuntime CreateBundleRuntime()
        {
            BundleRuntime runtime = new BundleRuntime();
#if DEBUG
            runtime.EnableAssemblyShadowCopy = true;
#endif
            return runtime;
        }

        /// <summary>
        /// 应用启动时处理函数。该函数用于初始化TopLevelReferncedAssemblies，并将UIShell.OSGi和UIShell.OSGi.WebExtension这两个程序集添加到该属性。
        /// 同时，启动Bundle运行时。
        /// </summary>
        /// <param name="sender">Sender。</param>
        /// <param name="e">事件参数。</param>
        protected virtual void Application_Start(object sender, EventArgs e)
        {
            FileLogUtility.Init(LogName, LogLocation);
            FileLogUtility.SetLogLevel(LogLevel);
            FileLogUtility.SetMaxFileSizeByMB(MaxLogFileSize);
            FileLogUtility.SetCreateNewFileOnMaxSize(CreateNewLogFileOnMaxSize);
            FileLogUtility.Debug("WebSite is starting.");
            AddPreDefinedRefAssemblies();
            AppDomain.CurrentDomain.SetData("SQLServerCompactEditionUnderWebHosting", true);
            BundleRuntime = CreateBundleRuntime();
            BundleRuntime.Instance.Framework.EventManager.AddBundleEventListener(
                new EventHandler<BundleStateChangedEventArgs>(BundleRuntimeHttpHostHelper.RefreshBundleTopLevelReferencedAssembliesByEvent),
                true);
            FileLogUtility.Debug("Framework is starting.");
            StateTimeoutToRestart(BundleRuntimeState.Started);
            BundleRuntime.Start();
            FileLogUtility.Debug("Framework is started.");
            FileLogUtility.Debug("WebSite is started.");
        }

        /// <summary>
        /// Restart the website if the bundle runtime state is still in specified state.
        /// </summary>
        /// <param name="expectedState">Specified state.</param>
        private void StateTimeoutToRestart(BundleRuntimeState expectedState)
        {
            _appStartingTimeout = new Timer(delegate(object state)
            {
                _appStartingTimeout.Dispose();
                if (BundleRuntime.State != (BundleRuntimeState)state)
                {
                    FileLogUtility.Warn(string.Format("Fail to start/stop framework. BundleRuntime state is not in '{0}'.", state));
                    RestartAppDomain();
                    return;
                }
                FileLogUtility.Inform(string.Format("Dectect that the framework is in '{0}' state.", state));
            },
                expectedState,
                90000,
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

        /// <summary>
        /// 添加预定义的引用程序集。目前只有UIShell.OSGi和UIShell.OSGi.WebExtension。
        /// </summary>
        protected virtual void AddPreDefinedRefAssemblies()
        {
            AddReferencedAssembly(typeof(BundleRuntime).Assembly);
            AddReferencedAssembly(base.GetType().Assembly);
        }
    }
}