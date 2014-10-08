// -----------------------------------------------------------------------
//  <copyright file="AbstractBuilder.cs" company="OBear开源团队">
//      Copyright (c) 2014 OBear. All rights reserved.
//  </copyright>
//  <last-editor>郭明锋</last-editor>
//  <last-date>2014:07:05 15:18</last-date>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;


namespace OBear.Utility.Secutiry
{
    /// <summary>
    /// 字符串Hash操作类
    /// </summary>
    public static class HashHelper
    {
        /// <summary>
        /// 获取字符串的MD5哈希值
        /// </summary>
        public static string GetMd5(string value)
        {
            value.CheckNotNull("value");
            byte[] bytes = Encoding.ASCII.GetBytes(value);
            return GetMd5(bytes);
        }

        /// <summary>
        /// 获取字节数组的MD5哈希值
        /// </summary>
        public static string GetMd5(byte[] bytes)
        {
            bytes.CheckNotNullOrEmpty("bytes");
            StringBuilder sb = new StringBuilder();
            MD5 hash = new MD5CryptoServiceProvider();
            bytes = hash.ComputeHash(bytes);
            foreach (byte b in bytes)
            {
                sb.AppendFormat("{0:x2}", b);
            }
            return sb.ToString();
        }

        /// <summary>
        /// 获取字符串的SHA1哈希值
        /// </summary>
        public static string GetSha1(string value)
        {
            value.CheckNotNullOrEmpty("value");

            StringBuilder sb = new StringBuilder();
            SHA1Managed hash = new SHA1Managed();
            byte[] bytes = hash.ComputeHash(Encoding.ASCII.GetBytes(value));
            foreach (byte b in bytes)
            {
                sb.AppendFormat("{0:x2}", b);
            }
            return sb.ToString();
        }

        /// <summary>
        /// 获取字符串的Sha256哈希值
        /// </summary>
        public static string GetSha256(string value)
        {
            value.CheckNotNullOrEmpty("value");

            StringBuilder sb = new StringBuilder();
            SHA256Managed hash = new SHA256Managed();
            byte[] bytes = hash.ComputeHash(Encoding.ASCII.GetBytes(value));
            foreach (byte b in bytes)
            {
                sb.AppendFormat("{0:x2}", b);
            }
            return sb.ToString();
        }

        /// <summary>
        /// 获取字符串的Sha512哈希值
        /// </summary>
        public static string GetSha512(string value)
        {
            value.CheckNotNullOrEmpty("value");

            StringBuilder sb = new StringBuilder();
            SHA512Managed hash = new SHA512Managed();
            byte[] bytes = hash.ComputeHash(Encoding.ASCII.GetBytes(value));
            foreach (byte b in bytes)
            {
                sb.AppendFormat("{0:x2}", b);
            }
            return sb.ToString();
        }
    }
}