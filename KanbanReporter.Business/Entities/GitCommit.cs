namespace KanbanReporter.Business.Entities
{
    internal class GitCommit
    {
        public Refupdate[] refUpdates { get; set; }
        public Commit[] commits { get; set; }
    }

    internal class Commit
    {
        public string comment { get; set; }
        public Change[] changes { get; set; }
    }

    internal class Change
    {
        public string changeType { get; set; }
        public Item item { get; set; }
        public Newcontent newContent { get; set; }
    }

    internal class Item
    {
        public string path { get; set; }
    }

    internal class Newcontent
    {
        public string content { get; set; }
        public string contentType { get; set; }
    }

}
