using Xunit;
using WFramework.CoreGameDevKit.NumericSystem;
using static WFramework.CoreGameDevKit.NumericSystem.NumericExtensions;

namespace NumericSystem.Tests
{
    /// <summary>
    /// NumericExtensions 扩展方法测试
    /// </summary>
    public class NumericExtensionsTests
    {
        [Fact]
        public void AddEquipment_ShouldAddEquipmentModifier()
        {
            // Arrange
            var health = new Numeric(100);

            // Act
            health.AddEquipment(20, "Armor");

            // Assert
            Assert.Equal(120, health.FinalValue);
            Assert.True(health.HasModifier("Armor"));
        }

        [Fact]
        public void AddBuff_ShouldAddBuffModifier()
        {
            // Arrange
            var health = new Numeric(100);

            // Act
            health.AddBuff(30, "Strength");

            // Assert
            Assert.Equal(130, health.FinalValue);
            Assert.True(health.HasModifier("Strength"));
        }

        [Fact]
        public void AddDebuff_ShouldAddNegativeModifier()
        {
            // Arrange
            var health = new Numeric(100);

            // Act
            health.AddDebuff(20, "Poison");

            // Assert
            Assert.Equal(80, health.FinalValue);
            Assert.True(health.HasModifier("Poison"));
        }

        [Fact]
        public void BoostBaseByPercentage_ShouldIncreaseBaseValue()
        {
            // Arrange
            var health = new Numeric(100);

            // Act
            health.BoostBaseByPercentage(150);

            // Assert
            Assert.Equal(250, health.FinalValue);
        }

        [Fact]
        public void ClampMin_ShouldEnforceMinimum()
        {
            // Arrange
            var health = new Numeric(100);
            health += (-150, Array.Empty<string>(), "Damage", 1);

            // Act
            health.ClampMin(50, "MinHP");

            // Assert
            Assert.Equal(50, health.FinalValue);
        }

        [Fact]
        public void ClampMax_ShouldEnforceMaximum()
        {
            // Arrange
            var health = new Numeric(100);
            health += (200, Array.Empty<string>(), "Heal", 1);

            // Act
            health.ClampMax(150, "MaxHP");

            // Assert
            Assert.Equal(150, health.FinalValue);
        }

        [Fact]
        public void ClampRange_ShouldEnforceRange()
        {
            // Arrange
            var health = new Numeric(100);
            health += (200, Array.Empty<string>(), "Heal", 1);

            // Act
            health.ClampRange(50, 150, "HPLimit");

            // Assert
            Assert.Equal(150, health.FinalValue);
        }

        [Fact]
        public void AddIf_ShouldOnlyAddWhenConditionIsTrue()
        {
            // Arrange
            var health = new Numeric(100);

            // Act - 条件为 true，应该添加
            health.AddIf(
                h => h.FinalValue < 150,
                () => new AdditionNumericModifier(20, new[] { "Buff" }, "Heal", 1)
            );

            // Assert
            Assert.Equal(120, health.FinalValue);
            Assert.True(health.HasModifier("Heal"));
        }

        [Fact]
        public void AddIf_ShouldNotAddWhenConditionIsFalse()
        {
            // Arrange
            var health = new Numeric(100);

            // Act - 条件为 false，不应该添加
            health.AddIf(
                h => h.FinalValue > 150,
                () => new AdditionNumericModifier(20, new[] { "Buff" }, "Heal", 1)
            );

            // Assert
            Assert.Equal(100, health.FinalValue);
            Assert.False(health.HasModifier("Heal"));
        }

        [Fact]
        public void DoIf_ShouldOnlyExecuteWhenConditionIsTrue()
        {
            // Arrange
            var health = new Numeric(100);

            // Act - 条件为 true，应该执行
            health.DoIf(
                h => h.FinalValue < 150,
                h => h += 10
            );

            // Assert
            Assert.Equal(110, health.FinalValue);
        }

        [Fact]
        public void DoIf_ShouldNotExecuteWhenConditionIsFalse()
        {
            // Arrange
            var health = new Numeric(100);

            // Act - 条件为 false，不应该执行
            health.DoIf(
                h => h.FinalValue > 150,
                h => h += 10
            );

            // Assert
            Assert.Equal(100, health.FinalValue);
        }

