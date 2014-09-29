// -----------------------------------------------------------------------
//  <copyright file="AbstractBuilder.cs" company="OBear开源团队">
//      Copyright (c) 2014 OBear. All rights reserved.
//  </copyright>
//  <last-editor>郭明锋</last-editor>
//  <last-date>2014-07-13 18:54</last-date>
// -----------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace OBear.Plugin.Mvc
{
    public class ControllerTypeCache
    {
        private static readonly ConcurrentDictionary<string, Type> ControllerTypes;

        static ControllerTypeCache()
        {
            // Note: this type is marked as 'beforefieldinit'.
            ControllerTypes = new ConcurrentDictionary<string, Type>();
        }

        private static string GetKey(object plugin, object controllerName)
        {
            return string.Format("{0}$:$:${1}", plugin, controllerName);
        }

        public static void AddControllerType(string plugin, string controllerName, Type controllerType)
        {
            ControllerTypes.AddOrUpdate(GetKey(plugin, controllerName), key => controllerType, (key, existing) => controllerType);
        }

        public static Type GetControllerType(string plugin, string controllerName)
        {
            Type result;
            ControllerTypes.TryGetValue(GetKey(plugin, controllerName), out result);
            return result;
        }
    }
}