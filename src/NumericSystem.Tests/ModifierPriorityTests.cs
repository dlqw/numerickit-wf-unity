using System.Linq;
using Xunit;
using WFramework.CoreGameDevKit.NumericSystem;

namespace NumericSystem.Tests
{
    /// <summary>
    /// 修饰符优先级系统测试
    /// </summary>
    public class ModifierPriorityTests
    {
        [Fact]
        public void Constructor_ShouldSupportPriorityParameter()
        {
            // Act
            var modifier = new AdditionNumericModifier(
                10,
                new[] { "Test" },
                "TestMod",
                1,
                ModifierPriority.Equipment);

            // Assert
            Assert.Equal(ModifierPriority.Equipment, modifier.Info.Priority);
        }

        [Fact]
        public void DefaultPriority_ShouldBeDefault()
        {
            // Act
            var modifier = new AdditionNumericModifier(10);

            // Assert
            Assert.Equal(ModifierPriority.Default, modifier.Info.Priority);
        }

        [Fact]
        public void FractionModifier_ShouldSupportPriority()
        {
            // Act
            var modifier = new FractionNumericModifier(
                150,
                FractionType.Increase,
                new[] { "Test" },
                "Boost",
                1,
                ModifierPriority.Multiplier);

            // Assert
            Assert.Equal(ModifierPriority.Multiplier, modifier.Info.Priority);
        }

        [Fact]
        public void CustomModifier_ShouldSupportPriority()
        {
            // Act
            var modifier = new CustomNumericModifier(
                x => x,
                new[] { "Test" },
                "Clamp",
                1,
                ModifierPriority.Clamp);

            // Assert
            Assert.Equal(ModifierPriority.Clamp, modifier.Info.Priority);
        }

        [Fact]
        public void Modifiers_ShouldApplyInPriorityOrder()
        {
            // Arrange
            var numeric = new Numeric(100);

            // 添加优先级不同的修饰符（顺序是乱序的）
            // Base (100) -> Equipment (200) -> Buff (300) -> Multiplier (500)
            numeric += new AdditionNumericModifier(50, new[] { "Base" }, "BaseMod", 1, ModifierPriority.Base);      // +50
            numeric += new AdditionNumericModifier(30, new[] { "Buff" }, "BuffMod", 1, ModifierPriority.Buff);      // +30
            numeric += new AdditionNumericModifier(20, new[] { "Equipment" }, "EquipMod", 1, ModifierPriority.Equipment); // +20

            // Act - 修饰符应按优先级顺序应用：Base(50) -> Equipment(20) -> Buff(30)
            // 结果 = 100 + 50 + 20 + 30 = 200
            var result = numeric.FinalValue;

            // Assert
            Assert.Equal(200, result);
        }

        [Fact]
        public void FractionMultipliers_ShouldApplyAfterAdditions()
        {
            // Arrange
            var numeric = new Numeric(100);

            // 添加加法修饰符（使用默认优先级 400）
            numeric += new AdditionNumericModifier(50, new[] { "Common" }, "Add", 1, ModifierPriority.Default);

            // 添加分数修饰符（Multiplier 优先级 = 500），使用 Common 标签影响加法值
            // 注意：Addition 用的是 Default(400)，Fraction 用的是 Multiplier(500)
            // 所以加法应该先应用
            numeric *= new FractionNumericModifier(150, FractionType.Increase, new[] { "Common" }, "Boost", 1, ModifierPriority.Multiplier);

            // Act - 计算顺序：
            // 1. Addition (Default=400): 100 + 50 = 150
            // 2. Fraction (Multiplier=500, Common tag): 100 (base) + 50*2.5 (Common addition increased) = 100 + 125 = 225
            var result = numeric.FinalValue;

            // Assert
            Assert.Equal(225, result);
        }

        [Fact]
        public void ClampModifiers_ShouldApplyLast()
        {
            // Arrange
            var numeric = new Numeric(100);
            numeric += new AdditionNumericModifier(200, new[] { "Buff" }, "BigBuff", 1, ModifierPriority.Buff);
            numeric.ClampMax(150, "MaxLimit", ModifierPriority.Clamp);

            // Act - Buff 应用后值是 300，但 Clamp 限制在 150
            var result = numeric.FinalValue;

            // Assert
            Assert.Equal(150, result);
        }

        [Fact]
        public void SamePriority_ShouldSortByName()
        {
            // Arrange
            var numeric = new Numeric(100);

            // 添加相同优先级的修饰符（按名称排序）
            numeric += new AdditionNumericModifier(10, Array.Empty<string>(), "Zebra", 1, ModifierPriority.Buff);
            numeric += new AdditionNumericModifier(20, Array.Empty<string>(), "Apple", 1, ModifierPriority.Buff);
            numeric += new AdditionNumericModifier(30, Array.Empty<string>(), "Mango", 1, ModifierPriority.Buff);

            // Act - 按名称排序应用：Apple(20) -> Mango(30) -> Zebra(10)
            // 结果 = 100 + 20 + 30 + 10 = 160
            var result = numeric.FinalValue;

            // Assert
            Assert.Equal(160, result);
        }

