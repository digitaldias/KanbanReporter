using Newtonsoft.Json;
using System;

namespace KanbanReporter.Business.Entities
{
    internal class CompleteWorkItem
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("fields")]
        public Fields Fields { get; set; }

        [JsonProperty("_links")]
        public Links Links { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        public WorkItemUpdate Updates { get; internal set; }
    }

    internal class Fields
    {

        [JsonProperty("System.IterationPath")]
        public string SystemIterationPath { get; set; }

        [JsonProperty("System.CreatedDate")]
        public DateTime SystemCreatedDate { get; set; }

        [JsonProperty("System.Title")]
        public string SystemTitle { get; set; }

        [JsonProperty("Microsoft.VSTS.Common.ClosedDate")]
        public DateTime MicrosoftVSTSCommonClosedDate
        {
            get; set;
        }
    }

    internal class Links
    {
        public Html Html { get; set; }
    }

    internal class Html
    {
        public string Href { get; set; }
    }
}
