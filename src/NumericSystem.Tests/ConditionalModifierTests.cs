using System;
using Xunit;
using WFramework.CoreGameDevKit.NumericSystem;

namespace NumericSystem.Tests
{
    /// <summary>
    /// 条件修饰符系统测试
    /// </summary>
    public class ConditionalModifierTests
    {
        [Fact]
        public void Constructor_ShouldCreateValidInstance()
        {
            // Arrange
            var condition = new PredicateCondition(n => n.GetOriginValue() > 100);
            var addition = new AdditionNumericModifier(50, Array.Empty<string>(), "Bonus", 1);

            // Act
            var conditional = new ConditionalNumericModifier(condition, addition, "TestConditional");

            // Assert
            Assert.NotNull(conditional);
            Assert.Equal("TestConditional", conditional.Info.Name);
            Assert.Same(condition, conditional.Condition);
            Assert.Same(addition, conditional.WrappedModifier);
        }

        [Fact]
        public void Apply_WhenConditionMet_ShouldApplyModifier()
        {
            // Arrange
            var numeric = new Numeric(100);
            var condition = new PredicateCondition(n => n.GetOriginValue() >= 1000000);
            var addition = new AdditionNumericModifier(50, Array.Empty<string>(), "Bonus", 1);
            var conditional = new ConditionalNumericModifier(condition, addition, "ConditionalBonus");

            // Act
            numeric.AddModifier(conditional);
            var result = numeric.FinalValue;

            // Assert - 条件满足（1000000 >= 1000000），应该应用 +50
            Assert.Equal(150, result);
        }

        [Fact]
        public void Apply_WhenConditionNotMet_ShouldNotApplyModifier()
        {
            // Arrange
            var numeric = new Numeric(100);
            // 使用固定点数值比较：100 * 10000 = 1000000, 300 * 10000 = 3000000
            var condition = new PredicateCondition(n => n.GetOriginValue() > 3000000);
            var addition = new AdditionNumericModifier(50, Array.Empty<string>(), "Bonus", 1);
            var conditional = new ConditionalNumericModifier(condition, addition, "ConditionalBonus");

            // Act
            numeric.AddModifier(conditional);
            var result = numeric.FinalValue;

            // Assert - 条件不满足（1000000 < 3000000），不应该应用修饰符
            Assert.Equal(100, result);
        }

        [Fact]
        public void CompositeCondition_And_ShouldRequireAllConditions()
        {
            // Arrange
            var numeric = new Numeric(100);

            // 创建 AND 条件：原始值 > 500000 且原始值 < 1500000
            var condition = new CompositeCondition(
                ConditionOperator.And,
                new PredicateCondition(n => n.GetOriginValue() > 500000),
                new PredicateCondition(n => n.GetOriginValue() < 1500000)
            );

            var conditional = ConditionalNumericModifier.ConditionalAdd(condition, 30, "DoubleBonus");

            // Act
            numeric.AddModifier(conditional);
            var result = numeric.FinalValue;

            // Assert - 两个条件都满足，应该应用 +30
            Assert.Equal(130, result);
        }

        [Fact]
        public void CompositeCondition_Or_ShouldRequireAnyCondition()
        {
            // Arrange
            var numeric = new Numeric(50);

            // 创建 OR 条件：原始值 < 600000 或原始值 > 2000000
            var condition = new CompositeCondition(
                ConditionOperator.Or,
                new PredicateCondition(n => n.GetOriginValue() < 600000),
                new PredicateCondition(n => n.GetOriginValue() > 2000000)
            );

            var conditional = ConditionalNumericModifier.ConditionalAdd(condition, 20, "SituationalBonus");

            // Act
            numeric.AddModifier(conditional);
            var result = numeric.FinalValue;

            // Assert - 第一个条件满足（500000 < 600000），应该应用 +20
            Assert.Equal(70, result);
        }

        [Fact]
        public void CompositeCondition_Not_ShouldInvertCondition()
        {
            // Arrange
            var numeric = new Numeric(100);

            // 创建 NOT 条件：原始值不大于 1500000
            var condition = new CompositeCondition(
                ConditionOperator.Not,
                new PredicateCondition(n => n.GetOriginValue() > 1500000)
            );

            var conditional = ConditionalNumericModifier.ConditionalAdd(condition, 30, "NotTooHigh");

            // Act
            numeric.AddModifier(conditional);
            var result = numeric.FinalValue;

            // Assert - 条件满足（1000000 不大于 1500000），应该应用 +30
            Assert.Equal(130, result);
        }

        [Fact]
        public void ConditionBuilder_Where_ShouldWork()
        {
            // Arrange
            var numeric = new Numeric(100);

            // Act - 使用 ConditionBuilder
            var condition = ConditionBuilder.Where(n => n.GetOriginValue() > 500000).Build();
            var conditional = ConditionalNumericModifier.ConditionalAdd(condition, 30, "BuilderBonus");

            numeric.AddModifier(conditional);

            // Assert
            Assert.Equal(130, numeric.FinalValue);
        }

        [Fact]
        public void ConditionBuilder_And_Or_Not_ShouldWork()
        {
            // Arrange
            var numeric = new Numeric(100);

            // Act - 复合条件：原始值 > 500000 且原始值 < 2000000
            var condition = ConditionBuilder
                .Where(n => n.GetOriginValue() > 500000)
                .And(n => n.GetOriginValue() < 2000000)
                .Build();

            var conditional = ConditionalNumericModifier.ConditionalAdd(condition, 25, "ComplexBonus");
            numeric.AddModifier(conditional);

            // Assert
            Assert.Equal(125, numeric.FinalValue);
        }

        [Fact]
        public void ConditionalModifier_ShouldRespectPriority()
        {
            // Arrange
            var numeric = new Numeric(100);
            var condition = new PredicateCondition(n => n.GetOriginValue() >= 1000000);

            // Act - 添加高优先级的条件修饰符
            var conditional = new ConditionalNumericModifier(
                condition,
                new AdditionNumericModifier(50, Array.Empty<string>(), "Bonus", 1, ModifierPriority.Critical),
                "CriticalBonus",
                1,
                ModifierPriority.Critical
            );

            numeric.AddModifier(conditional);

            // Assert
            Assert.Equal(ModifierPriority.Critical, conditional.Info.Priority);
            Assert.Equal(150, numeric.FinalValue);
        }

        [Fact]
        public void DifferentNumeric_WithSameConditional_ShouldWorkIndependently()
        {
            // Arrange - 创建条件修饰符 (80 * 10000 = 800000)
            var condition = new PredicateCondition(n => n.GetOriginValue() < 800000);
            var conditional = ConditionalNumericModifier.ConditionalAdd(condition, 20, "Recovery");

            // Act 1 - 高血量对象不满足条件
            var highHealth = new Numeric(100);
            highHealth.AddModifier(conditional);
            Assert.Equal(100, highHealth.FinalValue);

            // Act 2 - 低血量对象满足条件
            var lowHealth = new Numeric(70);
            lowHealth.AddModifier(conditional);
            var result = lowHealth.FinalValue;

            // Assert - 700000 < 800000，条件满足，应用 +20 恢复
            Assert.Equal(90, result);
        }
    }
}
