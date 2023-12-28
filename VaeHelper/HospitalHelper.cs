using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using VaeEntity.Hospital;
using System.Web;
using Microsoft.Extensions.Logging;
using System.Xml.Linq;
using Spire.Pdf;

namespace VaeHelper
{
    public class HospitalHelper
    {
        private ProxyHelper _proxyHelper;
        protected readonly ILogger _logger;
        private List<string> _unconcernedProducts;
        public HospitalHelper(ILoggerFactory logger, ProxyHelper proxyHelper)
        {
            _logger = logger.CreateLogger(this.GetType());
            _proxyHelper = proxyHelper;
            var token = GetApiToken(_proxyHelper.GetProxy("us")).Result;
            if (!string.IsNullOrEmpty(token)) _bearer = KeyValuePair.Create(DateTime.Now, token);
            var content = LogHelper.Instance.ReadFile($"{AppDomain.CurrentDomain.BaseDirectory}/Config/Unconcerned.txt");
            _unconcernedProducts = content.Split('\n').Select(p => p.Trim()).Where(p => !string.IsNullOrWhiteSpace(p)).Distinct().ToList();
        }
        private const string _url = "https://newapp.sysucc.org.cn:9017";
        //private const string _token = "584be464eab04c7382ddf1f98b51401a";
        //private const string _serviceKey = "839a1b4d0c6a4cabad474b8a4df46218";
        private const string _token = "3c3ec922b96e4352ab9c375f07eaa289";
        //private const string _serviceKey = "5b58accd9054465992850954674ae200";

        public async Task<JObject> Login(string loginId, string password, IWebProxy webProxy = null)
        {
            var sign = GetSginLogin();
            var url = $"{_url}/api/User/Login?loginId={loginId}&password={password}&deviceId=&appCode=d07e85e3&appVersion=1&timestamp={sign.tiemstamp}&sig={sign.sign}";
            var result = await HttpHelper.Get(url, webProxy: webProxy);
            if (result.Error || string.IsNullOrEmpty(result.ResponseString)) return null;
            var dataObject = JObject.Parse(result.ResponseString);
            return dataObject;
        }

        public async Task<Dictionary<string, string>> QuerySchema(IWebProxy webProxy = null)
        {
            var sign = GetSgin();
            var request = $"sig={sign.sign}&token={_token}&timestamp={sign.tiemstamp}";
            var url = $"{_url}/api/Patient/QuerySchema?{request}";
            var result = await HttpHelper.Get(url, GetHeader(request), webProxy: webProxy);
            if (result.Error || string.IsNullOrEmpty(result.ResponseString)) return null;
            var dataObject = JObject.Parse(result.ResponseString);
            if ((int)dataObject.SelectToken("msgCode") != 0) return null;
            return dataObject.SelectToken("data").ToDictionary(d => d.SelectToken("id").ToString(),
                d => $"{DateTimeHelper.ConvertLongToDateTime((long)d["seeDate"])}:{d["deptName"]} {d["noodCode"]}");
        }

