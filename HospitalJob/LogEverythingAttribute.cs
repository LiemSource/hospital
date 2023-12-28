using Hangfire.Client;
using Hangfire.Common;
using Hangfire.Server;
using Hangfire.States;
using Hangfire.Storage;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HospitalJob
{
    public class LogEverythingAttribute : JobFilterAttribute,
    IClientFilter, IServerFilter, IElectStateFilter, IApplyStateFilter
    {
        protected readonly ILogger _logger;
        public LogEverythingAttribute(ILoggerFactory logger)
        {

            _logger = logger.CreateLogger(this.GetType());
        }
        public void OnCreated(CreatedContext filterContext)
        {
            _logger.LogInformation($"Job that is based on method `{ filterContext.Job.Method.Name}` has been created with id `{ filterContext.BackgroundJob?.Id}`");
        }

        public void OnCreating(CreatingContext filterContext)
        {
            _logger.LogInformation($"Creating a job based on method `{filterContext.Job.Method.Name}`...");
        }

        public void OnPerformed(PerformedContext filterContext)
        {
            _logger.LogInformation($"Job `{filterContext.BackgroundJob.Id}` has been performed");
        }

        public void OnPerforming(PerformingContext filterContext)
        {
            _logger.LogInformation($"Starting to perform job `{filterContext.BackgroundJob.Id}`");
        }

        public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
        {
            _logger.LogInformation($"Job `{context.BackgroundJob.Id}` state was changed from `{context.OldStateName}` to `{context.NewState.Name}``");

        }

        public void OnStateElection(ElectStateContext context)
        {
            throw new NotImplementedException();
        }

        public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
        {
            _logger.LogInformation($"Job `{context.BackgroundJob.Id}` state `{context.OldStateName}` was unapplied.`");
        }
    }
}
