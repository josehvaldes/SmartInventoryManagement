using Xunit;

namespace SmartInventory.UnitTests
{
    public class MathTests
    {

        [Fact]
        public void Addition_TwoPlusTwo_EqualsFour()
        {
            // Arrange
            int a = 2;
            int b = 2;
            // Act
            int result = a + b;
            // Assert
            Assert.Equal(4, result);
        }

        [Theory]
        [InlineData(3, 5, 8)]
        [InlineData(1, 1, 2)]
        public void Addition_VariousInputs_ReturnsExpectedResult(int a, int b, int expected)
        {
            // Act
            int result = a + b;
            // Assert
            Assert.Equal(expected, result);
        }

    }
}
