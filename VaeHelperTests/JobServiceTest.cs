using Autofac;
using Hangfire.Server;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using VaeJob.Jobs;

namespace VaeHelperTests
{
    [TestClass()]
    public class JobServiceTest
    {

        private IContainer container;
        public JobServiceTest()
        {
            container = TestInitialize.Initialize<VaeJob.Startup>();
        }
        [TestMethod()]
        public void ReviewServiceTest()
        {
            var reviewService = container.Resolve<ReviewService>();
            reviewService.Excute(null);
        }

        [TestMethod()]
        public void QueryTicketOrderServiceTest()
        {
            var reviewService = container.Resolve<QueryTicketOrderService>();
            for (int dateIndex = 1; dateIndex < 30; dateIndex++)
            {
                reviewService.QueryOrders(DateTime.Today.AddDays(-dateIndex)).Wait();
            }
            //reviewService.Excute(null).Wait();
        }
        [TestMethod()]
        public void PolyTaskServiceTest()
        {
            var polyTaskService = container.Resolve<PolyTaskService>();
            polyTaskService.Excute(null).Wait();
        }
        [TestMethod()]
        public void PolyTaskServiceCancelOrderTest()
        {
            var polyTaskService = container.Resolve<PolyTaskService>();
            polyTaskService.CancelUserOrders().Wait();
        }
        [TestMethod()]
        public void PatientQueryServiceTest()
        {
            var polyTaskService = container.Resolve<PatientQueryService>();
            polyTaskService.Excute(null).Wait();
        }
    }
}
