// -----------------------------------------------------------------------
//  <copyright file="AbstractBuilder.cs" company="OBear开源团队">
//      Copyright (c) 2014 OBear. All rights reserved.
//  </copyright>
//  <last-editor>郭明锋</last-editor>
//  <last-date>2014-07-13 17:49</last-date>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;

using OBear.Plugin.Mvc;

using UIShell.OSGi;


namespace OBear.Plugin.Web
{
    /// <summary>
    /// ASP.NET BundleRuntime宿主。用于宿主插件内核框架以及预编译支持。
    /// </summary>
    /// <example>
    /// <para>以下代码展示了如何在一个Page类页面中获取该实例，从而来使用BundleRuntime。</para>
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
    ///             // The BundleHttpApplication implements the IBundleRuntimeHttpHost interface.
    ///             // Thus, we can get this instance from HttpContext.
    ///             IBundleRuntimeHttpHost bundleRuntimeHttpHost = (IBundleRuntimeHttpHost)Context.ApplicationInstance;
    ///             BundleRuntime runtime = bundleRuntimeHttpHost.BundleRuntime;
    ///         }
    ///     }
    /// }
    /// </code>
    /// </example>
    public interface IBundleRuntimeHttpHost
    {
        /// <summary>
        /// Bundle运行时实例。
        /// </summary>
        BundleRuntime BundleRuntime { get; }
        /// <summary>
        /// ASP.NET页面预编译时引用的程序集。
        /// </summary>
        ICollection<Assembly> TopLevelReferencedAssemblies { get; }

        /// <summary>
        /// 将插件本地程序集添加到ASP.NET页面预编译引用程序集列表。这个方法一般是内部使用。
        /// </summary>
        /// <param name="bundleSymbolicName">插件唯一名称。</param>
        /// <returns>返回插件所有本地程序集。</returns>
        ICollection<Assembly> AddReferencedAssemblies(string bundleSymbolicName);

        /// <summary>
        /// 将Assembly添加到ASP.NET页面预编译引用程序集列表。这个方法一般是内部使用。
        /// </summary>
        /// <param name="assembly">程序集对象。</param>
        void AddReferencedAssembly(Assembly assembly);

        /// <summary>
        /// 将程序集从ASP.NET页面预编译引用程序集列表中删除。
        /// </summary>
        /// <param name="assembly">程序集对象。</param>
        void RemoveReferencedAssemlby(Assembly assembly);

        /// <summary>
        /// 将程序集从ASP.NET页面预编译引用程序集列表中删除。
        /// </summary>
        /// <param name="assemblies">程序集列表。</param>
        void RemoveReferencedAssemblies(ICollection<Assembly> assemblies);

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
        Control LoadControlFromBundle(TemplateControl controlLoader, string path, string bundleSymbolicName);

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
        void RestartAppDomain();

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
        void RestartAppDomain(WriteHtmlContentAfterReboot writeHtmlContent);
    }


    /// <summary>
    /// 重启系统后，向用户展示的HTML页面信息的delegate。
    /// </summary>
    /// <param name="sw">HTML页面的StreamWriter。</param>
    public delegate void WriteHtmlContentAfterReboot(StreamWriter sw);
}