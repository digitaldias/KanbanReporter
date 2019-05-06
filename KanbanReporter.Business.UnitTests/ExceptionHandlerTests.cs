using KanbanReporter.Business.Contracts;
using KanbanReporter.Business.Entities;
using KanbanReporter.Business.Implementation;
using KanbanReporter.UnitTestingTools;
using Moq;
using Should;
using System;
using System.Threading.Tasks;
using Xunit;

namespace KanbanReporter.Business.UnitTests
{
    public class ExceptionHandlerTests 
    {
        private InternalTestClass test = new InternalTestClass();

        private class InternalTestClass : TestsFor<ExceptionHandler>
        {
        }

        [Fact]
        public void RunSyncronously_CalledWithNull_LogsWarningMessage()
        {
            // Arrange
            Func<Task> nullFunc = null;

            // Act
            test.Instance.RunSyncronously(nullFunc);

            // Assert
            test.GetMockFor<ILogger>().Verify(logger => logger.LogWarning(It.IsAny<string>()), Times.Once());
        }

        [Fact]
        public void RunSyncronously_CalledWithValidTask_ExecutesTaskWithoutLogging()
        {
            // Arrange
            bool flag = false;
            Func<Task> setFlag = () => Task.FromResult(flag = true);

            // Act
            test.Instance.RunSyncronously(setFlag);

            // Assert
            flag.ShouldBeTrue();
            test.GetMockFor<ILogger>().Verify(logger => logger.LogError(It.IsAny<string>(), null, null), Times.Never());
        }

        [Fact]
        public void RunSyncronously_FunctionThrowsException_ExceptionIsCaughtAndLogged()
        {
            // Arrange
            var errorMessage = "Something bad happened";
            var invalidProgramException = new InvalidProgramException(errorMessage);
            Func<Task> badFunction = () => throw invalidProgramException;

            // Act
            test.Instance.RunSyncronously(badFunction);

            // Assert
            test.GetMockFor<ILogger>().Verify(logger => logger.LogError(errorMessage, invalidProgramException, It.IsAny<string>()), Times.Once());
        }

        [Fact]
        public async Task GetAsync_CalledWithNull_LogsError()
        {
            // Arrange
            Func<Task<int>> nullFunction = null;
            // Act
            await test.Instance.GetAsync(nullFunction);

            // Assert
            test.GetMockFor<ILogger>()
                .Verify(logger => logger.LogWarning(It.IsAny<string>()), Times.Once());
        }

        [Fact]
        public async Task GetAsync_CalledWithNull_ReturnsDefaultValue()
        {
            // Arrange
            Func<Task<int>> nullFunction = null;
            // Act
            var result = await test.Instance.GetAsync(nullFunction);

            // Assert
            result.ShouldEqual(default(int));
        }

        [Fact]
        public async Task GetAsync_CalledWithValidFunction_ReturnsTheResultOfThatFunction()
        {
            // Arrange
            int returnValue = 1313;
            Func<Task<int>> validFunction = () => Task.FromResult(returnValue);
            
            // Act
            var result = await test.Instance.GetAsync(validFunction);

            // Assert
            result.ShouldEqual(1313);
        }

        [Fact]
        public async Task GetAsync_FunctionThrowsException_LoggerLogsError()
        {
            // Arrange
            var badException = new Exception("I'm bad");
            Func<Task<int>> badFunction = () => throw badException;

            // Act
            var result = await test.Instance.GetAsync(badFunction);

            // Assert
            test.GetMockFor<ILogger>()
                .Verify(logger => logger.LogError(It.IsAny<string>(), badException, It.IsAny<string>()), Times.Once());
        }

        [Fact]
        public async Task GetAsync_FunctionThrowsException_ReturnsDefaultValue()
        {
            // Arrange
            var badException = new Exception("I'm bad");
            Func<Task<Workitem>> badFunction = () => throw badException;

            // Act
            var result = await test.Instance.GetAsync(badFunction);

            // Assert
            result.ShouldEqual(default(Workitem));
        }
    }
}
