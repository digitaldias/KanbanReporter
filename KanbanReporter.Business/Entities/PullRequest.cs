namespace KanbanReporter.Business.Entities
{
    internal class PullRequest
    {
        public string sourceRefName { get; set; }
        public string targetRefName { get; set; }
        public string title { get; set; }
        public string description { get; set; }
    }
}
