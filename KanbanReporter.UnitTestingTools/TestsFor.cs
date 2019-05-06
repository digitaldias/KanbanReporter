using System;
using Moq;
using StructureMap.AutoMocking.Moq;

namespace KanbanReporter.UnitTestingTools
{
    public class TestsFor<TInstance> where TInstance : class
    {
        public MoqAutoMocker<TInstance> AutoMock { get; set; }

        public TInstance Instance { get; set; }

        public TestsFor()
        {
            AutoMock = new MoqAutoMocker<TInstance>();

            RunBeforeEachUnitTest();

            Instance = AutoMock.ClassUnderTest;

            RunAfterEachUnitTest();
        }

        /// <summary>
        /// Returns the Mock that owns the TContract interface
        /// </summary>
        /// <typeparam name="TContract">The interface for which you want to obtain a mock</typeparam>
        /// <returns>The mock that owns the instance of the interface</returns>
        public Mock<TContract> GetMockFor<TContract>() where TContract : class
        {
            return Mock.Get(AutoMock.Get<TContract>());
        }

        /// <summary>
        /// Override this method to execute code after each unit test
        /// </summary>
        public virtual void RunBeforeEachUnitTest(){            
        }

        /// <summary>
        /// Override this method to execute code after each unit test
        /// </summary>
        public virtual void RunAfterEachUnitTest() { 
        }
    }
}
