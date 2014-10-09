using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

using OBear.Utility;
using OBear.Utility.Exceptions;
using OBear.Utility.Secutiry;
using OBear.Web.Http.Internal;
using OBear.Web.Properties;


namespace OBear.Web.Http.Security
{
    /// <summary>
    /// 服务端通信加密解密消息处理器
    /// </summary>
    public class HostCryptoHandler : DelegatingHandler
    {
        private readonly string _privateKey;
        private readonly string _hashType;
        private string _publicKey;

        /// <summary>
        /// 使用服务端密钥初始化<see cref="HostCryptoHandler"/>类的新实例
        /// </summary>
        /// <param name="privateKey">服务端私钥</param>
        /// <param name="hashType">签名哈希类型，必须为MD5或SHA1</param>
        public HostCryptoHandler(string privateKey, string hashType = "MD5")
        {
            privateKey.CheckNotNullOrEmpty("privateKey");
            hashType.CheckNotNullOrEmpty("hashType");
            hashType = hashType.ToUpper();
            hashType.Required(str => hashType == "MD5" || hashType == "SHA1", Resources.Http_Security_RSA_Sign_HashType);
            _privateKey = privateKey;
            _hashType = hashType;
        }

        /// <summary>
        /// 以异步操作发送 HTTP 请求到内部管理器以发送到服务器。
        /// </summary>
        /// <returns>
        /// 返回 <see cref="T:System.Threading.Tasks.Task`1"/>。 表示异步操作的任务对象。
        /// </returns>
        /// <param name="request">要发送到服务器的 HTTP 请求消息。</param><param name="cancellationToken">取消操作的取消标记。</param><exception cref="T:System.ArgumentNullException"><paramref name="request"/> 为 null。</exception>
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var result = DecryptRequest(request);
            if (result != null)
            {
                return result;
            }
            return base.SendAsync(request, cancellationToken)
                .ContinueWith(task => EncryptResponse(task.Result), cancellationToken);
        }

        private Task<HttpResponseMessage> DecryptRequest(HttpRequestMessage request)
        {
            if (!request.Headers.Contains(HttpHeaderNames.OBearClientPublicKey))
            {
                return CreateResponseTask(request, HttpStatusCode.BadRequest, "在请求头中客户端公钥信息无法找到。");
            }
            _publicKey = request.Headers.GetValues(HttpHeaderNames.OBearClientPublicKey).First();

            if (request.Content == null)
            {
                return null;
            }
            string data = request.Content.ReadAsStringAsync().Result;
            if (string.IsNullOrEmpty(data))
            {
                return null;
            }
            try
            {
                data = CommunicationCryptor.DecryptAndVerifyData(data, _privateKey, _hashType, _publicKey);
                if (data == null)
                {
                    throw new OBearException("服务器解析请求数据时发生异常。");
                }
                HttpContent content = new StringContent(data);
                content.Headers.ContentType = request.Content.Headers.ContentType;
                request.Content = content;
                return null;
            }
            catch (CryptographicException ex)
            {
                return CreateResponseTask(request, HttpStatusCode.BadRequest, "服务器解析传输数据时发生异常。", ex);
            }
            catch (Exception ex)
            {
                return CreateResponseTask(request, HttpStatusCode.BadRequest, Resources.Http_Security_Host_DecryptRequest_Failt, ex);
            }
        }

        private HttpResponseMessage EncryptResponse(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                return response;
            }
            if (response.Content == null)
            {
                return response;
            }
            string data = response.Content.ReadAsStringAsync().Result;
            try
            {
                data = CommunicationCryptor.EncryptData(data, _privateKey, _publicKey, _hashType);
                HttpContent content = new StringContent(data);
                content.Headers.ContentType = response.Content.Headers.ContentType;
                response.Content = content;
                return response;
            }
            catch (Exception)
            {
                HttpError error = new HttpError(Resources.Http_Security_Host_EncryptResponse_Failt);
                return response.RequestMessage.CreateErrorResponse(HttpStatusCode.BadRequest, error);
            }
        }

        private Task<HttpResponseMessage> CreateResponseTask(HttpRequestMessage request,
            HttpStatusCode statusCode,
            string message,
            Exception ex = null)
        {
            return Task<HttpResponseMessage>.Factory.StartNew(() =>
            {
                if (statusCode >= HttpStatusCode.OK && statusCode <= (HttpStatusCode)299)
                {
                    if (_publicKey != null)
                    {
                        message = CommunicationCryptor.EncryptData(message, _privateKey, _publicKey, _hashType);
                        return request.CreateResponse(statusCode, message);
                    }
                }
                HttpResponseMessage response = request.CreateErrorResponse(statusCode, ex == null ? new HttpError(message) : new HttpError(ex, false));
                return response;
            });
        }
    }
}
