// -----------------------------------------------------------------------
//  <copyright file="AdminBaseController.cs" company="OBear开源团队">
//      Copyright (c) 2014 OBear. All rights reserved.
//  </copyright>
//  <last-editor>郭明锋</last-editor>
//  <last-date>2014-07-25 2:39</last-date>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;


namespace OBear.Web.Mvc
{
    [Authorize]
    public abstract class AdminBaseController : BaseController
    { }
}