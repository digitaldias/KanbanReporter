using KanbanReporter.Business.Contracts;
using System;
using System.Threading.Tasks;

namespace KanbanReporter.Business.Implementation
{
    internal class ExceptionHandler : IExceptionHandler
    {
        // External Dependencies
        private readonly ILogger _log;

        public ExceptionHandler(ILogger log)
        {
            _log = log;
        }

        public async Task<TResult> GetAsync<TResult>(Func<Task<TResult>> unsafeFunction)
        {
            if (unsafeFunction == null)
            {
                _log.LogWarning("GetAsync<TResult> was called with a null reference");
                return default;
            }
            try
            {
                return await unsafeFunction.Invoke();
            }
            catch(AggregateException aex)
            {
                _log.LogError($"AGGREGATE: {aex.InnerException.Message}");
            }
            catch(Exception ex)
            {
                _log.LogError(ex.Message, ex);
            }
            return default;
        }

        public void RunSyncronously(Func<Task> unsafeFunction)
        {
            if (unsafeFunction == null)
            {
                _log.LogWarning("RunAsync() was called with a null reference");
                return;
            }
            try
            {
                unsafeFunction.Invoke().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                _log.LogError(ex.Message, ex);
            }
        }
    }
}
