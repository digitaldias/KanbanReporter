using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KanbanReporter.Business.Entities;

namespace KanbanReporter.Business.Contracts
{
    internal interface IExceptionHandler
    {
        void RunSyncronously(Func<Task> unsafeFunction);

        Task<TResult> GetAsync<TResult>(Func<Task<TResult>> unsafeFunction);
    }
}
