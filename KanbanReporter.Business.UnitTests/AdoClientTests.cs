using KanbanReporter.Business.Contracts;
using KanbanReporter.Business.Implementation;
using KanbanReporter.UnitTestingTools;
using Moq;
using System;
using Xunit;

namespace KanbanReporter.Business.UnitTests
{

    public class AdoClientTests 
    {
        private InternalTestClass test = new InternalTestClass();

        private class InternalTestClass : TestsFor<AdoClient>
        {
            public override void RunBeforeEachUnitTest()
            {
                GetMockFor<ISettings>().SetupGet(p => p["AdoOrgName"]).Returns("not used");
                GetMockFor<ISettings>().SetupGet(p => p["AdoProjectName"]).Returns("not used");
                GetMockFor<ISettings>().SetupGet(p => p["AdoQueryGuid"]).Returns("not used");
                GetMockFor<ISettings>().SetupGet(p => p["AdoPersonalAccessToken"]).Returns("not used");
                GetMockFor<ISettings>().SetupGet(p => p["AdoRepositoryId"]).Returns("not used");
                GetMockFor<ISettings>().SetupGet(p => p["AdoRepositoryName"]).Returns("not used");
                GetMockFor<ISettings>().SetupGet(p => p["MarkdownFilePath"]).Returns("not used");
                GetMockFor<ISettings>().SetupGet(p => p["AdoBranchName"]).Returns("not used");

                // Assume all Guids are valid for these unit tests
                GetMockFor<IGuidValidator>().Setup(v => v.IsValid(It.IsAny<string>())).Returns(true);
            }
        }

        [Fact]
        public void WhenCreated_SettingsAreMissing_ThrowsException()
        {
            // Arrange            
            var settings      = new Mock<ISettings>().Object;
            var logger        = test.GetMockFor<ILogger>().Object;
            var guidValidator = test.GetMockFor<IGuidValidator>().Object;

            // Act & Assert
            Assert.Throws<InvalidProgramException>(() => new AdoClient(settings, logger, guidValidator));
        }        
    }
}
