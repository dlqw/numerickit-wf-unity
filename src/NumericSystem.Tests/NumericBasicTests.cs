using Xunit;
using WFramework.CoreGameDevKit.NumericSystem;

namespace NumericSystem.Tests
{
    /// <summary>
    /// Numeric 类基础功能测试
    /// </summary>
    public class NumericBasicTests
    {
        [Fact]
        public void Constructor_WithIntValue_ShouldSetCorrectOriginValue()
        {
            // Arrange & Act
            var numeric = new Numeric(100);

            // Assert
            Assert.Equal(100, numeric.GetOriginValue());
        }

        [Fact]
        public void Constructor_WithFloatValue_ShouldSetCorrectOriginValue()
        {
            // Arrange & Act
            var numeric = new Numeric(100.5f);

            // Assert
            Assert.Equal(1005000, numeric.GetOriginValue()); // 100.5 * 10000
        }

        [Fact]
        public void FinalValue_WithoutModifiers_ShouldReturnOriginValue()
        {
            // Arrange
            var numeric = new Numeric(100);

            // Act
            var result = numeric.FinalValue;

            // Assert
            Assert.Equal(100, result);
        }

        [Fact]
        public void FinalValueF_WithoutModifiers_ShouldReturnOriginValue()
        {
            // Arrange - use float constructor for float test
            var numeric = new Numeric(100.0f);

            // Act
            var result = numeric.FinalValueF;

            // Assert
            Assert.InRange(result, 99.99f, 100.01f);
        }

        [Fact]
        public void ImplicitOperator_FromInt_ShouldCreateNumeric()
        {
            // Arrange
            Numeric numeric = 100;

            // Act & Assert
            Assert.Equal(100, numeric.GetOriginValue());
            Assert.Equal(100, numeric.FinalValue);
        }

        [Fact]
        public void ImplicitOperator_FromFloat_ShouldCreateNumeric()
        {
            // Arrange
            Numeric numeric = 100.5f;

            // Act & Assert
            Assert.Equal(1005000, numeric.GetOriginValue());
        }

        [Fact]
        public void ImplicitOperator_ToInt_ShouldReturnFinalValue()
        {
            // Arrange
            Numeric numeric = 100;
            int result = numeric;

            // Act & Assert
            Assert.Equal(100, result);
        }

        [Fact]
        public void ImplicitOperator_ToFloat_ShouldReturnFinalValue()
        {
            // Arrange - use float for float test
            Numeric numeric = 100.0f;
            float result = numeric;

            // Act & Assert
            Assert.InRange(result, 99.99f, 100.01f);
        }

        [Fact]
        public void Constructor_WithNaN_ShouldThrowArgumentException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => new Numeric(float.NaN));
        }

        [Fact]
        public void Constructor_WithInfinity_ShouldThrowArgumentException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => new Numeric(float.PositiveInfinity));
            Assert.Throws<ArgumentException>(() => new Numeric(float.NegativeInfinity));
        }
    }
}
