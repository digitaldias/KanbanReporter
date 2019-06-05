using FizzWare.NBuilder;
using KanbanReporter.Business.Contracts;
using KanbanReporter.Business.Entities;
using KanbanReporter.Business.Implementation;
using KanbanReporter.UnitTestingTools;
using Moq;
using Should;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace KanbanReporter.Business.UnitTests
{
    public class MarkdownReportCreatorTests 
    {
        private class InternalTestClass : TestsFor<MarkdownReportCreator> { }

        private readonly InternalTestClass test = new InternalTestClass();

        [Fact]
        public void CreateFromWorkItems_WhenCalled_LogsTheMethodCall()
        {
            // Act
            var result = test.Instance.CreateFromWorkItems(null);

            // Assert
            test.GetMockFor<ILogger>().Verify(logger => logger.Enter(test.Instance, "CreateFromWorkItems", It.IsAny<object[]>()), Times.Once());
        }

        [Fact]
        public void CreateFromWorkItems_WorkItemsListIsNull_LogsWarningAndReturnsEmptyString()
        {
            // Act
            var result = test.Instance.CreateFromWorkItems(null);

            // Assert
            result.ShouldBeEmpty();
            test.GetMockFor<ILogger>().Verify(log => log.LogWarning(It.IsAny<string>()), Times.Once());
        }

        [Fact]
        public void CreateFromWorkItems_WorkItemsListIsEmpty_LogsWarningAndReturnsEmptyString()
        {
            // Arrange
            var emptyWorkItemsList = new List<CompleteWorkItem>();

            // Act
            var result = test.Instance.CreateFromWorkItems(emptyWorkItemsList);

            // Assert
            result.ShouldBeEmpty();
            test.GetMockFor<ILogger>().Verify(log => log.LogWarning(It.IsAny<string>()), Times.Once());
        }

        [Fact]
        public void CreateFromWorkItems_WorkItemsExist_ProducesTextOutput()
        {
            // Arrange
            var workItems = Builder<CompleteWorkItem>
                                .CreateListOfSize(100)
                                .All()
                                    .With(o => o.Fields = Builder<Fields>.CreateNew()
                                        .With(f => f.SystemIterationPath = "Project\\Sprint 1")                                                                            
                                    .Build())
                                    .With(wi => wi.Updates = Builder<WorkItemUpdate>
                                        .CreateNew()
                                            .With(wiu => wiu.value = Builder<UpdateValue>.CreateListOfSize(5).Build().ToArray())
                                        .Build())
                                    .With(wi => wi.Links = new Links { Html = new Html { Href = "some href" } })
                                .Build()
                                .ToList();

            // Act
            var result = test.Instance.CreateFromWorkItems(workItems);

            // Assert
            result.ShouldNotBeEmpty();
        }
    }
}
