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
            DashboardRoutes.Routes.AddRazorPage(RouteBase, x => new JobsInsightsPage());

            NavigationMenu.Items.Add(page => new MenuItem(JobsInsightsPage.Title, page.Url.To(RouteBase))
            {
                Active = page.RequestPath == RouteBase || page.RequestPath.StartsWith($"{RouteBase}/")
            });

            // ===== Recursos estáticos =====
            var assembly = typeof(JobsInsightsExtension).Assembly;

            // CSS
            DashboardRoutes.AddStylesheet(assembly,
                "Hangfire.Community.Dashboard.JobsInsights.Pages.Css.jobsinsights.css");

            // JavaScript (orden de dependencia)
            DashboardRoutes.AddJavaScript(assembly,
                "Hangfire.Community.Dashboard.JobsInsights.Pages.Js.jobsinsights.util.js");

            DashboardRoutes.AddJavaScript(assembly,
                "Hangfire.Community.Dashboard.JobsInsights.Pages.Js.jobsinsights.chart.js");

            DashboardRoutes.AddJavaScript(assembly,
                "Hangfire.Community.Dashboard.JobsInsights.Pages.Js.jobsinsights.summary.js");

            DashboardRoutes.AddJavaScript(assembly,
                "Hangfire.Community.Dashboard.JobsInsights.Pages.Js.jobsinsights.modal.js");

            DashboardRoutes.AddJavaScript(assembly,
                "Hangfire.Community.Dashboard.JobsInsights.Pages.Js.jobsinsights.init.js");


            return config;
        }
    }
}