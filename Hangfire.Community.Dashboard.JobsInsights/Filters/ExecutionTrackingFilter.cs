using Hangfire.Community.Dashboard.JobsInsights.Services;
using Hangfire.States;
using Hangfire.Storage;
using System;

public class ExecutionTrackingFilter : IApplyStateFilter
{
    private const string QueueParameterKey = "ExecutionInsights.Queue";
    private const string JobTypeParameterKey = "ExecutionInsights.JobType";

    public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
        var stateName = context.NewState.Name;

        // Al encolar, guardar cola y tipo de job
        if (context.NewState is EnqueuedState enqueuedState)
        {
            var queueInitial = enqueuedState.Queue ?? "default";
            context.Connection.SetJobParameter(context.BackgroundJob.Id, QueueParameterKey, queueInitial);

            var jobTypeInitial = GetJobType(context.BackgroundJob.Job);
            if (!string.IsNullOrEmpty(jobTypeInitial))
                context.Connection.SetJobParameter(context.BackgroundJob.Id, JobTypeParameterKey, jobTypeInitial);

            return;
        }

        // Al empezar a procesar, guardar el timestamp de inicio
        if (context.NewState is ProcessingState)
        {
            var processingStartedAt = DateTime.UtcNow.ToString("O");
            context.Connection.SetJobParameter(context.BackgroundJob.Id, "ExecutionInsights.ProcessingStartedAt", processingStartedAt);
            return;
        }

        // Solo procesamos estados finales
        if (stateName != SucceededState.StateName && stateName != FailedState.StateName)
            return;

        // Obtener cola y tipo de job almacenados
        var queue = GetStoredParameter(context.Connection, context.BackgroundJob.Id, QueueParameterKey) ?? "default";
        var jobType = GetStoredParameter(context.Connection, context.BackgroundJob.Id, JobTypeParameterKey);

        if (string.IsNullOrEmpty(jobType))
        {
            jobType = GetJobType(context.BackgroundJob.Job) ?? "Unknown";
        }

        // Recuperar el momento de inicio del procesamiento
        var startedAtStr = context.Connection.GetJobParameter(context.BackgroundJob.Id, "ExecutionInsights.ProcessingStartedAt");
        DateTime? startedAt = null;
        if (!string.IsNullOrEmpty(startedAtStr))
        {
            if (DateTime.TryParse(startedAtStr, null, System.Globalization.DateTimeStyles.RoundtripKind, out var parsed))
                startedAt = parsed;
        }

        // Registrar resultado final (con duración si existe)
        JobResultService.RecordResult(transaction, jobType, context.BackgroundJob.Id, queue, stateName, context.NewState, startedAt);
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