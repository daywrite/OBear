using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;

using OBear.Plugin.Mvc;

namespace OBear.Plugin.IoC.Autofac
{
    public class ServiceResolver : IServiceResolver
    {
        private readonly IContainer _container;

        public ServiceResolver(IContainer container)
        {
            _container = container;
        }

        /// <summary>
        /// 获取单个实例，构造函数注入方式可用
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public object Resolve(Type type)
        {
            return _container.ResolveOptional(type);
        }

        /// <summary>
        /// 获取强类型实例
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Resolve<T>()
        {
            return _container.Resolve<T>();
        }

        /// <summary>
        /// 对现有对象进行属性注入，可以先由无参构造函数生成对象，再进行属性注入
        /// </summary>
        /// <param name="instance">未进行属性注入的对象</param>
        public T InjectProperties<T>(T instance)
        {
            //只注入未设置的属性
            return _container.InjectUnsetProperties(instance);
        }
    }
}
