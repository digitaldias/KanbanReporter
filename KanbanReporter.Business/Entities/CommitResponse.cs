using System;

namespace KanbanReporter.Business.Entities
{
    internal class CommitResponse
    {
        public Commit[] commits { get; set; }
        public Refupdate[] refUpdates { get; set; }
        public Pushedby pushedBy { get; set; }
        public int pushId { get; set; }
        public DateTime date { get; set; }
        public string url { get; set; }
        public Links _links { get; set; }
    }

    internal class Pushedby
    {
        public string id { get; set; }
        public string displayName { get; set; }
        public string uniqueName { get; set; }
        public string url { get; set; }
        public string imageUrl { get; set; }
    }

 

    internal class Repository
    {
        public string href { get; set; }
    }

    internal class Commits
    {
        public string href { get; set; }
    }

    internal class Pusher
    {
        public string href { get; set; }
    }

    internal class Refs
    {
        public string href { get; set; }
    }


    internal class Author
    {
        public string name { get; set; }
        public string email { get; set; }
        public DateTime date { get; set; }
    }

    internal class Committer
    {
        public string name { get; set; }
        public string email { get; set; }
        public DateTime date { get; set; }
    }

    internal class Refupdate
    {
        public string repositoryId { get; set; }
        public string name { get; set; }
        public string oldObjectId { get; set; }
        public string newObjectId { get; set; }
    }

}
