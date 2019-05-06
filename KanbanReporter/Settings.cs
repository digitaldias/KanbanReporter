using KanbanReporter.Business.Contracts;
using Microsoft.Extensions.Configuration;

namespace KanbanReporter
{
    public class Settings : ISettings
    {
        private IConfigurationRoot _configuration;

        public Settings(IConfigurationRoot configuration)
        {
            _configuration = configuration;
        }

        public string this[string key]
        {
            get
            {
                return _configuration[key];
            }
        }
    }
}
