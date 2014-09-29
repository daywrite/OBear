// -----------------------------------------------------------------------
//  <copyright file="AbstractBuilder.cs" company="OBear开源团队">
//      Copyright (c) 2014 OBear. All rights reserved.
//  </copyright>
//  <last-editor>郭明锋</last-editor>
//  <last-date>2014-07-13 17:46</last-date>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Compilation;
using System.Web.Hosting;
using System.Web.UI;

using UIShell.OSGi;
using UIShell.OSGi.Configuration.BundleManifest;
using UIShell.OSGi.Core.Service;
using UIShell.OSGi.Loader;
using UIShell.OSGi.Utility;


namespace OBear.Plugin.Web
{
    public static class BundleRuntimeHttpHostHelper
    {
        private static ReaderWriterLock _refAssembliesLock;
        private static IList<Assembly> _topLevelReferencedAssemblies;
        private static ReaderWriterLock _cacheLock;
        private static Dictionary<BundleData, IList<Assembly>> _registeredBunldeCache;
        private static string WebConfigPhysicalPath;
        private static string RefreshHtmlPhysicalPath;
        private static string HostRestartPhysicalPath;

        static BundleRuntimeHttpHostHelper()
        {
            _refAssembliesLock = new ReaderWriterLock();
            _cacheLock = new ReaderWriterLock();
            _registeredBunldeCache = new Dictionary<BundleData, IList<Assembly>>();
            PropertyInfo property = typeof(BuildManager).GetProperty("TheBuildManager",
                BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.GetProperty);
            if (property != null)
            {
                BuildManager buildManager = property.GetValue(null, null) as BuildManager;
                if (buildManager != null)
                {
                    PropertyInfo property2 = typeof(BuildManager).GetProperty("TopLevelReferencedAssemblies",
                        BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetProperty);
                    if (property2 != null)
                    {
                        _topLevelReferencedAssemblies = (property2.GetValue(buildManager, null) as IList<Assembly>);
                        if (_topLevelReferencedAssemblies != null)
                        {
                            Assembly coreAssembly = typeof(CatchBlock).Assembly;
                            _topLevelReferencedAssemblies.Add(coreAssembly);
                        }
                    }
                }
            }
            WebConfigPhysicalPath = HostingEnvironment.MapPath("~/web.config");
            RefreshHtmlPhysicalPath = HostingEnvironment.MapPath("~/refresh.html");
            HostRestartPhysicalPath = HostingEnvironment.MapPath("~/bin/HostRestart");
        }

        /// <summary>
        /// ASP.NET页面预编译时引用的程序集。
        /// </summary>
        public static IList<Assembly> TopLevelReferencedAssemblies { get { return _topLevelReferencedAssemblies; } }

        public static void RefreshBundleTopLevelReferencedAssembliesByEvent(object sender, BundleStateChangedEventArgs e)
        {
            BundleData bundleDataByName =
                BundleRuntime.Instance.GetFirstOrDefaultService<IBundleInstallerService>().GetBundleDataByName(e.Bundle.SymbolicName);
            if (bundleDataByName == null)
            {
                return;
            }
            if (e.CurrentState == BundleState.Active)
            {
                AddReferencedAssemblies(bundleDataByName);
                return;
            }
            if (e.CurrentState == BundleState.Stopping)
            {
                RemoveReferencedAssemblies(bundleDataByName);
            }
        }

        public static void AddReferencedAssemblies(BundleData bundleData)
        {
            using (ReaderWriterLockHelper.CreateReaderLock(_cacheLock))
            {
                if (!_registeredBunldeCache.ContainsKey(bundleData))
                {
                    IList<Assembly> list = AddReferencedAssemblies(bundleData.SymbolicName);
                    FileLogUtility.Debug(
                        string.Format("Add the assemblies of bundle '{0}' to top level referenced assemblies since the bundle is active.",
                            bundleData.SymbolicName));
                    if (list != null && list.Count > 0)
                    {
                        using (ReaderWriterLockHelper.CreateWriterLock(_cacheLock))
                        {
                            _registeredBunldeCache[bundleData] = list;
                        }
                    }
                }
            }
        }

