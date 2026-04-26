using Hangfire.States;
using Hangfire.Storage;
using Hangfire.Community.Dashboard.ExecutionInsights.Services;

namespace Hangfire.Community.Dashboard.ExecutionInsights.Filters
{
    public class ExecutionTrackingFilter : IApplyStateFilter
    {
        private const string QueueParameterKey = "ExecutionInsights.Queue";
        private const string JobTypeParameterKey = "ExecutionInsights.JobType";

        public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
        {
            var stateName = context.NewState.Name;

            // Solo estados finales
            if (stateName != SucceededState.StateName && stateName != FailedState.StateName)
                return;

            var queue = GetStoredParameter(context.Connection, context.BackgroundJob.Id, QueueParameterKey) ?? "default";
            var jobType = GetStoredParameter(context.Connection, context.BackgroundJob.Id, JobTypeParameterKey);

            if (string.IsNullOrEmpty(jobType))
            {
                jobType = GetJobType(context.BackgroundJob.Job) ?? "Unknown";
            }

            JobResultService.RecordResult(transaction, jobType, context.BackgroundJob.Id, queue, stateName, context.NewState);
        }

        public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction) { }

        private static string GetStoredParameter(IStorageConnection connection, string jobId, string parameter)
        {
            return connection.GetJobParameter(jobId, parameter);
        }

        private static string GetJobType(global::Hangfire.Common.Job job)
        {
            if (job == null || job.Method == null) return null;
            var type = job.Method.DeclaringType;
            return type != null ? $"{type.FullName}.{job.Method.Name}" : job.Method.Name;
        }
    }
}