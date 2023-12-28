using BotHelper;
using Hangfire.Server;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;//IsoDateTimeConverter 
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
    /// <summary>
    /// 住院医嘱数据
    /// </summary>
    [DisplayName("医嘱数据")]
    public class MedicationQueryService : IHangfireJobService
    {
        protected readonly ILogger _logger;
        protected readonly ILoggerFactory _loggerFactory;
        protected readonly IConfiguration _configuration;
        protected readonly HospitalContext _hospitalContext;
        protected readonly HospitalHelper _hospitalHelper;
        protected readonly ProxyHelper _proxyHelper;
        protected readonly string[] _tokens;
        private readonly TelegramBotHelper _telegramBotHelper;
        private WebProxy _webProxy;
        public MedicationQueryService(ILoggerFactory logger,
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
            DateTime startDate = default(DateTime);
            DateTime endDate = default(DateTime);

            await QueryMedications(false, startDate, endDate);
        }


        public async Task QueryMedications(bool retry, DateTime startDate, DateTime endDate)
        {
            try
            {
                var totalStopwatch = Stopwatch.StartNew();
                var queryDate = DateTime.Today.AddDays(-7);
                var sql = "";
                if (retry)
                {
                    sql = $@"select * from patient where medication_failed=1 and patient_type=1 
                                and DATE_FORMAT(substring(in_date, 1, 10), '%Y-%m-%d')>='{startDate:yyyy-MM-dd}'
                            and DATE_FORMAT(substring(in_date, 1, 10), '%Y-%m-%d')<'{endDate:yyyy-MM-dd}'
                            ";
                }
                else
                {
                    var month = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                    var where = $"update_time>='{queryDate:yyyy-MM-dd}' || (p.medication_max_time is null && p.temp_medication_max_time is null && update_time>='{month:yyyy-MM-dd}')";

                    sql = $@"select *
        
                            from (
                                        select p.id,
                                            p.name,
                                            p.dept_id,
                                            p.dept_name,
                                            p.diagnosis,
                                            p.in_date,
                                            p.out_date,
                                            p.create_time,
                                            p.patient_type,
                                            p.remark,
                                            p.update_time,
                                            p.medication_failed,
                                            p.visit_id,
                                            p.see_no,
                                            p.chief_doct_name,
                                            p.schema_id,
                                            max(medication_time)  as   medication_max_time,
                                            max(tem_medication_time) as temp_medication_max_time
                                         from patient p
                                                  left join(
                                             select patient_id, place_date_time medication_time, null tem_medication_time
                                             from medication
                                             union
                                             select patient_id, null medication_time, place_date_time tem_medication_time
                                             from temp_medication
                                         ) a
                                                           on a.patient_id = p.id
                                         where p.patient_type = 1
                                         group by p.id
                                    ) p
                            where {where}";
                }

                IList<Patient> patients = null;
                using (var conn = _hospitalContext.Database.GetDbConnection())
                {
                    conn.Open();
                    var command = conn.CreateCommand();
                    command.CommandText = sql;
                    command.CommandTimeout = 60;
                    patients = command.ExecuteReader().ReaderToList<Patient>();
                }
                var title = retry ? "重试" : "";
                _logger.LogInformation($"{title}待查询病人数量:{patients.Count()}");
                var exCount = 0;

                var groups = patients.GroupBy(p => p.DeptId).OrderBy(p => p.Key);
                var names = groups.Select(g => $"{g.FirstOrDefault().DeptName}({g.Count()})");
                _webProxy = _proxyHelper.GetProxy();
                foreach (var depatGroup in groups)
                {
                    foreach (var patient in depatGroup.OrderBy(p => p.UpdateTime))
                    {
                        var result = await QueryMedication(title, patient);
                        if (!result) exCount += 1;
                    }
                }

                _logger.LogInformation($"{title}查询完成耗时:{totalStopwatch.Elapsed.TotalSeconds}秒,查询总数:{patients.Count()},异常数:{exCount}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查询处方信息异常");
                await _telegramBotHelper.SendMessage($"查询住院病人处方信息异常：{ex.Message}");
            }
        }

        public void ResetMedicationQuery(DateTime inDateStart, DateTime inDateEnd)
        {
            try
            {
                var sql = $@"update patient set medication_failed=1 
                        where patient_type = 1  
                        and DATE_FORMAT(substring(in_date, 1, 10), '%Y-%m-%d')>='{inDateStart:yyyy-MM-dd}'
                        and DATE_FORMAT(substring(in_date, 1, 10), '%Y-%m-%d')<'{inDateEnd:yyyy-MM-dd}'
                        ";
                using (var conn = _hospitalContext.Database.GetDbConnection())
                {
                    conn.Open();
                    var command = conn.CreateCommand();
                    command.CommandText = sql;
                    command.CommandTimeout = 600;
                    var result = command.ExecuteNonQuery();

                    _logger.LogInformation($"新增{result}条 inPatient待复核记录");
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, $"ResetMedicationQuery 异常{ex.Message}");               
            }
        }


        public async Task QueryMedicationsByPatientName(string name, DateTime startDate, DateTime endDate)
        {
            try
            {
                var totalStopwatch = Stopwatch.StartNew();

                //_logger.LogInformation($"QueryMedicationsByPatientId patient_id={id}");

                //var patients = new List<Patient>();
                //patients = _hospitalContext.Patients.Where(d => d.Id == id).ToList();


                var sql = @$"select * from patient where name='{name}' and patient_type=1 
                            and DATE_FORMAT(substring(in_date,1,10), '%Y-%m-%d')>='{startDate:yyyy-MM-dd}'
                            and DATE_FORMAT(substring(in_date,1,10), '%Y-%m-%d')<'{endDate:yyyy-MM-dd}'
                            ";

                IList<Patient> patients = null;
                using (var conn = _hospitalContext.Database.GetDbConnection())
                {
                    conn.Open();
                    var command = conn.CreateCommand();
                    command.CommandText = sql;
                    command.CommandTimeout = 60;
                    patients = command.ExecuteReader().ReaderToList<Patient>();
                }

                var title = "";

                if (patients == null)
                {
                    _logger.LogInformation($"QueryMedicationsByPatientId patients==null");
                    return;
                }

                if (patients.Count <= 0)
                {
                    _logger.LogInformation($"****************************patients.Count<=0*************");
                   
                    return;
                }

                //_logger.LogInformation($"****************************开始处理数据*************");

                var exCount = 0;


                foreach (Patient p in patients)
                {
                    var result = await QueryMedication(title, patients[0]);                    
                    if (!result) exCount += 1;
                }

                _logger.LogInformation($"{title}查询完成耗时:{totalStopwatch.Elapsed.TotalSeconds}秒,查询总数:{patients.Count()},异常数:{exCount}");


                //_logger.LogInformation($"{title}待查询病人数量:{patients.Count()}");
                //var exCount = 0;

                //var groups = patients.GroupBy(p => p.DeptId).OrderBy(p => p.Key);
                //var names = groups.Select(g => $"{g.FirstOrDefault().DeptName}({g.Count()})");
                //_webProxy = _proxyHelper.GetProxy();
                //foreach (var depatGroup in groups)
                //{
                //    foreach (var patient in depatGroup.OrderBy(p => p.UpdateTime))
                //    {
                //        var result = await QueryMedication(title, patient);
                //        if (!result) exCount += 1;
                //    }
                //}

                //_logger.LogInformation($"{title}查询完成耗时:{totalStopwatch.Elapsed.TotalSeconds}秒,查询总数:{patients.Count()},异常数:{exCount}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"QueryMedicationsByPatientId 查询处方信息异常:{ex.Message}");
                //await _telegramBotHelper.SendMessage($"QueryMedicationsByPatientId 查询信息异常：{ex.Message}");
            }
        }


        public async Task<bool> QueryMedication(string title, Patient patient)
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();
                var token = _tokens[new Random(Guid.NewGuid().GetHashCode()).Next(0, _tokens.Length)];

                var medications = await _hospitalHelper.QueryLongTermOrderList(patient.Id, _webProxy, token);

                //我加入的
                //if (patient.MedicationMaxTime == null) patient.MedicationMaxTime = DateTime.Parse(patient.InDate);

                //找出最新处方
                var newMedications = patient.MedicationMaxTime == null ? medications : medications.Where(m => DateTime.Parse(m.PlaceDateTime) > patient.MedicationMaxTime);

                if (newMedications == null) return true;

                await _hospitalContext.Medications.AddRangeAsync(newMedications);

                if (stopwatch.ElapsedMilliseconds > 5000)  _webProxy = _proxyHelper.GetProxy();

                if (newMedications.Any())   patient.MedicationMaxTime = newMedications.Max(m => DateTime.Parse(m.PlaceDateTime));

                if (patient.MedicationMaxTime == null && medications.Any())
                {
                    patient.MedicationMaxTime = medications.Max(m => DateTime.Parse(m.PlaceDateTime));
                }

                var tempMedications = await _hospitalHelper.QueryTempOrderList(patient.Id, _webProxy, token);

                //_logger.LogInformation($"QueryMedication 查询结果:{JsonConvert.SerializeObject(tempMedications)}"); 

                var newTempMedications = patient.TempMedicationMaxTime == null ? tempMedications :
                    tempMedications.Where(m => DateTime.Parse(m.PlaceDateTime) > patient.TempMedicationMaxTime);

                if (newTempMedications == null) return true;

                await _hospitalContext.TempMedications.AddRangeAsync(newTempMedications);

                if (newTempMedications.Any())
                    patient.TempMedicationMaxTime = newTempMedications.Max(m => DateTime.Parse(m.PlaceDateTime));

                if (patient.TempMedicationMaxTime == null && tempMedications.Any())
                    patient.TempMedicationMaxTime = tempMedications.Max(m => DateTime.Parse(m.PlaceDateTime));

                var result = "";
                if (newMedications.Any() || newTempMedications.Any())
                {
                    patient.MedicationFailed = false;
                    result = "已更新处方信息";
                }
                else
                {
                    patient.MedicationFailed = true;
                    result = "未更新处方信息";
                }
                
                _hospitalContext.Update(patient);
                await _hospitalContext.SaveChangesAsync();
                _logger.LogInformation($"{title}查询科室:{patient.DeptName},病人:{patient.Name},{result},耗时:{stopwatch.ElapsedMilliseconds}毫秒");

            }
            catch (Exception ex)
            {
                patient.MedicationFailed = true;
                _hospitalContext.Update(patient);
                await _hospitalContext.SaveChangesAsync();
                _webProxy = _proxyHelper.GetProxy();
                _logger.LogError(ex, $"{title}查询病人:{patient.Name}处方信息异常");
                return false;
            }
            return true;
        }

    }
}
