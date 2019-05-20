using KanbanReporter.Business.Entities;
using System.Threading.Tasks;

namespace KanbanReporter.Business.Contracts
{
    internal interface ISourceControlManager
    {
        Task<VersionedFileDetails> GetVersionDetailsForReadmeFileAsync();

        Task<Value> CommitReport(string finalReport, VersionedFileDetails readmefileDetails);

        Task<bool> CreatePullRequest(Value gitBranchReference);
    }
}
