using KanbanReporter.Business.Implementation;
using KanbanReporter.UnitTestingTools;
using Should;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace KanbanReporter.Business.UnitTests
{
    public class GuidValidatorTests
    {
        private TestInstance test = new TestInstance();

        private class TestInstance : TestsFor<GuidValidator>
        {
        }

        [Fact]
        public void IsValid_StringIsNull_ReturnsFalse()
        {
            // Arrange
            string nullString = null;

            // Act
            var result = test.Instance.IsValid(nullString);

            // Assert
            result.ShouldBeFalse();            
        }

        [Fact]
        public void IsValid_StringIsValidGuid_ReturnsTrue()
        {
            // Arrange
            string validGuid = Guid.NewGuid().ToString();

            // Act
            var result = test.Instance.IsValid(validGuid);

            // Assert
            result.ShouldBeTrue();
        }

        [Fact]
        public void IsValid_StringIsNotValid_ReturnsFalse()
        {
            // Arrange
            string badGuid = "I'm bad";

            // Act
            var result = test.Instance.IsValid(badGuid);
            
            // Assert
            result.ShouldBeFalse();
        }

    }
}
