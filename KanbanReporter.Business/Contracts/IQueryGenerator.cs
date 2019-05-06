using KanbanReporter.Business.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KanbanReporter.Business.Contracts
{
    public interface IQueryGenerator
    {
        Task<IEnumerable<AdoQuery>> LoadAllAsync();

        Task<AdoQuery> GenerateKanbanReportQueryAsync(string eXPECTED_QUERY_NAME);
    }
}
