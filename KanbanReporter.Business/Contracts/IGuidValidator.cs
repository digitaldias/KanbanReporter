namespace KanbanReporter.Business.Contracts
{
    internal interface IGuidValidator
    {
        bool IsValid(string candidate);
    }
}
