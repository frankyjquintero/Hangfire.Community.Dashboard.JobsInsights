# Hangfire.Community.Dashboard.JobsInsights

![License](https://img.shields.io/badge/license-MIT-orange.svg)
![NuGet](https://img.shields.io/nuget/v/Hangfire.Community.Dashboard.JobsInsights.svg)

​**JobsInsights** adds a powerful operational observability page to the Hangfire Dashboard.  
Monitor the health and performance of your background jobs at a glance, with a live summary grouped by job type, detailed historical views, interactive charts, and built‑in data management.

---

### ✨ Features

- **Job Type Summary**  
  See every job type with its last 15 execution results (green = success, red = failure) and average duration.  
  Quickly filter by state or job type name.

- **Full History**  
  Drill down into any job type to view all tracked executions with client‑side pagination, search, and sortable columns.  
  Errors are expandable, and each job links directly to its Hangfire details page.

- **Interactive Charts** *(per job type detail)*
  - **Duration Trends** – Explore avg duration over time (last 100 exec, 1h, 7d, 15d, 30d, 45d).  
  - **Throughput** – View total executions and success rate per hour for the last 24h.  
  - Configurable **alert threshold** – the chart turns red and a warning banner appears when the average duration exceeds your limit.  
  - Built‑in stats: min, max, P95, and a dashed average line on the chart.  
  - Export the chart as PNG image.

- **Data Management**  
  Set a retention period and delete older execution data directly from the dashboard.

- **Native Integration**  
  Uses only Hangfire’s own storage – no external databases, no additional infrastructure.  
  Blends seamlessly into the existing dashboard design with Bootstrap 3 and native JavaScript.

---

### 📸 Screenshots


![Summary view](https://raw.githubusercontent.com/frankyjquintero/Hangfire.Community.Dashboard.JobsInsights/refs/heads/master/images/summary.png)  
![Detail view with chart](https://raw.githubusercontent.com/frankyjquintero/Hangfire.Community.Dashboard.JobsInsights/refs/heads/master/images/detail1.png)
![Detail view with chart](https://raw.githubusercontent.com/frankyjquintero/Hangfire.Community.Dashboard.JobsInsights/refs/heads/master/images/detail2.png)
![Detail view with chart](https://raw.githubusercontent.com/frankyjquintero/Hangfire.Community.Dashboard.JobsInsights/refs/heads/master/images/detail3.png)
![Detail view with chart](https://raw.githubusercontent.com/frankyjquintero/Hangfire.Community.Dashboard.JobsInsights/refs/heads/master/images/detail4.png)
![Detail view with chart](https://raw.githubusercontent.com/frankyjquintero/Hangfire.Community.Dashboard.JobsInsights/refs/heads/master/images/detail5.png)

---

## Setup for ASP.NET Core

```csharp
using Hangfire;
using Hangfire.Community.Dashboard.JobsInsights;

namespace Application
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHangfire(configuration =>
            {
                configuration
                    .UseMemoryStorage()          // or your storage
                    .UseJobsInsights();          // Add the Jobs Insights page
            });

            services.AddHangfireServer();
        }
    }
}
```

## Setup for ASP.NET (.NET Framework)

```csharp
using Hangfire;
using Hangfire.Community.Dashboard.JobsInsights;

namespace Application
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            GlobalConfiguration.Configuration
                .UseJobsInsights();             // Add the Jobs Insights page

            app.UseHangfireDashboard();
        }
    }
}

```

## Requirements
Hangfire 1.7.0 or later

.NET Standard 2.0 / .NET Framework 4.6.1 / .NET Core 2.0 or later

Contributing
Contributions are welcome! Please open an issue or pull request for new features, bug fixes, or suggestions.


---

## 🙏 Acknowledgments

This project was heavily inspired by the architectural patterns and dashboard extension techniques demonstrated in 
[Hangfire.Community.Dashboard.Heatmap](https://github.com/brodrigz/Hangfire.Community.Dashboard.Heatmap) by [brodrigz](https://github.com/brodrigz). 

The Heatmap plugin served as a reference for integrating Razor-based pages, custom API dispatchers, and navigation menus within the Hangfire Dashboard ecosystem.
Many of the foundational approaches used in JobsInsights — such as `IDashboardDispatcher`, embedded Razor views, and native storage usage — were adapted 
from that excellent work. Thank you for sharing it with the community!


## License
MIT License

Copyright (c) 2025 FrankyJquintero

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
