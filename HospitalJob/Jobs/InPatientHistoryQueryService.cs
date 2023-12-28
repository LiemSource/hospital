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

namespace HospitalJob.Jobs
{
    [DisplayName("病人就诊历史记录（已废弃）")]
    public class InPatientHistoryQueryService
    {
        protected readonly ILogger _logger;
        protected readonly ILoggerFactory _loggerFactory;
        protected readonly IConfiguration _configuration;
        protected readonly HospitalContext _hospitalContext;
        protected readonly ProxyHelper _proxyHelper;
        protected readonly HospitalHelper _hospitalHelper;
        protected readonly string[] _tokens;
        private readonly Dictionary<string, string> _depts;
        public InPatientHistoryQueryService(ILoggerFactory logger, IConfiguration configuration, HospitalContext myContext, ProxyHelper proxyHelper, HospitalHelper hospitalHelper)
        {
            _logger = logger.CreateLogger(this.GetType());
            _loggerFactory = logger;
            _hospitalContext = myContext;
            _configuration = configuration;
            _proxyHelper = proxyHelper;
            _hospitalHelper = hospitalHelper;
            _tokens = (_configuration["HospitalTokens"] ?? "").Split(';');
            _depts = JsonConvert.DeserializeObject<Dictionary<string, string>>(_configuration["HospitalDept"] ?? "{}");
        }
        public async Task Excute(PerformContext context)
        {
            var exceptionVisitInfo = _hospitalContext.QuerySchemaInfos.FirstOrDefault(s => string.IsNullOrEmpty(s.LastQuerySchemaName));
            if (exceptionVisitInfo != null && !string.IsNullOrEmpty(exceptionVisitInfo.ExcetionSchemaIds))
            {
                await QueryByVisitIds(exceptionVisitInfo.ExcetionSchemaIds.Split(',').ToList(), exceptionVisitInfo);
            }
        }
        public async Task Query(DateTime startDate, DateTime endDate, string startVisitId = null)
        {
            try
            {
                var sql = @$"select distinct visit_id from patient where patient_type=1 and in_date>='{startDate:yyyy-MM-dd}' and in_date<'{endDate:yyyy-MM-dd}'";
                if (!string.IsNullOrEmpty(startVisitId))
                {
                    sql += $" and visit_id>='{startVisitId}'";
                }
                sql += " order by visit_id;";
                IList<Patient> visitInfos;
                using (var conn = _hospitalContext.Database.GetDbConnection())
                {
                    conn.Open();
                    var command = conn.CreateCommand();
                    command.CommandText = sql;
                    command.CommandTimeout = 600;
                    visitInfos = command.ExecuteReader().ReaderToList<Patient>();
                    if (!visitInfos.Any())
                    {
                        _logger.LogInformation("不存在病历号记录");
                        return;
                    }
                }
                var exceptionVisitInfo = _hospitalContext.QuerySchemaInfos.FirstOrDefault(s => string.IsNullOrEmpty(s.LastQuerySchemaName));
                var visitIds = new List<string>();

                if (exceptionVisitInfo == null)
                {
                    exceptionVisitInfo = new QuerySchemaInfo() { CreateTime = DateTime.Now, LastQueryTime = startDate };
                    _hospitalContext.QuerySchemaInfos.Add(exceptionVisitInfo);
                }
                else
                {
                    if (!string.IsNullOrEmpty(exceptionVisitInfo.ExcetionSchemaIds))
                        visitIds.AddRange(exceptionVisitInfo.ExcetionSchemaIds.Split(','));
                    exceptionVisitInfo.LastQueryTime = startDate;
                    _hospitalContext.UpdateWithSeletor(exceptionVisitInfo, i => i.LastQueryTime);
                }
                _hospitalContext.SaveChanges();
                visitIds.AddRange(visitInfos.Where(v => !string.IsNullOrEmpty(v.VisitId)).Select(v => v.VisitId));
                await QueryByVisitIds(visitIds, exceptionVisitInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"查询病人历史就诊记录异常");
            }
        }
        public async Task QueryByVisitIds(List<string> visitIds, QuerySchemaInfo exceptionVisitInfo)
        {
            var totalStopwatch = Stopwatch.StartNew();
            var newPatients = 0;
            var exceptionVisitIds = new List<string>();
            var webProxy = _proxyHelper.GetProxy();
            var pageSize = 50;
            var pages = Math.Ceiling((double)visitIds.Count / pageSize);
            for (int pageIndex = 0; pageIndex < pages; pageIndex++)
            {
                var pageVisitIds = visitIds.Skip(pageIndex * pageSize).Take(pageSize);
                try
                {
                    var patientsExist = _hospitalContext.Patients.Where(p => pageVisitIds.Contains(p.VisitId)).ToList();
                    var newPatientList = new List<Patient>();
                    foreach (var visitId in pageVisitIds)
                    {
                        try
                        {
                            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                            var visitHistories = await _hospitalHelper.QueryMyMarkPatientDetail(visitId, webProxy);
                            int patientVisitTime = 0;
                            foreach (var visitHistory in visitHistories.Where(h => h.InDateFormate >= exceptionVisitInfo.LastQueryTime))
                            {
                                if (patientsExist.Any(p => p.Id == visitHistory.Id))
                                    continue;
                                if (visitHistory.PatientType == PatientType.outpatient && visitHistory.ChiefDoctName == "None")
                                    continue;
                                patientVisitTime += 1;
                                visitHistory.CreateTime = DateTime.Now;
                                visitHistory.UpdateTime = DateTime.Now;
                                visitHistory.DeptId = _depts.FirstOrDefault(d => d.Value == visitHistory.DeptName).Key;
                                visitHistory.MedicationFailed = true;
                                if (string.IsNullOrEmpty(visitHistory.VisitId))
                                    visitHistory.VisitId = visitId;
                                if (string.IsNullOrEmpty(visitHistory.VisitId))
                                    visitHistory.VisitId = visitHistory.Id.Substring(4, 10);
                                if (!_hospitalContext.Patients.Any(p => p.Id == visitHistory.Id))
                                    newPatientList.Add(visitHistory);
                            }
                            newPatients += patientVisitTime;
                            if (patientVisitTime > 0)
                                _logger.LogInformation($"病例号:{visitId}新增就诊记录:{patientVisitTime}条,累计新增:{newPatients}条,耗时:{stopwatch.ElapsedMilliseconds}毫秒");
                            else
                                _logger.LogInformation($"病例号:{visitId}未新增就诊记录,耗时:{stopwatch.ElapsedMilliseconds}毫秒");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"查询病人历史就诊记录异常,病历号:{visitId}");
                            webProxy = _proxyHelper.GetProxy();
                            exceptionVisitIds.Add(visitId);
                        }
                    }
                    var savewatch = System.Diagnostics.Stopwatch.StartNew();
                    _hospitalContext.Patients.AddRange(newPatientList);
                    _hospitalContext.SaveChanges();
                    _logger.LogInformation($"保存数据,耗时:{savewatch.ElapsedMilliseconds}毫秒");

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"查询病人历史就诊记录异常,病历号:{string.Join(",", pageVisitIds)}");
                    exceptionVisitIds.AddRange(pageVisitIds);
                }
            }

            if (exceptionVisitIds.Any())
            {
                exceptionVisitInfo.ExcetionSchemaIds = string.Join(",", exceptionVisitIds);
            }
            else
            {
                exceptionVisitInfo.ExcetionSchemaIds = null;
            }
            _hospitalContext.Update(exceptionVisitInfo);
            _hospitalContext.SaveChanges();
            _logger.LogInformation($"查询病人历史就诊记录:{newPatients}条,查询完成耗时:{totalStopwatch.Elapsed.TotalSeconds}秒");
        }
    }
}
