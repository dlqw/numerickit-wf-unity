using System;
using System.Linq;
using Xunit;
using WFramework.CoreGameDevKit.NumericSystem;
using WFramework.CoreGameDevKit.NumericSystem.Serialization;

namespace NumericSystem.Tests
{
    /// <summary>
    /// 序列化系统测试
    /// </summary>
    public class NumericSerializationTests
    {
        [Fact]
        public void Serialize_AdditionModifiers_ShouldSucceed()
        {
            // Arrange
            var numeric = new Numeric(100);
            numeric += 50;
            numeric += (30, new[] { "Equipment" }, "Armor", 1);

            // Act
            var data = numeric.Serialize();

            // Assert
            Assert.NotNull(data);
            Assert.Equal(100 * 10000, data.OriginValue);
            Assert.Equal(2, data.Modifiers.Length);

            var firstModifier = data.Modifiers[0];
            Assert.Equal(ModifierType.Add, firstModifier.Type);
            Assert.Equal(50 * 10000, firstModifier.StoreValue);

            var secondModifier = data.Modifiers[1];
            Assert.Equal(ModifierType.Add, secondModifier.Type);
            Assert.Equal(30 * 10000, secondModifier.StoreValue);
            Assert.Contains("Equipment", secondModifier.Tags);
        }

        [Fact]
        public void Serialize_FractionModifiers_ShouldSucceed()
        {
            // Arrange
            var numeric = new Numeric(100);
            numeric *= (50, FractionType.Increase);
            numeric *= (200, FractionType.Override, new[] { "Buff" }, "StrengthBoost", 2);

            // Act
            var data = numeric.Serialize();

            // Assert
            Assert.NotNull(data);
            Assert.Equal(2, data.Modifiers.Length);

            var fractionModifier = data.Modifiers.FirstOrDefault(m => m.Type == ModifierType.Frac && m.Name.Contains("StrengthBoost"));
            Assert.NotNull(fractionModifier!);
            Assert.Equal(200, fractionModifier.Numerator);
            Assert.Equal(100, fractionModifier.Denominator);
            Assert.Equal(FractionType.Override, fractionModifier.FractionType);
            Assert.Equal(2, fractionModifier.Count);
            Assert.Contains("Buff", fractionModifier.Tags);
        }

        [Fact]
        public void Deserialize_AdditionModifiers_ShouldRestoreCorrectly()
        {
            // Arrange
            var original = new Numeric(100);
            original += 50;
            original += (30, new[] { "Equipment" }, "Armor", 1);
            var data = original.Serialize();

            // Act
            var restored = data.Deserialize();

            // Assert
            Assert.Equal(100, restored.GetOriginValue().ToFloat());
            Assert.Equal(180, restored.FinalValue);  // 100 + 50 + 30 = 180
        }

        [Fact]
        public void Deserialize_FractionModifiers_ShouldRestoreCorrectly()
        {
            // Arrange
            var original = new Numeric(100);
            original.AddModifier(new FractionNumericModifier(150, FractionType.Increase, Array.Empty<string>(), "StrengthBoost", 1));
            var data = original.Serialize();

            // Act
            var restored = data.Deserialize();

            // Assert
            // 100 * 2.5 = 250 (150% Increase = 1 + 150/100 = 2.5)
            Assert.Equal(250, original.FinalValue);
            Assert.Equal(original.FinalValue, restored.FinalValue);
        }

        [Fact]
        public void SerializeDeserialize_RoundTrip_ShouldPreserveValue()
        {
            // Arrange
            var original = new Numeric(100);
            original += 50;
            original *= (50, FractionType.Increase);
            original *= (200, FractionType.Override, new[] { "Buff" }, "StrengthBoost", 2);
            var originalValue = original.FinalValue;

            // Act
            var data = original.Serialize();
            var restored = data.Deserialize();

            // Assert
            Assert.Equal(originalValue, restored.FinalValue);
        }

        [Fact]
        public void Serialize_EmptyNumeric_ShouldSucceed()
        {
            // Arrange
            var numeric = new Numeric(100);

            // Act
            var data = numeric.Serialize();

            // Assert
            Assert.NotNull(data);
            Assert.Equal(100 * 10000, data.OriginValue);
            Assert.Empty(data.Modifiers);
        }

        [Fact]
        public void Deserialize_EmptyData_ShouldCreateEmptyNumeric()
        {
            // Arrange
            var data = new NumericData(100 * 10000, Array.Empty<ModifierData>());

            // Act
            var numeric = data.Deserialize();

            // Assert
            Assert.Equal(100, numeric.FinalValue);
            Assert.Empty(numeric.GetAllModifiers());
        }

