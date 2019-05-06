using Newtonsoft.Json;

namespace KanbanReporter.Business.Entities
{
    internal class Workitem
    {
        [JsonProperty("id")]
        public int id { get; set; }

        [JsonProperty("url")]
        public string url { get; set; }
    }

}
