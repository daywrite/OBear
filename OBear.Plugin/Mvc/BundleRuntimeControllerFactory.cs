// -----------------------------------------------------------------------
//  <copyright file="BundleRuntimeControllerFactory.cs" company="OBear开源团队">
//      Copyright (c) 2014 OBear. All rights reserved.
//  </copyright>
//  <last-editor>郭明锋</last-editor>
//  <last-date>2014-07-15 16:02</last-date>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

using UIShell.OSGi;
using UIShell.OSGi.Loader;
using UIShell.OSGi.Utility;


namespace OBear.Plugin.Mvc
{
    public class BundleRuntimeControllerFactory : DefaultControllerFactory
    {
        protected override Type GetControllerType(RequestContext requestContext, string controllerName)
        {
            const string namespacesKey = "Namespaces";
            string bundleSymbolicName = requestContext.GetBundleSymbolicName();
            if (bundleSymbolicName == null)
            {
                requestContext = HandleNonPluginRequestContext(requestContext, ref controllerName);
            }

            object namespaces;
            if (!requestContext.RouteData.DataTokens.TryGetValue(namespacesKey, out namespaces))
            {
                namespaces = bundleSymbolicName == null
                    ? DefaultConfig.GetHostingNamespaces()
                    : DefaultConfig.GetBundleNamespaces(bundleSymbolicName);
                requestContext.RouteData.DataTokens[namespacesKey] = namespaces;
            }

            string cacheKey = bundleSymbolicName ?? DefaultConfig.HostingName;
            //从缓存获取控制器类型
            Type controllerType = ControllerTypeCache.GetControllerType(cacheKey, controllerName);
            if (controllerType != null)
            {
                FileLogUtility.Inform(string.Format("Loaded controller '{0}' from bundle '{1}' by using cache.",
                    controllerName,
                    bundleSymbolicName));
                return controllerType;
            }

            if (bundleSymbolicName != null)
            {
                string value = controllerName + "Controller";
                IRuntimeService firstOrDefaultService = BundleRuntime.Instance.GetFirstOrDefaultService<IRuntimeService>();
                List<Assembly> assemblies = firstOrDefaultService.LoadBundleAssembly(bundleSymbolicName);
                foreach (Assembly assembly in assemblies)
                {
                    Type[] types = assembly.GetTypes();
                    foreach (Type type in types)
                    {
                        if (!type.Name.ToLower().Contains(value.ToLower()) || !typeof(IController).IsAssignableFrom(type) || type.IsAbstract)
                        {
                            continue;
                        }
                        controllerType = type;
                        ControllerTypeCache.AddControllerType(cacheKey, controllerName, controllerType);
                        FileLogUtility.Inform(string.Format("Loaded controller '{0}' from bundle '{1}' and then added to cache.",
                            controllerName,
                            bundleSymbolicName));
                        return controllerType;
                    }
                }
                FileLogUtility.Error(string.Format("Failed to load controller '{0}' from bundle '{1}'.", controllerName, bundleSymbolicName));
            }
            try
            {
                controllerType = base.GetControllerType(requestContext, controllerName);
                if (controllerType != null)
                {
                    ControllerTypeCache.AddControllerType(cacheKey, controllerName, controllerType);
                }
                return controllerType;
            }
            catch (Exception ex)
            {
                FileLogUtility.Error(string.Format("Failed to load controller '{0}'", controllerName));
                FileLogUtility.Error(ex);
            }
            return null;
        }

        private static RequestContext HandleNonPluginRequestContext(RequestContext requestContext, ref string controllerName)
        {
            const string idKey = "id", actionKey = "action", controllerKey = "controller", pluginKey = "plugin";
            RouteValueDictionary values = requestContext.RouteData.Values;
            RouteValueDictionary defaults = ((Route)requestContext.RouteData.Route).Defaults;

            if (!values.ContainsKey(pluginKey))
            {
                return requestContext;
            }
            bool actionEqual = values[actionKey].Equals(defaults[actionKey]);
            bool controllerEqual = values[controllerKey].Equals(defaults[controllerKey]);

            if (actionEqual && controllerEqual)
            {
                //类似：/members
                values[controllerKey] = values[pluginKey];
                controllerName = values[pluginKey].ToString();
            }
            else if (actionEqual && !controllerEqual)
            {
                //类似：/members/delete
                values[actionKey] = values[controllerKey];
                values[controllerKey] = values[pluginKey];
                controllerName = values[pluginKey].ToString();
            }
            else if (!actionEqual && !controllerEqual)
            {
                //类似：/members/delete/2
                values[idKey] = values[actionKey];
                values[actionKey] = values[controllerKey];
                values[controllerKey] = values[pluginKey];
            }
            values.Remove(pluginKey);
            return requestContext;
        }

        protected override IController GetControllerInstance(RequestContext requestContext, Type controllerType)
        {
            if (controllerType == null)
            {
                throw new HttpException(404, string.Format("创建Controller实例时控制器类型 controllerType 为空：{0}", requestContext.HttpContext.Request.Path));
            }
            if (!typeof(IController).IsAssignableFrom(controllerType))
            {
                throw new ArgumentException("创建Controller实例时传入类型不是IController类型", "controllerType");
            }
            IServiceResolver resolver = IoCBundleActivator.ServiceResolver;
            if (resolver != null)
            {
                IController controller = resolver.Resolve(controllerType) as IController;
                if (controller != null)
                {
                    controller = resolver.InjectProperties(controller);
                    return controller;
                } 
            }
            return base.GetControllerInstance(requestContext, controllerType);
        }
    }
}