        public static void RemoveReferencedAssemblies(BundleData bundleData)
        {
            using (ReaderWriterLockHelper.CreateReaderLock(_cacheLock))
            {
                if (_registeredBunldeCache.ContainsKey(bundleData))
                {
                    IList<Assembly> removedAssemblies = _registeredBunldeCache[bundleData];
                    using (ReaderWriterLockHelper.CreateWriterLock(_cacheLock))
                    {
                        _registeredBunldeCache.Remove(bundleData);
                    }
                    ResetTopLevelReferencedAssemblies(removedAssemblies);
                    FileLogUtility.Debug(
                        string.Format("Remove the assemblies of bundle '{0}' from top level referenced assemblies since the bundle is stopping.",
                            bundleData.SymbolicName));
                }
            }
        }

        /// <summary>
        /// 将插件本地程序集添加到ASP.NET页面预编译引用程序集列表。这个方法一般是内部使用。
        /// </summary>
        /// <param name="bundleSymbolicName">插件唯一名称。</param>
        /// <returns>返回插件所有本地程序集。</returns>
        public static IList<Assembly> AddReferencedAssemblies(string bundleSymbolicName)
        {
            BundleData bundleDataByName =
                BundleRuntime.Instance.GetFirstOrDefaultService<IBundleInstallerService>().GetBundleDataByName(bundleSymbolicName);
            if (bundleDataByName == null)
            {
                FileLogUtility.Debug(string.Format("Bundle '{0}' does not exist when trying to add its assemblies to referenced assemblies.",
                    bundleSymbolicName));
                return new List<Assembly>();
            }
            IList<Assembly> result;
            using (ReaderWriterLockHelper.CreateReaderLock(_cacheLock))
            {
                IList<Assembly> list;
                if (_registeredBunldeCache.TryGetValue(bundleDataByName, out list))
                {
                    result = list;
                }
                else
                {
                    IServiceManager serviceContainer = BundleRuntime.Instance.Framework.ServiceContainer;
                    IRuntimeService firstOrDefaultService = serviceContainer.GetFirstOrDefaultService<IRuntimeService>();
                    List<Assembly> list2 = firstOrDefaultService.LoadBundleAssembly(bundleSymbolicName);
                    FileLogUtility.Debug(string.Format("Add the assemblies of bundle '{0}' to top level referenced assemblies.", bundleSymbolicName));
                    using (ReaderWriterLockHelper.CreateWriterLock(_cacheLock))
                    {
                        _registeredBunldeCache[bundleDataByName] = list2;
                    }
                    ResetTopLevelReferencedAssemblies(null);
                    result = list2;
                }
            }
            return result;
        }

        /// <summary>
        /// 当启动/停止插件时，需要对TopLevelAssemblies重新处理确保同一个程序集的只有最高版本出现在列表中。
        /// 详细请查看“模块层与类加载设计规范的2.7小节”。
        /// </summary>
        /// <param name="removedAssemblies"></param>
        public static void ResetTopLevelReferencedAssemblies(IList<Assembly> removedAssemblies)
        {
            using (ReaderWriterLockHelper.CreateReaderLock(_cacheLock))
            {
                if (removedAssemblies != null)
                {
                    foreach (Assembly current in removedAssemblies)
                    {
                        RemoveReferencedAssemlby(current);
                    }
                }
                List<Assembly> list = new List<Assembly>();
                foreach (IList<Assembly> current2 in _registeredBunldeCache.Values)
                {
                    list.AddRange(current2);
                }
                list.Sort(new AssemblyComparer());
                list.ForEach(AddReferencedAssembly);
            }
        }

