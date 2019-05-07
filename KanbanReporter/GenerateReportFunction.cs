using KanbanReporter.Business.Implementation;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace KanbanReporter
{
    // Cron expressions during development:
    // Every sunday at 6AM: "0 0 6 * * SUN"
    // For debugging      : "*/10 * * * * *"
    public static class GenerateReportFunction
    {

        [FunctionName("GenerateReportFunction")]
        public static async Task Run([TimerTrigger("*/10 * * * * *")]TimerInfo myTimer, TraceWriter log, ExecutionContext context)
        {
            var configuration  = CreateConfiguration(context);
            var settings       = new Settings(configuration);
            var logger         = new Logger(log);

            var reportService  = new ReportService(logger, settings);

            // Run the report synchronously (We need async support in Azure Functions!)
            await reportService.CreateReportAsync();
                //.GetAwaiter()
                //.GetResult();                        

            log.Info($"C# Timer trigger function executed at: {DateTime.Now}");
        }
    
        private static IConfigurationRoot CreateConfiguration(ExecutionContext context)
        {
            return new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();
        }
    }
}