        public async Task<List<Patient>> QuerySchemaInpatientList(string schemaId, IWebProxy webProxy = null, string token = null, Action<string> logAction = null)
        {
            var sign = GetSgin();
            var request = $"sig={sign.sign}&token={(token ?? _token)}&timestamp={sign.tiemstamp}&SchemaId={schemaId}";
            var url = $"{_url}/api/Patient/QuerySchemaInpatient?{request}";
            var result = await HttpHelper.Get(url, GetHeader(request), timeOut: (int)TimeSpan.FromSeconds(60).TotalMilliseconds, webProxy: webProxy);
            //if (result.Error || string.IsNullOrEmpty(result.ResponseString))
            //{
            //    logAction?.Invoke(string.IsNullOrEmpty(result.ResponseString) ? result.ErrorResponse : result.ResponseString);
            //    return null;
            //}

            if (result.Error || string.IsNullOrEmpty(result.ResponseString))
            {
                _logger.LogInformation($"QuerySchemaInpatientList 查询结果异常 result{schemaId}={result.Error}");
                return null;
            }

            //var str = result.ResponseString;
            //if (string.IsNullOrEmpty(result.ResponseString)) str = string.Empty;
            //_logger.LogInformation($"QuerySchemaInpatientList 查询结果 result{schemaId}={str}");


            var dataObject = JObject.Parse(result.ResponseString);
            if ((int)dataObject.SelectToken("msgCode") != 0) return new List<Patient>();
            var patients = new List<Patient>();
            foreach (JObject data in dataObject.SelectToken("data"))
            {
                var patient = JsonConvert.DeserializeObject<Patient>(data.ToString());
                patient.DeptName = data.SelectToken("regDept").ToString();
                var seeBeginTime = DateTimeHelper.ConvertStringToDateTime(data.SelectToken("seeBeginTime").ToString());
                var seeEndTime = DateTimeHelper.ConvertStringToDateTime(data.SelectToken("seeEndTime").ToString());
                patient.InDate = $"{seeBeginTime:yyyy-MM-dd HH:mm}~{seeEndTime:HH:mm}";
                patient.InDateFormate = seeBeginTime;
                if (!string.IsNullOrEmpty(patient.OutDate))
                    patient.OutDate = DateTimeHelper.ConvertStringToDateTime(patient.OutDate).ToString("yyyy-MM-dd HH:mm");
                patient.Id = data.SelectToken("outPatientNo").ToString();
                patient.InPatientId= data.SelectToken("patientNo").ToString();
                patient.SchemaId = schemaId;
                patient.PatientContent = data.ToString();
                patient.CreateTime = DateTime.Now;
                patient.UpdateTime = DateTime.Now;
                patient.MedicationFailed = true;
                patients.Add(patient);
            }
            return patients;
        }

        public async Task<List<OutPatientMedication>> QueryOutPatientPrescript(string visitId, string outPatientId, IWebProxy webProxy = null, string token = null)
        {
            //outPatientId=patient.Id
            var sign = GetSgin();
            var request = $"sig={sign.sign}&token={(token ?? _token)}&timestamp={sign.tiemstamp}&patientId={visitId}&outPatientId={outPatientId}";
            var url = $"{_url}/api/Patient/QueryOutPatientPrescript?{request}";
            var result = await HttpHelper.Get(url, GetHeader(request), timeOut: (int)TimeSpan.FromSeconds(60).TotalMilliseconds, webProxy: webProxy);
                       

            if (result.Error || string.IsNullOrEmpty(result.ResponseString))
            {
                return null;
            }

            //var str = result.ResponseString;
            //if (string.IsNullOrEmpty(result.ResponseString)) str = string.Empty;
            //_logger.LogInformation($"QueryOutPatientPrescript 查询结果 result={str}");

            var dataObject = JObject.Parse(result.ResponseString);
            if ((int)dataObject.SelectToken("msgCode") != 0) return null;
            var medications = new List<OutPatientMedication>();
            foreach (JObject data in dataObject.SelectToken("data"))
            {
                var medication = JsonConvert.DeserializeObject<OutPatientMedication>(data.ToString());
                if (_unconcernedProducts.Contains(medication.Name)) continue;
                if (!string.IsNullOrEmpty(medication.BeginDateTime))
                    medication.BeginDateTime = DateTimeHelper.ConvertStringToDateTime(medication.BeginDateTime).ToString("yyyy-MM-dd HH:mm");
                medication.PatientId = outPatientId;
                medication.CreateTime = DateTime.Now;
                medications.Add(medication);
            }
            return medications;
        }
        public async Task<JToken> GetInPatientEmrPDFList(string inPatientId, IWebProxy webProxy = null)
        {
            var sign = GetSginLogin();
            var request = $"inPatientId={inPatientId}&sig={sign.sign}&token={_token}&timestamp={sign.tiemstamp}";
            var url = $"{_url}/api/Patient/GetInPatientEmrPDFList?{request}";
            var result = await HttpHelper.Get(url, GetHeader(request), timeOut: (int)TimeSpan.FromMinutes(1).TotalMilliseconds, webProxy: webProxy);
           

            if (result.Error || string.IsNullOrEmpty(result.ResponseString))
            {
                _logger.LogInformation($"QueryLongTermOrderList 查询结果异常 result{inPatientId}={result.Error}");
                return null;
            }

            //var str = result.ResponseString;
            //if (string.IsNullOrEmpty(result.ResponseString)) str = string.Empty;
            //_logger.LogInformation($"GetInPatientEmrPDFList 查询结果 result{inPatientId}={str}");


            var dataObject = JObject.Parse(result.ResponseString);
            if ((int)dataObject.SelectToken("msgCode") != 0) return null;
            return dataObject.SelectToken("data");
        }

