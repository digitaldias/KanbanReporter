using KanbanReporter.Business.Contracts;
using KanbanReporter.Business.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KanbanReporter.Business.Implementation
{
    internal class MarkdownReportCreator : IMarkdownReportCreator
    {
        private readonly ILogger _log;
        private readonly ISettings _settings;

        public MarkdownReportCreator(ILogger log, ISettings settings)
        {
            _log      = log;
            _settings = settings;
        }

        public string CreateFromWorkItems(List<CompleteWorkItem> workItems)
        {
            _log.Enter(this);

            var initialMarkdown = CreateListOfSprints(workItems);
            if (string.IsNullOrEmpty(initialMarkdown))
                return string.Empty;

            return AttachReportHeader(initialMarkdown);
        }

        private string AttachReportHeader(string markdown)
        {
            _log.Enter(this);

            var reportBuilder = new StringBuilder("# Kanban - Overview\n");
            reportBuilder.Append($"Generated automatically every sunday at 6AM\n\n");
            reportBuilder.Append($">{DateTime.Now.ToString("dddd, dd MMM yyyy hh:mm")}\n");
            reportBuilder.Append("\n\n");
            reportBuilder.Append(markdown);

            return reportBuilder.ToString();
        }

        private string CreateListOfSprints(List<CompleteWorkItem> workItems)
        {
            _log.Enter(this);

            if (workItems == null || !workItems.Any())
            {
                _log.LogWarning("No Workitems found to report on");
                return string.Empty;
            }

            var sprintGroups = workItems.GroupBy(w => w.Fields.SystemIterationPath).OrderBy(g => g.Key);
            var finalText = string.Empty;

            foreach (var group in sprintGroups)
            {
                _log.LogInfo($"Processing '{group.Key}'");
                var groupText = new List<string>();

                if (!group.Key.Contains("\\"))
                    continue;

                var sprintName = group.Key.Split('\\')[1];

                groupText.Add($@"## {sprintName}");
                groupText.Add($"During this sprint, {group.Count()} items were closed, with an average lead time of {CalculateAveragesForGroup(group)}.");
                groupText.Add(string.Empty);
                groupText.Add("| Id   | Title | Lead Time | Cycle Time | ");
                groupText.Add("| ---- | ----- | --------- | -----------| ");
                foreach (var workItem in group.OrderBy(w => w.Id))
                {
                    var leadTime = workItem.Fields.MicrosoftVSTSCommonClosedDate - workItem.Fields.SystemCreatedDate;
                    var cycleTime = CalculateCycleTime(workItem);
                    var line = $"| [Item {workItem.Id.ToString("###")}]({workItem.Links.Html.Href}) ";

                    line += $"| {workItem.Fields.SystemTitle} ";
                    line += $"| {string.Format("{0} days, {1} hours", leadTime.ToString("%d"), leadTime.ToString("%h"))} ";

                    if (cycleTime == TimeSpan.Zero)
                        line += $"| &nbsp; | ";
                    else
                        line += $"| {cycleTime.Days} days, {cycleTime.Hours} hours, {cycleTime.Minutes} minutes |";
                    groupText.Add(line);
                }

                groupText.Add("---");
                groupText.Add(string.Empty);
                finalText += string.Join("\n", groupText);
            }
            return finalText;
        }

        private string CalculateAveragesForGroup(IGrouping<string, CompleteWorkItem> group)
        {
            _log.Enter(this, args: group.Key);

            var averageTime = group.Average(workItem => (workItem.Fields.MicrosoftVSTSCommonClosedDate - workItem.Fields.SystemCreatedDate).TotalMilliseconds);

            return $"{TimeSpan.FromMilliseconds(averageTime).Days} days and {TimeSpan.FromMilliseconds(averageTime).Minutes} minutes";
        }

        private TimeSpan CalculateCycleTime(CompleteWorkItem workItem)
        {
            _log.Enter(this, args: workItem.Fields.SystemTitle);

            DateTime startTime = DateTime.MaxValue;
            DateTime stopTime  = DateTime.MinValue;
            
            var boardColumnUpdates = workItem.Updates.value.Where(v => v.fields != null && v.fields.SystemBoardColumn != null);
            var workColumnName     = _settings["WorkColumnName"];


            var enterWorkColumn = boardColumnUpdates.Where(c => c.fields.SystemBoardColumn.newValue == workColumnName).FirstOrDefault();
            var leftWorkColumn  = boardColumnUpdates.Where(c => c.fields.SystemBoardColumn.oldValue == workColumnName).LastOrDefault();

            if (enterWorkColumn == null || leftWorkColumn == null)
                return TimeSpan.Zero;

            startTime = enterWorkColumn.revisedDate;
            stopTime  = leftWorkColumn.revisedDate;

            // Found both dates, otherwise return no time
            if (startTime < DateTime.MaxValue && stopTime > DateTime.MinValue)
            {
                // Sometimes, the stoptime will give us weird values, so let's not accept more than 90 days off
                if (stopTime - startTime > TimeSpan.FromDays(90))
                    return TimeSpan.Zero;

                return stopTime - startTime;
            }

            return TimeSpan.Zero;
        }
    }
}
