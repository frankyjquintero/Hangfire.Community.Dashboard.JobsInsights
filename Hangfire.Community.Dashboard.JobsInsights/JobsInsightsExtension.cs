using Hangfire.Dashboard;
using Hangfire.Community.Dashboard.JobsInsights.Pages;
using Hangfire.Community.Dashboard.JobsInsights.Apis;

namespace Hangfire.Community.Dashboard.JobsInsights
{
    public static class JobsInsightsExtension
    {
        public static string RouteBase = "/jobs-insights";

        public static IGlobalConfiguration UseJobsInsights(this IGlobalConfiguration config)
        {
            GlobalJobFilters.Filters.Add(new ExecutionTrackingFilter());

            // APIs
            DashboardRoutes.Routes.Add($"{RouteBase}/api/jobtype-summary", new JobTypeSummaryApi());
            DashboardRoutes.Routes.Add($"{RouteBase}/api/jobtype-detail", new JobTypeDetailApi());
            DashboardRoutes.Routes.Add($"{RouteBase}/api/admin/(?<path>.+)", new PluginAdminApi());

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