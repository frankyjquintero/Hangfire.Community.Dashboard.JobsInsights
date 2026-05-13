using Hangfire.Dashboard;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.Community.Dashboard.JobsInsights.Apis
{
    internal static class JsonResponseHelper
    {
        public static async Task WriteJsonAsync(this DashboardContext context, object data)
        {
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Formatting = Formatting.None // Compacto
            };

            var json = JsonConvert.SerializeObject(data, settings);
            var bytes = Encoding.UTF8.GetBytes(json);

            context.Response.ContentType = "application/json";
            await context.Response.Body.WriteAsync(bytes, 0, bytes.Length);
        }
    }
}