        public async Task<string> GetEmrPDFDetail(string documentNo, IWebProxy webProxy = null)
        {
            var sign = GetSginLogin();
            var request = $"documentNo={documentNo}&sig={sign.sign}&token={_token}&timestamp={sign.tiemstamp}";
            var url = $"{_url}/api/Patient/GetEmrPDFDetail?{request}";
            var result = await HttpHelper.Get(url, GetHeader(request), webProxy: webProxy, timeOut: (int)TimeSpan.FromMinutes(1).TotalMilliseconds);
            if (result.Error || string.IsNullOrEmpty(result.ResponseString)) return null;

            if (result.Error || string.IsNullOrEmpty(result.ResponseString))
            {
                _logger.LogInformation($"GetEmrPDFDetail 查询结果异常 result{documentNo}={result.Error}");
                return null;
            }

            //var str = result.ResponseString;
            //if (string.IsNullOrEmpty(result.ResponseString)) str = string.Empty;
            //_logger.LogInformation($"GetEmrPDFDetail 查询结果 result{documentNo}={str}");


            var dataObject = JObject.Parse(result.ResponseString);
            if ((int)dataObject.SelectToken("msgCode") != 0) return null;
            return dataObject.SelectToken("data.pdfUrl")?.ToString();
        }

        public async Task<string> GetEmrPDFDetailStr(string documentNo, IWebProxy webProxy = null)
        {
            var sign = GetSginLogin();
            var request = $"documentNo={documentNo}&sig={sign.sign}&token={_token}&timestamp={sign.tiemstamp}";
            var url = $"{_url}/api/Patient/GetEmrPDFDetail?{request}";
            var result = await HttpHelper.Get(url, GetHeader(request), webProxy: webProxy, timeOut: (int)TimeSpan.FromMinutes(1).TotalMilliseconds);
            if (result.Error || string.IsNullOrEmpty(result.ResponseString)) return null;

            if (result.Error || string.IsNullOrEmpty(result.ResponseString))
            {
                _logger.LogInformation($"GetEmrPDFDetail 查询结果异常 result{documentNo}={result.Error}");
                return null;
            }

            //var str = result.ResponseString;
            //if (string.IsNullOrEmpty(result.ResponseString)) str = string.Empty;
            //_logger.LogInformation($"GetEmrPDFDetail 查询结果 result{documentNo}={str}");


            var dataObject = JObject.Parse(result.ResponseString);
            if ((int)dataObject.SelectToken("msgCode") != 0) return null;
            var pdfUrl = dataObject.SelectToken("data.pdfUrl")?.ToString();

            StringBuilder content = new StringBuilder();
            var pdfBytes = HttpHelper.HttpDownload(pdfUrl, webProxy);

            PdfDocument doc = new PdfDocument();
            doc.LoadFromBytes(pdfBytes);

            foreach (PdfPageBase page in doc.Pages)
                content.Append(page.ExtractText());

            //var lines = content.ToString().Split(Environment.NewLine);

            _logger.LogInformation($"GetEmrPDFDetail 查询结果 result{documentNo}={content.ToString()}");

            return content.ToString();
        }


