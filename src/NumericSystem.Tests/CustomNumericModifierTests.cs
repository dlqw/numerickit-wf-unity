using Xunit;
using System;
using WFramework.CoreGameDevKit.NumericSystem;

namespace NumericSystem.Tests
{
    /// <summary>
    /// CustomNumericModifier 自定义修饰符测试
    /// </summary>
    public class CustomNumericModifierTests
    {
        [Fact]
        public void CustomModifier_ClampValue_ShouldRespectBounds()
        {
            // Arrange
            var numeric = new Numeric(100);
            Func<int, int> clamp = value => value > 150 ? 150 : (value < 0 ? 0 : value);

            // Act
            numeric.AddModifier(new CustomNumericModifier(clamp));
            numeric += 100; // Would be 200, but clamped to 150

            // Assert
            Assert.Equal(150, numeric.FinalValue);
        }

        [Fact]
        public void CustomModifier_FloatClampValue_ShouldRespectBounds()
        {
            // Arrange
            var numeric = new Numeric(100.0f);
            Func<float, float> clamp = value => value > 150.0f ? 150.0f : (value < 0.0f ? 0.0f : value);

            // Act
            numeric.AddModifier(new CustomNumericModifier(clamp));
            numeric += 100.0f; // Would be 200, but clamped to 150

            // Assert
            Assert.Equal(150, numeric.FinalValue);
        }

        [Fact]
        public void CustomModifier_MinValue_ShouldEnforceMinimum()
        {
            // Arrange
            var numeric = new Numeric(100);
            Func<int, int> minClamp = value => value < 50 ? 50 : value;

            // Act
            numeric.AddModifier(new CustomNumericModifier(minClamp));
            numeric += -100; // 100 + (-100) = 0, clamped to 50

            // Assert
            Assert.Equal(50, numeric.FinalValue);
        }

        [Fact]
        public void CustomModifier_MaxValue_ShouldEnforceMaximum()
        {
            // Arrange
            var numeric = new Numeric(100);
            Func<int, int> maxClamp = value => value > 150 ? 150 : value;

            // Act
            numeric.AddModifier(new CustomNumericModifier(maxClamp));
            numeric += 100; // Would be 200, but clamped to 150

            // Assert
            Assert.Equal(150, numeric.FinalValue);
        }

        [Fact]
        public void CustomModifier_MultipleCustomModifiers_ShouldApplyInOrder()
        {
            // Arrange
            var numeric = new Numeric(100);
            Func<int, int> clamp1 = value => value > 150 ? 150 : value;
            Func<int, int> clamp2 = value => value < 50 ? 50 : value;

            // Act
            numeric.AddModifier(new CustomNumericModifier(clamp1));
            numeric.AddModifier(new CustomNumericModifier(clamp2));
            numeric += 100; // 200 -> clamped to 150 by first modifier

            // Assert
            Assert.Equal(150, numeric.FinalValue);
        }

        [Fact]
        public void CustomModifier_WithOtherModifiers_ShouldApplyLast()
        {
            // Arrange
            var numeric = new Numeric(100);
            Func<int, int> clamp = value => value > 150 ? 150 : value;

            // Act
            numeric += 100; // 200
            numeric.AddModifier(new CustomNumericModifier(clamp)); // Applied last, clamps to 150

            // Assert
            Assert.Equal(150, numeric.FinalValue);
        }

        [Fact]
        public void Constructor_WithNullIntFunc_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
            {
                new CustomNumericModifier((Func<int, int>)null!);
            });
        }

        [Fact]
        public void Constructor_WithNullFloatFunc_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
            {
                new CustomNumericModifier((Func<float, float>)null!);
            });
        }

        [Fact]
        public void ImplicitOperator_FromFunc_ShouldCreateModifier()
        {
            // Arrange
            Func<int, int> clamp = value => value > 150 ? 150 : value;
            var numeric = new Numeric(100);

            // Act
            numeric += clamp;
            numeric += 100;

            // Assert
            Assert.Equal(150, numeric.FinalValue);
        }

        [Fact]
        public void RemoveCustomModifier_ShouldRemoveConstraint()
        {
            // Arrange
            var numeric = new Numeric(100);
            Func<int, int> clamp = value => value > 150 ? 150 : value;
            var modifier = new CustomNumericModifier(clamp);

            numeric.AddModifier(modifier);
            numeric += 100; // Clamped to 150

            // Act
            numeric.RemoveModifier(modifier);

            // Assert
            Assert.Equal(200, numeric.FinalValue);
        }

        [Fact]
        public void CustomModifier_CustomLogic_ShouldWorkCorrectly()
        {
            // Arrange
            var numeric = new Numeric(100);
            Func<int, int> doubleIfOver100 = value => value > 100 ? value * 2 : value;

            // Act
            numeric.AddModifier(new CustomNumericModifier(doubleIfOver100));
            numeric += 50; // 150, which is > 100, so doubled to 300

            // Assert
            Assert.Equal(300, numeric.FinalValue);
        }
    }
}
