using System.Threading.Tasks;

namespace KanbanReporter.Business.Contracts
{
    public interface IReportService
    {
        Task CreateReportAsync();
    }
}
