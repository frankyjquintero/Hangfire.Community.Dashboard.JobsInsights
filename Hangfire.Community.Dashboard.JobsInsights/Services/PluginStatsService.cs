using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Hangfire.Storage;

namespace Hangfire.Community.Dashboard.JobsInsights.Services
{
    internal class PluginStatsService
    {
        private readonly IStorageConnection _connection;

        public PluginStatsService(IStorageConnection connection)
        {
            _connection = connection;
        }

        public PluginStats GetStats()
        {
            var jobTypesSet = _connection.GetAllItemsFromSet("hx:jobtypes:all");
            var jobTypes = jobTypesSet?.ToArray() ?? Array.Empty<string>();
            int totalTypes = jobTypes.Length;
            int totalJobs = 0;
            foreach (var jt in jobTypes)
            {
                var resultsHash = _connection.GetAllEntriesFromHash($"hx:results:{jt}");
                if (resultsHash != null)
                    totalJobs += resultsHash.Count;
            }

            return new PluginStats
            {
                TotalTypes = totalTypes,
                TotalJobs = totalJobs,
                JobTypes = jobTypes
            };
        }

        public int CleanupOldJobs(int retentionDays)
        {
            var cutoffDate = DateTime.UtcNow.Date.AddDays(-retentionDays);
            int removed = 0;

            var jobTypesSet = _connection.GetAllItemsFromSet("hx:jobtypes:all");
            var jobTypes = jobTypesSet?.ToArray() ?? Array.Empty<string>();

            foreach (var jt in jobTypes)
            {
                // Recorremos los días desde mucho antes hasta la fecha de corte (excluyendo días posteriores)
                for (var day = cutoffDate.AddDays(-1); day <= cutoffDate; day = day.AddDays(1))
                {
                    var dayKey = day.ToString("yyyyMMdd");
                    var hashKey = $"hx:results:{jt}:{dayKey}";
                    var entries = _connection.GetAllEntriesFromHash(hashKey);
                    if (entries == null || entries.Count == 0) continue;

                    removed += entries.Count;
                    using (var tx = _connection.CreateWriteTransaction())
                    {
                        tx.RemoveHash(hashKey);
                        tx.Commit();
                    }
                }
            }

            return removed;
        }

        private static JobResult DeserializeResult(string json)
        {
            try
            {
                return JsonSerializer.Deserialize<JobResult>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch { return null; }
        }
    }

    public class PluginStats
    {
        public int TotalTypes { get; set; }
        public int TotalJobs { get; set; }
        public ICollection<string> JobTypes { get; set; }
    }
}