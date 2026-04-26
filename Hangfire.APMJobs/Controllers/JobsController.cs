using Hangfire;
using Microsoft.AspNetCore.Mvc;

namespace Hangfire.APMJobs.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JobsController : ControllerBase
    {
        private readonly IBackgroundJobClient _backgroundJobClient;

        public JobsController(IBackgroundJobClient backgroundJobClient)
        {
            _backgroundJobClient = backgroundJobClient;
        }

        /// <summary>
        /// Dispara un job fire-and-forget.
        /// </summary>
        [HttpPost("fire-and-forget")]
        public IActionResult FireAndForget()
        {
            _backgroundJobClient.Enqueue(() => Console.WriteLine($"Job fire-and-forget ejecutado a las {DateTime.Now}"));
            return Ok("Job fire-and-forget encolado");
        }

        /// <summary>
        /// Programa un job recurrente (cada minuto).
        /// </summary>
        [HttpPost("recurrente")]
        public IActionResult Recurrente()
        {
            RecurringJob.AddOrUpdate("job-recurrente", () => Console.WriteLine($"Job recurrente ejecutado a las {DateTime.Now}"), Cron.Minutely);
            return Ok("Job recurrente programado (cada minuto)");
        }
    }
}
