namespace KanbanReporter.Business.Entities
{
    internal class VersionedFileResult
    {
        public VersionedFileDetails[] value { get; set; }
    }

    public class VersionedFileDetails
    {
        public string path { get; set; }
    }
}
