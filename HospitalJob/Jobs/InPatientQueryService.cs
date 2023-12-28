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
    /// 住院病人数据
    /// </summary>
    [DisplayName("住院病人数据")]
    public class InPatientQueryService : IHangfireJobService
    {
        protected readonly ILogger _logger;
        protected readonly ILoggerFactory _loggerFactory;
        protected readonly IConfiguration _configuration;
        protected readonly HospitalContext _hospitalContext;
        protected readonly ProxyHelper _proxyHelper;
        protected readonly HospitalHelper _hospitalHelper;
        private readonly Dictionary<string, string> _depts;
        protected readonly string[] _tokens;
        private readonly TelegramBotHelper _telegramBotHelper;
        public InPatientQueryService(ILoggerFactory logger,
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
            _tokens = (_configuration["HospitalTokens"] ?? "").Split(';');
            _telegramBotHelper = telegramBotHelper;
        }
        public async Task Excute(PerformContext context)
        {
            try
            {
                var values = _depts.Values.ToList();
                var totalStopwatch = Stopwatch.StartNew();
                var newPatients = 0;
                WebProxy wp = _proxyHelper.GetProxy();

                foreach (var dept in _depts)
                {
                    for (int tryIndex = 0; tryIndex < 3; tryIndex++)
                    {
                        if (tryIndex > 0)
                        {
                            wp = _proxyHelper.GetProxy();
                            _logger.LogInformation($"第{tryIndex}次查询科室信息");
                        }

                        //科室查询，查询失败继续尝试，共3次
                        if (QueryByDepartment(dept, wp, ref newPatients)) break; 
                        else if (tryIndex == 2)
                        {
                            await _telegramBotHelper.SendMessage($"查询住院病人信息，第{tryIndex}次查询科室信息异常，科室名称：{dept.Value}");
                        }
                    }
                }
                _logger.LogInformation($"新增住院病人:{newPatients}位,查询完成耗时:{totalStopwatch.Elapsed.TotalSeconds}秒");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查询病人信息异常");
                await _telegramBotHelper.SendMessage($"查询病人信息异常:{ex.Message}");
            }
            await Task.CompletedTask;
        }

        public bool QueryByDepartment(KeyValuePair<string, string> dept, IWebProxy wp, ref int newPatients)
        {
            try
            {
                var token = _tokens[new Random(Guid.NewGuid().GetHashCode()).Next(0, _tokens.Length)];


                var patients = _hospitalHelper.QueryDeptPatientList(dept.Key, wp, token).Result;


                foreach (var patient in patients)
                {
                    var stopwatch = Stopwatch.StartNew();

                    //查看记录是否存在
                    var patientExist = _hospitalContext.Patients.FirstOrDefault(p => p.Id == patient.Id);

                    if (patientExist == null) //不存在,增加
                    {
                        newPatients += 1;
                        patient.PatientType = PatientType.inpatient;
                        patient.CreateTime = DateTime.Now;
                        patient.UpdateTime = DateTime.Now;
                        if (string.IsNullOrEmpty(patient.VisitId))
                            patient.VisitId = patient.Id.Substring(4, 10);
                        _hospitalContext.Patients.Add(patient);
                    }
                    else //存在,更新
                    {
                        if (string.IsNullOrEmpty(patientExist.ChiefDoctName)) patientExist.ChiefDoctName = patient.ChiefDoctName;
                        if (string.IsNullOrEmpty(patientExist.VisitId))
                            patientExist.VisitId = patientExist.Id.Substring(4, 10);
                        if (patientExist.DeptId != patient.DeptId)
                        {
                            patientExist.Remark = $"{patientExist.Remark}{patientExist.DeptName};";
                            patientExist.DeptId = patient.DeptId;
                            patientExist.DeptName = patient.DeptName;
                            patientExist.UpdateTime = DateTime.Now;
                            _hospitalContext.Update(patientExist);
                        }
                        else
                        {
                            patientExist.UpdateTime = DateTime.Now;
                            _hospitalContext.Update(patientExist);
                        }
                        _logger.LogInformation($"{DateTime.Now:HH:mm:ss fff} 正在查询科室:{dept.Value},病人:{patient.Name}数据已存在");
                    }
                    _logger.LogInformation($"{DateTime.Now:HH:mm:ss fff} 查询科室:{dept.Value},病人:{patient.Name},耗时:{stopwatch.ElapsedMilliseconds}毫秒");
                }

                _hospitalContext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"查询住院病人信息异常,科室:{dept.Value}");
            }
            return false;
        }

    }
}
