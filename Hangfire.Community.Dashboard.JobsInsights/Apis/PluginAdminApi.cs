using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Hangfire.Community.Dashboard.JobsInsights.Services;
using Hangfire.Dashboard;

namespace Hangfire.Community.Dashboard.JobsInsights.Apis
{
    internal class PluginAdminApi : IDashboardDispatcher
    {
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public async Task Dispatch(DashboardContext context)
        {
            var path = context.UriMatch.Groups["path"].Value;
            context.Response.ContentType = "application/json";

            try
            {
                using (var connection = context.Storage.GetConnection())
                {
                    var service = new PluginStatsService(connection);

                    if (path == "stats")
                    {
                        var stats = service.GetStats();
                        await WriteJson(context, stats);
                    }
                    else if (path == "cleanup")
                    {
                        var daysParam = context.Request.GetQuery("days");
                        int days = 30;
                        if (!string.IsNullOrEmpty(daysParam))
                            int.TryParse(daysParam, out days);

                        var removed = service.CleanupOldJobs(days);
                        await WriteJson(context, new { removed, days });
                    }
                    else
                    {
                        context.Response.StatusCode = 404;
                    }
                }
            }
            catch (Exception ex)
            {
                var error = new { error = ex.Message };
                await WriteJson(context, error);
            }
        }

        private static async Task WriteJson(DashboardContext context, object data)
        {
            var json = JsonSerializer.Serialize(data, JsonOptions);
            var bytes = Encoding.UTF8.GetBytes(json);
            await context.Response.Body.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
        }
    }
}