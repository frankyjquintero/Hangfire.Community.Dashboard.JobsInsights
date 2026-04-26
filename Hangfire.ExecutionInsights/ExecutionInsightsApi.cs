using System.Text.Json;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using Hangfire.Dashboard;
using Hangfire.ExecutionInsights.Models;

namespace Hangfire.ExecutionInsights
{
    internal class ExecutionInsightsApi : IDashboardDispatcher
    {
        public async Task Dispatch(DashboardContext context)
        {
            context.Response.ContentType = "application/json";
            try
            {
                // Aquí va tu lógica para obtener los datos del grid
                var response = new ExecutionGridResponse { Data = new List<object>() };
                var json = JsonSerializer.Serialize(response);
                var bytes = Encoding.UTF8.GetBytes(json);
                await context.Response.Body.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                var errorResponse = new ErrorResponse { Error = ex.Message };
                var json = JsonSerializer.Serialize(errorResponse);
                var bytes = Encoding.UTF8.GetBytes(json);
                await context.Response.Body.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
            }
        }
    }

}


