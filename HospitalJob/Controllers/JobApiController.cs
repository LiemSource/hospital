using BotHelper;
using Hangfire;
using HospitalJob.Jobs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using VaeEntity;
using VaeHelper;

namespace HospitalJob.Controllers
{
    [Route("[controller]")]
    [ApiController]   
    public class JobApiController : Controller
    {
        protected readonly ILogger _logger;
        private readonly QueryOutPatientEmrPDFService _queryOutPatientEmrPDFService;
        private readonly OutPatientMedicationQueryService _outPatientMedicationQueryService;
        private readonly OutPatientQueryService _outPatientQueryService;
        private readonly OutpatientEmptyMedicationResetService _outpatientEmptyMedicationResetService;
        private readonly PatientVisitReviewedJob _inPatientHistoryQueryService;
        private readonly TelegramBotHelper _telegramBotHelper;
        private readonly QueryDoctScheduleService _queryDoctScheduleService;
        private readonly MedicationQueryService _medicationQueryService;

        public JobApiController(ILoggerFactory logger,
                                QueryOutPatientEmrPDFService queryOutPatientEmrPDFService,
                                OutPatientMedicationQueryService outPatientMedicationQueryService,
                                OutPatientQueryService outPatientQueryService,
                                OutpatientEmptyMedicationResetService outpatientEmptyMedicationResetService,
                                PatientVisitReviewedJob inPatientHistoryQueryService,
                                TelegramBotHelper telegramBotHelper,
                                QueryDoctScheduleService queryDoctScheduleService,
                                MedicationQueryService medicationQueryService)
        {
            _logger = logger.CreateLogger(this.GetType());
            _queryOutPatientEmrPDFService = queryOutPatientEmrPDFService;
            _outPatientMedicationQueryService = outPatientMedicationQueryService;
            _outPatientQueryService = outPatientQueryService;
            _outpatientEmptyMedicationResetService = outpatientEmptyMedicationResetService;
            _inPatientHistoryQueryService = inPatientHistoryQueryService;
            _telegramBotHelper = telegramBotHelper;
            _queryDoctScheduleService = queryDoctScheduleService;
            _medicationQueryService = medicationQueryService;
        }

        [HttpGet]
        public string Get()
        {           
            return "hello world";
        }


