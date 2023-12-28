using Hangfire.Server;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using VaeDbContext;
using VaeEntity.Hospital;
using VaeHelper;
using BotHelper;

namespace HospitalJob.Jobs
{
    /// <summary>
    /// PatientVisitReviewedJob
    /// </summary>
    [DisplayName("病人就诊记录复核")]
    public class PatientVisitReviewedJob
    {
        protected readonly ILogger _logger;
        protected readonly ILoggerFactory _loggerFactory;
        protected readonly IConfiguration _configuration;
        protected readonly HospitalContext _hospitalContext;
        protected readonly ProxyHelper _proxyHelper;
        protected readonly HospitalHelper _hospitalHelper;
        private IWebProxy _webProxy;
        private readonly Dictionary<string, string> _depts;
        private readonly TelegramBotHelper _telegramBotHelper;

        public PatientVisitReviewedJob(ILoggerFactory logger,
                                       IConfiguration configuration,
                                       HospitalContext myContext,
                                       ProxyHelper proxyHelper,
                                       HospitalHelper hospitalHelper,
                                       TelegramBotHelper telegramBotHelper)
        {
            _logger = logger.CreateLogger(this.GetType());
            _loggerFactory = logger;
            _hospitalContext = myContext;
            _configuration = configuration;
            _proxyHelper = proxyHelper;
            _hospitalHelper = hospitalHelper;
            _depts = JsonConvert.DeserializeObject<Dictionary<string, string>>(_configuration["HospitalDept"] ?? "{}");
            _telegramBotHelper = telegramBotHelper;
        }

        public void Excute1()
        {
            //var startDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            //var endDate = startDate.AddMonths(1);
            try
            {
                var startDateStr = "2022-10-01"; //startDate.ToString("yyyy-MM-dd");
                var endDateStr = "2022-11-01"; //endDate.ToString("yyyy-MM-dd");
                var sql = @$"insert into patient_visit_info(visit_id, is_reviewed, start_date, end_date)
                         select distinct visit_id, false, '{startDateStr}' start_date, '{endDateStr}' end_date
                         from patient
                         where in_date >= '{startDateStr}'
                         and in_date< '{endDateStr}'
                         and visit_id not in (select patient_visit_info.visit_id from patient_visit_info where start_date >= '{startDateStr}' and start_date< '{endDateStr}');";
                using (var conn = _hospitalContext.Database.GetDbConnection())
                {
                    conn.Open();
                    var command = conn.CreateCommand();
                    command.CommandText = sql;
                    command.CommandTimeout = 600;
                    var result = command.ExecuteNonQuery();
                    _logger.LogInformation($"新增{result}条待复核记录");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"初始化待复核记录异常");  
            }

            //await Query(startDate, endDate);
        }

        public async Task Excute(PerformContext context)
        {
            var startDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            var endDate = startDate.AddMonths(1);

            await ReviewedJob(startDate, endDate);

        }       

        public async void Test(string pid)
        {
            var webProxy = _proxyHelper.GetProxy();//0000633587
            //await _hospitalHelper.GetPatient(pid,webProxy);
            await _hospitalHelper.QueryMyMarkPatientDetail(pid, webProxy);
        }
     
        public async Task ReviewedJob(DateTime startDate, DateTime endDate, bool orderByDesc = false,string visit_id = null)
        {
            try
            {
                var startDateStr = startDate.ToString("yyyy-MM-dd");
                var endDateStr = endDate.ToString("yyyy-MM-dd");

                
                 var sql = @$"insert into patient_visit_info(visit_id, is_reviewed, start_date, end_date)
                         select distinct visit_id, false, '{startDateStr}' start_date, '{endDateStr}' end_date
                         from patient
                         where in_date >= '{startDateStr}'
                         and in_date< '{endDateStr}' 
                         and visit_id not in (select patient_visit_info.visit_id from patient_visit_info where start_date >= '{startDateStr}' and start_date< '{endDateStr}');";

                if (!string.IsNullOrEmpty(visit_id))
                {
                    sql = @$"insert into patient_visit_info(visit_id, is_reviewed, start_date, end_date)
                         select distinct visit_id, false, '{startDateStr}' start_date, '{endDateStr}' end_date
                         from patient
                         where in_date >= '{startDateStr}'
                         and in_date< '{endDateStr}' 
                         and  visit_id='{visit_id}'";
                }              
                
                using (var conn = _hospitalContext.Database.GetDbConnection())
                {
                    conn.Open();
                    var command = conn.CreateCommand();
                    command.CommandText = sql;
                    command.CommandTimeout = 600;
                    var result = command.ExecuteNonQuery();
                    _logger.LogInformation($"新增{result}条待复核记录");
                }

                await Query(startDate, endDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"初始化待复核记录异常");
                //await _telegramBotHelper.SendMessage($"初始化待复核记录异常：{ex.Message}");
            }
        }       

