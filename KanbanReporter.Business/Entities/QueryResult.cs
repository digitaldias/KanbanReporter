using Newtonsoft.Json;
using System;

namespace KanbanReporter.Business.Entities
{
    internal class QueryResult
    {
        [JsonProperty("columns")]
        public Column[] columns { get; set; }

        [JsonProperty("workItems")]
        public Workitem[] workItems { get; set; }
    }
}