        /// <summary>
        /// 将Assembly添加到ASP.NET页面预编译引用程序集列表。这个方法一般是内部使用。
        /// </summary>
        /// <param name="assembly">程序集对象。</param>
        public static void AddReferencedAssembly(Assembly assembly)
        {
            Assembly assembly2 = null;
            using (ReaderWriterLockHelper.CreateWriterLock(_refAssembliesLock))
            {
                foreach (Assembly current in _topLevelReferencedAssemblies)
                {
                    AssemblyName name = current.GetName();
                    AssemblyName name2 = assembly.GetName();
                    if (name.Name.Equals(name2.Name))
                    {
                        if (name2.Version.CompareTo(name.Version) <= 0)
                        {
                            return;
                        }
                        assembly2 = current;
                    }
                }
                if (assembly2 != null)
                {
                    _topLevelReferencedAssemblies.Remove(assembly2);
                }
                _topLevelReferencedAssemblies.Add(assembly);
            }
            FileLogUtility.Debug(string.Format("Add assembly '{0} to top level referenced assemblies.'", assembly.FullName));
        }

        /// <summary>
        /// 将程序集从ASP.NET页面预编译引用程序集列表中删除。
        /// </summary>
        /// <param name="assembly">程序集对象。</param>
        public static void RemoveReferencedAssemlby(Assembly assembly)
        {
            using (ReaderWriterLockHelper.CreateWriterLock(_refAssembliesLock))
            {
                _topLevelReferencedAssemblies.Remove(assembly);
            }
            FileLogUtility.Debug(string.Format("Remove assembly '{0} from top level referenced assemblies.'", assembly.FullName));
        }