        public async Task<Dictionary<string, string>> QueryDeptList(string token = null, IWebProxy webProxy = null)
        {
            var sign = GetSgin();
            var request = $"sig={sign.sign}&token={(token ?? _token)}&timestamp={sign.tiemstamp}";
            var url = $"{_url}/api/User/QueryLoginDeptList?{request}";
            var result = await HttpHelper.Get(url, GetHeader(request), webProxy: webProxy);
            if (result.Error || string.IsNullOrEmpty(result.ResponseString)) return null;
            var dataObject = JObject.Parse(result.ResponseString);
            if ((int)dataObject.SelectToken("msgCode") != 0) return null;
            return dataObject.SelectToken("data").ToDictionary(d => d.SelectToken("id").ToString(), d => d.SelectToken("name").ToString());
        }
        public async Task<List<Patient>> QueryDeptPatientList(string deptId, IWebProxy webProxy = null, string token = null)
        {
            var sign = GetSgin();
            var request = $"sig={sign.sign}&token={(token ?? _token)}&timestamp={sign.tiemstamp}&deptId={deptId}";
            var url = $"{_url}/api/Patient/QueryDeptPatientList?{request}";
            var result = await HttpHelper.Get(url, GetHeader(request), timeOut: (int)TimeSpan.FromSeconds(60).TotalMilliseconds, webProxy: webProxy);
            if (result.Error || string.IsNullOrEmpty(result.ResponseString)) return null;
            var dataObject = JObject.Parse(result.ResponseString);
            if ((int)dataObject.SelectToken("msgCode") != 0) return null;
            var patients = new List<Patient>();
            foreach (JObject data in dataObject.SelectToken("data"))
            {
                var patient = JsonConvert.DeserializeObject<Patient>(data.ToString());
                if (!string.IsNullOrEmpty(patient.InDate))
                    patient.InDate = DateTimeHelper.ConvertStringToDateTime(patient.InDate).ToString("yyyy-MM-dd HH:mm");
                if (!string.IsNullOrEmpty(patient.OutDate))
                    patient.OutDate = DateTimeHelper.ConvertStringToDateTime(patient.OutDate).ToString("yyyy-MM-dd HH:mm");
                patient.DeptId = deptId;
                if (data["bedId"] != null)
                    patient.Remark = data["bedId"].ToString();
                patients.Add(patient);
            }
            return patients;
        }
        public async Task<JToken> GetPatient(string patientId, IWebProxy webProxy = null, string token = null)
        {
           
            var sign = GetSgin();
            var request = $"sig={sign.sign}&token={(token ?? _token)}&timestamp={sign.tiemstamp}&patientId={patientId}";
            var url = $"{_url}/api/Patient/GetPatient?{request}";
            var result = await HttpHelper.Get(url, GetHeader(request), timeOut: (int)TimeSpan.FromSeconds(60).TotalMilliseconds, webProxy: webProxy);
            if (result.Error || string.IsNullOrEmpty(result.ResponseString))
            {
                _logger.LogInformation($"GetPatient patientId({patientId})查询结果异常： result.Error || string.IsNullOrEmpty(result.ResponseString)");
                return null;
            }

            //var str = result.ResponseString;
            //if (string.IsNullOrEmpty(result.ResponseString)) str = string.Empty;
            //_logger.LogInformation($"GetPatient patientId({patientId})查询结果 result={str}");

            var dataObject = JObject.Parse(result.ResponseString);
            if ((int)dataObject.SelectToken("msgCode") != 0) return null;
            return dataObject.SelectToken("data");
        }


