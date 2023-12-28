using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
class Program
{
    public static void Main(string[] args)
    {
        var JSESSID = "b14cfa1ac8ecd40b7FC7D2d204599ead3d8efbec22";
        var userAgent = "VaeHome/2.3.3 (iPhone; iOS 14.5; Scale/3.00)";
        Console.WriteLine(SignIn(JSESSID, userAgent));
    }
    public static string SignIn(string JSESSID, string userAgent)
    {
        ServicePointManager.ServerCertificateValidationCallback = Validator;
        var headers = new WebHeaderCollection();
        headers.Add("Accept-Language", "zh-Hans-CN;q=1");
        headers.Add("Accept-Encoding", "gzip, deflate, br");
        var cookies = new CookieContainer();
        cookies.Add(new Uri("https://api1-xusong.taihe.com"), new Cookie("JSESSID", JSESSID));
        var url = "https://api1-xusong.taihe.com/USER_HOME/getRecordByMonth";
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var response = Post(url, "", headers, cookies, contentType: "application/x-www-form-urlencoded", acceptType: "*/*", userAgent: userAgent);
        stopwatch.Stop();
        var dateTimeNow = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff");
        Regex regex = new Regex(@"(?<=\""serverTime\"":)[^,]*");
        MatchCollection matches = regex.Matches(response);
        var serverTime = ConvertStringToDateTime(matches[0].Value).ToString("yyyy-MM-dd HH:mm:ss");
        Console.WriteLine("耗时:" + stopwatch.ElapsedMilliseconds + "毫秒,服务器时间:" + serverTime + "当前时间:" + dateTimeNow);
        return response;
    }
    public static bool Validator(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
    {
        return true;
    }
    public static string Post(string url, string datas, WebHeaderCollection requestHeader = null, CookieContainer cookies = null, string contentType = "application/json", string acceptType = "application/json", string userAgent = "")
    {

        string httpResponse = "";
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

        request.Method = "POST";
        if (!string.IsNullOrEmpty(contentType))
            request.ContentType = contentType;
        if (!string.IsNullOrEmpty(acceptType))
            request.Accept = acceptType;
        if (requestHeader != null) request.Headers = requestHeader;

        if (!string.IsNullOrEmpty(userAgent))
            request.UserAgent = userAgent;
        request.ContentLength = 0;
        request.Timeout = 10000;
        if (cookies != null)
            request.CookieContainer = cookies;
        var response = request.GetResponse() as HttpWebResponse;
        string encodingName = response.ContentEncoding;
        if (encodingName != null && encodingName.ToLower() == "gzip")
        {
            GZipStream gzip = new GZipStream(response.GetResponseStream(), CompressionMode.Decompress);//解压缩
            using (var sr = new StreamReader(gzip, Encoding.UTF8))
            {
                httpResponse = sr.ReadToEnd();
            }
        }
        return httpResponse;
    }
    public static DateTime ConvertStringToDateTime(string timeStamp)
    {
        DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
        long lTime;
        if (timeStamp.Length.Equals(10))//判断是10位
        {
            lTime = long.Parse(timeStamp + "0000000");
        }
        else
        {
            lTime = long.Parse(timeStamp + "0000");//13位
        }
        TimeSpan toNow = new TimeSpan(lTime);
        return dtStart.Add(toNow);
    }
}