using KanbanReporter.Business.Contracts;
using KanbanReporter.Business.DependencyInversion;
using StructureMap;
using System.Linq;
using System.Threading.Tasks;

namespace KanbanReporter.Business.Implementation
{
    public class ReportService : IReportService
    {
        private static Container _dependencyResolver = null;

        // Constants
        private const string EXPECTED_QUERY_NAME = "KanbanReporterQuery";

        // External dependencies
        private readonly ILogger                _log;
        private readonly ISettings              _settings;
        private readonly IMarkdownReportCreator _markdownReportCreator;
        private readonly IExceptionHandler      _exceptionHandler;
        private readonly IQueryManager          _queryManager;
        private readonly ISourceControlManager  _sourceControlManager;

        public ReportService(ILogger log, ISettings settings)
        {
            if(_dependencyResolver == null)
                 _dependencyResolver = new Container(new ServiceRegistry(log, settings));

            _log                   = log;
            _settings              = settings;
            _markdownReportCreator = _dependencyResolver.GetInstance<IMarkdownReportCreator>();
            _exceptionHandler      = _dependencyResolver.GetInstance<IExceptionHandler>();
            _queryManager          = _dependencyResolver.GetInstance<IQueryManager>();
            _sourceControlManager  = _dependencyResolver.GetInstance<ISourceControlManager>();
        }

        /// <summary>
        /// Used for adding support for unit-testing. Do not make this constructor public
        /// </summary>
        internal ReportService(ILogger log, IMarkdownReportCreator markdownReportCreator, IExceptionHandler exceptionHandler, IQueryManager queryManager, ISourceControlManager sourceControlManager)
        {
            _log                   = log;            
            _markdownReportCreator = markdownReportCreator;
            _exceptionHandler      = exceptionHandler;
            _queryManager          = queryManager;
            _sourceControlManager  = sourceControlManager;
        }


        public async Task CreateReportAsync()
        {
            _log.Enter(this);

            // If a Query does not exist, generate it
            var adoQueries = await _queryManager.LoadAllAsync();            
            var reportQuery = adoQueries.FirstOrDefault(q => q.Name == EXPECTED_QUERY_NAME);

            if (reportQuery == null)
            {
                reportQuery = await _queryManager.GenerateKanbanReportQueryAsync(EXPECTED_QUERY_NAME);
            }

            // Get AzDO WorkItems from Query
            var workItems = await _exceptionHandler.GetAsync(() => _queryManager.GetWorkItemsFromQueryAsync(reportQuery));
            if (workItems == null || !workItems.Any())
            {
                _log.LogWarning("CreateReportAsync() did not find any workItems to process");
                return;
            }

            // Generate Markdown Report
            var finalReport  = _markdownReportCreator.CreateFromWorkItems(workItems);
            if (string.IsNullOrEmpty(finalReport))
            {
                _log.LogWarning("MarkdownReportCreator failed to create a valid markdown report");
                return;
            }

            // Retrieve version details for our target README.md file
            var readmefileDetails = await _sourceControlManager.GetVersionDetailsForReadmeFileAsync();
            if (readmefileDetails == null)
            {
                _log.LogWarning($"Unable to find target file: {_settings["MarkdownFilePath"]}");
                return;
            }

            // Commit and create pull request
            if (!await _sourceControlManager.CommitReportAndCreatePullRequestAsync(finalReport, readmefileDetails))
            {
                _log.LogWarning("Unable to push the latest report to source control");
                return;
            }

            _log.LogInfo("Report created and pushed to source control.");
        }
    }
}
