namespace KanbanReporter.Business.Contracts
{
    public interface ISettings
    {
        string this[string key] { get; }
    }
}
