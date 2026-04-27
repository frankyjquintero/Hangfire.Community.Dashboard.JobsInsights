using Hangfire.Community.Dashboard.ExecutionInsights.Services;
using Hangfire.Dashboard;
using Hangfire.States;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Hangfire.Community.Dashboard.ExecutionInsights.Apis
{
    internal class JobTypeSummaryApi : IDashboardDispatcher
    {
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public async Task Dispatch(DashboardContext context)
        {
            context.Response.ContentType = "application/json";
            try
            {
                var stateFilter = context.Request.GetQuery("state");

                using (var connection = context.Storage.GetConnection())
                {
                    var jobTypesSet = connection.GetAllItemsFromSet("hx:jobtypes:all");
                    var jobTypes = jobTypesSet != null ? new List<string>(jobTypesSet) : new List<string>();
                    var summaries = new List<object>();

                    foreach (var jt in jobTypes)
                    {
                        var summaryEntries = connection.GetAllEntriesFromHash($"hx:summary:{jt}");
                        if (summaryEntries == null || summaryEntries.Count == 0) continue;

                        // Convertir a lista, ordenar por clave descendente (ticks más grande primero)
                        var sorted = summaryEntries
                            .Select(kv => new { Key = kv.Key, Value = kv.Value })
                            .OrderByDescending(x => x.Key) // la clave comienza con ticks, orden lexicográfico funciona
                            .Take(15)
                            .Select(x =>
                            {
                                try
                                {
                                    var stateObj = JsonSerializer.Deserialize<ExecutionState>(x.Value, JsonOptions);
                                    return stateObj;
                                }
                                catch { return null; }
                            })
                            .Where(s => s != null)
                            .ToList();

                        if (sorted.Count == 0) continue;

                        int success = sorted.Count(s => s.State?.Equals("Succeeded", StringComparison.OrdinalIgnoreCase) == true);
                        int failed = sorted.Count(s => s.State?.Equals("Failed", StringComparison.OrdinalIgnoreCase) == true);

                        // Filtro por estado
                        if (!string.IsNullOrEmpty(stateFilter))
                        {
                            if (stateFilter.Equals("Succeeded", StringComparison.OrdinalIgnoreCase) && success == 0) continue;
                            if (stateFilter.Equals("Failed", StringComparison.OrdinalIgnoreCase) && failed == 0) continue;
                        }

                        summaries.Add(new
                        {
                            jobType = jt,
                            avgDuration = sorted.Where(s => s.Duration.HasValue).Select(s => s.Duration.Value).DefaultIfEmpty(0).Average(),
                            recentExecutions = sorted, // lista de ExecutionState
                            successCount = success,
                            failedCount = failed,
                            lastExecution = sorted[0].Timestamp
                        });
                    }

                    var json = JsonSerializer.Serialize(new { summaries }, JsonOptions);
                    var bytes = Encoding.UTF8.GetBytes(json);
                    await context.Response.Body.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                var error = JsonSerializer.Serialize(new { error = ex.Message }, JsonOptions);
                var bytes = Encoding.UTF8.GetBytes(error);
                await context.Response.Body.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
            }
        }
    }
}