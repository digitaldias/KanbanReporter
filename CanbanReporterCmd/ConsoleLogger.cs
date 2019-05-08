using KanbanReporter.Business.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace CanbanReporterCmd
{
    public class ConsoleLogger : ILogger
    {
        public void Enter(object sender, [CallerMemberName] string methodName = "", params object[] args)
        {
            var arguments = string.Join(',', args.Select(a => $"'{a}'"));
            Console.WriteLine(sender.ToString() + $"{methodName}({arguments})");
        }

        public void LogError(string errorMessage, Exception exception = null, [CallerMemberName] string source = null)
        {
            Console.WriteLine(">>> ERROR >>> " + $"{source}: {errorMessage}");
        }

        public void LogInfo(string message)
        {
            Console.WriteLine($"[Info]{message}");
        }

        public void LogWarning(string message)
        {
            Console.WriteLine($"[Warning]{message}");
        }
    }
}
