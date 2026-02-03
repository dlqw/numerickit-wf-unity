using Xunit;
using System;
using WFramework.CoreGameDevKit.NumericSystem;

namespace NumericSystem.Tests
{
    /// <summary>
    /// FractionNumericModifier 分数修饰符测试
    /// </summary>
    public class FractionNumericModifierTests
    {
        [Fact]
        public void MultiplyModifier_OverrideType_ShouldOverrideValue()
        {
            // Arrange
            var numeric = new Numeric(100);

            // Act
            numeric.AddModifier(new FractionNumericModifier(200, 100, FractionType.Override));

            // Assert
            Assert.Equal(200, numeric.FinalValue);
        }

        [Fact]
        public void MultiplyModifier_IncreaseType_ShouldIncreaseValue()
        {
            // Arrange
            var numeric = new Numeric(100);

            // Act
            numeric.AddModifier(new FractionNumericModifier(100, 100, FractionType.Increase));

            // Assert
            Assert.Equal(200, numeric.FinalValue);
        }

        [Fact]
        public void MultiplyModifier_WithPercent200Override_ShouldDoubleValue()
        {
            // Arrange
            var numeric = new Numeric(100);

            // Act
            numeric.AddModifier(new FractionNumericModifier(200, 100, FractionType.Override));

            // Assert
            Assert.Equal(200, numeric.FinalValue);
        }

        [Fact]
        public void MultiplyModifier_WithPercent50Increase_ShouldIncreaseByHalf()
        {
            // Arrange
            var numeric = new Numeric(100);

            // Act
            numeric.AddModifier(new FractionNumericModifier(50, 100, FractionType.Increase));

            // Assert
            Assert.Equal(150, numeric.FinalValue);
        }

        [Fact]
        public void MultiplyModifier_ComplexFraction_ShouldCalculateCorrectly()
        {
            // Arrange
            var numeric = new Numeric(100);

            // Act: 1/2 of 100 = 50
            numeric.AddModifier(new FractionNumericModifier(1, 2, FractionType.Override));

            // Assert
            Assert.Equal(50, numeric.FinalValue);
        }

        [Fact]
        public void MultiplyModifier_WithTags_ShouldOnlyAffectTaggedValues()
        {
            // Arrange
            var numeric = new Numeric(100);
            numeric += (50, new[] { "Equipment" }, "Armor", 1);

            // Act
            numeric.AddModifier(new FractionNumericModifier(200, 100, FractionType.Override, new[] { "Equipment" }, "Upgrade", 1));

            // Assert: 100 (base) + 100 (equipment modified) = 200
            Assert.Equal(200, numeric.FinalValue);
        }

        [Fact]
        public void MultiplyModifier_WithSelfTag_ShouldAffectBaseValue()
        {
            // Arrange
            var numeric = new Numeric(100);

            // Act
            numeric.AddModifier(new FractionNumericModifier(150, 100, FractionType.Increase, new[] { NumericModifierConfig.TagSelf }, "Boost", 1));

            // Assert: 100 * 1.5 = 150
            Assert.Equal(150, numeric.FinalValue);
        }

        [Fact]
        public void MultiplyModifier_MultipleModifiers_ShouldComposeCorrectly()
        {
            // Arrange
            var numeric = new Numeric(100);
            numeric += (50, new[] { "Equipment" }, "Armor", 1);

            // Act: First modifier doubles equipment, second increases equipment by 50%
            numeric.AddModifier(new FractionNumericModifier(200, 100, FractionType.Override, new[] { "Equipment" }, "Upgrade1", 1));
            numeric.AddModifier(new FractionNumericModifier(150, 100, FractionType.Increase, new[] { "Equipment" }, "Upgrade2", 1));

            // Assert: Base (100) + Equipment modified (50 * 2.0 * 1.5 = 150) = 250
            Assert.Equal(250, numeric.FinalValue);
        }

        [Fact]
        public void Constructor_WithZeroDenominator_ShouldThrowArgumentException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                new FractionNumericModifier(1, 0, FractionType.Override));
        }

        [Fact]
        public void Constructor_WithNegativeDenominator_ShouldWork()
        {
            // Arrange & Act
            var numeric = new Numeric(100);
            numeric.AddModifier(new FractionNumericModifier(1, -2, FractionType.Override));

            // Assert
            Assert.Equal(-50, numeric.FinalValue);
        }

        [Fact]
        public void RemoveModifier_ShouldDecreaseValue()
        {
            // Arrange
            var numeric = new Numeric(100);
            var modifier = new FractionNumericModifier(200, 100, FractionType.Override);
            numeric.AddModifier(modifier);

            // Act
            numeric.RemoveModifier(modifier);

            // Assert
            Assert.Equal(100, numeric.FinalValue);
        }

        [Fact]
        public void MultiplyModifier_WithLargeCount_ShouldHandleCorrectly()
        {
            // Arrange
            var numeric = new Numeric(100);
            var modifier = new FractionNumericModifier(
                2, 1,
                FractionType.Override,
                Array.Empty<string>(),
                "Double",
                3); // Count = 3, so 2^3 = 8

            // Act
            numeric.AddModifier(modifier);

            // Assert: 100 * 2^3 = 800
            Assert.Equal(800, numeric.FinalValue);
        }

        [Fact]
        public void MultiplyModifier_Overflow_ShouldThrowOverflowException()
        {
            // Arrange
            var numeric = new Numeric(1000000);
            numeric += (1, Array.Empty<string>(), "Stack", 1000);

            // Act & Assert
            Assert.Throws<OverflowException>(() =>
            {
                numeric.AddModifier(new FractionNumericModifier(500, 100, FractionType.Increase, Array.Empty<string>(), "BigBoost", 1));
            });
        }
    }
}
