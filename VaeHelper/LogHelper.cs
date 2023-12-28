using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace VaeHelper
{
    public class LogHelper
    {
        public static readonly LogHelper Instance = new LogHelper();

        private LogHelper() { }
        public string logPath = AppDomain.CurrentDomain.BaseDirectory + "ServiceLog\\";
        private static object lockObject = new object();
        public void Log(string msg)
        {
            lock (lockObject)
            {
                if (!Directory.Exists(logPath))
                    Directory.CreateDirectory(logPath);
                if (!Write(logPath + DateTime.Now.ToString("yyyy-MM-dd") + ".txt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff") + ": " + msg + "\r\n", FileMode.Append))
                    Write(logPath + DateTime.Now.ToString("yyyy-MM-dd") + ".txt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff") + ": " + msg + "\r\n", FileMode.Append);
            }
        }
        /// <summary>
        /// 存在Ticket文件夹时记录日志
        /// </summary>
        /// <param name="logContent"></param>
        public void LogIfExistTicketDirectory(object logContent)
        {
            try
            {
                if (!Directory.Exists(logPath + "Ticket\\")) return;
                var content = logContent is string ? logContent.ToString() : JsonConvert.SerializeObject(logContent);
                lock (lockObject)
                {

                    if (!Write(logPath + "Ticket\\" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ": " + JsonConvert.SerializeObject(logContent) + "\r\n", FileMode.Append))
                        Write(logPath + "Ticket\\" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ": " + JsonConvert.SerializeObject(logContent) + "\r\n", FileMode.Append);
                }
            }
            catch (Exception)
            {

            }
        }
        /// <summary>
        /// 存在Ticket文件夹时记录日志
        /// </summary>
        /// <param name="logContent"></param>
        public void LogIfExistTicketInfoDirectory(string logContent)
        {
            lock (lockObject)
            {
                if (!Directory.Exists(logPath + "TicketInfo\\")) return;
                if (!Write(logPath + "TicketInfo\\" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ": " + logContent + "\r\n", FileMode.Append))
                    Write(logPath + "TicketInfo\\" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ": " + logContent + "\r\n", FileMode.Append);
            }
        }
        /// <summary>
        /// 记录http请求
        /// </summary>
        /// <param name="logContent"></param>
        public void LogHttpRequest(string logContent)
        {
            lock (lockObject)
            {
                if (!Directory.Exists(logPath + "Http\\")) return;
                if (!Write(logPath + "Http\\" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ": " + logContent + "\r\n", FileMode.Append))
                    Write(logPath + "Http\\" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ": " + logContent + "\r\n", FileMode.Append);
            }
        }
        /// <summary>
        /// 记录http请求
        /// </summary>
        /// <param name="logContent"></param>
        public void LogHttpRequest(string logTitle, object logContent)
        {
            try
            {
                if (!Directory.Exists(logPath + "Http\\")) return;
                var content = logTitle + JsonConvert.SerializeObject(logContent);
                lock (lockObject)
                {
                    if (!Write(logPath + "Http\\" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ": " + content + "\r\n", FileMode.Append))
                        Write(logPath + "Http\\" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ": " + content + "\r\n", FileMode.Append);
                }
            }
            catch (Exception)
            {

            }
        }

        public bool Write(string fileName, string content, FileMode fileMode = FileMode.Open)
        {
            // 判断文件是否存在，不存在则创建，否则读取值显示到窗体
            try
            {
                StreamWriter sw = null;
                FileStream fs = null;
                if (!File.Exists(fileName))
                {
                    var files = new DirectoryInfo(logPath).GetFiles("*.txt");
                    if (files.Count() > 5)
                    {
                        var fileNames = files.Select(f => f.Name).OrderByDescending(f => f).ToArray();
                        for (int i = 5; i < fileNames.Count(); i++)
                            File.Delete(logPath + fileNames[i]);//只保留前五天日志
                    }
                    fs = new FileStream(fileName, FileMode.Create, FileAccess.Write);//创建写入文件 
                }
                else
                {
                    fs = new FileStream(fileName, fileMode, FileAccess.Write);//创建写入文件               
                }
                sw = new StreamWriter(fs);
                sw.WriteLine(content);//开始写入值
                sw.Flush();
                sw.Close();
                sw.Dispose();
                fs.Close();
                fs.Dispose();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public string Read(string fileName)
        {
            if (!File.Exists(logPath + fileName))
                return null;
            using (var fs = new FileStream(logPath + fileName, FileMode.Open, FileAccess.Read))//创建读文件 
            {
                StreamReader sr = new StreamReader(fs);
                string config = sr.ReadToEnd();
                sr.Close();
                fs.Close();
                return config;
            }
        }

        /// <summary>
        /// 写配置文件
        /// </summary>
        /// <param name="configName"></param>
        /// <param name="configContent"></param>
        public void WriteConfig(string configName, string configContent)
        {
            var fileName = AppDomain.CurrentDomain.BaseDirectory + configName + ".txt";
            try
            {
                StreamWriter sw = null;
                FileStream fs = null;
                fs = new FileStream(fileName, FileMode.Create, FileAccess.Write);//创建写入文件         
                sw = new StreamWriter(fs);
                sw.WriteLine(configContent);//开始写入值
                sw.Flush();
                sw.Close();
                sw.Dispose();
                fs.Close();
                fs.Dispose();
            }
            catch (Exception)
            {
            }
        }
        /// <summary>
        /// 读配置文件
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public string ReadConfig(string configName)
        {
            var fileName = AppDomain.CurrentDomain.BaseDirectory + configName + ".txt";
            if (!File.Exists(fileName))
                return null;
            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))//创建读文件 
            {
                StreamReader sr = new StreamReader(fs);
                string config = sr.ReadToEnd();
                sr.Close();
                fs.Close();
                return config.Trim();
            }
        }
        /// <summary>
        /// 读配置文件
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public string ReadConfig(string path, string configName)
        {
            var fileName = path + "\\" + configName + ".txt";
            if (!File.Exists(fileName))
            {
                Log(string.Format("文件:{0}不存在!,路径:{1}", configName, fileName));
                return null;
            }
            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))//创建读文件 
            {
                StreamReader sr = new StreamReader(fs);
                string config = sr.ReadToEnd();
                sr.Close();
                fs.Close();
                Log(string.Format("文件:{0} 内容:{1}", configName, config));
                return config.Trim();
            }
        }
        public string ReadFile( string fullFileName)
        {
            if (!File.Exists(fullFileName))
            {
                Log($"文件:{fullFileName}不存在!");
                return null;
            }
            using (var fs = new FileStream(fullFileName, FileMode.Open, FileAccess.Read))//创建读文件 
            {
                StreamReader sr = new StreamReader(fs);
                string config = sr.ReadToEnd();
                sr.Close();
                fs.Close();
                return config.Trim();
            }
        }
    }
}
