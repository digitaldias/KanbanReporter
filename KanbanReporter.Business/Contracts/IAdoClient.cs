using KanbanReporter.Business.Entities;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace KanbanReporter.Business.Contracts
{
    internal interface IAdoClient
    {
        Task<List<CompleteWorkItem>> GetWorkItemsFromQueryAsync(AdoQuery adoQuery);

        Task<VersionedFileDetails> GetVersionDetailsForReadmeFileAsync();

        Task<bool> CommitReportAndCreatePullRequestAsync(string finalReport, VersionedFileDetails readmefileDetails);
    }
}
