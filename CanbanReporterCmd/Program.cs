using System;
using System.Diagnostics;
using System.Reflection;

namespace CanbanReporterCmd
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("KanbanReporter Tool " + GetAssemblyVersionString());
        }

        private static string GetAssemblyVersionString()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            return string.Format("{0}.{1}", fileVersionInfo.ProductMajorPart, fileVersionInfo.FileMinorPart);
        }
    }
}
