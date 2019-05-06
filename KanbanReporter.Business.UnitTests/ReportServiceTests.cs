using FizzWare.NBuilder;
using KanbanReporter.Business.Contracts;
using KanbanReporter.Business.Entities;
using KanbanReporter.Business.Implementation;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace KanbanReporter.Business.UnitTests
{
    public class ReportServiceTests 
    {
        private readonly Mock<ILogger>                _loggerMock;
        private readonly Mock<IAdoClient>             _adoClientMock;
        private readonly Mock<IQueryGenerator>        _queryGeneratorMock;
        private readonly Mock<IMarkdownReportCreator> _markdownReportCreatorMock;

        // Note that we need a "real" ExceptionHandler to get any testing done
        private readonly ExceptionHandler _exceptionHandler;

        private ReportService Instance { get; set; }

        public ReportServiceTests()
        {
            _loggerMock                = new Mock<ILogger>();
            _adoClientMock             = new Mock<IAdoClient>();
            _queryGeneratorMock        = new Mock<IQueryGenerator>();
            _markdownReportCreatorMock = new Mock<IMarkdownReportCreator>();
            _exceptionHandler          = new ExceptionHandler(_loggerMock.Object);

            Instance = new ReportService(_loggerMock.Object, _adoClientMock.Object, _markdownReportCreatorMock.Object, _exceptionHandler, _queryGeneratorMock.Object);
        }

        [Fact]
        public async Task CreateReportAsync_WhenCalled_LogsTheCall()
        {
            // Act
            await Instance.CreateReportAsync();

            // Assert            
            _loggerMock.Verify(logger => logger.Enter(Instance, nameof(Instance.CreateReportAsync)), Times.Once());
        }

        [Fact]
        public async Task CreateReportAsync_WhenCalled_UsesAdoQueryRunner()
        {
            // Act
            await Instance.CreateReportAsync();

            // Assert
            _adoClientMock.Verify(client => client.GetWorkItemsFromQueryAsync(It.IsAny<AdoQuery>()), Times.Once());
        }

        [Fact]
        public async Task CreateReportAsync_WorkItemsFound_InvolesMarkdownReportCreator()
        {
            // Arrange
            var oneHundredWorkItems = AdoClientWillReturnOneHundredWorkItems();

            // Act
            await Instance.CreateReportAsync();

            // Assert
            _markdownReportCreatorMock.Verify(creator => creator.CreateFromWorkItems(oneHundredWorkItems), Times.Once());
        }

        [Fact]
        public async Task CreateReportAsync_NoWorkItemsExist_DoesNotProceedToVersionControl()
        {
            // Act
            await Instance.CreateReportAsync();

            // Assert
            _adoClientMock.Verify(adoClient => adoClient.GetVersionDetailsForReadmeFileAsync(), Times.Never());
        }

        [Fact]
        public async Task CreateReportAsync_WorkItemsExist_ProceedsToGetVersionDetailsForReadmeFile()
        {
            // Arrange
            var workItems = AdoClientWillReturnOneHundredWorkItems();
            _markdownReportCreatorMock.Setup(creator => creator.CreateFromWorkItems(workItems)).Returns("#awesome");

            // Act
            await Instance.CreateReportAsync();

            // Assert
            _adoClientMock.Verify(adoClient => adoClient.GetVersionDetailsForReadmeFileAsync(), Times.Once());
        }

        [Fact]
        public async Task CreateReportAsync_VersionDetailFound_CommitsReport()
        {
            // Arrange
            var workItems = AdoClientWillReturnOneHundredWorkItems();
            var fakeVersion = Builder<VersionedFileDetails>.CreateNew().Build();
            _markdownReportCreatorMock.Setup(creator => creator.CreateFromWorkItems(workItems)).Returns("#awesome");
            _adoClientMock.Setup(client => client.GetVersionDetailsForReadmeFileAsync()).Returns(Task.FromResult(fakeVersion));

            // Act
            await Instance.CreateReportAsync();

            // Assert
            _adoClientMock.Verify(client => client.CommitReportAndCreatePullRequestAsync(It.IsAny<string>(), fakeVersion), Times.Once);
        }


        [Fact]
        public async Task CreateReportAsync_CommittingFails_WarningIsLogged()
        {
            // Arrange
            var workItems = AdoClientWillReturnOneHundredWorkItems();
            var fakeVersion = Builder<VersionedFileDetails>.CreateNew().Build();
            _markdownReportCreatorMock.Setup(creator => creator.CreateFromWorkItems(workItems)).Returns("#awesome");
            _adoClientMock.Setup(client => client.GetVersionDetailsForReadmeFileAsync()).Returns(Task.FromResult(fakeVersion));

            // Act
            await Instance.CreateReportAsync();

            // Assert
            _loggerMock.Verify(logger => logger.LogWarning(It.IsAny<string>()), Times.Once());
        }

        [Fact]
        public async Task CreateReportAsync_CommitSucceeds_SuccessInformationIsLogged()
        {
            // Arrange
            var workItems = AdoClientWillReturnOneHundredWorkItems();
            var fakeVersion = Builder<VersionedFileDetails>.CreateNew().Build();
            _markdownReportCreatorMock.Setup(creator => creator.CreateFromWorkItems(workItems)).Returns("#awesome");
            _adoClientMock.Setup(client => client.GetVersionDetailsForReadmeFileAsync()).Returns(Task.FromResult(fakeVersion));
            _adoClientMock.Setup(client => client.CommitReportAndCreatePullRequestAsync(It.IsAny<string>(), fakeVersion)).Returns(Task.FromResult(true));

            // Act
            await Instance.CreateReportAsync();

            // Assert
            _loggerMock.Verify(logger => logger.LogInfo(It.IsAny<string>()), Times.Once());
        }


        private List<CompleteWorkItem> AdoClientWillReturnOneHundredWorkItems()
        {            
            var oneHundredWorkItems = Builder<CompleteWorkItem>.CreateListOfSize(100).Build().ToList();
            _adoClientMock.Setup(client => client.GetWorkItemsFromQueryAsync(It.IsAny<AdoQuery>())).Returns(Task.FromResult(oneHundredWorkItems));

            var manyAdoQueries = Builder<AdoQuery>.CreateListOfSize(15).Build().AsEnumerable();
            _queryGeneratorMock.Setup(client => client.LoadAllAsync()).Returns(Task.FromResult(manyAdoQueries));

            return oneHundredWorkItems;
        }
    }
}
