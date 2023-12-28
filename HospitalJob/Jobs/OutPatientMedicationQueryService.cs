using Hangfire.Server;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;
using VaeDbContext;
using VaeEntity.Hospital;
using VaeHelper;
using BotHelper;

namespace HospitalJob.Jobs
{
    /// <summary>
    /// 门诊处方
    /// </summary>
    [DisplayName("门诊处方")]
    public class OutPatientMedicationQueryService : IHangfireJobService
    {
        protected readonly ILogger _logger;
        protected readonly ILoggerFactory _loggerFactory;
        protected readonly IConfiguration _configuration;
        protected readonly HospitalContext _hospitalContext;
        protected readonly ProxyHelper _proxyHelper;
        protected readonly HospitalHelper _hospitalHelper;
        protected readonly string[] _tokens;
        protected IWebProxy _webProxy;
        private readonly TelegramBotHelper _telegramBotHelper;
        public OutPatientMedicationQueryService(ILoggerFactory logger,
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
            _tokens = (_configuration["HospitalTokens"] ?? "").Split(';');
            _telegramBotHelper = telegramBotHelper;
        }
        public async Task Excute(PerformContext context)
        {
            var inDateStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            await QueryOutPatientMedications(inDateStart, DateTime.Today.AddDays(1));
        }

        public async Task QueryOutPatientMedicationsN(DateTime inDateStart, DateTime inDateEnd, int? section = null, int? sectionIndex = null)
        {
            await QueryOutPatientMedications(inDateStart, inDateEnd, section, sectionIndex);
        }


        public async Task QueryOutPatientMedications(DateTime inDateStart, DateTime inDateEnd, int? section = null, int? sectionIndex = null)
        {
            try
            {
                var totalStopwatch = Stopwatch.StartNew();
                var pageSize = 50;
                var exCount = 0;
                var depts = (LogHelper.Instance.ReadConfig("/Config/Department") ?? "").Split(Environment.NewLine).Where(d => !string.IsNullOrEmpty(d));
                var deptsString = string.Join(",", depts.Select(d => $"'{d}'"));

                long total = 0;
                using (var conn = _hospitalContext.Database.GetDbConnection())
                {
                    conn.Open();
                    var command = conn.CreateCommand();
                    var sql = $"select count(1) from patient where patient_type = 0 " +
                        $"and DATE_FORMAT(substring(in_date, 1, 10), '%Y-%m-%d')>='{inDateStart:yyyy-MM-dd}'" +
                        $"and DATE_FORMAT(substring(in_date, 1, 10), '%Y-%m-%d')<'{inDateEnd:yyyy-MM-dd}'" +
                        $" and medication_failed=1";
                    if (!string.IsNullOrEmpty(deptsString))
                        sql += $" and dept_name not in ({deptsString})";
                    if (section != null && sectionIndex != null)
                        sql += $" and id%{section}={sectionIndex}";
                    command.CommandText = sql;
                    command.CommandTimeout = 60;
                    total = (long)command.ExecuteScalar();
                }

                var totalQuery = 0;
                var sectionMsg = (sectionIndex == null ? "" : $"第{sectionIndex + 1}片");

                try
                {
                    var patients = QueryPatients(inDateStart, inDateEnd, deptsString, pageSize, section, sectionIndex);
                    while (patients != null && patients.Any())
                    {
                        exCount += await QueryMedications(patients, sectionMsg);
                        totalQuery += patients.Count();
                        patients = QueryPatients(inDateStart, inDateEnd, deptsString, pageSize, section, sectionIndex);
                        _logger.LogInformation($"{sectionMsg}应查询总数:{total},累计查询总数:{totalQuery}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"{sectionMsg}查询门诊处方信息异常");
                    await _telegramBotHelper.SendMessage($"查询门诊处方信息异常：{ex.Message}");
                }

                _logger.LogInformation($"{sectionMsg}查询完成耗时:{totalStopwatch.Elapsed.TotalSeconds}秒,应查询总数:{total},实际查询总数:{totalQuery},异常数:{exCount}");
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"QueryOutPatientMedications异常: {ex.Message}");
            }
        }

        private IList<Patient> QueryPatients(DateTime inDateStart, DateTime inDateEnd, string deptsString, int pageSize, int? section = null, int? sectionIndex = null)
        {
            try
            {
                IList<Patient> patients;
                using (var conn = _hospitalContext.Database.GetDbConnection())
                {
                    conn.Open();
                    var command = conn.CreateCommand();
                    var sql = $"select * from patient where patient_type = 0 " +
                       $"and DATE_FORMAT(substring(in_date, 1, 10), '%Y-%m-%d')>='{inDateStart:yyyy-MM-dd}' " +
                       $"and DATE_FORMAT(substring(in_date, 1, 10), '%Y-%m-%d')<'{inDateEnd:yyyy-MM-dd}' " +
                       $"and medication_failed=1";
                    if (!string.IsNullOrEmpty(deptsString))
                        sql += $" and dept_name not in ({deptsString})";
                    if (section != null && sectionIndex != null)
                        sql += $" and id%{section}={sectionIndex}";
                    sql += $" order by id limit {pageSize};";
                    command.CommandText = sql;
                    command.CommandTimeout = 60;
                    patients = command.ExecuteReader().ReaderToList<Patient>();
                }
                return patients;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"QueryPatients 异常: {ex.Message}");      
                return new List<Patient>();
            }
        }


