using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;


namespace OBear.Utility.Extensions
{
    /// <summary>
    /// <see cref="HttpRequest"/>扩展辅助操作类
    /// </summary>
    public static class HttpRequestExtensions
    {
        /// <summary>
        /// 获取IP地址
        /// </summary>
        public static string GetIpAddress(this HttpRequest request)
        {
            string result = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (result.IsNullOrEmpty())
            {
                result = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            }
            return result;
        }
    }
}
