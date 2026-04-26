using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Hangfire.Dashboard;
using Hangfire.Community.Dashboard.ExecutionInsights.Services;

namespace Hangfire.Community.Dashboard.ExecutionInsights.Apis
{
    internal class JobTypeDetailApi : IDashboardDispatcher
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
                var jobType = context.Request.GetQuery("jobType");
                var stateFilter = context.Request.GetQuery("state");

                if (string.IsNullOrEmpty(jobType))
                {
                    await WriteError(context, "jobType required");
                    return;
                }

                using (var connection = context.Storage.GetConnection())
                {
                    var entries = connection.GetAllEntriesFromHash($"hx:results:{jobType}");
                    if (entries == null || entries.Count == 0)
                    {
                        await WriteJson(context, new { jobs = new List<object>(), total = 0 });
                        return;
                    }

                    var allJobs = new List<object>();
                    foreach (var key in entries.Keys)
                    {
                        var value = entries[key];
                        var result = JsonSerializer.Deserialize<JobResult>(value, JsonOptions);
                        if (result == null) continue;

                        if (!string.IsNullOrEmpty(stateFilter) &&
                            !result.State.Equals(stateFilter, StringComparison.OrdinalIgnoreCase))
                            continue;

                        allJobs.Add(new
                        {
                            jobId = result.JobId,
                            queue = result.Queue,
                            lastState = result.State,
                            lastTimestamp = result.Timestamp,
                            errorMessage = result.ErrorMessage
                        });
                    }

                    await WriteJson(context, new { jobs = allJobs, total = allJobs.Count });
                }
            }
            catch (Exception ex)
            {
                await WriteError(context, ex.Message);
            }
        }

        private static async Task WriteJson(DashboardContext context, object data)
        {
            var json = JsonSerializer.Serialize(data, JsonOptions);
            var bytes = Encoding.UTF8.GetBytes(json);
            await context.Response.Body.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
        }

        private static async Task WriteError(DashboardContext context, string msg)
        {
            var error = new { error = msg };
            var json = JsonSerializer.Serialize(error, JsonOptions);
            var bytes = Encoding.UTF8.GetBytes(json);
            await context.Response.Body.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
        }
    }
}