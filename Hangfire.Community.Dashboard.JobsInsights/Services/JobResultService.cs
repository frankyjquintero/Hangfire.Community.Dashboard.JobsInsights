using Hangfire.States;
using Hangfire.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Hangfire.Community.Dashboard.JobsInsights.Services
{
    internal static class JobResultService
    {
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public static void RecordResult(IWriteOnlyTransaction transaction, string jobType, string jobId, string queue, string stateName, IState state, DateTime? startedAt)
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

            // Calcular duración si tenemos el inicio
            if (startedAt.HasValue)
            {
                result.Duration = (now - startedAt.Value).TotalSeconds;
            }

            var json = JsonSerializer.Serialize(result, JsonOptions);

            // Guardar en hash diario
            var dayKey = now.ToString("yyyyMMdd");
            transaction.SetRangeInHash($"hx:results:{jobType}:{dayKey}", new[] { new KeyValuePair<string, string>(jobId, json) });

            // Actualizar resumen (ahora pasamos la duración)
            UpdateSummary(transaction, jobType, jobId, stateName, now, result.Duration);

            transaction.AddToSet("hx:jobtypes:all", jobType);
        }

        private static void UpdateSummary(IWriteOnlyTransaction transaction, string jobType, string jobId, string state, DateTime timestamp, double? duration)
        {
            // Construir el objeto a guardar
            var summaryEntry = new
            {
                state,
                timestamp,
                jobId,
                duration
            };
            var entryJson = JsonSerializer.Serialize(summaryEntry, JsonOptions);

            // Clave única: ticks - jobId (ya usado actualmente)
            var fieldKey = $"{timestamp.Ticks:D19}-{jobId}";

            // Guardar en el hash del resumen
            transaction.SetRangeInHash($"hx:summary:{jobType}",
                new[] { new KeyValuePair<string, string>(fieldKey, entryJson) });

            // Limpiar los campos antiguos hasta dejar solo los últimos 15
            // Leer el hash actual para saber cuántos hay (necesitamos una conexión de solo lectura)
            using (var conn = JobStorage.Current.GetConnection())
            {
                var allEntries = conn.GetAllEntriesFromHash($"hx:summary:{jobType}");
                if (allEntries != null && allEntries.Count > 15)
                {
                    // Obtener todas las claves, ordenarlas descendente (más recientes primero)
                    var sortedKeys = allEntries.Keys
                        .OrderByDescending(k => k) // el orden lexicográfico coincide por empezar por ticks
                        .ToList();

                    // Conservar solo los primeros 15 (los más recientes)
                    var keysToKeep = sortedKeys.Take(15).ToList();
                    var keysToRemove = sortedKeys.Skip(15).ToList();

                    if (keysToRemove.Any())
                    {
                        // Para borrar campos individuales, debemos reescribir el hash completo
                        var entriesToKeep = new Dictionary<string, string>();
                        foreach (var key in keysToKeep)
                        {
                            if (allEntries.ContainsKey(key))
                                entriesToKeep[key] = allEntries[key];
                        }

                        using (var tx = conn.CreateWriteTransaction())
                        {
                            // Borrar el hash entero
                            tx.RemoveHash($"hx:summary:{jobType}");
                            // Volver a escribir solo las entradas que se conservan
                            if (entriesToKeep.Any())
                            {
                                tx.SetRangeInHash($"hx:summary:{jobType}",
                                    entriesToKeep.Select(kv => new KeyValuePair<string, string>(kv.Key, kv.Value)));
                            }
                            tx.Commit();
                        }
                    }
                }
            }
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
        public double? Duration { get; set; } // en segundos
    }

    internal class ExecutionState
    {
        public string State { get; set; }
        public DateTime Timestamp { get; set; }
        public string JobId { get; set; }
        public double? Duration { get; set; }
    }
}