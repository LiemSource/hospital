using Hangfire.Server;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using VaeDbContext;
using VaeEntity.Hospital;
using VaeHelper;
using BotHelper;

namespace HospitalJob.Jobs
{
    /// <summary>
    /// 门诊病人数据
    /// </summary>
    [DisplayName("门诊病人数据")]
    public class OutPatientQueryService : IHangfireJobService
    {
        protected readonly ILogger _logger;
        protected readonly ILoggerFactory _loggerFactory;
        protected readonly IConfiguration _configuration;
        protected readonly HospitalContext _hospitalContext;
        protected readonly ProxyHelper _proxyHelper;
        protected readonly HospitalHelper _hospitalHelper;
        protected readonly string[] _tokens;
        protected WebProxy _webProxy;
        private readonly TelegramBotHelper _telegramBotHelper;
        public OutPatientQueryService(ILoggerFactory logger,
                                      IConfiguration configuration,
                                      HospitalContext myContext,
                                      ProxyHelper proxyHelper,
                                      HospitalHelper hospitalHelper,
                                      TelegramBotHelper telegramBotHelper)
        {
            _logger = logger.CreateLogger(this.GetType());
            _loggerFactory = logger;
            _hospitalContext = myContext;
            _proxyHelper = proxyHelper;
            _hospitalHelper = hospitalHelper;
            _configuration = configuration;
            _tokens = (_configuration["HospitalTokens"] ?? "").Split(';');
            _telegramBotHelper = telegramBotHelper;
        }