        [Fact]
        public void Serialize_MultipleModifierTypes_ShouldPreserveAll()
        {
            // Arrange
            var numeric = new Numeric(100);
            numeric += 50;
            numeric *= (50, FractionType.Increase);
            numeric += (20, new[] { "Equipment" }, "Weapon", 1);

            // Act
            var data = numeric.Serialize();

            // Assert
            Assert.NotNull(data);
            Assert.Equal(3, data.Modifiers.Length);

            var addModifiers = data.Modifiers.Where(m => m.Type == ModifierType.Add).ToArray();
            Assert.Equal(2, addModifiers.Length);

            var fracModifiers = data.Modifiers.Where(m => m.Type == ModifierType.Frac).ToArray();
            Assert.Equal(1, fracModifiers.Length);
        }

        [Fact]
        public void Deserialize_ShouldPreserveModifierPriority()
        {
            // Arrange
            var original = new Numeric(100);
            original.AddModifier(new AdditionNumericModifier(50, new[] { "High" }, "HighPriority", 1, ModifierPriority.Critical));
            original.AddModifier(new AdditionNumericModifier(30, new[] { "Buff" }, "BuffPriority", 1, ModifierPriority.Buff));
            var data = original.Serialize();

            // Act
            var restored = data.Deserialize();
            var modifiers = restored.GetAllModifiers();

            // Assert
            Assert.Equal(2, modifiers.Count);
            var highPriority = modifiers.OfType<AdditionNumericModifier>().FirstOrDefault(m => m.Info.Name == "HighPriority");
            var buffPriority = modifiers.OfType<AdditionNumericModifier>().FirstOrDefault(m => m.Info.Name == "BuffPriority");

            Assert.NotNull(highPriority!);
            Assert.NotNull(buffPriority!);
            Assert.Equal(ModifierPriority.Critical, highPriority.Info.Priority);
            Assert.Equal(ModifierPriority.Buff, buffPriority.Info.Priority);
        }

        [Fact]
        public void Serialize_Deserialize_ComplexScenario_ShouldWork()
        {
            // Arrange - 模拟游戏角色属性
            var baseHealth = new Numeric(100);
            baseHealth += (50, new[] { "Equipment" }, "Armor", 1);
            baseHealth += (30, new[] { "Buff" }, "Vitality", 1);
            baseHealth *= (50, FractionType.Increase, new[] { "Buff" }, "HealthPercentBoost", 1);
            baseHealth *= (200, FractionType.Override, new[] { "Equipment" }, "HealthMultiplier", 2);

            var originalValue = baseHealth.FinalValue;

            // Act
            var data = baseHealth.Serialize();
            var restored = data.Deserialize();

            // Assert
            Assert.Equal(originalValue, restored.FinalValue);
        }

        [Fact]
        public void Serialize_ShouldNotIncludeCustomModifiers()
        {
            // Arrange
            var numeric = new Numeric(100);
            numeric += 50;
            numeric.AddModifier(new CustomNumericModifier(n => n + 20, Array.Empty<string>(), "CustomBoost"));

            // Act
            var data = numeric.Serialize();

            // Assert
            // CustomNumericModifier 包含不可序列化的委托，不应该被包含
            var customModifiers = data.Modifiers.Where(m => m.Type == ModifierType.Custom);
            Assert.Empty(customModifiers);
        }

        [Fact]
        public void Serialize_ShouldNotIncludeConditionalModifiers()
        {
            // Arrange
            var numeric = new Numeric(100);
            numeric += 50;
            var condition = new PredicateCondition(n => n.GetOriginValue() > 500000);
            var conditional = ConditionalNumericModifier.ConditionalAdd(condition, 30, "ConditionalBonus");
            numeric.AddModifier(conditional);

            // Act
            var data = numeric.Serialize();

            // Assert
            // ConditionalNumericModifier 包含不可序列化的条件委托，不应该被包含
            var conditionalModifiers = data.Modifiers.Where(m => m.Type == ModifierType.Custom);
            Assert.Empty(conditionalModifiers);
        }

        [Fact]
        public void Deserialize_NullData_ShouldThrow()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => NumericSerializationExtensions.Deserialize(null!));
        }

        [Fact]
        public void Serialize_NullNumeric_ShouldThrow()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => NumericSerializationExtensions.Serialize(null!));
        }
    }
}
