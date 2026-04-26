using Hangfire;
using Hangfire.Dashboard;
using Hangfire.Community.Dashboard.ExecutionInsights.Apis;
using Hangfire.Community.Dashboard.ExecutionInsights.Pages;

namespace Hangfire.Community.Dashboard.ExecutionInsights
{
    public static class ExecutionInsightsExtension
    {
        public static string RouteBase = "/execution-insights";

        public static IGlobalConfiguration UseExecutionInsights(this IGlobalConfiguration config)
        {
            GlobalJobFilters.Filters.Add(new ExecutionTrackingFilter());

            // APIs
            DashboardRoutes.Routes.Add($"{RouteBase}/api/jobtype-summary", new JobTypeSummaryApi());
            DashboardRoutes.Routes.Add($"{RouteBase}/api/jobtype-detail", new JobTypeDetailApi());

            // Página principal
            DashboardRoutes.Routes.AddRazorPage(RouteBase, x => new JobStatus());

            NavigationMenu.Items.Add(page => new MenuItem(JobStatus.Title, page.Url.To(RouteBase))
            {
                Active = page.RequestPath == RouteBase || page.RequestPath.StartsWith($"{RouteBase}/")
            });

            return config;
        }
    }
}