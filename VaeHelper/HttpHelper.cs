
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
namespace VaeHelper
{
    public class HttpHelper
    {
        /// <summary>
        /// POST
        /// </summary>
        /// <param name="url"></param>
        /// <param name="datas"></param>
        /// <param name="origDatas"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public static HttpResponse Post(string url, MultipartFormDataContent datas, WebHeaderCollection requestHeader = null, CookieCollection cookies = null, int timeOut = 10000)
        {
            var httpResponse = new HttpResponse();
            try
            {

                using (var client = new HttpClient())
                {
                    HttpResponseMessage response = client.PostAsync(url, datas).Result;

                    //请求成功
                    if (response.IsSuccessStatusCode)
                    {

                        var res = response.Content.ReadAsStringAsync().Result;

                        httpResponse.ResponseString = res;

                    }
                }
                return httpResponse;
            }
            catch (Exception ex)
            {
                var webExption = ex as WebException;
                if (webExption.Response != null)
                {
                    StreamReader reader = new StreamReader(webExption.Response.GetResponseStream(), Encoding.GetEncoding("UTF-8"));
                    httpResponse.ErrorResponse = reader.ReadToEnd();
                    httpResponse.Error = true;
                    return httpResponse;
                }
                else
                    throw ex;
            }
        }


        /// <summary>
        /// POST
        /// </summary>
        /// <param name="url"></param>
        /// <param name="datas"></param>
        /// <param name="origDatas"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public static async Task<HttpResponse> Post(string url,
                                                    string datas,
                                                    WebHeaderCollection requestHeader = null,
                                                    CookieContainer cookies = null,
                                                    string contentType = "application/json;charset=UTF-8",
                                                    string acceptType = "application/json",
                                                    string userAgent = "",
                                                    bool keepAlive = true,
                                                    string referer = "", int timeOut = 10000, IWebProxy webProxy = null)
        {
            var httpResponse = new HttpResponse();
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";
                if (webProxy != null)
                    request.Proxy = webProxy;
                if (!string.IsNullOrEmpty(referer))
                    request.Referer = referer;
                if (!string.IsNullOrEmpty(contentType))
                    request.ContentType = contentType;
                if (!string.IsNullOrEmpty(acceptType))
                    request.Accept = acceptType;

                if (requestHeader != null) request.Headers = requestHeader;
                byte[] origDatas = Encoding.UTF8.GetBytes(datas);
                if (!string.IsNullOrEmpty(userAgent))
                    request.UserAgent = userAgent;
                //request.ProtocolVersion = HttpVersion.Version10;
                request.ContentLength = origDatas.Length;
                request.KeepAlive = keepAlive;
                request.Timeout = timeOut;
                if (cookies != null)
                    request.CookieContainer = cookies;

                using (Stream reqStream = request.GetRequestStream())
                    reqStream.Write(origDatas, 0, origDatas.Length); //提交数据 
                var response = (HttpWebResponse)(await request.GetResponseAsync());
                httpResponse.ResponseHeader = response.Headers;
                string encodingName = response.ContentEncoding;
                if (encodingName != null && encodingName.ToLower() == "gzip")
                {
                    GZipStream gzip = new GZipStream(response.GetResponseStream(), CompressionMode.Decompress);//解压缩
                    using (var sr = new StreamReader(gzip, Encoding.UTF8))
                    {
                        httpResponse.ResponseString = sr.ReadToEnd();
                        return httpResponse;
                    }
                }
                if (encodingName == null || encodingName.Length < 1)
                    encodingName = "UTF-8"; //默认编码    

                using (var streamResponse = response.GetResponseStream())
                {
                    byte[] btContent = new byte[1024];
                    int intSize = 0;
                    intSize = streamResponse.Read(btContent, 0, 1024);
                    var memoryStream = new MemoryStream();
                    while (intSize > 0)
                    {
                        memoryStream.Write(btContent, 0, intSize);
                        intSize = streamResponse.Read(btContent, 0, 1024);
                    }
                    var byteResPonse = memoryStream.ToArray();
                    var responseStr = Encoding.UTF8.GetString(byteResPonse, 0, byteResPonse.Length);
                    httpResponse.ResponseString = responseStr;
                    return httpResponse;
                }
            }
            catch (Exception ex)
            {
                var webExption = ex as WebException;
                if (webExption?.Response != null)
                {
                    StreamReader reader = new StreamReader(webExption.Response.GetResponseStream(), Encoding.GetEncoding("UTF-8"));
                    httpResponse.ErrorResponse = reader.ReadToEnd();
                    if (string.IsNullOrEmpty(httpResponse.ErrorResponse)) httpResponse.ErrorResponse = ex.Message;
                    httpResponse.Error = true;
                    return httpResponse;
                }
                else
                    throw ex;
            }
        }

        /// <summary>
        /// POST
        /// </summary>
        /// <param name="url"></param>
        /// <param name="datas"></param>
        /// <param name="origDatas"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public static TResponse Post<TRequest, TResponse>(string url, TRequest datas, WebHeaderCollection requestHeader = null, string contentType = "application/json", string acceptType = "application/json", Encoding encoding = null, int timeOut = 10000)
        {
            var httpResponse = new HttpResponse();
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";
                if (!string.IsNullOrEmpty(contentType))
                    request.ContentType = contentType;
                if (!string.IsNullOrEmpty(acceptType))
                    request.Accept = acceptType;
                if (requestHeader != null) request.Headers = requestHeader;
                if (encoding == null) encoding = Encoding.UTF8;
                byte[] origDatas = encoding.GetBytes(JsonConvert.SerializeObject(datas));
                request.ContentLength = origDatas.Length;
                request.Timeout = timeOut;
                using (Stream reqStream = request.GetRequestStream())
                    reqStream.Write(origDatas, 0, origDatas.Length); //提交数据 
                var response = (HttpWebResponse)request.GetResponse();
                string encodingName = response.ContentEncoding;
                if (encodingName != null && encodingName.ToLower() == "gzip")
                {
                    GZipStream gzip = new GZipStream(response.GetResponseStream(), CompressionMode.Decompress);//解压缩
                    using (var sr = new StreamReader(gzip, Encoding.UTF8))
                    {
                        httpResponse.ResponseString = sr.ReadToEnd();
                        return JsonConvert.DeserializeObject<TResponse>(httpResponse.ResponseString);
                    }
                }
                if (encodingName == null || encodingName.Length < 1)
                    encodingName = "UTF-8"; //默认编码    
                using (var streamResponse = response.GetResponseStream())
                {
                    byte[] btContent = new byte[1024];
                    int intSize = 0;
                    intSize = streamResponse.Read(btContent, 0, 1024);
                    var memoryStream = new MemoryStream();
                    while (intSize > 0)
                    {
                        memoryStream.Write(btContent, 0, intSize);
                        intSize = streamResponse.Read(btContent, 0, 1024);
                    }
                    var byteResPonse = memoryStream.ToArray();
                    var responseStr = encoding.GetString(byteResPonse, 0, byteResPonse.Length);
                    httpResponse.ResponseString = responseStr;
                    return JsonConvert.DeserializeObject<TResponse>(httpResponse.ResponseString);
                }
            }
            catch (Exception ex)
            {
                var webExption = ex as WebException;
                if (webExption.Response != null)
                {
                    using (var reader = new StreamReader(webExption.Response.GetResponseStream(), Encoding.GetEncoding("UTF-8")))
                        throw new Exception($"请求:{url}异常:{reader.ReadToEnd()}");
                }
                throw ex;
            }
        }
        /// <summary>
        /// POST
        /// </summary>
        /// <param name="url"></param>
        /// <param name="datas"></param>
        /// <param name="origDatas"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public static HttpResponse Post(string url, byte[] origDatas = null, WebHeaderCollection requestHeader = null, string contentType = "application/json", string acceptType = "application/json", int timeOut = 10000, bool responseHeader = false)
        {
            var httpResponse = new HttpResponse();
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";
                if (!string.IsNullOrEmpty(contentType))
                    request.ContentType = contentType;
                if (!string.IsNullOrEmpty(acceptType))
                    request.Accept = acceptType;
                if (requestHeader != null) request.Headers = requestHeader;
                request.ContentLength = origDatas.Length;
                request.Timeout = timeOut;
                using (Stream reqStream = request.GetRequestStream())
                    reqStream.Write(origDatas, 0, origDatas.Length); //提交数据 
                var response = (HttpWebResponse)request.GetResponse();
                string encoding = response.ContentEncoding;
                if (encoding?.ToLower() == "gzip")
                {
                    GZipStream gzip = new GZipStream(response.GetResponseStream(), CompressionMode.Decompress);//解压缩
                    using (var sr = new StreamReader(gzip, Encoding.UTF8))
                    {
                        httpResponse.ResponseString = sr.ReadToEnd();
                        return httpResponse;
                    }
                }
                if (encoding == null || encoding.Length < 1)
                    encoding = "UTF-8"; //默认编码   
                if (responseHeader) httpResponse.ResponseHeader = response.Headers;
                using (Stream streamResponse = response.GetResponseStream())
                {
                    byte[] btContent = new byte[1024];
                    int intSize = 0;
                    intSize = streamResponse.Read(btContent, 0, 1024);
                    var memoryStream = new MemoryStream();
                    while (intSize > 0)
                    {
                        memoryStream.Write(btContent, 0, intSize);
                        intSize = streamResponse.Read(btContent, 0, 1024);
                    }
                    var byteResPonse = memoryStream.ToArray();
                    var responseStr = Encoding.UTF8.GetString(byteResPonse, 0, byteResPonse.Length);
                    httpResponse.ResponseString = responseStr;
                    return httpResponse;
                }
            }
            catch (Exception ex)
            {
                var webExption = ex as WebException;
                if (webExption.Response != null)
                {
                    StreamReader reader = new StreamReader(webExption.Response.GetResponseStream(), Encoding.GetEncoding("UTF-8"));
                    httpResponse.ErrorResponse = reader.ReadToEnd();
                    httpResponse.Error = true;
                    return httpResponse;
                }
                else
                    throw ex;
            }
        }
        public static async Task<HttpResponse> Get(string url, WebHeaderCollection requestHeader = null, CookieContainer cookies = null,
            string contentType = "application/json", string acceptType = "application/json", string userAgent = "", int timeOut = 10000, IWebProxy webProxy = null)
        {
            var httpResponse = new HttpResponse();
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                if (requestHeader != null)
                {
                    //有些请求需要把Date的格式放在head只能用反射的方式添加。注意request.Date
                    System.Reflection.MethodInfo priMethod = request.Headers.GetType().GetMethod("AddWithoutValidate", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                    foreach (var key in requestHeader.AllKeys)
                    {
                        priMethod.Invoke(request.Headers, new[] { key, requestHeader[key] });
                    }
                }
                if (!string.IsNullOrEmpty(userAgent))
                    request.UserAgent = userAgent;
                request.Timeout = timeOut;
                request.Method = "GET";
                if (webProxy != null)
                    request.Proxy = webProxy;
                if (cookies != null)
                    request.CookieContainer = cookies;
                var response = (HttpWebResponse)(await request.GetResponseAsync());
                string encoding = response.ContentEncoding;

                if (encoding == null || encoding.Length < 1)
                    encoding = "UTF-8"; //默认编码    
                if (encoding.ToLower() == "gzip")
                {
                    GZipStream gzip = new GZipStream(response.GetResponseStream(), CompressionMode.Decompress);//解压缩
                    using (var sr = new StreamReader(gzip, Encoding.UTF8))
                    {
                        httpResponse.ResponseString = sr.ReadToEnd();
                        return httpResponse;
                    }
                }

                using (Stream streamResponse = response.GetResponseStream())
                {
                    byte[] btContent = new byte[1024];
                    int intSize = 0;
                    intSize = streamResponse.Read(btContent, 0, 1024);
                    var memoryStream = new MemoryStream();
                    while (intSize > 0)
                    {
                        memoryStream.Write(btContent, 0, intSize);
                        intSize = streamResponse.Read(btContent, 0, 1024);
                    }
                    var byteResPonse = memoryStream.ToArray();
                    var responseStr = Encoding.UTF8.GetString(byteResPonse, 0, byteResPonse.Length);
                    httpResponse.ResponseString = responseStr.Trim('"');
                    return httpResponse;
                }
            }
            catch (Exception ex)
            {
                var webExption = ex as WebException;
                if (webExption?.Response != null)
                {
                    StreamReader reader = new StreamReader(webExption.Response.GetResponseStream(), Encoding.GetEncoding("UTF-8"));
                    httpResponse.ErrorResponse = reader.ReadToEnd();
                    if (string.IsNullOrEmpty(httpResponse.ErrorResponse)) httpResponse.ErrorResponse = ex.Message;
                    httpResponse.Error = true;
                    return httpResponse;
                }
                else
                    throw ex;
            }
        }
        /// <summary>
        /// post请求方法
        /// </summary>
        /// <param name="url"></param>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string Post(string url, string str, string token)
        {
            string result = "";
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "POST";
            req.ContentType = "application/json";
            req.Headers = new WebHeaderCollection();
            req.Headers.Add("token", token);
            req.Headers.Add("Content-Type", "application/json;charset=UTF-8");
            byte[] data = Encoding.UTF8.GetBytes(str);  // 把字符串转换为字节

            req.ContentLength = data.Length;  // 请求长度

            using (Stream reqStream = req.GetRequestStream())  // 获取
            {
                reqStream.Write(data, 0, data.Length);  // 向当前流中写入字节
                reqStream.Close();                      // 关闭当前流
            }

            HttpWebResponse resp = (HttpWebResponse)req.GetResponse(); //响应结果
            Stream stream = resp.GetResponseStream();
            // 获取响应内容
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                result = reader.ReadToEnd();
            }
            return result;
        }


        public static byte[] HttpDownload(string url, IWebProxy webProxy = null)
        {
            byte[] arraryByte;
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
            if (webProxy != null)
                req.Proxy = webProxy;
            req.Method = "GET";
            using (WebResponse wr = req.GetResponse())
            {
                StreamReader responseStream = new StreamReader(wr.GetResponseStream(), Encoding.UTF8);
                int length = (int)wr.ContentLength;
                byte[] bs = new byte[length];

                HttpWebResponse response = wr as HttpWebResponse;
                Stream stream = response.GetResponseStream();

                //读取到内存
                MemoryStream stmMemory = new MemoryStream();
                byte[] buffer1 = new byte[length];
                int i;
                //将字节逐个放入到Byte 中
                while ((i = stream.Read(buffer1, 0, buffer1.Length)) > 0)
                {
                    stmMemory.Write(buffer1, 0, i);
                }
                arraryByte = stmMemory.ToArray();
                stmMemory.Close();
            }
            return arraryByte;
        }
    }



}
public class HttpResponse
{
    public string ResponseString { get; set; }

    public WebHeaderCollection ResponseHeader { get; set; }

    public string ResponseHeaderString
    {
        get
        {
            if (ResponseHeader != null && ResponseHeader.Count > 0)
            {
                return ResponseHeader.ToString();
            }
            else
                return string.Empty;
        }
    }

    public string ErrorResponse { get; set; }

    public bool Error { get; set; }

    public string Img { get; set; }
}
