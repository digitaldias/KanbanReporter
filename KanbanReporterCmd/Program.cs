using KanbanReporter.Business.Implementation;
using System;
using System.Diagnostics;
using System.Reflection;

namespace CanbanReporterCmd
{
    class Program
    {
        static void Main(string[] args)
        {
            var stopwatch = Stopwatch.StartNew();

            Console.WriteLine("KanbanReporter Tool " + GetAssemblyVersionString());



            var reportService = new ReportService(new ConsoleLogger(), new ConsoleSettings(args));

            reportService
                .CreateReportAsync()
                .GetAwaiter()
                .GetResult();

            Console.WriteLine($"KanbanReporter finished in {stopwatch.ElapsedMilliseconds}ms.");
        }

        private static string GetAssemblyVersionString()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            return string.Format("{0}.{1}", fileVersionInfo.ProductMajorPart, fileVersionInfo.FileMinorPart);
        }
    }
}
