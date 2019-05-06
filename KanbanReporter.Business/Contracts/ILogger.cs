using System;
using System.Runtime.CompilerServices;

namespace KanbanReporter.Business.Contracts
{
    public interface ILogger
    {
        void Enter(object sender, [CallerMemberName] string methodName = "", params object[] args);

        void LogInfo(string message);

        void LogWarning(string message);

        void LogError(string errorMessage, Exception exception = null, [CallerMemberName] string source = null);
    }
}
