using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using VaeEntity.Hospital;

namespace VaeHelper
{
    public class PatientHelper
    {
        private ProxyHelper _proxyHelper;
        protected readonly ILogger _logger;

        public PatientHelper(ILoggerFactory logger, ProxyHelper proxyHelper)
        {
            _logger = logger.CreateLogger(this.GetType());
            _proxyHelper = proxyHelper;
            var token = GetApiToken(_proxyHelper.GetProxy("us")).Result;
            if (!string.IsNullOrEmpty(token)) _bearer = KeyValuePair.Create(DateTime.Now, token);
        }
        private const string _patientUrl = "https://newapp.sysucc.org.cn:9015";
        static string _privateKey = "MIICeAIBADANBgkqhkiG9w0BAQEFAASCAmIwggJeAgEAAoGBAMp7CqQznzhyJKuguw9CB8ePUjSw58Xas0UPuH/ujpTRD6+NhziBBToeUosXd8Bltnp5btTnmaVJtJiU6kT2z0oEEjV38z6pJKpBVk6YfKhyxVriZfo4VUro3BurCR3hvdZEOjmTKPYWqQPW24fHWr7ekAcWRzDJmkEyC3Xd+Nz5AgMBAAECgYBkkxNpFn8rAX93hMIFxS2qEWWq6Ihnvcc4MaPaX/uQrfuVnr4g8e1PvgoQLtr7xUoLsc+8j0HBWUgMVkO7d8DkR3JxcKY2znc+hpR8pm35gphlfr/cK2fKYUtcDh8fQe9RXosB0cOEU8qku7axTPgXU4DF6AHJ1Z6mu8v1UceJlQJBAPDOMOGd9iPllfWNTIpWnhRoUepfZV26+TTf4rmBNYPB3Q3+8q4ukgmUQGm9gQEzcRIugguCjgavP0rfWVzIW+8CQQDXQccMyO/e+7UmUg3KBl1Ifnz6mC+V+R9fRnTvVd2NtPtcHilDqsQf1lOAZ5JHJzMIO9r1hY8hIEaryPnBWI2XAkEAs/Ri8zhoyUq5VlfR60/vsrCDBvNjfThNJAZsGNVGeMBXwi3UjfafjCkOOfc0gQFAbqXy6UcXfEdatUlULtJMYQJBAKfBoKIrGx35pqTVv99ZdGuiAD7ASh2kSDnVTB6WGZNtn5OcAea1eCGjw/HHRe3j89aP50X/L5vObqVEfLidDcMCQQC+GSQyKtmQZBVV65unsS4H1RK6ZDgAIhd5FXjk1Zu4BVoxGfFUZjVmuAD/MWyyKHybGwTakXDg9uPGf5AKZ0rr";
        private KeyValuePair<DateTime, string> _bearer;
        public WebHeaderCollection GetPatientHeader(string request)
        {
            if (_bearer.Key == default || string.IsNullOrEmpty(_bearer.Value) || _bearer.Key.AddDays(14) < DateTime.Now)
            {
                var token = GetApiToken(_proxyHelper.GetProxy()).Result;
                if (!string.IsNullOrEmpty(token)) _bearer = KeyValuePair.Create(DateTime.Now, token);
            }
            var headers = new WebHeaderCollection();
            var timestamp = DateTimeHelper.ConvertDateTimeToLong(DateTime.Now);
            var appCode = "0082752c";
            headers.Add("appCode", appCode);
            headers.Add("timestamp", timestamp.ToString());
            var nonce = Guid.NewGuid().ToString();
            var startIndex = (int)(timestamp % 10);
            var substring = nonce.Substring(startIndex, nonce.Length - startIndex);
            headers.Add("nonce", nonce);
            headers.Add("appVersion", "4.0.0");
            var signContent = $"123456{timestamp}{nonce}{substring}{request}";
            var sign = SHA256WithRSAHelper.M1050B(signContent);
            headers.Add("signature", sign);
            headers.Add("Content-Type", "application/json");
            return headers;
        }
        public async Task<string> GetApiToken(IWebProxy webProxy = null)
        {
            try
            {
                var random = new Random(Guid.NewGuid().GetHashCode()).Next(1, 100);
                var dateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                var url = $"https://newapp.sysucc.org.cn:9032/Token";
                var password = HttpUtility.UrlEncode("Password@123");
                var username = HttpUtility.UrlEncode("zszl@sysucc.org.cn");
                var signContent = $"zszl@sysucc.org.cn|Password@123|{dateTime}|{random}";
                var sign = SHA256WithRSAHelper.Sign(signContent, _privateKey);
                var request = $"grant_type=password&username={username}&Password={password}&TimeStamp={dateTime}&RandomNum={random}&Sign={HttpUtility.UrlEncode(sign)}";
                var result = await HttpHelper.Post(url, request, contentType: "application/x-www-form-urlencoded",
                    timeOut: (int)TimeSpan.FromMinutes(1).TotalMilliseconds, webProxy: webProxy);
                if (result.Error || string.IsNullOrEmpty(result.ResponseString)) return null;
                var dataObject = JObject.Parse(result.ResponseString);
                return dataObject["access_token"]?.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError($"获取ApiToken失败:{ex.Message},尝试重新获取");
                return await GetApiToken();
            }
        }
        public async Task<JObject> PatientLogin(string phone, string password, IWebProxy webProxy = null)
        {
            var request = new Dictionary<string, string> { { "password", password }, { "deviceId", "" }, { "verifyCodeId", "" }, { "verifyCode", "" }, { "tel", phone } };
            var url = $"{_patientUrl}/h5union/User/Login";
            url = "https://patientcloud.sysucc.org.cn/h5union/user/login";
            var headers = GetPatientHeader(JsonConvert.SerializeObject(request));
            var result = await HttpHelper.Post(url, JsonConvert.SerializeObject(request), headers, webProxy: webProxy, contentType: "application/json");
            if (result.Error || string.IsNullOrEmpty(result.ResponseString)) return null;
            var dataObject = JObject.Parse(result.ResponseString);
            return dataObject;
        }
        public async Task<JToken> QueryFamilyMemberListNew(string token, IWebProxy webProxy = null)
        {
            var sign = GetSginLogin();
            var request = $"token={token}&channelId=app&timestamp={sign.tiemstamp}&sig={sign.sign}";
            var url = $"{_patientUrl}/api/User/QueryFamilyMemberListNew?{request}";
            var result = await HttpHelper.Get(url, GetPatientHeader(request), webProxy: webProxy);
            if (result.Error || string.IsNullOrEmpty(result.ResponseString)) return null;
            var dataObject = JObject.Parse(result.ResponseString);
            return dataObject;
        }
        public async Task<JToken> GetDeptDisease(string orgID, string areaType, IWebProxy webProxy = null)
        {
            var url = $"{_patientUrl}/api/Register/GetDeptDisease?OrgID={orgID}&type={areaType}";
            var result = await HttpHelper.Get(url, timeOut: (int)TimeSpan.FromSeconds(60).TotalMilliseconds, webProxy: webProxy);
            if (result.Error || string.IsNullOrEmpty(result.ResponseString)) return null;
            var dataObject = JObject.Parse(result.ResponseString);
            if ((int)dataObject.SelectToken("msgCode") != 0) return null;
            return dataObject.SelectToken("data");
        }
        private static string _scheduleUrl = "https://appregist.sysucc.org.cn:10005";
        public async Task<JToken> QueryEntityDateList(string entityName, string orgID = "ALL", int newPatientFlag = 1, IWebProxy webProxy = null)
        {
            var request = $"entityName={entityName}&OrgID={orgID}&newPatientFlag={newPatientFlag}";
            var url = $"{_scheduleUrl}/api/Register/QueryEntityDateList?{request}";
            var result = await HttpHelper.Get(url, GetPatientHeader(request), timeOut: (int)TimeSpan.FromSeconds(60).TotalMilliseconds, webProxy: webProxy);
            if (result.Error || string.IsNullOrEmpty(result.ResponseString)) return null;
            var dataObject = JObject.Parse(result.ResponseString);
            if ((int)dataObject.SelectToken("msgCode") != 0) return null;
            return dataObject.SelectToken("data");
        }
        public async Task<JToken> QueryDeptScheduleDateForWXNew(string depId, string orgID, IWebProxy webProxy = null, string token = null)
        {
            var url = "https://patientcloud.sysucc.org.cn/h5union/register/getScheduleTimeList";
            var data = new { appType = "", appId = "", id = depId, type = 1, orgId = orgID, newPatientFlag = 1 };

            var request = JsonConvert.SerializeObject(data);
            var header = GetPatientHeader(request);

            var result = await HttpHelper.Post(url, request, header, timeOut: (int)TimeSpan.FromSeconds(60).TotalMilliseconds, webProxy: webProxy, acceptType: "*/*");
            if (result.Error || string.IsNullOrEmpty(result.ResponseString)) return null;
            var dataObject = JObject.Parse(result.ResponseString);
            if ((int)dataObject.SelectToken("status") != 0) return JToken.Parse("[]");
            return dataObject.SelectToken("data");
        }
        /// <summary>
        /// 按病种查询
        /// </summary>
        /// <param name="date"></param>
        /// <param name="depId"></param>
        /// <param name="entityName"></param>
        /// <param name="orgID"></param>
        /// <param name="newPatientFlag"></param>
        /// <param name="webProxy"></param>
        /// <returns></returns>
        public async Task<JToken> QueryOneDayEntityScheduleList(string date,
                                                                       string depId = null,
                                                                       string entityName = null,
                                                                       string orgID = "ALL",
                                                                       int newPatientFlag = 1,
                                                                       IWebProxy webProxy = null)
        {
            var request = $"entityName={entityName}&date={date}&orgID={orgID}&newPatientFlag={newPatientFlag}";

            var url = $"{_scheduleUrl}/api/Register/QueryOneDayEntityScheduleList?{request}";
            var result = await HttpHelper.Get(url, GetPatientHeader(request), timeOut: (int)TimeSpan.FromSeconds(60).TotalMilliseconds, webProxy: webProxy);
            if (result.Error || string.IsNullOrEmpty(result.ResponseString)) return null;
            var dataObject = JObject.Parse(result.ResponseString);
            if ((int)dataObject.SelectToken("msgCode") != 0) return null;
            return dataObject.SelectToken("data");
        }
        public async Task<JToken> QueryJTokenOneDayScheduleListEntity(string date,
                                                                    string depId = null,
                                                                    string orgID = "01",
                                                                    IWebProxy webProxy = null)
        {
            var sign = GetSginLogin();
            var request = $"depId={depId}&date={date}&timestamp={sign.tiemstamp}&sig={sign.sign}&token=&orgID={orgID}&patientId=&newPatientFlag=";

            var url = $"{_scheduleUrl}/api/Register/QueryOneDayScheduleListEntity?{request}";
            var result = await HttpHelper.Get(url, GetPatientHeader(request), timeOut: (int)TimeSpan.FromSeconds(60).TotalMilliseconds, webProxy: webProxy);
            if (result.Error || string.IsNullOrEmpty(result.ResponseString)) return null;
            var dataObject = JObject.Parse(result.ResponseString);
            return dataObject;
        }
        /// <summary>
        /// 按部门查询
        /// </summary>
        /// <param name="date"></param>
        /// <param name="depId"></param>
        /// <param name="orgID"></param>
        /// <param name="webProxy"></param>
        /// <returns></returns>
        public async Task<List<DoctSchedule>> QueryOneDayScheduleListEntity(string date,
                                                                      string depId = null,
                                                                      string orgID = "01",
                                                                      IWebProxy webProxy = null, string token = "")
        {
            var url = "https://patientcloud.sysucc.org.cn/h5union/register/getDeptScheduleList";
            var request = new { appType = "", appId = "", deptCode = depId, scheduleDate = date, orgId = orgID, newPatientFlag = 1 };
            var requestContent = JsonConvert.SerializeObject(request);
            var result = await HttpHelper.Post(url, requestContent, GetPatientHeader(requestContent),
                timeOut: (int)TimeSpan.FromSeconds(60).TotalMilliseconds, webProxy: webProxy, acceptType: "*/*");
            if (result.Error || string.IsNullOrEmpty(result.ResponseString)) return null;
            var dataObject = JObject.Parse(result.ResponseString);
            if ((int)dataObject.SelectToken("status") != 0) return new List<DoctSchedule>();
            var schedules = new List<DoctSchedule>();
            foreach (var data in dataObject.SelectToken("data.entityDoctorSchedules"))
            {
                var entityName = data["entityName"].ToString();
                foreach (var regDoc in data.SelectToken("doctorSchedules"))
                {
                    var schedule = new DoctSchedule()
                    {
                        RegPointID = regDoc.SelectToken("scheduleDto.schemaId")?.ToString(),
                        DeptId = regDoc.SelectToken("scheduleDto.deptCode")?.ToString(),
                        DeptName = regDoc.SelectToken("scheduleDto.deptName")?.ToString(),
                        DoctId = regDoc.SelectToken("doctorDto.doctorCode")?.ToString(),
                        DoctName = regDoc.SelectToken("doctorDto.doctorName")?.ToString(),
                        EntityName = entityName,
                        NoonCode = regDoc.SelectToken("scheduleDto.noonCode")?.ToString(),
                        OrgId = regDoc.SelectToken("scheduleDto.orgId")?.ToString(),
                        OrgName = regDoc.SelectToken("scheduleDto.orgName")?.ToString(),
                        VisitDate = DateTimeHelper.ConvertStringToDateTime(date),
                        CreateTime = DateTime.Now
                    };
                    schedules.Add(schedule);
                }
            }
            var groupSchedules = schedules.GroupBy(s => s.RegPointID).Select(sg =>
            {
                var schedule = sg.FirstOrDefault();
                schedule.EntityName = string.Join(";", sg.Select(s => s.EntityName));
                return schedule;
            }).ToList();
            return groupSchedules;
        }
        public async Task<JToken> QueryOneDayRegPointListNoDeptNew(string docId,
                                                                   string patientId,
                                                                   long date,
                                                                   string orgID,
                                                                   string token,
                                                                   IWebProxy webProxy = null)
        {
            var sign = GetSginLogin();
            var request = $"docId={docId}&patientId={patientId}&date={date}&timestamp={sign.tiemstamp}&sig={sign.sign}&token={token}&orgID={orgID}";
            var url = $"{_scheduleUrl}/api/Register/QueryOneDayRegPointListNoDeptNew?{request}";
            var result = await HttpHelper.Get(url, GetPatientHeader(request), timeOut: (int)TimeSpan.FromSeconds(60).TotalMilliseconds, webProxy: webProxy);
            if (result.Error || string.IsNullOrEmpty(result.ResponseString)) return null;
            var dataObject = JObject.Parse(result.ResponseString);
            return dataObject;
        }
        public async Task<JToken> OccupyRegPointNew(string patientId,
                                                                   string name,
                                                                   string deptId,
                                                                   string deptName,
                                                                   string docId,
                                                                   string docName,
                                                                   string regPointId,
                                                                   string visitTime,
                                                                   string orgID,
                                                                   string token,
                                                                   IWebProxy webProxy = null)
        {
            var sign = GetSginLogin();
            var request = $"patientId={patientId}&name={name}&deptId={deptId}&deptName={deptName}&docId={docId}" +
                $"&docName={docName}&regPointId={regPointId}&visitTime={visitTime}&timestamp={sign.tiemstamp}&sig={sign.sign}&token={token}&orgID={orgID}";
            var url = $"{_scheduleUrl}/api/Register/OccupyRegPointNew?{request}";
            var result = await HttpHelper.Get(url, GetPatientHeader(request), timeOut: (int)TimeSpan.FromSeconds(60).TotalMilliseconds, webProxy: webProxy);
            if (result.Error || string.IsNullOrEmpty(result.ResponseString)) return null;
            var dataObject = JObject.Parse(result.ResponseString);
            return dataObject;
        }
        public async Task<JToken> QueryAppoitmentListNew(string patientId, string token, IWebProxy webProxy = null)
        {
            try
            {
                var sign = GetSginLogin();
                var request = $"patientId={patientId}&timestamp={sign.tiemstamp}&sig={sign.sign}&token={token}";
                var url = $"{_scheduleUrl}/api/Register/QueryAppoitmentListNew?{request}";
                var result = await HttpHelper.Get(url, GetPatientHeader(request), timeOut: (int)TimeSpan.FromSeconds(60).TotalMilliseconds, webProxy: webProxy);
                if (result.Error || string.IsNullOrEmpty(result.ResponseString)) return null;
                var dataObject = JObject.Parse(result.ResponseString);
                return dataObject;
            }
            catch (Exception)
            {
                return null;
            }
        }
        public async Task<JToken> QueryDocRegPointBySchemaID(string depId, string date, string token, IWebProxy webProxy = null)
        {
            try
            {
                var sign = GetSginLogin();
                var request = $"depId={depId}&date={date}&docId=B00744&schemaId=123&timestamp={sign.tiemstamp}&sig={sign.sign}&token={token}";
                var url = $"http://newapp.sysucc.org.cn:9015/api/Register/QueryDoctorListByDate?{request}";
                var result = await HttpHelper.Get(url, GetPatientHeader(request), timeOut: (int)TimeSpan.FromSeconds(600).TotalMilliseconds, webProxy: webProxy);
                if (result.Error || string.IsNullOrEmpty(result.ResponseString)) return null;
                var dataObject = JObject.Parse(result.ResponseString);
                return dataObject;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public (string sign, long tiemstamp) GetSginLogin()
        {
            var timestamp = DateTimeHelper.ConvertDateTimeToInt(DateTime.Now);
            var sign = $"6e7b872de1754dfeb9d979ebb7e77e3a{timestamp}";
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(sign));
            StringBuilder hex = new StringBuilder(hash.Length * 2);

            foreach (var byteH in hash)
            {
                if ((byteH & 255) < 16)
                {
                    hex.Append(0);
                }
                hex.Append(Convert.ToString(byteH & 255, 16));
            }
            return (hex.ToString(), timestamp);
        }
    }
}
