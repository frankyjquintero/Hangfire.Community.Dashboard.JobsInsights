using System;
using System.Collections.Generic;
using System.Threading;
using Hangfire;
using Hangfire.States;
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
        /// Dispara un job fire-and-forget simple en la cola especificada.
        /// </summary>
        [HttpPost("fire-and-forget")]
        public IActionResult FireAndForget(string queue = "default")
        {
            _backgroundJobClient.Enqueue(queue, () => Console.WriteLine($"Job fire-and-forget ejecutado a las {DateTime.Now} en la cola {queue}"));
            return Ok("Job fire-and-forget encolado");
        }

        /// <summary>
        /// Dispara los 17 jobs de simulación en una sola llamada.
        /// </summary>
        [HttpPost("simulate-all")]
        public IActionResult SimulateAll()
        {
            var jobs = new List<string>();
            jobs.Add(_backgroundJobClient.Enqueue(() => SimulatedTasks.SendWelcomeEmail()));
            jobs.Add(_backgroundJobClient.Enqueue(() => SimulatedTasks.ProcessOrderPayment()));
            jobs.Add(_backgroundJobClient.Enqueue(() => SimulatedTasks.GenerateMonthlyReport()));
            jobs.Add(_backgroundJobClient.Enqueue(() => SimulatedTasks.CleanupExpiredTokens()));
            jobs.Add(_backgroundJobClient.Enqueue(() => SimulatedTasks.ImportCustomerData()));
            jobs.Add(_backgroundJobClient.Enqueue(() => SimulatedTasks.SyncProductCatalog()));
            jobs.Add(_backgroundJobClient.Enqueue(() => SimulatedTasks.CreateBackup()));
            jobs.Add(_backgroundJobClient.Enqueue(() => SimulatedTasks.ValidateEmailAddresses()));
            jobs.Add(_backgroundJobClient.Enqueue(() => SimulatedTasks.UpdateExchangeRates()));
            jobs.Add(_backgroundJobClient.Enqueue(() => SimulatedTasks.PurgeOldLogs()));
            // Los siguientes fallarán a propósito
            jobs.Add(_backgroundJobClient.Enqueue(() => SimulatedTasks.FailingJob_ProcessRefund()));
            //jobs.Add(_backgroundJobClient.Enqueue(() => SimulatedTasks.FailingJob_ChargeCreditCard()));
            //jobs.Add(_backgroundJobClient.Enqueue(() => SimulatedTasks.FailingJob_UpdateInventory()));
            // Jobs con algo de demora para aparecer como "Processing"
            jobs.Add(_backgroundJobClient.Enqueue(() => SimulatedTasks.SlowJob_GenerateInvoice()));
            jobs.Add(_backgroundJobClient.Enqueue(() => SimulatedTasks.SlowJob_ResizeImages()));
            // Más jobs exitosos
            jobs.Add(_backgroundJobClient.Enqueue(() => SimulatedTasks.NotifyUsersAboutDowntime()));
            jobs.Add(_backgroundJobClient.Enqueue(() => SimulatedTasks.RebuildSearchIndex()));

            return Ok(new { message = "17 jobs encolados", jobIds = jobs });
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

    public static class SimulatedTasks
    {
        private static readonly Random _random = new Random();

        public static void SendWelcomeEmail()
        {
            Thread.Sleep(_random.Next(1000, 5000));
            Console.WriteLine($"[{DateTime.Now}] Welcome email sent.");
        }
        public static void ProcessOrderPayment()
        {
            Thread.Sleep(_random.Next(1000, 5000));
            Console.WriteLine($"[{DateTime.Now}] Order payment processed.");
        }
        public static void GenerateMonthlyReport()
        {
            Thread.Sleep(_random.Next(1000, 5000));
            Console.WriteLine($"[{DateTime.Now}] Monthly report generated.");
        }
        public static void CleanupExpiredTokens()
        {
            Thread.Sleep(_random.Next(1000, 5000));
            Console.WriteLine($"[{DateTime.Now}] Expired tokens cleaned up.");
        }
        public static void ImportCustomerData()
        {
            Thread.Sleep(_random.Next(1000, 5000));
            Console.WriteLine($"[{DateTime.Now}] Customer data imported.");
        }
        public static void SyncProductCatalog()
        {
            Thread.Sleep(_random.Next(1000, 5000));
            Console.WriteLine($"[{DateTime.Now}] Product catalog synced.");
        }
        public static void CreateBackup()
        {
            Thread.Sleep(_random.Next(1000, 5000));
            Console.WriteLine($"[{DateTime.Now}] Backup created successfully.");
        }
        public static void ValidateEmailAddresses()
        {
            Thread.Sleep(_random.Next(1000, 5000));
            Console.WriteLine($"[{DateTime.Now}] Email addresses validated.");
        }
        public static void UpdateExchangeRates()
        {
            Thread.Sleep(_random.Next(1000, 5000));
            Console.WriteLine($"[{DateTime.Now}] Exchange rates updated.");
        }
        public static void PurgeOldLogs()
        {
            Thread.Sleep(_random.Next(1000, 5000));
            Console.WriteLine($"[{DateTime.Now}] Old logs purged.");
        }

        // Jobs que fallan
        public static void FailingJob_ProcessRefund()
        {
            Thread.Sleep(_random.Next(1000, 5000));
            Console.WriteLine($"[{DateTime.Now}] ProcessRefund.");
        }
        public static void FailingJob_ChargeCreditCard()
        {
            Thread.Sleep(_random.Next(1000, 5000));
            throw new Exception("Credit card charge declined.");
        }
        public static void FailingJob_UpdateInventory()
        {
            Thread.Sleep(_random.Next(1000, 5000));
            throw new InvalidOperationException("Inventory update failed: insufficient stock.");
        }

        // Jobs lentos (mantienen sus tiempos fijos)
        public static void SlowJob_GenerateInvoice()
        {
            Thread.Sleep(3000);
            Console.WriteLine($"[{DateTime.Now}] Invoice generated after delay.");
        }
        public static void SlowJob_ResizeImages()
        {
            Thread.Sleep(5000);
            Console.WriteLine($"[{DateTime.Now}] Images resized after delay.");
        }

        // Más jobs exitosos
        public static void NotifyUsersAboutDowntime()
        {
            Thread.Sleep(_random.Next(1000, 5000));
            Console.WriteLine($"[{DateTime.Now}] Users notified about planned downtime.");
        }
        public static void RebuildSearchIndex()
        {
            Thread.Sleep(_random.Next(1000, 5000));
            Console.WriteLine($"[{DateTime.Now}] Search index rebuilt.");
        }
    }
}