        [HttpGet("status")]
        public JsonResult Status()
        {
            return Json(new { RetCode = RetCode.BizOK, RetMessage = "正常" });
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="startSchema"></param>
        /// <param name="endSchema"></param>
        /// <param name="monthDate"></param>
        /// <returns></returns>
        [HttpGet("patient/query")]
        public JsonResult QueryPatientInfo(string pid)
        {
            try
            {
                //BackgroundJob.Enqueue(() => _queryDoctScheduleService.QueryDoctSchedule(startDate));
                _inPatientHistoryQueryService.Test(pid);
                return Json(new { RetCode = RetCode.BizOK, RetMessage = "job action ok" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发起Job:QueryDoctSchedule");
                return Json(new { RetCode = RetCode.BizError, RetMessage = "job调度异常" });
            }
        }


        /// <summary>
        /// 门诊排班
        /// </summary>
        /// <param name="startSchema"></param>
        /// <param name="endSchema"></param>
        /// <param name="monthDate"></param>
        /// <returns></returns>
        [HttpGet("outPatient/DoctSchedule")]
        public JsonResult QueryDoctSchedule(DateTime startDate)
        {
            try
            {
                BackgroundJob.Enqueue(() => _queryDoctScheduleService.QueryDoctSchedule(startDate));
                return Json(new { RetCode = RetCode.BizOK, RetMessage = "job调度成功" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发起Job:QueryDoctSchedule");
                return Json(new { RetCode = RetCode.BizError, RetMessage = "job调度异常" });
            }
        }


        /// <summary>
        /// 重试查询门诊病人
        /// </summary>
        /// <param name="startSchema"></param>
        /// <param name="endSchema"></param>
        /// <param name="monthDate"></param>
        /// <returns></returns>
        [HttpGet("outPatient/patients")]
        public JsonResult QueryOutPatientRetrieval(int startSchema, int endSchema, DateTime monthDate)
        {
            try
            {
                BackgroundJob.Enqueue(() => _outPatientQueryService.QueryOutPatientRetrieval(startSchema, endSchema, monthDate));
                //BackgroundJob.Enqueue(() => _outPatientQueryService.test(startSchema));
                return Json(new { RetCode = RetCode.BizOK, RetMessage = "job ok" });

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发起Job:QueryOutPatientRetrieval异常");
                return Json(new { RetCode = RetCode.BizError, RetMessage = "job调度异常" });
            }
        }

        /// <summary>
        /// 重试查询门诊病人,通过排班数据
        /// </summary>
        /// <param name="dateStart"></param>
        /// <param name="dateEnd"></param>
        /// <returns></returns>
        [HttpGet("outPatient/queryByDate")]
        public JsonResult QueryOutPatientDate(DateTime startDate)
        {
            try
            {
                BackgroundJob.Enqueue(() => _outPatientQueryService.ExcuteQueryOutPatient(startDate));
                return Json(new { RetCode = RetCode.BizOK, RetMessage = "job调度成功" });

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发起Job:QueryOutPatientDate异常");
                return Json(new { RetCode = RetCode.BizError, RetMessage = "job调度异常" });
            }
        }

        /// <summary>
        ///  查询门诊处方单
        /// </summary>
        /// <param name="name"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        [HttpGet("outPatient/PDF")]
        public JsonResult QueryOutPatientEmrPDF(string name, DateTime startDate, DateTime endDate)
        {
            try
            {
                BackgroundJob.Enqueue(() => _queryOutPatientEmrPDFService.Excute(name, startDate, endDate));
                return Json(new { RetCode = RetCode.BizOK, RetMessage = "job调度成功" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发起Job:QueryOutPatientEmrPDF异常");
                return Json(new { RetCode = RetCode.BizError, RetMessage = "job调度异常" });
            }
        }


        /// <summary>
        /// 根据ID查询病人药方
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="sectionCount"></param>
        /// <param name="sectionIndex"></param>
        /// <returns></returns>
        [HttpGet("inPatient/medicationsByName")]
        public JsonResult QueryinPatientMedicationsByPName(string name, DateTime startDate, DateTime endDate)
        {
            try
            {
                
                _medicationQueryService.QueryMedicationsByPatientName(name, startDate, endDate).Wait();
                //BackgroundJob.Enqueue(() => _medicationQueryService.QueryMedicationsByPatientId(pid));
                return Json(new { RetCode = RetCode.BizOK, RetMessage = "job调度成功" });

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发起Job:QueryOutPatientMedicationsBySection异常");
                return Json(new { RetCode = RetCode.BizError, RetMessage = "job调度异常" });
            }
        }

        /// <summary>
        /// 重设inPatient 的查询结果为FAIL
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="sectionCount"></param>
        /// <param name="sectionIndex"></param>
        /// <returns></returns>
        [HttpGet("inPatient/resetMedicationsQuery")]
        public JsonResult ResetMedicationQuery(DateTime startDate, DateTime endDate)
        {
            try
            {
                _medicationQueryService.ResetMedicationQuery(startDate, endDate);
                return Json(new { RetCode = RetCode.BizOK, RetMessage = "job ok" });

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发起Job:QueryOutPatientMedicationsBySection异常");
                return Json(new { RetCode = RetCode.BizError, RetMessage = "job调度异常" });
            }
        }

        /// <summary>
        /// 重新查询 inPatient Medication
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="sectionCount"></param>
        /// <param name="sectionIndex"></param>
        /// <returns></returns>
        [HttpGet("inPatient/retryMedicationsQuery")]
        public JsonResult RetryMedicationQuery(DateTime startDate, DateTime endDate)
        {
            try
            {              
                
                BackgroundJob.Enqueue(() => _medicationQueryService.QueryMedications(true,startDate,endDate));

                return Json(new { RetCode = RetCode.BizOK, RetMessage = "job ok" });

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发起Job:QueryOutPatientMedicationsBySection异常");
                return Json(new { RetCode = RetCode.BizError, RetMessage = "job调度异常" });
            }
        }



        /// <summary>
        /// 分片查询门诊病人药方
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="sectionCount"></param>
        /// <param name="sectionIndex"></param>
        /// <returns></returns>
        [HttpGet("outPatient/medications")]
        public JsonResult QueryOutPatientMedicationsBySection(DateTime startDate, DateTime endDate, int sectionCount, int sectionIndex)
        {
            try
            {
                BackgroundJob.Enqueue(() => _outPatientMedicationQueryService.QueryOutPatientMedications(startDate, endDate, sectionCount, sectionIndex));
                return Json(new { RetCode = RetCode.BizOK, RetMessage = "job调度成功" });

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发起Job:QueryOutPatientMedicationsBySection异常");
                return Json(new { RetCode = RetCode.BizError, RetMessage = "job调度异常" });
            }
        }


        [HttpGet("outPatient/medicationsByName")]
        public JsonResult QueryOutPatientMedicationsByName(string name,DateTime startDate, DateTime endDate)
        {
            try
            {
                _outPatientMedicationQueryService.QueryMedicationByPatientName(name, startDate, endDate).Wait();
                //BackgroundJob.Enqueue(() => _outPatientMedicationQueryService.QueryMedicationByPatientName(name, startDate, endDate));
                return Json(new { RetCode = RetCode.BizOK, RetMessage = "job ok" });

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发起Job:QueryOutPatientMedicationsBySection异常");
                return Json(new { RetCode = RetCode.BizError, RetMessage = "job调度异常" });
            }
        }


        /// <summary>
        /// 分片查询门诊病人药方
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="sectionCount"></param>
        /// <param name="sectionIndex"></param>
        /// <returns></returns>
        [HttpGet("outPatient/medicationsN")]
        public JsonResult QueryOutPatientMedicationsBySectionN(DateTime startDate, DateTime endDate, int sectionCount, int sectionIndex)
        {
            try
            {
                BackgroundJob.Enqueue(() => _outPatientMedicationQueryService.QueryOutPatientMedicationsN(startDate, endDate, sectionCount, sectionIndex));
                return Json(new { RetCode = RetCode.BizOK, RetMessage = "job successed" });

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发起Job:QueryOutPatientMedicationsBySectionN异常");
                return Json(new { RetCode = RetCode.BizError, RetMessage = "job调度异常" });
            }
        }

        /// <summary>
        /// 重置门诊空处方病人查询状态
        /// </summary>
        /// <param name="startDate"></param>
        /// <returns></returns>
        [HttpGet("outPatient/resetMedicationQuery")]
        public JsonResult ResetOutpatientMedicationQuery(DateTime startDate)
        {
            try
            {
                BackgroundJob.Enqueue(() => _outpatientEmptyMedicationResetService.ResetOutpatientMedicationQuery(startDate));
                return Json(new { RetCode = RetCode.BizOK, RetMessage = "job调度成功" });

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发起Job:ResetOutpatientMedicationQuery异常");
                return Json(new { RetCode = RetCode.BizError, RetMessage = "job调度异常" });
            }
        }

        /// <summary>
        /// 查询病人就诊历史记录
        /// </summary>
        /// <param name="startDate"></param>
        /// <returns></returns>
        [HttpGet("inPatient/Excutesql")]
        public JsonResult Excutesql(DateTime startDate, DateTime endDate, bool orderByDesc)
        {
            try
            {
                _inPatientHistoryQueryService.Excute1();
                //BackgroundJob.Enqueue(() => _inPatientHistoryQueryService.Query(startDate, endDate, orderByDesc));
                return Json(new { RetCode = RetCode.BizOK, RetMessage = $"job {startDate} | {endDate}" });

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发起Job:InPatientHistoryQueryServiceQuery");
                return Json(new { RetCode = RetCode.BizError, RetMessage = "job调度异常" });
            }
        }

        /// <summary>
        /// 查询病人就诊历史记录
        /// </summary>
        /// <param name="startDate"></param>
        /// <returns></returns>
        [HttpGet("inPatient/visitHistoryQuery")]
        public JsonResult InPatientHistoryQueryServiceQuery(DateTime startDate, DateTime endDate, bool orderByDesc)
        {
            try
            {
                //BackgroundJob.Enqueue(() => _inPatientHistoryQueryService.Query(startDate, endDate, orderByDesc));
                BackgroundJob.Enqueue(() => _inPatientHistoryQueryService.ReviewedJob(startDate, endDate, orderByDesc,null));
                return Json(new { RetCode = RetCode.BizOK, RetMessage = "job调度成功" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发起Job:InPatientHistoryQueryServiceQuery");
                return Json(new { RetCode = RetCode.BizError, RetMessage = "job调度异常" });
            }
        }

        /// <summary>
        /// 通过姓名查询病人就诊历史记录
        /// </summary>
        /// <param name="startDate"></param>
        /// <returns></returns>
        [HttpGet("inPatient/visitHistoryQueryByVisitId")]
        public JsonResult InPatientHistoryQueryServiceQueryByName(DateTime startDate, DateTime endDate, string visitId)
        {
            try
            {
                var orderByDesc = false;
                _inPatientHistoryQueryService.ReviewedJob(startDate, endDate, orderByDesc, visitId).Wait();
                //BackgroundJob.Enqueue(() => _inPatientHistoryQueryService.ReviewedJob(startDate, endDate, orderByDesc, name));
                return Json(new { RetCode = RetCode.BizOK, RetMessage = "job ok" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发起Job:InPatientHistoryQueryServiceQuery");
                return Json(new { RetCode = RetCode.BizError, RetMessage = "job调度异常" });
            }
        }

        /// <summary>
        /// 查询病人就诊历史记录
        /// </summary>
        /// <param name="startDate"></param>
        /// <returns></returns>
        [HttpGet("inPatient/reviewQuery")]
        public JsonResult InPatientHistoryReviewQuery(DateTime startDate, DateTime endDate, bool orderByDesc)
        {
            try
            {
                //BackgroundJob.Enqueue(() => _inPatientHistoryQueryService.Query(startDate, endDate, orderByDesc));
                BackgroundJob.Enqueue(() => _inPatientHistoryQueryService.Query(startDate, endDate, orderByDesc));
                //_inPatientHistoryQueryService.Query(startDate, endDate, orderByDesc);
                return Json(new { RetCode = RetCode.BizOK, RetMessage = "job调度成功" });

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发起Job:InPatientHistoryReviewQuery");
                return Json(new { RetCode = RetCode.BizError, RetMessage = "job调度异常" });
            }
        }

    }
}
