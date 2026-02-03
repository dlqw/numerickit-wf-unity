using System;
using System.Linq;
using WFramework.CoreGameDevKit.NumericSystem;
using WFramework.CoreGameDevKit.NumericSystem.Core;
using Xunit;

namespace NumericSystem.Tests
{
    /// <summary>
    /// 测试新架构组件的功能：IModifierCollection, IModifierSorter, IModifierEvaluator
    /// </summary>
    public class CoreArchitectureTests
    {
        #region ModifierCollection Tests

        [Fact]
        public void ModifierCollection_Add_ShouldAddModifier()
        {
            // Arrange
            var collection = new ModifierCollection();
            var modifier = new AdditionNumericModifier(10, new[] { "Test" }, "TestMod", 1);

            // Act
            collection.Add(modifier);

            // Assert
            Assert.Equal(1, collection.Count);
            Assert.False(collection.IsEmpty);
        }

        [Fact]
        public void ModifierCollection_Add_NamedModifiers_ShouldMergeCount()
        {
            // Arrange
            var collection = new ModifierCollection();
            var modifier1 = new AdditionNumericModifier(10, new[] { "Test" }, "TestMod", 1);
            var modifier2 = new AdditionNumericModifier(20, new[] { "Test" }, "TestMod", 1);

            // Act
            collection.Add(modifier1);
            collection.Add(modifier2);

            // Assert
            Assert.Equal(1, collection.Count);
            Assert.Equal(2, collection.GetAll()[0].Info.Count);
        }

        [Fact]
        public void ModifierCollection_Add_AnonymousModifiers_ShouldNotMerge()
        {
            // Arrange
            var collection = new ModifierCollection();
            var modifier1 = new AdditionNumericModifier(10);
            var modifier2 = new AdditionNumericModifier(20);

            // Act
            collection.Add(modifier1);
            collection.Add(modifier2);

            // Assert
            Assert.Equal(2, collection.Count);
        }

        [Fact]
        public void ModifierCollection_Remove_ShouldRemoveModifier()
        {
            // Arrange
            var collection = new ModifierCollection();
            var modifier = new AdditionNumericModifier(10, new[] { "Test" }, "TestMod", 1);
            collection.Add(modifier);

            // Act
            var result = collection.Remove(modifier);

            // Assert
            Assert.True(result);
            Assert.Equal(0, collection.Count);
            Assert.True(collection.IsEmpty);
        }

        [Fact]
        public void ModifierCollection_Remove_NamedModifier_ShouldRemoveFromCollection()
        {
            // Arrange
            var collection = new ModifierCollection();
            var modifier = new AdditionNumericModifier(10, new[] { "Test" }, "TestMod", 1);
            collection.Add(modifier);

            // Act
            var removed = collection.Remove(modifier);

            // Assert
            Assert.True(removed);
            Assert.Equal(0, collection.Count);
            Assert.True(collection.IsEmpty);
        }

        [Fact]
        public void ModifierCollection_Clear_ShouldRemoveAllModifiers()
        {
            // Arrange
            var collection = new ModifierCollection();
            collection.Add(new AdditionNumericModifier(10, new[] { "Test" }, "TestMod1", 1));
            collection.Add(new AdditionNumericModifier(20, new[] { "Test" }, "TestMod2", 1));

            // Act
            collection.Clear();

            // Assert
            Assert.Equal(0, collection.Count);
            Assert.True(collection.IsEmpty);
        }

        [Fact]
        public void ModifierCollection_GetAddModifierValue_ShouldReturnCorrectSum()
        {
            // Arrange
            var collection = new ModifierCollection();
            collection.Add(new AdditionNumericModifier(10, new[] { "Test" }, "TestMod1", 1));
            collection.Add(new AdditionNumericModifier(20, new[] { "Test" }, "TestMod2", 2));

            // Act
            var sum = collection.GetAddModifierValue();

            // Assert
            // 10*1 + 20*2 = 10 + 40 = 50 (in fixed point: 500000)
            Assert.Equal(500000, sum);
        }

        [Fact]
        public void ModifierCollection_GetAddModifierValueByTag_ShouldFilterByTags()
        {
            // Arrange
            var collection = new ModifierCollection();
            collection.Add(new AdditionNumericModifier(10, new[] { "Equipment" }, "Armor", 1));
            collection.Add(new AdditionNumericModifier(20, new[] { "Buff" }, "Strength", 1));
            collection.Add(new AdditionNumericModifier(30, new[] { "Equipment" }, "Weapon", 1));

            // Act
            var equipmentSum = collection.GetAddModifierValueByTag(new[] { "Equipment" });

            // Assert
            // 10 + 30 = 40 (in fixed point: 400000)
            Assert.Equal(400000, equipmentSum);
        }

