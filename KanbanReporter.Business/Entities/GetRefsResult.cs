namespace KanbanReporter.Business.Entities
{
    internal class GetRefsResult
    {
        public Value[] value { get; set; }
        public int count { get; set; }
    }


    internal class Value
    {
        public string name { get; set; }
        public string objectId { get; set; }
    }

}
