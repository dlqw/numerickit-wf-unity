using Xunit;
using System;
using WFramework.CoreGameDevKit.NumericSystem;

namespace NumericSystem.Tests
{
    /// <summary>
    /// AdditionNumericModifier 加法修饰符测试
    /// </summary>
    public class AdditionNumericModifierTests
    {
        [Fact]
        public void AddModifier_WithIntValue_ShouldIncreaseFinalValue()
        {
            // Arrange
            var numeric = new Numeric(100);

            // Act
            numeric += 20;

            // Assert
            Assert.Equal(120, numeric.FinalValue);
        }

        [Fact]
        public void AddModifier_WithFloatValue_ShouldIncreaseFinalValue()
        {
            // Arrange
            var numeric = new Numeric(100);

            // Act
            numeric += 20.5f;

            // Assert
            Assert.Equal(120, numeric.FinalValue); // 120.5 * 10000 = 1205000 -> 120
        }

        [Fact]
        public void AddModifier_MultipleTimes_ShouldAccumulate()
        {
            // Arrange
            var numeric = new Numeric(100);

            // Act
            numeric += 10;
            numeric += 20;
            numeric += 30;

            // Assert
            Assert.Equal(160, numeric.FinalValue);
        }

        [Fact]
        public void RemoveModifier_WithIntValue_ShouldDecreaseFinalValue()
        {
            // Arrange
            var numeric = new Numeric(100);
            numeric += 20;

            // Act
            numeric -= 20;

            // Assert
            Assert.Equal(100, numeric.FinalValue);
        }

        [Fact]
        public void AddModifier_WithNamedModifier_ShouldStackCount()
        {
            // Arrange
            var numeric = new Numeric(100);
            var modifier1 = new AdditionNumericModifier(10, Array.Empty<string>(), "Buff", 1);
            var modifier2 = new AdditionNumericModifier(10, Array.Empty<string>(), "Buff", 1);

            // Act
            numeric.AddModifier(modifier1);
            numeric.AddModifier(modifier2);

            // Assert
            Assert.Equal(120, numeric.FinalValue);
            Assert.Equal(2, modifier1.Info.Count);
        }

        [Fact]
        public void AddModifier_WithDifferentNames_ShouldNotStack()
        {
            // Arrange
            var numeric = new Numeric(100);
            var modifier1 = new AdditionNumericModifier(10, Array.Empty<string>(), "Buff1", 1);
            var modifier2 = new AdditionNumericModifier(10, Array.Empty<string>(), "Buff2", 1);

            // Act
            numeric.AddModifier(modifier1);
            numeric.AddModifier(modifier2);

            // Assert
            Assert.Equal(120, numeric.FinalValue);
        }

        [Fact]
        public void Clear_ShouldRemoveAllModifiers()
        {
            // Arrange
            var numeric = new Numeric(100);
            numeric += 10;
            numeric += 20;

            // Act
            numeric.Clear();

            // Assert
            Assert.Equal(100, numeric.FinalValue);
        }

        [Fact]
        public void Constructor_AdditionModifierWithNaN_ShouldThrowArgumentException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => new AdditionNumericModifier(float.NaN));
        }

        [Fact]
        public void Constructor_AdditionModifierWithInfinity_ShouldThrowArgumentException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => new AdditionNumericModifier(float.PositiveInfinity));
        }
    }
}