        public async Task Excute(PerformContext context)
        {
            try
            {
                var visitDate = DateTime.Today;
                //var dateEnd = visitDate.AddDays(1);
                await ExcuteQueryOutPatient(visitDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查询门诊病人信息异常");
                await _telegramBotHelper.SendMessage($"查询门诊病人信息异常：{ex.Message}");
            }
        }

        public async Task ExcuteQueryOutPatient(DateTime startDate)
        {
            try
            {
                var totalStopwatch = Stopwatch.StartNew();
                var newPatients = 0;
                _webProxy = _proxyHelper.GetProxy(timeOut: 3000); //获取代理
                QuerySchemaInfo schemaQuery = _hospitalContext.QuerySchemaInfos.FirstOrDefault();

                var exceptionSchemas = new List<int>();                

                //DoctSchedule类:Id,DoctId,DoctName,DeptId,DeptName,RegPointID,OrgId,OrgName,EntityName,NoonCode,VisitDate,CreateTime
                List<DoctSchedule> doctSchedules = _hospitalContext.DoctSchedules.Where(s => s.VisitDate >= startDate && !string.IsNullOrEmpty(s.RegPointID)).ToList();

                if (schemaQuery == null) schemaQuery = new QuerySchemaInfo() { CreateTime = DateTime.Now };

                //查询异常ID
                if (!string.IsNullOrEmpty(schemaQuery.ExcetionSchemaIds))
                {
                    _logger.LogInformation($"重试异常门诊Id:{schemaQuery.ExcetionSchemaIds}");
                    var excetionSchemaIds = schemaQuery.ExcetionSchemaIds.Split(',');
                    foreach (var excetionSchemaId in excetionSchemaIds)
                    {
                        var query = await QueryOutPatient(schemaQuery, int.Parse(excetionSchemaId), exceptionSchemas, true, 3000);
                        newPatients += query.newPatientCount;
                    }
                }

                //查询需查询项
                foreach (var doctSchedule in doctSchedules.GroupBy(s => s.RegPointID))
                {
                    var query = await QueryOutPatient(schemaQuery, int.Parse(doctSchedule.Key), exceptionSchemas, doctName: doctSchedule.LastOrDefault().DoctName);
                    newPatients += query.newPatientCount;
                }

                schemaQuery.ExcetionSchemaIds = string.Join(",", exceptionSchemas);
                _hospitalContext.UpdateWithSeletor(schemaQuery, s => s.ExcetionSchemaIds);
                await _hospitalContext.SaveChangesAsync();

                _logger.LogInformation($"新增门诊病人:{newPatients}位,查询完成耗时:{totalStopwatch.Elapsed.TotalSeconds}秒");
                _logger.LogWarning($"查询异常门诊Id:{string.Join(",", exceptionSchemas)}");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查询门诊病人信息异常");
                await _telegramBotHelper.SendMessage($"查询门诊病人信息异常：{ex.Message}");
            }
        }

        public async Task test(int startIndex)
        {
            _logger.LogInformation($"已成功执行test");
        }

        public async Task QueryOutPatientRetrieval(int startIndex, int endIndex, DateTime monthDate)
        {
            var sql = $"select distinct  schema_id from patient where patient_type=0 and schema_id>={startIndex}  order by schema_id desc";
            IList<Patient> ids;
            using (var conn = _hospitalContext.Database.GetDbConnection())
            {
                conn.Open();
                var command = conn.CreateCommand();
                command.CommandText = sql;
                command.CommandTimeout = 60;
                ids = command.ExecuteReader().ReaderToList<Patient>();
            }

            var schemaQuery = _hospitalContext.QuerySchemaInfos.FirstOrDefault();
            var newPatients = 0;
            var exceptionSchemas = new List<int>();
            _webProxy = _proxyHelper.GetProxy(timeOut: 2000);
            for (int schemaIndex = startIndex; schemaIndex < endIndex; schemaIndex++)
            {
                if (ids.Any(i => i.SchemaId == schemaIndex.ToString())) continue;
                var query = await QueryOutPatient(schemaQuery, schemaIndex, exceptionSchemas, true, 2000, monthDate);
                _logger.LogInformation($"补偿查询门诊Id:{schemaIndex},新增病人数:{query.newPatientCount}");
                newPatients += query.newPatientCount;
            }

            schemaQuery.ExcetionSchemaIds = string.Join(",", exceptionSchemas);
            _logger.LogWarning($"查询异常门诊Id:{schemaQuery.ExcetionSchemaIds}");
            _hospitalContext.UpdateWithSeletor(schemaQuery, s => s.ExcetionSchemaIds);
            await _hospitalContext.SaveChangesAsync();
        }

        public async Task<(int newPatientCount, bool done)> QueryOutPatient(QuerySchemaInfo schemaQuery,
                                                                             int schemaId,
                                                                             List<int> exceptionSchemas,
                                                                             bool retry = false, int timeOut = 5000, DateTime? monthDate = null, string doctName = null)
        {
            var token = _tokens[new Random(Guid.NewGuid().GetHashCode()).Next(0, _tokens.Length)];

            List<Patient> schemaPatients = null;

            for (int tryIndex = 0; tryIndex < 3; tryIndex++)
            {
                try
                {
                    if (tryIndex > 0)
                        _logger.LogWarning($"第{tryIndex}次查询门诊Id:{schemaId}");
                    schemaPatients = _hospitalHelper.QuerySchemaInpatientList(schemaId.ToString(), _webProxy, token, res => _logger.LogError($"查询门诊:{schemaId}响应:{res}")).Result;
                    if (schemaPatients != null)
                        break;
                    _webProxy = _proxyHelper.GetProxy(timeOut: timeOut);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"查询门诊Id:{schemaId}异常:{ex.Message}");
                    _webProxy = _proxyHelper.GetProxy();
                }
            }

            if (schemaPatients == null)
            {
                _logger.LogError($"查询门诊Id:{schemaId}三次异常");
                await _telegramBotHelper.SendMessage($"查询门诊Id:{schemaId}三次异常");
                Thread.Sleep(TimeSpan.FromMinutes(1));
                exceptionSchemas.Add(schemaId);
                _webProxy = _proxyHelper.GetProxy();
                return (0, false);
            }

            if (!string.IsNullOrEmpty(doctName)) schemaPatients.ForEach(p => p.ChiefDoctName = doctName);

            if (monthDate != null && schemaPatients.Any(p => p.InDateFormate?.Year != monthDate.Value.Year || p.InDateFormate?.Month != monthDate.Value.Month))
            {
                _logger.LogInformation($"{schemaPatients.FirstOrDefault().InDate}:{schemaPatients.Count}位病人,已忽略");
                return (0, false);                
            }

            var valiedPatients = schemaPatients.Where(p => p.SeeNo != "0");

            if (valiedPatients.Any())
            {
                try
                {
                    var valiedPatientCount = 0;
                    var first = valiedPatients.FirstOrDefault();
                    var ids = valiedPatients.Select(p => p.Id);
                    var existsPatients = _hospitalContext.Patients.Where(p => p.SchemaId == schemaId.ToString() || ids.Contains(p.Id)).ToList();

                    foreach (var patient in existsPatients)
                    {
                        var samePatient = valiedPatients.FirstOrDefault(p => p.Id == patient.Id);
                        if (samePatient == null) continue;
                        if (patient.VisitId != samePatient.VisitId)
                            patient.VisitId = samePatient.VisitId;
                        if (patient.SchemaId != samePatient.SchemaId)
                            patient.SchemaId = samePatient.SchemaId;

                        patient.InPatientId= samePatient.InPatientId;

                        _hospitalContext.Update(patient);
                    }

                    foreach (var valiedPatient in valiedPatients)
                    {
                        if (existsPatients.Any(p => p.Id == valiedPatient.Id)) continue;
                        _hospitalContext.Add(valiedPatient);
                        valiedPatientCount += 1;
                    }

                    _logger.LogInformation($"{first.SchemaId},{first.DeptName}{first.InDate}:{valiedPatients.Count()}位病人,新增{valiedPatientCount}位病人");
                }
                catch (Exception ex)
                {
                    exceptionSchemas.Add(schemaId);
                    _logger.LogError(ex, $"查询门诊Id:{schemaId}异常!");
                }
            }

            if (!retry)
            {
                schemaQuery.LastQuerySchemaId = schemaId;
                schemaQuery.LastQueryTime = DateTime.Now;
                schemaQuery.LastQuerySchemaName = valiedPatients.FirstOrDefault()?.DeptName;
                if (schemaQuery.Id == 0) await _hospitalContext.AddAsync(schemaQuery);
                else _hospitalContext.Update(schemaQuery);
            }

            try
            {
                await _hospitalContext.SaveChangesAsync();
                return (valiedPatients.Count(), false);
            }
            catch (Exception ex)
            {
                exceptionSchemas.Add(schemaId);
                _logger.LogError($"查询门诊Id:{schemaId}保存数据异常:{ex.Message}!");
                await _telegramBotHelper.SendMessage($"查询门诊Id:{schemaId}保存数据异常:{ex.Message}!");
            }

            return (0, false);

        }
    }
}
