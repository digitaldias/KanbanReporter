using Newtonsoft.Json;
using System;

namespace KanbanReporter.Business.Entities
{
    internal class WorkItemUpdate
    {
        public int count { get; set; }

        [JsonProperty("value")]
        public UpdateValue[] value { get; set; }
    }

    public class UpdateValue
    {
        [JsonProperty("revisedDate")]
        public DateTime revisedDate { get; set; }

        [JsonProperty("fields")]
        public UpdateFields fields { get; set; }
    }

    public class UpdateFields
    {
        [JsonProperty("System.BoardColumn")]
        public SystemBoardcolumn SystemBoardColumn { get; set; }
    }

    public class SystemBoardcolumn
    {
        public string newValue { get; set; }
        public string oldValue { get; set; }
    }
}
