using Hangfire.Community.Dashboard.ExecutionInsights.Services;
using Hangfire.States;
using Hangfire.Storage;

public class ExecutionTrackingFilter : IApplyStateFilter
{
    private const string QueueParameterKey = "ExecutionInsights.Queue";
    private const string JobTypeParameterKey = "ExecutionInsights.JobType";

    public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
        var stateName = context.NewState.Name;

        // 1. Al encolar, guardar la cola y el tipo de job
        if (context.NewState is EnqueuedState enqueuedState)
        {
            string queueInitial = enqueuedState.Queue ?? "default";
            context.Connection.SetJobParameter(context.BackgroundJob.Id, QueueParameterKey, queueInitial);

            string jobTypeInitial = GetJobType(context.BackgroundJob.Job);
            if (!string.IsNullOrEmpty(jobTypeInitial))
                context.Connection.SetJobParameter(context.BackgroundJob.Id, JobTypeParameterKey, jobTypeInitial);

            return; // No necesitamos registrar nada más en este estado
        }

        // 2. Solo estados finales para registrar resultados
        if (stateName != SucceededState.StateName && stateName != FailedState.StateName)
            return;

        string queue = GetStoredParameter(context.Connection, context.BackgroundJob.Id, QueueParameterKey) ?? "default";
        string jobType = GetStoredParameter(context.Connection, context.BackgroundJob.Id, JobTypeParameterKey);

        if (string.IsNullOrEmpty(jobType))
        {
            jobType = GetJobType(context.BackgroundJob.Job) ?? "Unknown";
        }

        JobResultService.RecordResult(transaction, jobType, context.BackgroundJob.Id, queue, stateName, context.NewState);
    }

    public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction) { }

    private static string GetStoredParameter(IStorageConnection connection, string jobId, string parameter) =>
        connection.GetJobParameter(jobId, parameter);

    private static string GetJobType(global::Hangfire.Common.Job job)
    {
        if (job == null || job.Method == null) return null;
        var type = job.Method.DeclaringType;
        return type != null ? $"{type.FullName}.{job.Method.Name}" : job.Method.Name;
    }
}