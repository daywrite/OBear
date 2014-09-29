// -----------------------------------------------------------------------
//  <copyright file="AbstractBuilder.cs" company="OBear开源团队">
//      Copyright (c) 2014 OBear. All rights reserved.
//  </copyright>
//  <last-editor>郭明锋</last-editor>
//  <last-date>2014-07-13 17:56</last-date>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;

using UIShell.OSGi;
using UIShell.OSGi.Configuration.BundleManifest;
using UIShell.OSGi.Core.Service;
using UIShell.OSGi.Utility;


namespace OBear.Plugin.Web
{
    /// <summary>
    /// 该类是Web Bundle中ASPX页面的PageHandlerFacgtory。它用于辅助ASP.NET运行时动态编译ASPX页面。基于UIOSP的Web应用程序我们需要修改Web.config，从而使对ASPX页面的请求
    /// 交给BundlePageHandlerFactory来处理，而不是默认的PageHandlerFactory。
    /// <example>
    /// <para>下面是一个使用该类的Web.config配置。</para>
    /// <code>
    /// <![CDATA[
    /// <?xml version="1.0" encoding="utf-8"?>
    /// <configuration>
    ///     <appSettings/>
    ///     <connectionStrings/>
    ///
    ///     <system.web>
    ///       <compilation debug="true"></compilation>
    ///
    ///       <httpHandlers>
    ///         <remove path="*.aspx" verb="*"/>
    ///         <add path="*.aspx" verb="*" type="UIShell.OSGi.WebExtension.BundlePageHandlerFactory"/>
    ///       </httpHandlers>
    ///     </system.web>
    /// </configuration>
    /// ]]>
    /// </code>
    /// </example>
    /// </summary>
    public class BundlePageHandlerFactory : PageHandlerFactory
    {
        private object _syncObject;

        static BundlePageHandlerFactory()
        {
            ErrorHandlerPage = "~/ErrorHandler.aspx";
            FrameworkBusyHandlerPage = "~/FrameworkBusyHandler.aspx";
            DefaultPage = "~/Default.aspx";
        }

        public BundlePageHandlerFactory()
        {
            _syncObject = new object();
        }

        /// <summary>
        /// 当系统出现无法处理的错误时，对应的错误处理页面。
        /// </summary>
        public static string ErrorHandlerPage { get; set; }
        /// <summary>
        /// 当系统正在启动/停止/已经停止时，用于提示用户等待或者启动。
        /// </summary>
        public static string FrameworkBusyHandlerPage { get; set; }
        /// <summary>
        /// 默认首页。
        /// </summary>
        public static string DefaultPage { get; set; }
        /// <summary>
        /// 当前错误信息。
        /// </summary>
        public static string CurrentErrorMessage
        {
            get
            {
                if (HttpContext.Current != null)
                {
                    return HttpContext.Current.Application["ErrorMessage"] as string;
                }
                return string.Empty;
            }
        }

