using KanbanReporter.Business.Contracts;
using KanbanReporter.Business.Implementation;
using StructureMap;

namespace KanbanReporter.Business.DependencyInversion
{
    internal class ServiceRegistry : Registry
    {
        public ServiceRegistry(ILogger log, ISettings settings)
        {
            // Multiple instances
            For<IExceptionHandler>().Use<ExceptionHandler>();
            For<IMarkdownReportCreator>().Use<MarkdownReportCreator>();
            For<IGuidValidator>().Use<GuidValidator>();
            For<IQueryGenerator>().Use<QueryGenerator>();
            For<IAdoClient>().Use<AdoClient>();

            // Singletons
            For<ILogger>().Singleton().Use(log);
            For<ISettings>().Singleton().Use(settings);
        }
    }
}
