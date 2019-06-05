using KanbanReporter.Business.Contracts;
using System;

namespace KanbanReporter.Business.Implementation
{
    internal class GuidValidator : IGuidValidator
    {
        private readonly ILogger _log;

        public GuidValidator(ILogger log)
        {
            _log = log;
        }


        public bool IsValid(string candidate)
        {
            _log.Enter(this, args: candidate);

            if (string.IsNullOrEmpty(candidate))
                return false;

            if (Guid.TryParse(candidate, out _))
                return true;

            return false;
        }
    }
}