        /// <summary>
        /// 将程序集从ASP.NET页面预编译引用程序集列表中删除。
        /// </summary>
        /// <param name="assemblies">程序集列表。</param>
        public static void RemoveReferencedAssemblies(ICollection<Assembly> assemblies)
        {
            if (assemblies != null)
            {
                foreach (Assembly current in assemblies)
                {
                    RemoveReferencedAssemlby(current);
                }
            }
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
        public static Control LoadControlFromBundle(TemplateControl controlLoader, string path, string bundleSymbolicName)
        {
            AddReferencedAssemblies(bundleSymbolicName);
            return controlLoader.LoadControl(path);
        }

        private static void WriteRebootMessage(TextWriter sw)
        {
            HttpContext current = HttpContext.Current;
            if (current != null)
            {
                string arg = current.Request.Url.ToString();
                sw.Write("<HTML><HEAD> <meta http-equiv=\"content-type\" content=\"text/html;charset=utf-8\"> <TITLE>页面跳转中……</TITLE> <META HTTP-EQUIV=\"refresh\" content=\"15; url={0}?Message=操作已成功，请按 F5 刷新浏览器或退出系统后重新登录!\"> </HEAD>", arg);
                sw.Write(
                    "<style>body{TEXT-ALIGN:center;} .center{MARGIN-RIGHT:auto;MARGIN-LEFT:auto;margin-top:200px;height:200px;width:400px;vertical-align:middle;line-height:40px;}</style>");
                sw.Write("<BODY><div class=\"center\"><p>操作已成功，15 秒后将返回操作页面！请稍候……</p><p></p></div></BODY></HTML>");
            }
        }

        /// <summary>
        /// 重启ASP.NET应用域，包括Web站点和BundleRuntime。
        /// </summary>
        /// <example>
        /// <para>以下代码是在一个页面中重启了ASP.NET应用域。</para>
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
        ///         protected void RestartAppDomain_Clicked(object sender, EventArgs e)
        ///         {
        ///             IBundleRuntimeHttpHost bundleRuntimeHttpHost = (IBundleRuntimeHttpHost)Context.ApplicationInstance;
        ///             bundleRuntimeHttpHost.RestartAppDomain();
        ///         }
        ///     }
        /// }
        /// </code>
        /// </example>
        public static void RestartAppDomain()
        {
            RestartAppDomain(WriteRebootMessage);
        }

        /// <summary>
        /// 重启ASP.NET应用域，包括Web站点和BundleRuntime，并填写向用户展示的HTML页面。
        /// </summary>
        /// <param name="writeHtmlContent">向用户展示的HTML页面信息的delegate。</param>
        /// <example>
        /// <para>以下代码是在一个页面中重启了ASP.NET应用域。</para>
        ///
        /// <code>
        /// <![CDATA[
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
        ///         protected void RestartAppDomain_Clicked(object sender, EventArgs e)
        ///         {
        ///             IBundleRuntimeHttpHost bundleRuntimeHttpHost = (IBundleRuntimeHttpHost)Context.ApplicationInstance;
        ///             bundleRuntimeHttpHost.RestartAppDomain(WriteMessageOnly);
        ///         }
        ///     }
        /// }
        ///
        /// private void WriteMessageOnly(StreamWriter sw)
        /// {
        ///     sw.Write("<HTML><HEAD> <meta http-equiv=\"content-type\" content=\"text/html;charset=utf-8\"> <TITLE>关闭浏览器</TITLE> </HEAD>");
        ///     sw.Write("<style>body{TEXT-ALIGN:center;} .center{MARGIN-RIGHT:auto;MARGIN-LEFT:auto;margin-top:200px;height:200px;width:400px;vertical-align:middle;line-height:40px;}</style>");
        ///     sw.Write(string.Format("<BODY><div class=\"center\"><p>操作已成功，请关闭浏览器重新访问。</p><p></p></div></BODY></HTML>"));
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public static void RestartAppDomain(WriteHtmlContentAfterReboot writeHtmlContent)
        {
            FileLogUtility.Debug("Restarting the website by write bin forder or web config.");
            if (!TryWriteBinFolder() && !TryWriteWebConfig())
            {
                throw new BundleException(
                    string.Format(
                        "UIShell.OSGi needs to be restarted due to bundle uninstalling or updating, but was unable to do so.\r\nTo prevent this issue in the future, a change to the web server configuration is required:\r\n- run the application in a full trust environment, or\r\n- give the application write access to the '{0}' folder, or\r\n- give the application write access to the '{1}' file.",
                        HostRestartPhysicalPath,
                        WebConfigPhysicalPath));
            }
            HttpContext current = HttpContext.Current;
            if (current != null)
            {
                if (current.Request.RequestType == "GET")
                {
                    current.Response.Redirect(current.Request.Url.ToString(), true);
                }
                else
                {
                    string refreshHtmlPhysicalPath = RefreshHtmlPhysicalPath;
                    try
                    {
                        if (File.Exists(refreshHtmlPhysicalPath))
                        {
                            File.Delete(refreshHtmlPhysicalPath);
                        }
                        using (StreamWriter streamWriter = File.CreateText(refreshHtmlPhysicalPath))
                        {
                            if (writeHtmlContent != null)
                            {
                                writeHtmlContent(streamWriter);
                            }
                        }
                    }
                    catch
                    {
                        throw new BundleException(
                            string.Format(
                                "UIShell.OSGi needs to be restarted due to bundle uninstalling or updating, but was unable to do so.\r\nTo prevent this issue in the future, a change to the web server configuration is required:\r\n- give the application create/write access to the '{0}' file.",
                                refreshHtmlPhysicalPath));
                    }
                    current.Response.WriteFile(RefreshHtmlPhysicalPath);
                    current.Response.End();
                }
                FileLogUtility.Debug("Restart website successfully.");
            }
        }

        private static bool TryWriteWebConfig()
        {
            bool result;
            try
            {
                File.SetLastWriteTimeUtc(WebConfigPhysicalPath, DateTime.UtcNow);
                result = true;
            }
            catch
            {
                result = false;
            }
            return result;
        }

        private static bool TryWriteBinFolder()
        {
            bool result;
            try
            {
                string hostRestartPhysicalPath = HostRestartPhysicalPath;
                Directory.CreateDirectory(hostRestartPhysicalPath);
                using (StreamWriter streamWriter = File.CreateText(Path.Combine(hostRestartPhysicalPath, "marker.txt")))
                {
                    streamWriter.WriteLine("Restart on '{0}'", DateTime.UtcNow);
                    streamWriter.Flush();
                }
                result = true;
            }
            catch
            {
                result = false;
            }
            return result;
        }
    }
}