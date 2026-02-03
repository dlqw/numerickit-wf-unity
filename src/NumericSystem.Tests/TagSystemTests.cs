using Xunit;
using System;
using WFramework.CoreGameDevKit.NumericSystem;

namespace NumericSystem.Tests
{
    /// <summary>
    /// 标签系统测试
    /// </summary>
    public class TagSystemTests
    {
        [Fact]
        public void TagSystem_EquipmentModifier_ShouldOnlyAffectTaggedValues()
        {
            // Arrange
            var health = new Numeric(100);
            health += (20, new[] { "Equipment" }, "Armor", 1);
            health += (30, new[] { "Buff" }, "Strength", 1);

            // Act: Only Equipment tag should be affected
            health.AddModifier(new FractionNumericModifier(150, 100, FractionType.Increase, new[] { "Equipment" }, "ArmorUpgrade", 1));

            // Assert: 100 (base) + 30 (Buff) + 30 (Equipment modified: 20 * 1.5) = 160
            Assert.Equal(160, health.FinalValue);
        }

        [Fact]
        public void TagSystem_SelfTag_ShouldAffectBaseValue()
        {
            // Arrange
            var numeric = new Numeric(100);
            numeric += (50, new[] { "Bonus" }, "Extra", 1);

            // Act: Affect base value with SELF tag
            numeric.AddModifier(new FractionNumericModifier(150, 100, FractionType.Increase, new[] { NumericModifierConfig.TagSelf }, "BaseBoost", 1));

            // Assert: 150 (base modified: 100 * 1.5) + 50 (Bonus) = 200
            Assert.Equal(200, numeric.FinalValue);
        }

        [Fact]
        public void TagSystem_MultipleTags_ShouldMatchAny()
        {
            // Arrange
            var numeric = new Numeric(100);
            numeric += (20, new[] { "Equipment", "Rare" }, "Armor", 1);
            numeric += (30, new[] { "Buff" }, "Strength", 1);

            // Act: Modifier with "Equipment" tag should affect value with ["Equipment", "Rare"]
            numeric.AddModifier(new FractionNumericModifier(200, 100, FractionType.Override, new[] { "Equipment" }, "Upgrade", 1));

            // Assert: 100 (base) + 30 (Buff) + 40 (Equipment modified: 20 * 2.0) = 170
            Assert.Equal(170, numeric.FinalValue);
        }

        [Fact]
        public void TagSystem_NoTagsModifier_ShouldNotAffectTaggedValues()
        {
            // Arrange
            var numeric = new Numeric(100);
            numeric += (50, new[] { "Equipment" }, "Armor", 1);

            // Act: Modifier with empty tags should not affect Equipment
            numeric.AddModifier(new FractionNumericModifier(150, 100, FractionType.Increase, Array.Empty<string>(), "GlobalBoost", 1));

            // Assert: Only base value affected: 150 (100 * 1.5) + 50 (Equipment) = 200
            Assert.Equal(200, numeric.FinalValue);
        }

        [Fact]
        public void TagSystem_Intersection_ShouldMatchCorrectly()
        {
            // Arrange
            var numeric = new Numeric(100);
            numeric += (20, new[] { "Fire", "Damage" }, "FireBonus", 1);
            numeric += (30, new[] { "Ice", "Damage" }, "IceBonus", 1);

            // Act: Modifier with "Fire" tag
            numeric.AddModifier(new FractionNumericModifier(200, 100, FractionType.Override, new[] { "Fire" }, "FireBoost", 1));

            // Assert: 100 (base) + 30 (Ice) + 40 (Fire modified: 20 * 2.0) = 170
            Assert.Equal(170, numeric.FinalValue);
        }

        [Fact]
        public void TagSystem_ComplexScenario_ShouldCalculateCorrectly()
        {
            // Arrange: Example from documentation
            var health = new Numeric(100);
            health += (20, new[] { "Equipment" }, "Armor", 1);
            // Expected: 120

            // Act
            health.AddModifier(new FractionNumericModifier(120, 100, FractionType.Override, new[] { "Equipment" }, "ArmorUpgrade", 1));
            // Expected: 100 + 24 = 124

            health.AddModifier(new FractionNumericModifier(150, 100, FractionType.Increase, new[] { NumericModifierConfig.TagSelf }, "Upgrade", 1));
            // Expected: 150 + 24 = 174

            // Assert
            Assert.Equal(174, health.FinalValue);
        }

        [Fact]
        public void TagSystem_AdditionWithSameTag_ShouldBeAffectedByFraction()
        {
            // Arrange
            var numeric = new Numeric(100);
            numeric += (10, new[] { "Stackable" }, "Stack1", 1);
            numeric += (20, new[] { "Stackable" }, "Stack2", 1);

            // Act
            numeric.AddModifier(new FractionNumericModifier(150, 100, FractionType.Increase, new[] { "Stackable" }, "Boost", 1));

            // Assert: 100 (base) + 45 (Stackable modified: (10 + 20) * 1.5) = 145
            Assert.Equal(145, numeric.FinalValue);
        }
    }
}
