using Newtonsoft.Json;

namespace KanbanReporter.Business.Entities
{
    internal class Column
    {
        [JsonProperty("referenceName")]
        public string referenceName { get; set; }

        [JsonProperty("name")]
        public string name { get; set; }

        [JsonProperty("url")]
        public string url { get; set; }
    }

}
