using KanbanReporter.Business.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KanbanReporter.Business.Contracts
{
    internal interface IQueryManager
    {
        Task<IEnumerable<AdoQuery>> LoadAllAsync();

        Task<List<CompleteWorkItem>> GetWorkItemsFromQueryAsync(AdoQuery adoQuery);

        Task<AdoQuery> GenerateKanbanReportQueryAsync(string queryName);
    }
}
