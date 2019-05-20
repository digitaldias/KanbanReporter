using KanbanReporter.Business.Implementation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace KanbanReporterCmd
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var stopwatch = Stopwatch.StartNew();

            Console.WriteLine("KanbanReporter Tool " + GetAssemblyVersionString());

            if (args.Length == 0 || args.Contains("--help"))
            {
                DisplayHelp();
                return;
            }

            var reportService = new ReportService(new ConsoleLogger(), new ConsoleSettings(args));
            await reportService.CreateReportAsync();

            Console.WriteLine($"KanbanReporter finished in {stopwatch.ElapsedMilliseconds}ms.");
        }

        private static void DisplayHelp()
        {
            var builder = new StringBuilder();
            builder.Append($"Usage:" + Environment.NewLine);
            builder.Append("> KanbanReporter --settings-file <path to settings file>" + Environment.NewLine);
            builder.Append(Environment.NewLine);
            builder.Append(Environment.NewLine);
            builder.Append("Sample settings: " + Environment.NewLine);

            var jObject = new JObject
            {
                ["AdoOrgName"]             = "[Azure Devops Organisation Name]",
                ["AdoProjectName"]         = "[The Project Name in Azure Devops]",
                ["AdoPersonalAccessToken"] = "[Personal Access token for KanbanReporter (Guid)]",
                ["AdoRepositoryId"]        = "[The GUID of the repository for which KanbanReporter will submit its report]",
                ["AdoRepositoryName"]      = "[The name of the repository for which KanbanReporter will submit its report]",
                ["AdoBranchName"]          = "[The branch that KanbanReporter will operate on]",                                
                ["MarkdownFilePath"]       = "[Relative path to the readme file, i.e. /refs/heads/KanbanReporter/Readme.md]",
                ["CreatePullRequest"]      = true
            };

            builder.Append(jObject.ToString(Formatting.Indented));
            builder.Append(Environment.NewLine);
            builder.Append("End of help");

            Console.WriteLine(builder.ToString());
        }

        private static string GetAssemblyVersionString()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            return string.Format("{0}.{1}", fileVersionInfo.ProductMajorPart, fileVersionInfo.FileMinorPart);
        }
    }
}
