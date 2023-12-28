using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VaeHelper;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using Newtonsoft.Json;
using VaeEntity.Hospital;
using Autofac;
using System.Net;
using System.Web;

namespace VaeHelperTests.Hospital
{
    [TestClass]
    public class PatientTest
    {
        private IContainer container;
        private ProxyHelper _proxyHelper;
        private PatientHelper _patientHelper;
        public PatientTest()
        {
            container = TestInitialize.Initialize<HospitalJob.Startup>();
            //_proxyHelper = container.Resolve<ProxyHelper>();
            _patientHelper = container.Resolve<PatientHelper>();

        }
        [TestMethod]
        public void TokenTest()
        {
            var logoin = _patientHelper.GetApiToken().Result;
        }
        static string _privateKey = "MIICeAIBADANBgkqhkiG9w0BAQEFAASCAmIwggJeAgEAAoGBAMp7CqQznzhyJKuguw9CB8ePUjSw58Xas0UPuH/ujpTRD6+NhziBBToeUosXd8Bltnp5btTnmaVJtJiU6kT2z0oEEjV38z6pJKpBVk6YfKhyxVriZfo4VUro3BurCR3hvdZEOjmTKPYWqQPW24fHWr7ekAcWRzDJmkEyC3Xd+Nz5AgMBAAECgYBkkxNpFn8rAX93hMIFxS2qEWWq6Ihnvcc4MaPaX/uQrfuVnr4g8e1PvgoQLtr7xUoLsc+8j0HBWUgMVkO7d8DkR3JxcKY2znc+hpR8pm35gphlfr/cK2fKYUtcDh8fQe9RXosB0cOEU8qku7axTPgXU4DF6AHJ1Z6mu8v1UceJlQJBAPDOMOGd9iPllfWNTIpWnhRoUepfZV26+TTf4rmBNYPB3Q3+8q4ukgmUQGm9gQEzcRIugguCjgavP0rfWVzIW+8CQQDXQccMyO/e+7UmUg3KBl1Ifnz6mC+V+R9fRnTvVd2NtPtcHilDqsQf1lOAZ5JHJzMIO9r1hY8hIEaryPnBWI2XAkEAs/Ri8zhoyUq5VlfR60/vsrCDBvNjfThNJAZsGNVGeMBXwi3UjfafjCkOOfc0gQFAbqXy6UcXfEdatUlULtJMYQJBAKfBoKIrGx35pqTVv99ZdGuiAD7ASh2kSDnVTB6WGZNtn5OcAea1eCGjw/HHRe3j89aP50X/L5vObqVEfLidDcMCQQC+GSQyKtmQZBVV65unsS4H1RK6ZDgAIhd5FXjk1Zu4BVoxGfFUZjVmuAD/MWyyKHybGwTakXDg9uPGf5AKZ0rr";
        [TestMethod]
        public void GetApiToken()
        {
            var random = new Random(Guid.NewGuid().GetHashCode()).Next(1, 100);
            var dateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var url = $"https://newapp.sysucc.org.cn:9032/Token";
            var password = HttpUtility.UrlEncode("Password@123");
            var username = HttpUtility.UrlEncode("zszl@sysucc.org.cn");
            var signContent = $"zszl@sysucc.org.cn|Password@123|{dateTime}|{random}";
            var sign = SHA256WithRSAHelper.Sign(signContent, _privateKey);
            var request = $"grant_type=password&username={username}&Password={password}&TimeStamp={dateTime}&RandomNum={random}&Sign={HttpUtility.UrlEncode(sign)}";
            var result = HttpHelper.Post(url, request, contentType: "application/x-www-form-urlencoded").Result;
        }
        [TestMethod]
        public void LoginTest()
        {
            var schedules = _patientHelper.QueryDeptScheduleDateForWXNew("34C1", "01").Result;
            var logoin = _patientHelper.PatientLogin("13266871776", "123456").Result;
            var token = logoin.SelectToken("data.token").ToString();
            //var members = HospitalHelper.QueryFamilyMemberListNew(token).Result;
            //var member = members["data"].FirstOrDefault(m => m["name"].ToString() == "林柏威");
            var proxy = _proxyHelper.GetProxy();
            var result = _patientHelper.QueryDocRegPointBySchemaID("21C1", DateTimeHelper.ConvertDateTimeToLong(new DateTime(2022, 03, 16)).ToString(), token, proxy).Result;
            //var result = HospitalHelper.QueryAppoitmentListNew(member["defaultHospitalCardId"].ToString(), logoin.SelectToken("data.token").ToString()).Result;
        }
        [TestMethod]
        public void ReservationTest()
        {
            try
            {
                var token = "b811c81f558342658e16e2ea2b89971d";
                var members = _patientHelper.QueryFamilyMemberListNew(token).Result;
                var patientId = members.SelectToken("data").FirstOrDefault().SelectToken("defaultHospitalCardId").ToString();
                var patientName = members.SelectToken("data").FirstOrDefault().SelectToken("name").ToString();
                var date = DateTimeHelper.ConvertDateTimeToLong(new DateTime(2022, 03, 15));
                var doctId = "B00762";
                var doctName = "";
                var regPoints = _patientHelper.QueryOneDayRegPointListNoDeptNew(doctId, patientId, date, "ALL", token).Result;
                foreach (JObject data in regPoints.SelectToken("data"))
                    foreach (JObject regPoint in data.SelectToken("regPointList"))
                    {
                        var startTime = DateTimeHelper.ConvertLongToDateTime((long)regPoint.SelectToken("startTime"));
                        var id = regPoint.SelectToken("startTime").ToString();
                        var regRemainder = (int)regPoint.SelectToken("regRemainder");
                        var deptId = data.SelectToken("deptId").ToString();
                        var deptName = data.SelectToken("deptName").ToString();
                        var regPointId = regPoint.SelectToken("id").ToString();
                        Trace.WriteLine($"DeptName:{deptName},SchemaId:{data.SelectToken("regPointId")},Doctor:{doctId},VisitTime:{startTime},RegisterRest:{regRemainder}");
                        if (regRemainder == 0) continue;

                        var visitTime = $"{startTime:yyyy-MM-dd HH:mm}-{startTime.AddMinutes((int)regPoint.SelectToken("interval")):HH:mm}";
                        var orgID = regPoint.SelectToken("orgId").ToString();
                        var result = _patientHelper.OccupyRegPointNew(patientId, patientName, deptId, deptName, doctId, doctName,
                            regPointId, visitTime, orgID, token).Result;
                        if (result != null && (int)result.SelectToken("msgCode") == 0 && result.SelectToken("msg").ToString() == "成功")
                        {
                            Trace.WriteLine($"Reservation Successed");
                            break;
                        }
                    }
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        [TestMethod]
        public void GetDeptDiseaseTest()
        {
            var schedules1 = _patientHelper.QueryOneDayScheduleListEntity("1658851200000", depId: "41C3", "01").Result;
            var deptAll = new List<dynamic> { };
            var depts = _patientHelper.GetDeptDisease("01", "1").Result;//越秀,按科室查询
            foreach (var dept in depts)
            {
                foreach (var deptInfo in dept.SelectToken("deptDiseaseList"))
                {
                    deptAll.Add(new { orgID = "01", ddId = deptInfo["ddId"], ddName = deptInfo["ddName"] });
                }
            }
            depts = _patientHelper.GetDeptDisease("02", "1").Result;//黄埔,按科室查询
            foreach (var dept in depts)
            {
                foreach (var deptInfo in dept.SelectToken("deptDiseaseList"))
                {
                    deptAll.Add(new { orgID = "02", ddId = deptInfo["ddId"], ddName = deptInfo["ddName"] });
                }
            }
            var schedulesDate = _patientHelper.QueryEntityDateList("鼻咽肿瘤放射治疗").Result;
            var schedules = _patientHelper.QueryOneDayEntityScheduleList("1645977600000", entityName: "鼻咽肿瘤放射治疗").Result;
        }
        [TestMethod]
        public void QueryHotOutPatients()
        {
            var outpatientsContent = LogHelper.Instance.ReadFile($"{AppDomain.CurrentDomain.BaseDirectory}/Config/Outpatients.json");
            var outpatients = JsonConvert.DeserializeObject<JToken>(outpatientsContent);
            var scheduleDate = DateTimeHelper.ConvertDateTimeToLong(DateTime.Today.AddDays(7));
            var validList = new List<string>();
            foreach (var outpatient in outpatients)
            {
                var ddId = outpatient["ddId"].ToString();
                var ddName = outpatient["ddName"].ToString();
                var orgID = outpatient["orgID"].ToString();
                var schedules = _patientHelper.QueryJTokenOneDayScheduleListEntity(scheduleDate.ToString(), ddId, orgID).Result;
                if (schedules == null) continue;
                foreach (var data in schedules.SelectToken("data.doctSchedule"))
                {
                    var entityName = data["entityName"].ToString();
                    foreach (var regDoc in data.SelectToken("regDocList"))
                    {
                        var schedule = JsonConvert.DeserializeObject<DoctSchedule>(regDoc.ToString());
                        if ((int)regDoc["scheduleTypeCode"] == 2)
                        {
                            var msg = $"{regDoc["orgName"]},门诊:{schedule.DeptName}({schedule.DeptId}),医生:{schedule.DoctName}({schedule.DoctId}),{regDoc["regLevel"]}已约满";
                            Trace.WriteLine(msg);
                            validList.Add(msg);
                        }
                        if ((int)regDoc["scheduleTypeCode"] == 1)
                        {
                            Trace.WriteLine($"{regDoc["orgName"]},门诊:{schedule.DeptName}({schedule.DeptId}),医生:{schedule.DoctName}({schedule.DoctId})有号");

                        }
                    }
                }
            }
            Trace.WriteLine(string.Join(Environment.NewLine, validList));
        }
    }
}
