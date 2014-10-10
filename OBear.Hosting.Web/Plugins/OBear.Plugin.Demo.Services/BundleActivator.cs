// -----------------------------------------------------------------------
//  <copyright file="BundleActivator.cs" company="OBear开源团队">
//      Copyright (c) 2014 OBear. All rights reserved.
//  </copyright>
//  <last-editor>郭明锋</last-editor>
//  <last-date>2014-08-07 18:16</last-date>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using AutoMapper;

using OBear.Core.Data.Entity;
using OBear.Core.Data.Entity.Migrations;
using OBear.Plugin.Demo.Dtos;
using OBear.Plugin.Demo.Models;

using UIShell.OSGi;


namespace OBear.Plugin.Demo.Services
{
    public class BundleActivator : IBundleActivator
    {
        #region Implementation of IBundleActivator

        public void Start(IBundleContext context)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            DatabaseInitializer.AddMapperAssembly(assembly);

            SeedDataInitialize();
            MapperCreate();
        }

        public void Stop(IBundleContext context)
        { }

        #endregion

        private static void SeedDataInitialize()
        {
            ISeedAction seedAction = new SeedAction();
            CreateDatabaseIfNotExistsWithSeed.SeedActions.Add(seedAction);
        }

        private static void MapperCreate()
        {
            Mapper.CreateMap<DemoEntityDto, DemoEntity>();
        }
    }
}