        [Fact]
        public void PriorityEnum_ShouldHaveCorrectValues()
        {
            // Assert - 验证优先级枚举值
            Assert.Equal(0, (int)ModifierPriority.Critical);
            Assert.Equal(100, (int)ModifierPriority.Base);
            Assert.Equal(200, (int)ModifierPriority.Equipment);
            Assert.Equal(300, (int)ModifierPriority.Buff);
            Assert.Equal(400, (int)ModifierPriority.Default);
            Assert.Equal(500, (int)ModifierPriority.Multiplier);
            Assert.Equal(600, (int)ModifierPriority.Clamp);
        }

        [Fact]
        public void ComplexPriorityOrder_ShouldApplyCorrectly()
        {
            // Arrange - 模拟复杂的 RPG 属性计算
            var health = new Numeric(100);

            // 添加各种优先级的修饰符
            health += new AdditionNumericModifier(20, new[] { "Base" }, "RaceBonus", 1, ModifierPriority.Base);          // +20 (优先级 100)
            health += new AdditionNumericModifier(50, new[] { "Equipment" }, "ArmorHP", 1, ModifierPriority.Equipment);  // +50 (优先级 200)
            health += new AdditionNumericModifier(30, new[] { "Buff" }, "Potion", 1, ModifierPriority.Buff);            // +30 (优先级 300)
            health *= new FractionNumericModifier(150, FractionType.Increase, new[] { "SELF" }, "Vitality", 1, ModifierPriority.Multiplier); // ×1.5 (优先级 500)
            health.ClampMax(300, "MaxHP", ModifierPriority.Clamp); // Max 300 (优先级 600)

            // Act - 计算顺序：
            // 1. Base: 100 + 20 = 120
            // 2. Equipment: 120 + 50 = 170
            // 3. Buff: 170 + 30 = 200
            // 4. Multiplier: 200 * 1.5 = 300
            // 5. Clamp: min(300, 300) = 300
            var result = health.FinalValue;

            // Assert
            Assert.Equal(300, result);
        }

        [Fact]
        public void Builder_ShouldUseCorrectPriorities()
        {
            // Act - 使用 Builder 构建
            var health = NumericExtensions.Build(100, builder =>
            {
                builder.AddEquipment(20, "Armor");       // Equipment 优先级
                builder.AddBuff(30, "Strength");         // Buff 优先级
                builder.BoostBase(150, "BaseBoost");     // Multiplier 优先级
                builder.WithMaxLimit(300, "MaxHP");      // Clamp 优先级
            });

            // Assert - 计算顺序：
            // 1. Equipment (200): 100 + 20 = 120
            // 2. Buff (300): 120 + 30 = 150
            // 3. Multiplier (500): 150 * 2.5 = 375
            // 4. Clamp (600): min(375, 300) = 300
            Assert.Equal(300, health.FinalValue);

            // 验证修饰符优先级
            var modifiers = health.GetAllModifiers();
            var armor = modifiers.FirstOrDefault(m => m.Info.Name == "Armor");
            var strength = modifiers.FirstOrDefault(m => m.Info.Name == "Strength");
            var baseBoost = modifiers.FirstOrDefault(m => m.Info.Name == "BaseBoost");

            Assert.NotNull(armor);
            Assert.NotNull(strength);
            Assert.NotNull(baseBoost);

            Assert.Equal(ModifierPriority.Equipment, armor.Info.Priority);
            Assert.Equal(ModifierPriority.Buff, strength.Info.Priority);
            Assert.Equal(ModifierPriority.Multiplier, baseBoost.Info.Priority);
        }

        [Fact]
        public void AnonymousModifiers_ShouldUseDefaultPriority()
        {
            // Arrange
            var numeric = new Numeric(100);

            // Act - 匿名修饰符（使用运算符）
            numeric += 10;
            numeric += 20;
            numeric *= (150, FractionType.Increase);

            // Assert - 加法修饰符使用 Default 优先级，分数修饰符使用 Multiplier 优先级
            var modifiers = numeric.GetAllModifiers();
            var additionModifiers = modifiers.Where(m => m is AdditionNumericModifier);
            var fractionModifiers = modifiers.Where(m => m is FractionNumericModifier);

            foreach (var modifier in additionModifiers)
            {
                Assert.Equal(ModifierPriority.Default, modifier.Info.Priority);
            }
            foreach (var modifier in fractionModifiers)
            {
                Assert.Equal(ModifierPriority.Multiplier, modifier.Info.Priority);
            }
        }

        [Fact]
        public void PriorityWithCount_ShouldStackCorrectly()
        {
            // Arrange
            var numeric = new Numeric(100);

            // 添加两个相同名称的修饰符（会合并 Count）
            numeric += new AdditionNumericModifier(10, new[] { "Buff" }, "Stackable", 1, ModifierPriority.Buff);
            numeric += new AdditionNumericModifier(10, new[] { "Buff" }, "Stackable", 1, ModifierPriority.Buff);

            // Act - 两个修饰符合并，Count = 2，总效果 = 10 * 2 = 20
            var result = numeric.FinalValue;
            var modifiers = numeric.GetAllModifiers();

            // Assert
            Assert.Equal(120, result);
            Assert.Single(modifiers);
            Assert.Equal(2, modifiers[0].Info.Count);
            Assert.Equal(ModifierPriority.Buff, modifiers[0].Info.Priority);
        }
    }
}