        public async Task Query(DateTime startDate, DateTime endDate, bool orderByDesc = false)
        {
            var webProxy = _proxyHelper.GetProxy();
            while (true)
            {
                var visitInfos = new List<PatientVisitInfo>();
                var newPatientList = new List<Patient>();
                var updatePatientList = new List<Patient>();
                var patientsExist = new List<Patient>();
                var newPatients = 0;



                try
                {
                    if (orderByDesc)
                        visitInfos = _hospitalContext.PatientVisitInfos.Where(s => s.StartDate >= startDate && s.StartDate < endDate && !s.IsReviewed)
                            .OrderByDescending(s => s.VisitId).Take(50).ToList();
                    else
                        visitInfos = _hospitalContext.PatientVisitInfos.Where(s => s.StartDate >= startDate && s.StartDate < endDate && !s.IsReviewed)
                            .OrderBy(s => s.VisitId).Take(50).ToList();
                    if (visitInfos == null || !visitInfos.Any()) break;
                    var pageVisitIds = string.Join(",", visitInfos.Select(s => $"'{s.VisitId}'"));
                    var sql = $"select * from patient where DATE_FORMAT(substring(in_date,1,10), '%Y-%m-%d')>='{startDate:yyyy-MM-dd}' and visit_id in ({pageVisitIds})";
                    using (var conn = _hospitalContext.Database.GetDbConnection())
                    {
                        conn.Open();
                        var command = conn.CreateCommand();
                        command.CommandText = sql;
                        command.CommandTimeout = 600;
                        patientsExist = (List<Patient>)command.ExecuteReader().ReaderToList<Patient>();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"查询待复核记录异常");
                }

                foreach (var visitInfo in visitInfos)
                {
                    try
                    {
                        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                        var visitHistories = await _hospitalHelper.QueryMyMarkPatientDetail(visitInfo.VisitId, webProxy);
                        int patientVisitTime = 0;
                        foreach (var visitHistory in visitHistories.Where(h => h.InDateFormate >= visitInfo.StartDate && h.InDateFormate < visitInfo.EndDate))
                        {
                            if (patientsExist.Any(p => p.Id == visitHistory.Id))
                            {
                                var patientExits1 = patientsExist.FirstOrDefault(p => p.Id == visitHistory.Id); 
                                patientExits1.Remark = visitHistory.Remark;
                                patientExits1.UpdateTime = DateTime.Now;
                                updatePatientList.Add(patientExits1);
                                _logger.LogInformation($"就诊Id:{visitHistory.Id}已存在");
                                continue;
                            }

                            if (visitHistory.PatientType == PatientType.outpatient && visitHistory.ChiefDoctName == "None")   continue;

                            visitHistory.CreateTime = DateTime.Now;
                            visitHistory.UpdateTime = DateTime.Now;
                            visitHistory.DeptId = _depts.FirstOrDefault(d => d.Value == visitHistory.DeptName).Key;
                            visitHistory.MedicationFailed = true;

                            if (string.IsNullOrEmpty(visitHistory.VisitId))
                                visitHistory.VisitId = visitInfo.VisitId;
                            if (string.IsNullOrEmpty(visitHistory.VisitId))
                                visitHistory.VisitId = visitHistory.Id.Substring(4, 10);

                            var patientExits = _hospitalContext.Patients.FirstOrDefault(p => p.Id == visitHistory.Id);

                            if (patientExits == null)
                            {
                                patientVisitTime += 1;
                                newPatientList.Add(visitHistory);
                            }
                            else
                            {
                                patientExits.VisitId = visitHistory.VisitId;
                                patientExits.Remark = visitHistory.Remark;
                                patientExits.UpdateTime = DateTime.Now;
                                updatePatientList.Add(patientExits);
                                _logger.LogInformation($"就诊Id:{visitHistory.Id}已存在");
                            }
                        }

                        newPatients += patientVisitTime;

                        visitInfo.IsReviewed = true;

                        if (patientVisitTime > 0)
                            _logger.LogInformation($"病例号:{visitInfo.VisitId}新增就诊记录:{patientVisitTime}条,累计新增:{newPatients}条,耗时:{stopwatch.ElapsedMilliseconds}毫秒");
                        else
                            _logger.LogInformation($"病例号:{visitInfo.VisitId}未新增就诊记录,耗时:{stopwatch.ElapsedMilliseconds}毫秒");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"查询病人历史就诊记录异常,病历号:{visitInfo.VisitId}");
                        webProxy = _proxyHelper.GetProxy();
                    }
                }

                try
                {
                    var savewatch = System.Diagnostics.Stopwatch.StartNew();
                    _hospitalContext.Patients.AddRange(newPatientList);
                    _hospitalContext.SaveChanges();
                    _hospitalContext.UpdateRange(visitInfos);
                    _hospitalContext.UpdateRange(updatePatientList);
                    _hospitalContext.SaveChanges();
                    _logger.LogInformation($"保存数据{newPatientList.Count}条,耗时:{savewatch.ElapsedMilliseconds}毫秒");
                }
                catch (Exception ex)
                {
                    _hospitalContext.Patients.RemoveRange(newPatientList);
                    _logger.LogError(ex, $"保存已复核记录异常");
                    //await _telegramBotHelper.SendMessage($"保存已复核记录异常：{ex.Message}");
                }
            }
        }
    }
}