        public async Task QueryMedicationByPatientName(string name, DateTime inDateStart, DateTime inDateEnd)
        {           

           

            try
            {
                 
                var sectionMsg = $"查询[{name}]";

                IList<Patient> patients;
                using (var conn = _hospitalContext.Database.GetDbConnection())
                {
                    conn.Open();
                    var command = conn.CreateCommand();
                    var sql = $"select * from patient where name='{name}'" +
                          $"and DATE_FORMAT(substring(in_date, 1, 10), '%Y-%m-%d')>='{inDateStart:yyyy-MM-dd}' " +
                          $"and DATE_FORMAT(substring(in_date, 1, 10), '%Y-%m-%d')<'{inDateEnd:yyyy-MM-dd}' ";
                    command.CommandText = sql;
                    command.CommandTimeout = 60;
                    patients = command.ExecuteReader().ReaderToList<Patient>();
                }

                if(patients == null || !patients.Any())
                {
                    _logger.LogInformation($"QueryMedicationByPatientName 查询 {name} 门诊处方信息异常:patients == null && !patients.Any()");
                    return;
                }

                await QueryMedications(patients, sectionMsg);

              
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"QueryMedicationByPatientName 查询 {name} 门诊处方信息异常");                
            }
        }

        public async Task<int> QueryMedications(IList<Patient> patients, string sectionMsg)
        {

            //_logger.LogInformation($"*************************QueryMedications {patients.Count} 开始查询**************************");

            var exCount = 0;
            if (_webProxy == null)
                _webProxy = _proxyHelper.GetProxy();
            var medicationsPage = new List<OutPatientMedication>();
            foreach (var patient in patients)
            {
                var stopwatch = Stopwatch.StartNew();
                try
                {
                    var token = _tokens[new Random(Guid.NewGuid().GetHashCode()).Next(0, _tokens.Length)];
                    var medications = await _hospitalHelper.QueryOutPatientPrescript(patient.VisitId, patient.Id, _webProxy, token);
                    var queryElapes = stopwatch.ElapsedMilliseconds;
                    if (medications == null)
                    {                        
                        _webProxy = _proxyHelper.GetProxy();
                        continue;
                    }

                    if (medications.Any())  medicationsPage.AddRange(medications);

                    patient.MedicationFailed = false;
                    patient.UpdateTime = DateTime.Now;

                    if (queryElapes > 5000)
                        _webProxy = _proxyHelper.GetProxy();
                    if (medications.Any())
                        _logger.LogInformation($"{sectionMsg}病人:{patient.Name}({patient.Id}),已更新处方信息{medications.Count}条记录,请求耗时:{queryElapes}");
                }
                catch (Exception ex)
                {
                    _webProxy = _proxyHelper.GetProxy();
                    exCount += 1;
                    _logger.LogError(ex, $"{sectionMsg}查询门诊:{patient.DeptName},病人:{patient.Name}处方信息异常");
                }
            }
                       

            try
            {
                using (var hospitalContext = new HospitalContext(_configuration))
                {
                    hospitalContext.UpdateRange(patients);
                    await hospitalContext.OutPatientMedications.AddRangeAsync(medicationsPage);
                    await hospitalContext.SaveChangesAsync();

                    //_logger.LogInformation($"*************************QueryMedications 已完成更新 **************************");
                }
            }
            catch (Exception ex)
            {
                exCount += patients.Count;
                _logger.LogError(ex, $"保存病人及处方信息异常");
                await _telegramBotHelper.SendMessage($"保存门诊处方信息异常：{ex.Message}");
            }
            return exCount;
        }

        //public async Task<List<OutPatientMedication>> ProcQueryOutPatientPrescript(string patient_VisitId, string patient_Id)
        //{
        //    var token = _tokens[new Random(Guid.NewGuid().GetHashCode()).Next(0, _tokens.Length)];
        //    var medications = await _hospitalHelper.QueryOutPatientPrescript(patient_VisitId, patient_Id, _webProxy, token);

        //    return medications;
        //}
    }
}
