using KanbanReporter.Business.Entities;
using System.Threading.Tasks;

namespace KanbanReporter.Business.Contracts
{
    internal interface ISourceControlManager
    {

        Task<VersionedFileDetails> GetVersionDetailsForReadmeFileAsync();

        Task<bool> CommitReportAndCreatePullRequestAsync(string finalReport, VersionedFileDetails readmefileDetails);
    }
}