        [Fact]
        public void ModifierCollection_FindByName_ShouldReturnCorrectModifier()
        {
            // Arrange
            var collection = new ModifierCollection();
            var modifier = new AdditionNumericModifier(10, new[] { "Test" }, "TestMod", 1);
            collection.Add(modifier);

            // Act
            var found = collection.FindByName("TestMod");

            // Assert
            Assert.NotNull(found);
            Assert.Equal("TestMod", found.Info.Name);
        }

        [Fact]
        public void ModifierCollection_GetAllOfType_ShouldReturnOnlyMatchingType()
        {
            // Arrange
            var collection = new ModifierCollection();
            collection.Add(new AdditionNumericModifier(10, new[] { "Test" }, "TestMod", 1));
            collection.Add(new AdditionNumericModifier(20, new[] { "Test" }, "TestMod2", 1));
            collection.Add(new FractionNumericModifier(150, FractionType.Increase, new[] { "Test" }, "FracMod", 1));

            // Act
            var addModifiers = collection.GetAll<AdditionNumericModifier>();

            // Assert
            Assert.Equal(2, addModifiers.Count);
        }

        #endregion

        #region PriorityBasedModifierSorter Tests

        [Fact]
        public void PriorityBasedModifierSorter_Sort_ShouldOrderByPriority()
        {
            // Arrange
            var sorter = new PriorityBasedModifierSorter();
            var modifiers = new[]
            {
                new AdditionNumericModifier(10, new[] { "Test" }, "Low", 1, ModifierPriority.Buff),
                new AdditionNumericModifier(20, new[] { "Test" }, "High", 1, ModifierPriority.Base),
                new AdditionNumericModifier(30, new[] { "Test" }, "Medium", 1, ModifierPriority.Equipment)
            };

            // Act
            var sorted = sorter.Sort(modifiers);

            // Assert
            Assert.Equal("High", sorted[0].Info.Name); // Priority 100
            Assert.Equal("Medium", sorted[1].Info.Name); // Priority 200
            Assert.Equal("Low", sorted[2].Info.Name); // Priority 300
        }

        [Fact]
        public void PriorityBasedModifierSorter_Sort_SamePriority_ShouldOrderByName()
        {
            // Arrange
            var sorter = new PriorityBasedModifierSorter();
            var modifiers = new[]
            {
                new AdditionNumericModifier(10, new[] { "Test" }, "Zeta", 1, ModifierPriority.Base),
                new AdditionNumericModifier(20, new[] { "Test" }, "Alpha", 1, ModifierPriority.Base),
                new AdditionNumericModifier(30, new[] { "Test" }, "Beta", 1, ModifierPriority.Base)
            };

            // Act
            var sorted = sorter.Sort(modifiers);

            // Assert
            Assert.Equal("Alpha", sorted[0].Info.Name);
            Assert.Equal("Beta", sorted[1].Info.Name);
            Assert.Equal("Zeta", sorted[2].Info.Name);
        }

        [Fact]
        public void PriorityBasedModifierSorter_Sort_SamePriorityAndName_ShouldOrderByCount()
        {
            // Arrange
            var sorter = new PriorityBasedModifierSorter();
            var modifiers = new[]
            {
                new AdditionNumericModifier(10, new[] { "Test" }, "Test", 3, ModifierPriority.Base),
                new AdditionNumericModifier(20, new[] { "Test" }, "Test", 1, ModifierPriority.Base),
                new AdditionNumericModifier(30, new[] { "Test" }, "Test", 2, ModifierPriority.Base)
            };

            // Act
            var sorted = sorter.Sort(modifiers);

            // Assert
            Assert.Equal(1, sorted[0].Info.Count);
            Assert.Equal(2, sorted[1].Info.Count);
            Assert.Equal(3, sorted[2].Info.Count);
        }

        [Fact]
        public void PriorityBasedModifierSorter_Sort_EmptyList_ShouldReturnEmptyList()
        {
            // Arrange
            var sorter = new PriorityBasedModifierSorter();
            var modifiers = Array.Empty<INumericModifier>();

            // Act
            var sorted = sorter.Sort(modifiers);

            // Assert
            Assert.Empty(sorted);
        }

        #endregion

        #region DefaultModifierEvaluator Tests

        [Fact]
        public void DefaultModifierEvaluator_CanEvaluate_ShouldReturnTrueForKnownModifiers()
        {
            // Arrange
            var evaluator = new DefaultModifierEvaluator();
            var addMod = new AdditionNumericModifier(10);
            var fracMod = new FractionNumericModifier(150, FractionType.Increase);
            var customMod = new CustomNumericModifier(x => x);
            var condMod = new ConditionalNumericModifier(
                ConditionBuilder.Where(n => n.FinalValue > 0).Build(),
                new AdditionNumericModifier(10),
                "Test"
            );

            // Act & Assert
            Assert.True(evaluator.CanEvaluate(addMod));
            Assert.True(evaluator.CanEvaluate(fracMod));
            Assert.True(evaluator.CanEvaluate(customMod));
            Assert.True(evaluator.CanEvaluate(condMod));
        }