        public async Task<List<Patient>> QueryMyMarkPatientDetail(string visitId, IWebProxy webProxy = null, string token = null)
        {
            //UserInfo 

            //_logger.LogInformation($"QueryMyMarkPatientDetail **************开始****************");

            var sign = GetSgin();
            var request = $"sig={sign.sign}&token={(token ?? _token)}&timestamp={sign.tiemstamp}&patientId={visitId}";
            var url = $"{_url}/api/Patient/QueryMyMarkPatientDetail?{request}";
            var result = await HttpHelper.Get(url, GetHeader(request), timeOut: (int)TimeSpan.FromSeconds(60).TotalMilliseconds, webProxy: webProxy);

            if (result.Error || string.IsNullOrEmpty(result.ResponseString)) return null;

            //var str = result.ResponseString;
            //if (string.IsNullOrEmpty(result.ResponseString)) str = string.Empty;
            //_logger.LogInformation($"QueryMyMarkPatientDetail visitId({visitId})查询结果 result={str}");


            var dataObject = JObject.Parse(result.ResponseString);
            if ((int)dataObject.SelectToken("msgCode") != 0) return null;
            var patients = new List<Patient>();
            foreach (JObject data in dataObject.SelectToken("data"))
            {
                var patient = JsonConvert.DeserializeObject<Patient>(data.ToString());
                if (!string.IsNullOrEmpty(patient.InDate))
                {
                    patient.InDateFormate = DateTimeHelper.ConvertStringToDateTime(patient.InDate);
                    patient.InDate = patient.InDateFormate?.ToString("yyyy-MM-dd HH:mm");
                }

                if (!string.IsNullOrEmpty(patient.OutDate))
                {
                    if (string.IsNullOrEmpty(patient.InDate))
                    {
                        patient.InDateFormate = DateTimeHelper.ConvertStringToDateTime(patient.OutDate);
                        patient.InDate = patient.InDateFormate?.ToString("yyyy-MM-dd HH:mm");
                    }
                    patient.OutDate = DateTimeHelper.ConvertStringToDateTime(patient.OutDate).ToString("yyyy-MM-dd HH:mm");
                }

                patient.PatientType = data["patientType"].ToString() == "outpatient" ? PatientType.outpatient : PatientType.inpatient;

                if (patient.PatientType == PatientType.outpatient)
                {
                    patient.Id = data["outPatientNo"]?.ToString();
                    patient.ChiefDoctName = data["docId"]?.ToString();
                }

                List<string> rl = new List<string>();
                ////gender age homeAddress homeTel linkManTel
                if (data["name"] != null) rl.Add($"{data["name"]}");

                List<string> l = new List<string>();
                if (data["linkManTel"] != null) l.Add($"{data["linkManTel"]}");
                if (data["homeTel"] != null) l.Add($"{data["homeTel"]}");
                if (l.Count > 0) rl.Add(string.Join(",", l.ToArray()));

                if (data["gender"] != null) rl.Add($"{data["gender"]}");
                if (data["age"] != null) rl.Add($"{data["age"]}");
                if (data["homeAddress"] != null) rl.Add($"{data["homeAddress"]}");
                //if (data["bedId"] != null)
                //{
                //    if (!string.IsNullOrEmpty(data["bedId"].ToString()))   rl.Add($"[{data["bedId"]}]");
                //}

                patient.Remark = $"{string.Join(" ",rl.ToArray())}";

               //if (data["bedId"] != null)
               //patient.Remark = data["bedId"].ToString();

                patients.Add(patient);
            }
            return patients;
        }
        public async Task<List<Medication>> QueryLongTermOrderList(string inPatientNo, IWebProxy webProxy = null, string token = null)
        {
            var sign = GetSgin();
            var request = $"sig={sign.sign}&token={(token ?? _token)}&timestamp={sign.tiemstamp}&InPatientNo={inPatientNo}";
            var url = $"{_url}/api/Patient/QueryLongTermOrderListNew?{request}";
            var result = await HttpHelper.Get(url, GetHeader(request), timeOut: (int)TimeSpan.FromSeconds(10).TotalMilliseconds, webProxy: webProxy);

            if (result.Error || string.IsNullOrEmpty(result.ResponseString))
            {
                _logger.LogInformation($"QueryLongTermOrderList 查询结果异常 result{inPatientNo}={result.Error}");
                return null;
            }

            //var str = result.ResponseString;
            //if (string.IsNullOrEmpty(result.ResponseString)) str = string.Empty;
            //_logger.LogInformation($"QueryLongTermOrderList 查询结果 result{inPatientNo}={str}");

            var dataObject = JObject.Parse(result.ResponseString);
            if ((int)dataObject.SelectToken("msgCode") != 0) return null;
            var medications = new List<Medication>();
            foreach (JObject data in dataObject.SelectToken("data"))
            {
                var medication = JsonConvert.DeserializeObject<Medication>(data.ToString());
                if (_unconcernedProducts.Contains(medication.Name)) continue;
                medication.PatientId = inPatientNo;
                if (!string.IsNullOrEmpty(medication.PlaceDateTime))
                    medication.PlaceDateTime = DateTimeHelper.ConvertStringToDateTime(medication.PlaceDateTime).ToString("yyyy-MM-dd HH:mm");
                if (!string.IsNullOrEmpty(medication.BeginDateTime))
                    medication.BeginDateTime = DateTimeHelper.ConvertStringToDateTime(medication.BeginDateTime).ToString("yyyy-MM-dd HH:mm");
                if (!string.IsNullOrEmpty(medication.EndDateTime))
                    medication.EndDateTime = DateTimeHelper.ConvertStringToDateTime(medication.EndDateTime).ToString("yyyy-MM-dd HH:mm");
                medication.CreateTime = DateTime.Now;
                medications.Add(medication);
            }

            return medications;
        }
        public async Task<JObject> QueryInpatientInfoForDoc(string inPatientNo, IWebProxy webProxy = null)
        {
            var sign = GetSgin();
            var request = $"sig={sign.sign}&token={_token}&timestamp={sign.tiemstamp}&InPatientNo={inPatientNo}";
            var url = $"{_url}/api/Patient/QueryInpatientInfoForDoc?{request}";
            var result = await HttpHelper.Get(url, GetHeader(request), timeOut: (int)TimeSpan.FromSeconds(10).TotalMilliseconds, webProxy: webProxy);
            if (result.Error || string.IsNullOrEmpty(result.ResponseString))
            {
                _logger.LogInformation($"QueryInpatientInfoForDoc 无查询结果 result{inPatientNo}={result.Error}");
                return null;
            }

            //var str = result.ResponseString;
            //if (string.IsNullOrEmpty(result.ResponseString)) str = string.Empty;
            //_logger.LogInformation($"QueryInpatientInfoForDoc 查询结果 result{inPatientNo}={str}");

            var dataObject = JObject.Parse(result.ResponseString);
            if ((int)dataObject.SelectToken("msgCode") != 0) return null;
            return dataObject;
        }

