using Hangfire.Server;
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
    /// 医生门诊排班数据
    /// </summary>
    [DisplayName("医生门诊排班数据")]
    public class QueryDoctScheduleService
    {
        protected readonly ILogger _logger;
        protected readonly ILoggerFactory _loggerFactory;
        protected readonly IConfiguration _configuration;
        protected readonly HospitalContext _hospitalContext;
        protected readonly ProxyHelper _proxyHelper;
        protected readonly PatientHelper _patientHelper;
        private IWebProxy _webProxy;
        private readonly TelegramBotHelper _telegramBotHelper;
        public QueryDoctScheduleService(ILoggerFactory logger,
                                        IConfiguration configuration,
                                        HospitalContext myContext,
                                        ProxyHelper proxyHelper,
                                        PatientHelper patientHelper,
                                        TelegramBotHelper telegramBotHelper)
        {
            _logger = logger.CreateLogger(this.GetType());
            _loggerFactory = logger;
            _hospitalContext = myContext;
            _configuration = configuration;
            _proxyHelper = proxyHelper;
            _patientHelper = patientHelper;
            _telegramBotHelper = telegramBotHelper;
        }
        public async Task Excute(PerformContext context)
        {
            //var totalStopwatch = Stopwatch.StartNew();
            //var _webProxy = _proxyHelper.GetProxy("us");//_proxyHelper.GetProxy();
            try
            {
                //var outpatientsContent = LogHelper.Instance.ReadFile($"{AppDomain.CurrentDomain.BaseDirectory}/Config/Outpatients.json");
                //if (string.IsNullOrEmpty(outpatientsContent))
                //{
                //    _logger.LogInformation("不存在门诊信息");
                //    return;
                //}
                //var outpatients = JsonConvert.DeserializeObject<JToken>(outpatientsContent);
                //var scheduleExits = _hospitalContext.DoctSchedules.Where(d => d.VisitDate >= DateTime.Today).ToList();
                //var exceptions = new List<string>();
                //foreach (var outpatient in outpatients)
                //{
                //    var ddId = outpatient["ddId"].ToString();
                //    var ddName = outpatient["ddName"].ToString();
                //    var orgID = outpatient["orgID"].ToString();
                //    var scheduleDates = await QueryDeptScheduleDateForWXNew(ddId, orgID, outpatient["ddName"].ToString());
                //    if (scheduleDates == null || scheduleDates.SelectToken("dateList") == null)
                //    {
                //        exceptions.Add($"查询门诊:{orgID},{ddId},{ddName}失败!");
                //        continue;
                //    }
                //    var newVisit = new List<string>();
                //    foreach (var scheduleDate in scheduleDates.SelectToken("dateList"))
                //    {
                //        var schedules = await QueryOneDayScheduleListEntity(scheduleDate["scheduleDate"].ToString(), ddId, orgID, ddName);
                //        if (schedules == null)
                //        {
                //            var date = DateTimeHelper.ConvertStringToDateTime(scheduleDate["scheduleDate"].ToString());
                //            exceptions.Add($"查询{date:yyyy-MM-dd}门诊:{orgID},{ddId},{ddName},{scheduleDate["scheduleDate"]}失败!");
                //            continue;
                //        }
                //        var newSchedules = schedules.Where(s => !scheduleExits.Any(e => s.RegPointID == s.RegPointID));
                //        newVisit.AddRange(newSchedules.Select(s => s.VisitDate.ToShortDateString()));
                //        await _hospitalContext.AddRangeAsync(newSchedules);
                //    }
                //    _logger.LogInformation($"查询门诊:{orgID},{ddId},{ddName},新增日程:{string.Join(",", newVisit.Distinct())}");
                //    await _hospitalContext.SaveChangesAsync();
                //}
                //_logger.LogInformation($"查询完成耗时:{totalStopwatch.Elapsed.TotalSeconds}秒");
                //if (exceptions.Any())
                //{
                //    var msg = $"查询医生门诊异常:{Environment.NewLine}{string.Join(Environment.NewLine, exceptions)}";
                //    _logger.LogError(msg);
                //    await _telegramBotHelper.SendMessage(msg);
                //}

                await QueryDoctSchedule(DateTime.Today);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"查询医生门诊异常");
                await _telegramBotHelper.SendMessage($"查询医生门诊异常：{ex.Message}");
            }
        }

        public async Task QueryDoctSchedule(DateTime startDate)
        {
            _logger.LogInformation($"******************开始执行排班任务 startDate={startDate:d}*****************");

            var totalStopwatch = Stopwatch.StartNew();
            _webProxy = _proxyHelper.GetProxy();//_proxyHelper.GetProxy();
            try
            {
                var outpatientsContent = LogHelper.Instance.ReadFile($"{AppDomain.CurrentDomain.BaseDirectory}/Config/Outpatients.json");
                if (string.IsNullOrEmpty(outpatientsContent))
                {
                    _logger.LogInformation("不存在排班门诊信息");
                    return;
                }
                var outpatients = JsonConvert.DeserializeObject<JToken>(outpatientsContent);
                var scheduleExits = _hospitalContext.DoctSchedules.Where(d => d.VisitDate >= startDate).ToList();

                var exceptions = new List<string>();

                foreach (var outpatient in outpatients)
                {
                    var ddId = outpatient["ddId"].ToString();
                    var ddName = outpatient["ddName"].ToString();
                    var orgID = outpatient["orgID"].ToString();
                    var scheduleDates = await QueryDeptScheduleDateForWXNew(ddId, orgID, outpatient["ddName"].ToString());
                    if (scheduleDates == null || scheduleDates.SelectToken("dateList") == null)
                    {
                        exceptions.Add($"查询排班门诊:{orgID},{ddId},{ddName}失败!");
                        continue;
                    }
                    var newVisit = new List<string>();
                    foreach (var scheduleDate in scheduleDates.SelectToken("dateList"))
                    {
                        var schedules = await QueryOneDayScheduleListEntity(scheduleDate["scheduleDate"].ToString(), ddId, orgID, ddName);
                        if (schedules == null)
                        {
                            var date = DateTimeHelper.ConvertStringToDateTime(scheduleDate["scheduleDate"].ToString());
                            exceptions.Add($"查询{date:yyyy-MM-dd}排班门诊:{orgID},{ddId},{ddName},{scheduleDate["scheduleDate"]}失败!");
                            continue;
                        }
                        var newSchedules = schedules.Where(s => !scheduleExits.Any(e => s.RegPointID == s.RegPointID));
                        newVisit.AddRange(newSchedules.Select(s => s.VisitDate.ToShortDateString()));
                        await _hospitalContext.AddRangeAsync(newSchedules);
                    }

                    _logger.LogInformation($"查询医生排班门诊:{orgID},{ddId},{ddName},新增日程:{string.Join(",", newVisit.Distinct())}");
                    await _hospitalContext.SaveChangesAsync();
                }
                _logger.LogInformation($"查询医生排班门诊完成耗时:{totalStopwatch.Elapsed.TotalSeconds}秒");
                if (exceptions.Any())
                {
                    var msg = $"查询医生排班门诊异常:{Environment.NewLine}{string.Join(Environment.NewLine, exceptions)}";
                    _logger.LogError(msg);
                    await _telegramBotHelper.SendMessage(msg);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"查询医生排班门诊异常");
                await _telegramBotHelper.SendMessage($"查询医生排班门诊异常：{ex.Message}");
            }
        }

        private async Task<JToken> QueryDeptScheduleDateForWXNew(string ddId, string orgID, string ddName)
        {
            JToken scheduleDates = null;
            for (int tryIndex = 0; tryIndex < 3; tryIndex++)
            {
                try
                {
                    if (tryIndex > 0)
                        _logger.LogWarning($"第{tryIndex}次查询门诊:{orgID},{ddId},{ddName}");
                    scheduleDates = await _patientHelper.QueryDeptScheduleDateForWXNew(ddId, orgID, _webProxy);
                    if (scheduleDates != null)
                        break;
                    _webProxy = _proxyHelper.GetProxy();
                }
                catch (Exception ex)
                {
                    _logger.LogError($"查询门诊:{orgID},{ddId},{ddName}异常:{ex.Message}");
                    _webProxy = _proxyHelper.GetProxy();
                }
            }
            if (scheduleDates == null)
                scheduleDates = await _patientHelper.QueryDeptScheduleDateForWXNew(ddId, orgID);

            return scheduleDates;
        }
        private async Task<List<DoctSchedule>> QueryOneDayScheduleListEntity(string scheduleDate, string ddId, string orgID, string ddName)
        {
            List<DoctSchedule> schedules = null;
            for (int tryIndex = 0; tryIndex < 3; tryIndex++)
            {
                try
                {
                    if (tryIndex > 0)
                        _logger.LogWarning($"第{tryIndex}次查询{DateTimeHelper.ConvertStringToDateTime(scheduleDate)}门诊:{orgID},{ddId},{ddName}");
                    schedules = await _patientHelper.QueryOneDayScheduleListEntity(scheduleDate, ddId, orgID, _webProxy);
                    if (schedules != null)
                        break;
                    _webProxy = _proxyHelper.GetProxy();
                }
                catch (Exception ex)
                {
                    _logger.LogError($"查询{DateTimeHelper.ConvertStringToDateTime(scheduleDate)}门诊:{orgID},{ddId},{ddName}异常:{ex.Message}");
                    _webProxy = _proxyHelper.GetProxy();
                }
            }
            if (schedules == null)
                schedules = await _patientHelper.QueryOneDayScheduleListEntity(scheduleDate, ddId, orgID);
            return schedules;
        }

    }
}
