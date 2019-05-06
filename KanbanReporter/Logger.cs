using System;
using System.Linq;
using System.Runtime.CompilerServices;
using KanbanReporter.Business.Contracts;
using Microsoft.Azure.WebJobs.Host;

namespace KanbanReporter
{
    public class Logger : ILogger
    {
        private readonly TraceWriter _log;

        public Logger(TraceWriter log)
        {
            _log = log;
        }

        public void Enter(object sender, [CallerMemberName] string methodName = "", params object[] args)
        {
            var arguments = TransformArgumentsToCommaseparatedString();
            _log.Info($"[{DateTime.Now.ToString("hh:mm:ss")}] {sender.GetType().FullName}.{methodName}({arguments})");
        }

        public void LogInfo(string message)
        {
            _log.Info(message);
        }

        public void LogWarning(string message)
        {
            _log.Warning(message);
        }

        public void LogError(string errorMessage, Exception exception = null, [CallerMemberName] string source = null)
        {
            _log.Error(errorMessage, exception, source);
        }

        private string TransformArgumentsToCommaseparatedString(params object[] args)
        {
            if (args == null || !args.Any())
                return string.Empty;

            var arguments = args.Select(a => $"'{a?.ToString()}'");
            return string.Join(", ", arguments);
        }
    }
}
