using Hangfire.States;
using Hangfire.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Hangfire.Community.Dashboard.ExecutionInsights.Services
{
    internal static class JobResultService
    {
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public static void RecordResult(IWriteOnlyTransaction transaction, string jobType, string jobId, string queue, string stateName, IState state)
        {
            var now = DateTime.UtcNow;
            var result = new JobResult
            {
                Timestamp = now,
                JobId = jobId,
                JobType = jobType,
                Queue = queue,
                State = stateName
            };

            if (state is FailedState failedState)
                result.ErrorMessage = failedState.Exception?.Message;

            var json = JsonSerializer.Serialize(result, JsonOptions);

            // Guardar exclusivamente en el hash diario
            var dayKey = now.ToString("yyyyMMdd");
            transaction.SetRangeInHash($"hx:results:{jobType}:{dayKey}", new[] { new KeyValuePair<string, string>(jobId, json) });

            // Actualizar resumen de los últimos 15 estados (esto sigue igual, usa su propio hash)
            UpdateSummary(transaction, jobType, jobId, stateName, now);

            // Registrar el tipo de job
            transaction.AddToSet("hx:jobtypes:all", jobType);
        }

        private static void UpdateSummary(IWriteOnlyTransaction transaction, string jobType, string jobId, string state, DateTime timestamp)
        {
            // Clave única para este resultado: ticks-jobId
            var fieldKey = $"{timestamp.Ticks:D19}-{jobId}";
            var summaryJson = JsonSerializer.Serialize(new { state, timestamp }, JsonOptions);

            // Agregar un nuevo campo al hash del resumen
            transaction.SetRangeInHash($"hx:summary:{jobType}", new[] { new KeyValuePair<string, string>(fieldKey, summaryJson) });
        }
    }

    internal class JobResult
    {
        public DateTime Timestamp { get; set; }
        public string JobId { get; set; }
        public string JobType { get; set; }
        public string Queue { get; set; }
        public string State { get; set; }
        public string ErrorMessage { get; set; }
    }

    internal class ExecutionState
    {
        public string State { get; set; }
        public DateTime Timestamp { get; set; }
    }
}