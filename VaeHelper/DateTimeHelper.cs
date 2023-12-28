using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace VaeHelper
{
    public class DateTimeHelper
    {
        private static DateTime? _beiJingDateTimeStart;
        public static DateTime DateTimeStart
        {
            get
            {
                if (_beiJingDateTimeStart != null) return _beiJingDateTimeStart.Value;
                bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
                if (isWindows)
                {
                    _beiJingDateTimeStart = TimeZoneInfo.ConvertTime(new DateTime(1970, 1, 1), TimeZoneInfo.Utc,
                TimeZoneInfo.FindSystemTimeZoneById("China Standard Time"));
                }
                else
                {
                    _beiJingDateTimeStart = TimeZoneInfo.ConvertTime(new DateTime(1970, 1, 1), TimeZoneInfo.Utc,
               TimeZoneInfo.FindSystemTimeZoneById("Asia/Shanghai"));
                }
                return _beiJingDateTimeStart.Value;
            }
        }
        public static DateTime ConvertStringToDateTime(string timeStamp)
        {
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
            return DateTimeStart.Add(toNow);
        }

        public static long ConvertDateTimeToInt(System.DateTime time)
        {
            long t = (time.Ticks - DateTimeStart.Ticks) / 10000000;   //除10000调整为10位      
            return t;
        }
        public static long ConvertDateTimeToLong(System.DateTime time)
        {
            long t = (time.Ticks - DateTimeStart.Ticks) / 10000;   //除10000调整为13位      
            return t;
        }
        public static DateTime ConvertLongToDateTime(long timeStamp)
        {
            TimeSpan toNow = new TimeSpan(timeStamp * 10000);
            return DateTimeStart.Add(toNow);
        }
    }
}