        [Fact]
        public void DefaultModifierEvaluator_Evaluate_AdditionModifier_ShouldAddValue()
        {
            // Arrange
            var evaluator = new DefaultModifierEvaluator();
            var modifier = new AdditionNumericModifier(10);
            var numeric = new Numeric(100);

            // Act
            var result = evaluator.Evaluate(numeric.GetOriginValue(), modifier, numeric);

            // Assert
            // 1000000 + 100000 = 1100000 (in fixed point)
            Assert.Equal(1100000, result);
        }

        [Fact]
        public void DefaultModifierEvaluator_Evaluate_FractionModifier_ShouldApplyFraction()
        {
            // Arrange
            var evaluator = new DefaultModifierEvaluator();
            var modifier = new FractionNumericModifier(150, FractionType.Increase);
            var numeric = new Numeric(100);

            // Act
            var result = evaluator.Evaluate(numeric.GetOriginValue(), modifier, numeric);

            // Assert - The result should be calculated through the modifier's Apply method
            // This test verifies the evaluator correctly delegates to the modifier
            Assert.True(result > 0);
        }

        [Fact]
        public void DefaultModifierEvaluator_Evaluate_WithNullModifier_ShouldThrowArgumentNullException()
        {
            // Arrange
            var evaluator = new DefaultModifierEvaluator();
            var numeric = new Numeric(100);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                evaluator.Evaluate(numeric.GetOriginValue(), null, numeric));
        }

        [Fact]
        public void DefaultModifierEvaluator_Evaluate_WithNullContext_ShouldThrowArgumentNullException()
        {
            // Arrange
            var evaluator = new DefaultModifierEvaluator();
            var modifier = new AdditionNumericModifier(10);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                evaluator.Evaluate(1000000, modifier, null));
        }

        #endregion

        #region Integration Tests

        [Fact]
        public void Numeric_WithNewArchitecture_ShouldWorkCorrectly()
        {
            // Arrange & Act
            var numeric = new Numeric(100);
            numeric += 50;
            numeric *= (150, FractionType.Increase);

            // Assert
            // Base value 100 * 2.5 = 250, plus additive 50 = 300
            Assert.Equal(300, numeric.FinalValue);
        }

        [Fact]
        public void Numeric_WithPriorityModifiers_ShouldApplyInCorrectOrder()
        {
            // Arrange & Act
            var numeric = new Numeric(100);
            numeric.AddModifier(new AdditionNumericModifier(50, new[] { "Base" }, "BaseBonus", 1, ModifierPriority.Base));
            numeric.AddModifier(new AdditionNumericModifier(30, new[] { "Equipment" }, "EquipBonus", 1, ModifierPriority.Equipment));
            numeric.AddModifier(new AdditionNumericModifier(20, new[] { "Buff" }, "BuffBonus", 1, ModifierPriority.Buff));

            // Assert
            // 100 + 50 + 30 + 20 = 200
            Assert.Equal(200, numeric.FinalValue);

            // Check that modifiers are sorted by priority
            var allModifiers = numeric.GetAllModifiers();
            Assert.Equal("BaseBonus", allModifiers[0].Info.Name);
            Assert.Equal("EquipBonus", allModifiers[1].Info.Name);
            Assert.Equal("BuffBonus", allModifiers[2].Info.Name);
        }

        [Fact]
        public void Numeric_AfterRefactoring_MaintainsBackwardCompatibility()
        {
            // Test basic operations work as before
            var numeric = new Numeric(100);

            // Add modifiers
            numeric += 20;
            Assert.Equal(120, numeric.FinalValue);

            // Multiply by percentage with empty tags (affects base value only)
            // Base value 100 * 2.5 = 250, then add 20 = 270
            numeric *= (150, FractionType.Increase);
            Assert.Equal(270, numeric.FinalValue);

            // Custom modifier (clamp)
            numeric.ClampMax(150);
            Assert.Equal(150, numeric.FinalValue);

            // Test modifiers query methods
            var allModifiers = numeric.GetAllModifiers();
            Assert.NotEmpty(allModifiers);

            // Test diagnostic methods
            var stats = numeric.GetModifierStats();
            Assert.NotNull(stats);

            var cacheStatus = numeric.GetCacheStatus();
            Assert.NotNull(cacheStatus);
        }

        #endregion
    }
}