        /// <summary>
        /// 将插件的一个ASP.NET页面编译并构建成一个IHttpHandler实例。
        /// </summary>
        /// <param name="context">HttpContext。</param>
        /// <param name="requestType">请求类型。</param>
        /// <param name="virtualPath">页面虚拟路径。</param>
        /// <param name="path">页面物理路径。</param>
        /// <returns>IHttpHandler实例。</returns>
        public override IHttpHandler GetHandler(HttpContext context, string requestType, string virtualPath, string path)
        {
            BundleRuntime instance = BundleRuntime.Instance;
            if (instance.State != BundleRuntimeState.Started)
            {
                try
                {
                    FileLogUtility.Debug(string.Format("Framework is not in 'Started' state when access page '{0}'.", path));
                    return base.GetHandler(context, requestType, FrameworkBusyHandlerPage, "");
                }
                catch (Exception ex)
                {
                    FileLogUtility.Warn("Failed to redirect framework Busy Handler page when Framework is not in 'Started'.");
                    FileLogUtility.Warn(ex);
                }
                return null;
            }
            string value = string.Empty;
            IBundleRuntimeHttpHost bundleRuntimeHttpHost = (IBundleRuntimeHttpHost)context.ApplicationInstance;
            BundleData bundleData = bundleRuntimeHttpHost.BundleRuntime.GetFirstOrDefaultService<IBundleInstallerService>()
                .FindBundleContainPath(Directory.GetParent(path).FullName);
            if (bundleData != null)
            {
                value = bundleData.SymbolicName;
            }
            if (string.IsNullOrEmpty(value))
            {
                FileLogUtility.Debug(string.Format(
                    "Failed to get the bundle contains requested page '{0}' and just compile this page into IHttpHandler. Just compile the page directly.",
                    path));
                return SafelyGetHandler(context, requestType, virtualPath, path);
            }
            IBundle bundle = bundleRuntimeHttpHost.BundleRuntime.Framework.GetBundle(bundleData.Path);
            if (bundle == null)
            {
                return SafelyGetHandler(context, requestType, virtualPath, path);
            }
            FileLogUtility.Debug(string.Format("The bundle state of requested page '{0}' is '{1}'.", path, bundle.State));
            switch (bundle.State)
            {
                case BundleState.Installed:
                case BundleState.Resolved:
                    {
                        object syncObject;
                        Monitor.Enter(syncObject = _syncObject);
                        try
                        {
                            bundle.Start(BundleStartOptions.General);
                            bundleRuntimeHttpHost.AddReferencedAssemblies(bundleData.SymbolicName);
                        }
                        finally
                        {
                            Monitor.Exit(syncObject);
                        }
                        return SafelyGetHandler(context, requestType, virtualPath, path);
                    }
                case BundleState.Starting:
                    {
                        object syncObject2;
                        Monitor.Enter(syncObject2 = _syncObject);
                        try
                        {
                            bundleRuntimeHttpHost.AddReferencedAssemblies(bundleData.SymbolicName);
                        }
                        finally
                        {
                            Monitor.Exit(syncObject2);
                        }
                        return SafelyGetHandler(context, requestType, virtualPath, path);
                    }
                case BundleState.Active:
                    return SafelyGetHandler(context, requestType, virtualPath, path);
                case BundleState.Stopping:
                    return HandleException(context, requestType, new HttpException("Access denied, for the bundle is stopping."));
                case BundleState.Uninstalled:
                    return HandleException(context, requestType, new HttpException("Access denied, for the bundle is uninstalled."));
                default:
                    throw new NotSupportedException();
            }
        }

        private IHttpHandler SafelyGetHandler(HttpContext context, string requestType, string virtualPath, string path)
        {
            IHttpHandler result;
            try
            {
                result = base.GetHandler(context, requestType, virtualPath, path);
            }
            catch (Exception e)
            {
                result = HandleException(context, requestType, e);
            }
            return result;
        }

        /// <summary>
        /// 处理异常。
        /// </summary>
        /// <param name="context">HttpContext。</param>
        /// <param name="requestType">请求类型。</param>
        /// <param name="e">异常。</param>
        /// <returns>IHttpHandler实例，或者直接抛出异常。</returns>
        private IHttpHandler HandleException(HttpContext context, string requestType, Exception e)
        {
            FileLogUtility.Error(string.Format("Log the exception when compiling the request page '{0}'.", context.Request.Path));
            FileLogUtility.Error(e);
            context.Application["ErrorMessage"] = e.Message;
            IHttpHandler result;
            try
            {
                IHttpHandler handler = base.GetHandler(context, requestType, ErrorHandlerPage, "");
                FileLogUtility.Warn(string.Format("Return the Error Handler page when failed to get the request page '{0}'.", context.Request.Path));
                result = handler;
            }
            catch (Exception ex)
            {
                FileLogUtility.Error(
                    string.Format("Failed to get Error Handler page '{0}' when getting the request page '{1}' failed.", ErrorHandlerPage,
                    context.Request.Path));
                FileLogUtility.Error(ex);
                throw e;
            }
            return result;
        }
    }
}