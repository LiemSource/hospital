using Hangfire;
using Hangfire.Logging;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HospitalJob
{
    public class NotReentryServerHangfireFilter : IServerFilter
    {
        /// <summary>
        /// 判断job是否正在运行
        /// </summary>
        static ConcurrentDictionary<string, DateTime> JobRunnings = new ConcurrentDictionary<string, DateTime>();
        private static readonly ILog _logger = LogProvider.GetCurrentClassLogger();

        public NotReentryServerHangfireFilter()
        {
        }

        public void OnPerforming(PerformingContext filterContext)
        {
            var jobId = BuildJobId(filterContext.BackgroundJob);
            if (!JobRunnings.TryAdd(jobId, DateTime.Now))
            {
                filterContext.Canceled = true;
                return;
            }
            _logger.InfoFormat($"{jobId} starting...");
        }

        public void OnPerformed(PerformedContext filterContext)
        {
            var jobId = BuildJobId(filterContext.BackgroundJob);
            JobRunnings.TryRemove(jobId, out var tmp);
            _logger.InfoFormat($"{jobId} finished.");
        }

        public string BuildJobId(BackgroundJob job)
        {
            return $"{job.Job.Type.FullName}.{job.Job.Method.Name}";
        }
    }
}