        [Fact]
        public void HasModifier_ShouldReturnTrueForExistingModifier()
        {
            // Arrange
            var health = new Numeric(100);
            health += (20, new[] { "Equipment" }, "Armor", 1);

            // Act
            var hasArmor = health.HasModifier("Armor");

            // Assert
            Assert.True(hasArmor);
        }

        [Fact]
        public void HasModifier_ShouldReturnFalseForNonExistingModifier()
        {
            // Arrange
            var health = new Numeric(100);

            // Act
            var hasWeapon = health.HasModifier("Weapon");

            // Assert
            Assert.False(hasWeapon);
        }

        [Fact]
        public void GetModifierCount_ShouldReturnCorrectCount()
        {
            // Arrange
            var health = new Numeric(100);
            health += (10, Array.Empty<string>(), "Stack", 1);
            health += (10, Array.Empty<string>(), "Stack", 1);

            // Act
            var count = health.GetModifierCount("Stack");

            // Assert
            Assert.Equal(2, count);
        }

        [Fact]
        public void GetTaggedModifierValue_ShouldReturnCorrectValue()
        {
            // Arrange
            var health = new Numeric(100);
            health += (20, new[] { "Equipment" }, "Armor", 1);
            health += (30, new[] { "Buff" }, "Strength", 1);

            // Act
            var equipmentValue = health.GetTaggedModifierValue("Equipment");

            // Assert
            Assert.Equal(20, equipmentValue);
        }

        [Fact]
        public void GetModifiersByTags_ShouldReturnMatchingModifiers()
        {
            // Arrange
            var health = new Numeric(100);
            health += (20, new[] { "Equipment" }, "Armor", 1);
            health += (30, new[] { "Buff" }, "Strength", 1);

            // Act
            var equipmentMods = health.GetModifiersByTags("Equipment");

            // Assert
            Assert.Single(equipmentMods);
            Assert.Equal("Armor", equipmentMods[0].Info.Name);
        }

        [Fact]
        public void ToFormattedString_ShouldReturnSimpleString()
        {
            // Arrange
            var health = new Numeric(100);

            // Act
            var formatted = health.ToFormattedString(showDetails: false);

            // Assert
            Assert.Equal("100", formatted);
        }

        [Fact]
        public void ToFormattedString_WithDetails_ShouldReturnDetailedString()
        {
            // Arrange
            var health = new Numeric(100);
            health += (20, new[] { "Equipment" }, "Armor", 1);

            // Act
            var formatted = health.ToFormattedString(showDetails: true);

            // Assert
            Assert.Contains("Value: 120", formatted);
            Assert.Contains("Origin:", formatted);
            Assert.Contains("Additions:", formatted);
            Assert.Contains("Modifiers: 1", formatted);
        }

        [Fact]
        public void Build_WithBuilder_ShouldCreateCorrectNumeric()
        {
            // Act
            var health = NumericExtensions.Build(100, builder =>
            {
                builder.AddEquipment(20, "Armor");
                builder.AddBuff(30, "Strength");
                builder.BoostBase(150, "BaseBoost");
                builder.WithMaxLimit(300, "MaxHP");
            });

            // Assert: 100 * 2.5 (BoostBase) + 20 (Armor) + 30 (Strength) = 300
            Assert.Equal(300, health.FinalValue);
            Assert.True(health.HasModifier("Armor"));
            Assert.True(health.HasModifier("Strength"));
            Assert.True(health.HasModifier("BaseBoost"));
            // Note: MaxHP is a CustomNumericModifier, might not be found by HasModifier
        }

        [Fact]
        public void AddModifiers_Multiple_ShouldAddAllModifiers()
        {
            // Arrange
            var health = new Numeric(100);

            // Act
            health.AddModifiers(
                new AdditionNumericModifier(10, new[] { "A" }, "Mod1", 1),
                new AdditionNumericModifier(20, new[] { "B" }, "Mod2", 1),
                new AdditionNumericModifier(30, new[] { "C" }, "Mod3", 1)
            );

            // Assert
            Assert.Equal(160, health.FinalValue);
            Assert.Equal(3, health.GetAllModifiers().Count);
        }
    }
}