        public async Task<List<TempMedication>> QueryTempOrderList(string inPatientNo, IWebProxy webProxy = null, string token = null)
        {
            //inPatientNo = patient.id
            var sign = GetSgin();
            var request = $"sig={sign.sign}&token={token ?? _token}&timestamp={sign.tiemstamp}&InPatientNo={inPatientNo}";
            var url = $"{_url}/api/Patient/QueryTempOrderListNew?{request}";
            var result = await HttpHelper.Get(url, GetHeader(request), timeOut: (int)TimeSpan.FromSeconds(60).TotalMilliseconds, webProxy: webProxy);

            if (result.Error || string.IsNullOrEmpty(result.ResponseString))
            {
                _logger.LogInformation($"QueryTempOrderList 无查询结果 result{inPatientNo}={result.Error}");
                return null;
            }

            //var str = result.ResponseString;
            //if (string.IsNullOrEmpty(result.ResponseString)) str = string.Empty;
            //_logger.LogInformation($"QueryTempOrderList 查询结果 result{inPatientNo}={str}");


            var dataObject = JObject.Parse(result.ResponseString);
            if ((int)dataObject.SelectToken("msgCode") != 0) return null;
            var medications = new List<TempMedication>();
            foreach (JObject data in dataObject.SelectToken("data"))
            {
                var medication = JsonConvert.DeserializeObject<TempMedication>(data.ToString());
                if (_unconcernedProducts.Contains(medication.Name)) continue;
                medication.PatientId = inPatientNo;
                if (!string.IsNullOrEmpty(medication.PlaceDateTime))
                    medication.PlaceDateTime = DateTimeHelper.ConvertStringToDateTime(medication.PlaceDateTime).ToString("yyyy-MM-dd HH:mm");
                if (!string.IsNullOrEmpty(medication.BeginDateTime))
                    medication.BeginDateTime = DateTimeHelper.ConvertStringToDateTime(medication.BeginDateTime).ToString("yyyy-MM-dd HH:mm");
                if (!string.IsNullOrEmpty(medication.EndDateTime))
                    medication.EndDateTime = DateTimeHelper.ConvertStringToDateTime(medication.EndDateTime).ToString("yyyy-MM-dd HH:mm");
                medication.CreateTime = DateTime.Now;
                medications.Add(medication);
            }
            return medications;
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
        static string _privateKey = "MIICeAIBADANBgkqhkiG9w0BAQEFAASCAmIwggJeAgEAAoGBAMp7CqQznzhyJKuguw9CB8ePUjSw58Xas0UPuH/ujpTRD6+NhziBBToeUosXd8Bltnp5btTnmaVJtJiU6kT2z0oEEjV38z6pJKpBVk6YfKhyxVriZfo4VUro3BurCR3hvdZEOjmTKPYWqQPW24fHWr7ekAcWRzDJmkEyC3Xd+Nz5AgMBAAECgYBkkxNpFn8rAX93hMIFxS2qEWWq6Ihnvcc4MaPaX/uQrfuVnr4g8e1PvgoQLtr7xUoLsc+8j0HBWUgMVkO7d8DkR3JxcKY2znc+hpR8pm35gphlfr/cK2fKYUtcDh8fQe9RXosB0cOEU8qku7axTPgXU4DF6AHJ1Z6mu8v1UceJlQJBAPDOMOGd9iPllfWNTIpWnhRoUepfZV26+TTf4rmBNYPB3Q3+8q4ukgmUQGm9gQEzcRIugguCjgavP0rfWVzIW+8CQQDXQccMyO/e+7UmUg3KBl1Ifnz6mC+V+R9fRnTvVd2NtPtcHilDqsQf1lOAZ5JHJzMIO9r1hY8hIEaryPnBWI2XAkEAs/Ri8zhoyUq5VlfR60/vsrCDBvNjfThNJAZsGNVGeMBXwi3UjfafjCkOOfc0gQFAbqXy6UcXfEdatUlULtJMYQJBAKfBoKIrGx35pqTVv99ZdGuiAD7ASh2kSDnVTB6WGZNtn5OcAea1eCGjw/HHRe3j89aP50X/L5vObqVEfLidDcMCQQC+GSQyKtmQZBVV65unsS4H1RK6ZDgAIhd5FXjk1Zu4BVoxGfFUZjVmuAD/MWyyKHybGwTakXDg9uPGf5AKZ0rr";
        private KeyValuePair<DateTime, string> _bearer;
        public WebHeaderCollection GetHeader(string request)
        {
            var headers = new WebHeaderCollection();
            var randomNum = new Random(Guid.NewGuid().GetHashCode()).Next(0, 100).ToString();
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            headers.Add("RandomNum", randomNum);
            headers.Add("UserName", "zszl@sysucc.org.cn");
            headers.Add("TimeStamp", timestamp);
            if (_bearer.Key == default || string.IsNullOrEmpty(_bearer.Value) || _bearer.Key.AddDays(14) < DateTime.Now)
            {
                var token = GetApiToken(_proxyHelper.GetProxy()).Result;
                if (!string.IsNullOrEmpty(token)) _bearer = KeyValuePair.Create(DateTime.Now, token);
            }
            var bearer = _bearer.Value;
            headers.Add("Authorization", $"Bearer {bearer}");
            var signContent = $"\"zszl@sysucc.org.cn\"|\"{bearer}\"|\"{timestamp}\"|\"{randomNum}\"|\"requestBODY{HttpUtility.UrlDecode(request)}\"";
            var sign = SHA256WithRSAHelper.Sign(signContent, _privateKey);

            headers.Add("Sign", sign);
            return headers;
        }
        public (string sign, long tiemstamp) GetSgin()
        {
            var timestamp = DateTimeHelper.ConvertDateTimeToLong(DateTime.Now);
            var sign = $"6e7b872de1754dfeb9d979ebb7e77e3a{Guid.NewGuid().ToString("N")}{timestamp}";
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
