// -----------------------------------------------------------------------
//  <copyright file="AbstractBuilder.cs" company="OBear开源团队">
//      Copyright (c) OBear 2014. All rights reserved.
//  </copyright>
//  <last-editor>郭明锋</last-editor>
//  <last-date>2014:06:28 17:41</last-date>
// -----------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

using UIShell.OSGi;


namespace OBear.Plugin.Mvc
{
    public class BundleRuntimeViewEngine : IViewEngine
    {
        private readonly ConcurrentDictionary<string, IBundleViewEngine> _viewEngines;

        public BundleRuntimeViewEngine(IBundleViewEngineFactory viewEngineFactory)
        {
            _viewEngines = new ConcurrentDictionary<string, IBundleViewEngine>();
            Extensions.BundleSymbolicNames = () => _viewEngines.Select(m => m.Key).ToArray();
            BundleViewEngineFactory = viewEngineFactory;
            BundleRuntime.Instance.Framework.EventManager.AddBundleEventListener(new EventHandler<BundleStateChangedEventArgs>(BundleEventListener), true);
            BundleRuntime.Instance.Framework.EventManager.AddFrameworkEventListener(new EventHandler<FrameworkEventArgs>(FrameworkEventListener));
            BundleRuntime.Instance.Framework.Bundles.ForEach(new Action<IBundle>(AddViewEngine));
        }

        public IBundleViewEngineFactory BundleViewEngineFactory { get; private set; }

        public ViewEngineResult FindPartialView(ControllerContext controllerContext, string partialViewName, bool useCache)
        {
            IViewEngine viewEngine = GetViewEngine(controllerContext);
            if (viewEngine != null)
            {
                return viewEngine.FindPartialView(controllerContext, partialViewName, useCache);
            }
            return new ViewEngineResult(new string[0]);
        }

        public ViewEngineResult FindView(ControllerContext controllerContext, string viewName, string masterName, bool useCache)
        {
            IViewEngine viewEngine = GetViewEngine(controllerContext);
            if (viewEngine != null)
            {
                return viewEngine.FindView(controllerContext, viewName, masterName, useCache);
            }
            return new ViewEngineResult(new string[0]);
        }

        public void ReleaseView(ControllerContext controllerContext, IView view)
        {
            IViewEngine viewEngine = GetViewEngine(controllerContext);
            if (viewEngine != null)
            {
                viewEngine.ReleaseView(controllerContext, view);
            }
        }

        private void FrameworkEventListener(object sender, FrameworkEventArgs e)
        {
            if (e.EventType == FrameworkEventType.Stopped)
            {
                BundleRuntime.Instance.Framework.EventManager.RemoveBundleEventListener(
                    new EventHandler<BundleStateChangedEventArgs>(BundleEventListener),
                    true);
                BundleRuntime.Instance.Framework.EventManager.RemoveFrameworkEventListener(new EventHandler<FrameworkEventArgs>(FrameworkEventListener));
            }
        }

        private void BundleEventListener(object sender, BundleStateChangedEventArgs e)
        {
            if (e.CurrentState == BundleState.Active)
            {
                AddViewEngine(e.Bundle);
                return;
            }
            if (e.CurrentState == BundleState.Stopping)
            {
                RemoveViewEngine(e.Bundle);
            }
        }

        private void AddViewEngine(IBundle bundle)
        {
            _viewEngines.TryAdd(bundle.SymbolicName, BundleViewEngineFactory.CreateViewEngine(bundle));
        }

        private void RemoveViewEngine(IBundle bundle)
        {
            IBundleViewEngine bundleViewEngine;
            _viewEngines.TryRemove(bundle.SymbolicName, out bundleViewEngine);
        }

        private IViewEngine GetViewEngine(ControllerContext controllerContext)
        {
            string bundleSymbolicName = controllerContext.GetBundleSymbolicName();
            if (bundleSymbolicName == null)
            {
                return null;
            }
            IBundleViewEngine result;
            if (_viewEngines.TryGetValue(bundleSymbolicName, out result))
            {
                return result;
            }
            return null;
        }
    }
}