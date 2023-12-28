using Hangfire.Server;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Spire.Pdf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using VaeDbContext;
using VaeEntity.Hospital;
using VaeHelper;

namespace HospitalJob.Jobs
{
    [DisplayName("查询门诊病人PDF")]
    public class QueryOutPatientEmrPDFService
    {
        protected readonly ILogger _logger;
        protected readonly ILoggerFactory _loggerFactory;
        protected readonly IConfiguration _configuration;
        protected readonly HospitalContext _hospitalContext;
        protected readonly ProxyHelper _proxyHelper;
        protected readonly PatientHelper _patientHelper;
        protected readonly HospitalHelper _hospitalHelper;
        protected readonly string[] _tokens;

        public QueryOutPatientEmrPDFService(ILoggerFactory logger,
                                            IConfiguration configuration,
                                            HospitalContext myContext,
                                            ProxyHelper proxyHelper,
                                            PatientHelper patientHelper,
                                            HospitalHelper hospitalHelper)
        {
            _logger = logger.CreateLogger(this.GetType());
            _loggerFactory = logger;
            _hospitalContext = myContext;
            _configuration = configuration;
            _proxyHelper = proxyHelper;
            _patientHelper = patientHelper;
            _hospitalHelper = hospitalHelper;
            _tokens = (_configuration["HospitalTokens"] ?? "").Split(';');
        }
        public void Excute(string medicationName, DateTime startDate, DateTime endDate)
        {
            try
            {
                QueryPDF(medicationName, startDate, endDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查询门诊病人PDF异常");
            }
        }
        public void QueryPDF(string medicationName, DateTime startDate, DateTime endDate)
        {
            var sql = $@"select m.id,
                           m.patient_id,
                           m.name,
                           m.doct_name,
                           m.group_id,
                           m.dosage,
                           p.schema_id frequency,
                           m.`usage`,
                           m.unit,
                           m.begin_date_time,
                           m.create_time,
                           m.prescript,
                           m.prescript_url,
                           m.usage_detail
                        from out_patient_medication m
                                 join patient p on p.id = m.patient_id
                        where m.name like '%{medicationName}%'
                          and begin_date_time >= '{startDate:yyyy-MM-dd}'
                          and begin_date_time < '{endDate:yyyy-MM-dd}' and usage_detail is null ";


            IList<OutPatientMedication> prescripts = null;
            using (var conn = _hospitalContext.Database.GetDbConnection())
            {
                conn.Open();
                var command = conn.CreateCommand();
                command.CommandText = sql;
                command.CommandTimeout = 60;
                prescripts = command.ExecuteReader().ReaderToList<OutPatientMedication>();
            }

            _logger.LogInformation($"待查询病人处方单:{prescripts.Count}条,产品名称:{medicationName},日期:{startDate:yyyy-MM-dd}~{endDate:yyyy-MM-dd}");

            int getProxyTimeount = 3000;
            var webProxy = _proxyHelper.GetProxy(timeOut: getProxyTimeount);
            foreach (var schemaGroups in prescripts.GroupBy(p => p.Frequency))
            {
                if (webProxy == null) webProxy = _proxyHelper.GetProxy(timeOut: getProxyTimeount);
                var token = _tokens[new Random(Guid.NewGuid().GetHashCode()).Next(0, _tokens.Length)];
                List<Patient> schemaPatients = null;
                try
                {
                    schemaPatients = _hospitalHelper.QuerySchemaInpatientList(schemaGroups.Key, webProxy, token).Result;
                }
                catch (Exception ex)
                {
                    webProxy = _proxyHelper.GetProxy(timeOut: getProxyTimeount);
                    _logger.LogError(ex, "查询门诊信息异常");
                }

                if (schemaPatients == null) continue;
                foreach (var prescript in schemaGroups)
                {
                    try
                    {
                        var patient = schemaPatients.FirstOrDefault(p => p.Id == prescript.PatientId);
                        var inPatientId = JObject.Parse(patient.PatientContent)["patientNo"].ToString();
                        var emrPdfList = _hospitalHelper.GetInPatientEmrPDFList(inPatientId, webProxy).Result;

                        var inpatientPdfs = emrPdfList.Where(e => e["visitSqNo"].ToString() == prescript.PatientId && e["documentAuthorName"].ToString() == prescript.DoctorName);
                        if (!inpatientPdfs.Any())
                        {
                            inpatientPdfs = emrPdfList.Where(e => e["visitSqNo"].ToString() == prescript.PatientId &&
                              DateTimeHelper.ConvertLongToDateTime(Convert.ToInt64(e["createTime"])).Date == DateTime.Parse(prescript.BeginDateTime).Date);
                            if (!inpatientPdfs.Any()) continue;
                        }

                        //prescript.Usage = "--";

                        foreach (var inpatientPdf in inpatientPdfs)
                        {
                            var pdfUrl = _hospitalHelper.GetEmrPDFDetail(inpatientPdf["documentNo"]?.ToString(), webProxy).Result;

                            StringBuilder content = new StringBuilder();
                            var pdfBytes = HttpHelper.HttpDownload(pdfUrl, webProxy);

                            PdfDocument doc = new PdfDocument();
                            doc.LoadFromBytes(pdfBytes);

                            foreach (PdfPageBase page in doc.Pages)
                                content.Append(page.ExtractText());
                            var lines = content.ToString().Split(Environment.NewLine);

                            //找药名
                            var target = lines?.FirstOrDefault(l => l.Contains(medicationName) && l.Contains("总量"));

                            if (!string.IsNullOrEmpty(target))
                            {
                                prescript.PrescriptUrl = pdfUrl;
                                prescript.Prescript = content.ToString();
                                prescript.UsageDetail = target.Trim();
                                var usage = prescript.UsageDetail.Split(',').FirstOrDefault(d => d.Contains("总量"));
                                if (!string.IsNullOrEmpty(usage))
                                    prescript.Usage = usage.Replace("总量:", "").Trim().Replace(prescript.Unit, "");

                                _logger.LogInformation($"PatientId:{prescript.PatientId},prescriptId:{prescript.Id},{prescript.UsageDetail}");
                                break;
                            }
                            else
                                _logger.LogInformation($"PatientId:{prescript.PatientId},prescriptId:{prescript.Id},未查询到有效PDF");
                        }

                        _hospitalContext.Update(prescript);
                    }
                    catch (Exception ex)
                    {
                        webProxy = _proxyHelper.GetProxy(timeOut: getProxyTimeount);
                        _logger.LogError(ex, "查询PDF异常");
                    }
                }

                try
                {
                    _hospitalContext.SaveChanges();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "提交数据异常");
                }
            }
        }
    }
}
