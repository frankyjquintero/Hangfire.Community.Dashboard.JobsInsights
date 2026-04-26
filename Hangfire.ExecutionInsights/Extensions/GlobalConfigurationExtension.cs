using Hangfire.Dashboard;
using Hangfire.ExecutionInsights.Pages;

namespace Hangfire.ExecutionInsights.Extensions
{
    public static class GlobalConfigurationExtension
    {
        public static string RouteBase = "/execution-insights";

        public static IGlobalConfiguration UseExecutionInsightsPage(this IGlobalConfiguration config)
        {
            RegisterDashboard();
            RegisterApi();
            return config;
        }

        private static void RegisterDashboard()
        {
            DashboardRoutes.Routes.AddRazorPage(RouteBase, x => new ExecutionInsightsPage());

            NavigationMenu.Items.Add(page => new MenuItem(ExecutionInsightsPage.Title, page.Url.To(RouteBase))
            {
                Active = page.RequestPath == RouteBase || page.RequestPath.StartsWith($"{RouteBase}/")
            });
        }

        private static void RegisterApi()
        {
            DashboardRoutes.Routes.Add(
                $"{RouteBase}/api/grid",
                new ExecutionInsightsApi()
            );
        }
    }
}


