using Autofac;
using HospitalJob.Jobs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using VaeDbContext;

namespace VaeHelperTests
{
    [TestClass()]
    public class HospitalJobServiceTest
    {

        private IContainer container;
        public HospitalJobServiceTest()
        {
            container = TestInitialize.Initialize<HospitalJob.Startup>();
        }

        [TestMethod()]
        public void PatientQueryServiceTest()
        {
            var polyTaskService = container.Resolve<InPatientQueryService>();
            polyTaskService.Excute(null).Wait();
        }
        [TestMethod()]
        public void OutPatientQueryServiceTest()
        {
            var polyTaskService = container.Resolve<OutPatientQueryService>();
            var hospitalContext = container.Resolve<HospitalContext>();
            var doctSchedules = hospitalContext.DoctSchedules.Where(s => s.VisitDate >= new DateTime(2023, 12, 01) && s.VisitDate < new DateTime(2024, 01, 01)
            && !string.IsNullOrEmpty(s.RegPointID)).ToList();
            var schemaQuery = hospitalContext.QuerySchemaInfos.FirstOrDefault();
            var ids = new List<int>();
            foreach (var schedule in doctSchedules.Where(s => !string.IsNullOrEmpty(s.RegPointID)))
            {
                polyTaskService.QueryOutPatient(schemaQuery, int.Parse(schedule.RegPointID), ids).Wait();
            }
            schemaQuery.ExcetionSchemaIds = string.Join(",", ids);
            hospitalContext.UpdateWithSeletor(schemaQuery, s => s.ExcetionSchemaIds);

            //polyTaskService.QueryOutPatientRetrieval(1434064, 1441701, new DateTime(2022, 08, 01)).Wait();
            //polyTaskService.Excute(null).Wait();
            //polyTaskService.QueryOutPatient(new VaeEntity.Hospital.QuerySchemaInfo(), 1418602, new System.Collections.Generic.List<int>()).Wait();
        }
        [TestMethod()]
        public void MedicationQueryServiceTest()
        {
            var polyTaskService = container.Resolve<MedicationQueryService>();
            var hospitalContext = container.Resolve<HospitalContext>();
            //var patient = hospitalContext.Patients.FirstOrDefault(p => p.Id == "ZY010000591430");
            polyTaskService.QueryMedications(true, new DateTime(2023, 12, 01), new DateTime(2024, 01, 01)).Wait();
            //polyTaskService.QueryMedication("", patient).Wait();
            //polyTaskService.Excute(null).Wait();
        }
        [TestMethod()]
        public void OutPatientMedicationQueryServiceTest()
        {
            var polyTaskService = container.Resolve<OutPatientMedicationQueryService>();
            //polyTaskService.Excute(null).Wait();
            polyTaskService.QueryOutPatientMedications(new DateTime(2022, 08, 01), new DateTime(2022, 09, 01), 1, 0).Wait();
        }
        [TestMethod()]
        public void QueryDoctScheduleServiceTest()
        {
            var polyTaskService = container.Resolve<QueryDoctScheduleService>();
            polyTaskService.Excute(null).Wait();
        }

        [TestMethod()]
        public void OutpatientEmptyMedicationResetServiceTest()
        {
            var polyTaskService = container.Resolve<OutpatientEmptyMedicationResetService>();
            polyTaskService.Excute(null).Wait();
        }

        [TestMethod()]
        public void PatientVisitReviewedJobTest()
        {
            var polyTaskService = container.Resolve<PatientVisitReviewedJob>();
            polyTaskService.Query(new DateTime(2022, 09, 01), new DateTime(2022, 10, 01), true).Wait();
            //polyTaskService.Query(new DateTime(2022, 07, 01), new DateTime(2022, 08, 01)).Wait();
        }

        [TestMethod()]
        public void InPatientHistoryQueryServiceTest()
        {
            var historyQuery = container.Resolve<InPatientHistoryQueryService>();
            historyQuery.QueryByVisitIds(new List<string>() { "0000327847" }, new VaeEntity.Hospital.QuerySchemaInfo() { LastQueryTime = new DateTime(2022, 07, 01) { } }).Wait();
        }
    }
}
