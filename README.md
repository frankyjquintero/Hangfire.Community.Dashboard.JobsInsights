# Hangfire.Community.Dashboard.Heatmap

![MIT License](https://img.shields.io/badge/license-MIT-orange.svg)
![NuGet](https://img.shields.io/nuget/v/Hangfire.Community.Dashboard.Heatmap.svg)

Hangfire.Community.Dashboard.Heatmap adds a visual timeline and heatmap view to your Hangfire Dashboard, letting you see exactly when your recurring jobs are scheduled to run throughout the day.

---

Visualize your recurring job schedules at a glance with an timeline showing past and upcoming executions, or switch to the heatmap view to visualize execution density by hour.

<img width="1865" height="876" alt="image" src="https://github.com/user-attachments/assets/5368b8a6-e8a8-4dcd-a813-f882a34585d7" />
<img width="1853" height="872" alt="image" src="https://github.com/user-attachments/assets/d69b0f15-4e9e-4042-b49b-9c254a883251" />

---


## Setup for ASP.NET Core

```csharp
using Hangfire;
using Hangfire.Community.Dashboard.Heatmap;

namespace Application
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHangfire(configuration =>
            {
                configuration
                    .UseMemoryStorage() 
                    ... // Your other stuff
                    .UseHeatmapPage(); // Add the Cron Heatmap page
            });

            services.AddHangfireServer();
        }
    }
}
```

## Setup for ASP.NET (.NET Framework)

```csharp
using Hangfire;
using Hangfire.Community.Dashboard.Heatmap;

namespace Application
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            GlobalConfiguration.Configuration
                .UseHeatmapPage(); // Add the Cron Heatmap page

            app.UseHangfireDashboard();
        }
    }
}
```

---

## Requirements

- Hangfire 1.7.0 or later
- .NET Standard 2.0 / .NET Framework 4.6.1 / .NET Core 2.0 or later
---

## Contributing

Contributions are welcome! Please open an issue or pull request for new features, bug fixes, or suggestions.

## License

Copyright (c) 2026

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
