using Hangfire.Server;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using VaeDbContext;
using VaeEntity.Hospital;

namespace HospitalJob.Jobs
{
    public class OutpatientEmptyMedicationResetService
    {
        protected readonly ILogger _logger;
        protected readonly ILoggerFactory _loggerFactory;
        protected readonly HospitalContext _hospitalContext;
        protected readonly IConfiguration _configuration;

        public OutpatientEmptyMedicationResetService(ILoggerFactory logger, IConfiguration configuration, HospitalContext myContext)
        {
            _loggerFactory = logger;
            _logger = logger.CreateLogger(this.GetType());
            _configuration = configuration;
            _hospitalContext = myContext;
        }
        public async Task Excute(PerformContext context)
        {
            var monthDate = DateTime.Today.Day > 15 ?
                new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1) :
                new DateTime(DateTime.Today.AddMonths(-1).Year, DateTime.Today.AddMonths(-1).Month, 1);
            ResetOutpatientMedicationQuery(monthDate);
            await Task.CompletedTask;
        }

        public void ResetOutpatientMedicationQuery(DateTime startDate)
        {
            var sql = @$"select distinct p.id
                        from patient p
                                 left join out_patient_medication m on p.id = m.patient_id
                        where DATE_FORMAT(substring(in_date, 1, 10), '%Y-%m-%d') >= '{startDate:yyyy-MM-dd}'
                          and p.patient_type = 0
                          and medication_failed = 0
                          and m.id is null;";
            IList<Patient> patients;
            var updateCount = 0;
            using (var conn = _hospitalContext.Database.GetDbConnection())
            {
                conn.Open();
                var command = conn.CreateCommand();
                command.CommandText = sql;
                command.CommandTimeout = 600;
                patients = command.ExecuteReader().ReaderToList<Patient>();
                if (!patients.Any())
                {
                    _logger.LogInformation("不存在已查询处方的空处方病人");
                    return;
                }
            }
            var ids = patients.Select(p => p.Id).ToList();
            _hospitalContext.Database.SetCommandTimeout(600);
            var pageSize = 10000;
            var pages = Math.Ceiling((double)ids.Count / pageSize);
            for (int pageIndex = 0; pageIndex < pages; pageIndex++)
            {
                var idPage = ids.Skip(pageIndex * pageSize).Take(pageSize);
                var idsParam = _hospitalContext.CreateParameter("@ids", string.Join(",", idPage));
                updateCount += _hospitalContext.Database.ExecuteSqlRaw("update patient set medication_failed=1 where FIND_IN_SET (id,@ids)", idsParam);
            }
            _logger.LogInformation($"{startDate:yyyy-MM-dd}后共{patients?.Count}条没有处方记录,实际更新{updateCount}条记录");
        }
    }
}
