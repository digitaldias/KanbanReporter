using KanbanReporter.Business.Entities;
using System.Collections.Generic;

namespace KanbanReporter.Business.Contracts
{
    internal interface IMarkdownReportCreator
    {
        string CreateFromWorkItems(List<CompleteWorkItem> workItems);
    }
}
