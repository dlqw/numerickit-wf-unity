using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using WFramework.CoreGameDevKit.NumericSystem;

namespace NumericSystem.Tests.Benchmarks
{
    /// <summary>
    /// 架构重构后的性能基准测试
    /// </summary>
    /// <remarks>
    /// 此测试套件验证新架构的性能是否满足要求：
    /// - 性能差异 < 5% （与参考实现相比）
    /// - 内存分配持平或更少
    /// - GC 压力持平或更少
    ///
    /// 运行方式：
    /// dotnet run -c Release --project NumericSystem.Tests -- --filter *ArchitecturePerformanceBenchmarks*
    /// </remarks>
    [MemoryDiagnoser]
    [SimpleJob(warmupCount: 3, iterationCount: 10)]
    public class ArchitecturePerformanceBenchmarks
    {
        private const int BaseValue = 100;

        #region 修饰符创建性能测试

        [Benchmark]
        [BenchmarkCategory("Creation")]
        public void CreateSingleNumeric()
        {
            var numeric = new Numeric(BaseValue);
        }

        [Benchmark]
        [BenchmarkCategory("Creation")]
        public void CreateNumericWithSingleModifier()
        {
            var numeric = new Numeric(BaseValue);
            numeric += 50;
        }

        [Benchmark]
        [BenchmarkCategory("Creation")]
        public void CreateNumericWithMultipleModifiers()
        {
            var numeric = new Numeric(BaseValue);
            numeric += 20;
            numeric += 30;
            numeric *= (50, FractionType.Increase);
        }

        #endregion

        #region 大量修饰符场景

        private List<AdditionNumericModifier> _modifiers100 = null!;
        private List<AdditionNumericModifier> _modifiers1000 = null!;

        [GlobalSetup(Targets = new[] {
            nameof(Add100Modifiers),
            nameof(Add1000Modifiers),
            nameof(CalculateWith100Modifiers),
            nameof(CalculateWith1000Modifiers)
        })]
        public void Setup_ManyModifiers()
        {
            _modifiers100 = new List<AdditionNumericModifier>();
            for (int i = 0; i < 100; i++)
            {
                _modifiers100.Add(new AdditionNumericModifier(
                    10,
                    new[] { $"Tag{i % 10}" },
                    $"Modifier{i}",
                    1
                ));
            }

            _modifiers1000 = new List<AdditionNumericModifier>();
            for (int i = 0; i < 1000; i++)
            {
                _modifiers1000.Add(new AdditionNumericModifier(
                    10,
                    new[] { $"Tag{i % 10}" },
                    $"Modifier{i}",
                    1
                ));
            }
        }

        [Benchmark]
        [BenchmarkCategory("Scalability")]
        public void Add100Modifiers()
        {
            var numeric = new Numeric(BaseValue);
            for (int i = 0; i < 100; i++)
            {
                numeric.AddModifier(_modifiers100[i]);
            }
        }

        [Benchmark]
        [BenchmarkCategory("Scalability")]
        public void Add1000Modifiers()
        {
            var numeric = new Numeric(BaseValue);
            for (int i = 0; i < 1000; i++)
            {
                numeric.AddModifier(_modifiers1000[i]);
            }
        }

        private Numeric _numericWith100Modifiers = null!;
        private Numeric _numericWith1000Modifiers = null!;

        [GlobalSetup(Targets = new[] { nameof(CalculateWith100Modifiers), nameof(CalculateWith1000Modifiers) })]
        public void Setup_CalculateManyModifiers()
        {
            _numericWith100Modifiers = new Numeric(BaseValue);
            for (int i = 0; i < 100; i++)
            {
                _numericWith100Modifiers.AddModifier(new AdditionNumericModifier(
                    10,
                    new[] { $"Tag{i % 10}" },
                    $"Modifier{i}",
                    1
                ));
            }

            _numericWith1000Modifiers = new Numeric(BaseValue);
            for (int i = 0; i < 1000; i++)
            {
                _numericWith1000Modifiers.AddModifier(new AdditionNumericModifier(
                    10,
                    new[] { $"Tag{i % 10}" },
                    $"Modifier{i}",
                    1
                ));
            }
        }

        [Benchmark]
        [BenchmarkCategory("Scalability")]
        public int CalculateWith100Modifiers()
        {
            return _numericWith100Modifiers.FinalValue;
        }

        [Benchmark]
        [BenchmarkCategory("Scalability")]
        public int CalculateWith1000Modifiers()
        {
            return _numericWith1000Modifiers.FinalValue;
        }

        #endregion

        #region 缓存性能测试

        [Benchmark]
        [BenchmarkCategory("Cache")]
        public int GetFinalValue_CachedHit()
        {
            var numeric = new Numeric(BaseValue);
            numeric += 50;
            // 第一次计算
            var value1 = numeric.FinalValue;
            // 第二次计算（应该使用缓存）
            return numeric.FinalValue;
        }

        [Benchmark]
        [BenchmarkCategory("Cache")]
        public int GetFinalValue_CacheInvalidate()
        {
            var numeric = new Numeric(BaseValue);
            numeric += 50;
            var value1 = numeric.FinalValue; // 计算并缓存

            numeric += 10; // 使缓存失效

            return numeric.FinalValue; // 重新计算
        }

        #endregion

        #region 复杂场景性能测试

        [Benchmark]
        [BenchmarkCategory("Complex")]
        public int ComplexGameScenario()
        {
            // 模拟RPG角色属性计算
            var health = new Numeric(100);

            // 种族加成
            health.AddModifier(new AdditionNumericModifier(
                20,
                new[] { "Base", "Race" },
                "HumanBonus",
                1,
                ModifierPriority.Base
            ));

            // 职业加成
            health.AddModifier(new AdditionNumericModifier(
                30,
                new[] { "Base", "Class" },
                "WarriorBonus",
                1,
                ModifierPriority.Base
            ));

            // 装备加成
            health.AddModifier(new AdditionNumericModifier(
                50,
                new[] { "Equipment" },
                "Armor",
                1,
                ModifierPriority.Equipment
            ));
            health.AddModifier(new AdditionNumericModifier(
                20,
                new[] { "Equipment" },
                "Weapon",
                1,
                ModifierPriority.Equipment
            ));
            health.AddModifier(new AdditionNumericModifier(
                30,
                new[] { "Equipment" },
                "Helmet",
                1,
                ModifierPriority.Equipment
            ));

            // Buff 加成
            health.AddModifier(new AdditionNumericModifier(
                30,
                new[] { "Buff" },
                "Strength",
                1,
                ModifierPriority.Buff
            ));
            health.AddModifier(new AdditionNumericModifier(
                20,
                new[] { "Buff" },
                "Vitality",
                1,
                ModifierPriority.Buff
            ));

            // 技能加成
            health.AddModifier(new AdditionNumericModifier(
                40,
                new[] { "Skill" },
                "PassiveSkill",
                1,
                ModifierPriority.Skill
            ));

            // 百分比加成（装备）
            health.AddModifier(new FractionNumericModifier(
                50,
                FractionType.Increase,
                new[] { "Equipment" },
                "EquipmentMultiplier",
                1
            ));

            // 百分比加成（Buff）
            health.AddModifier(new FractionNumericModifier(
                200,
                FractionType.Override,
                new[] { "Buff" },
                "BuffMultiplier",
                1
            ));

            // 约束修饰符
            health.ClampMax(500, "MaxHealthCap");

            return health.FinalValue;
        }

        #endregion

        #region 修饰符查询性能测试

        private Numeric _queryNumeric = null!;

        [GlobalSetup(Targets = new[] {
            nameof(Query_GetAddModifierValue),
            nameof(Query_GetAddModifierValueByTag),
            nameof(Query_GetAllModifiers)
        })]
        public void Setup_QueryTests()
        {
            _queryNumeric = new Numeric(BaseValue);

            // 添加50个修饰符
            for (int i = 0; i < 50; i++)
            {
                _queryNumeric.AddModifier(new AdditionNumericModifier(
                    10 + i,
                    new[] { $"Tag{i % 5}", $"Category{i % 3}" },
                    $"Modifier{i}",
                    1
                ));
            }
        }

        [Benchmark]
        [BenchmarkCategory("Query")]
        public int Query_GetAddModifierValue()
        {
            return _queryNumeric.GetAddModifierValue();
        }

        [Benchmark]
        [BenchmarkCategory("Query")]
        public int Query_GetAddModifierValueByTag()
        {
            return _queryNumeric.GetAddModifierValueByTag(new[] { "Tag2" });
        }

        [Benchmark]
        [BenchmarkCategory("Query")]
        public IReadOnlyList<INumericModifier> Query_GetAllModifiers()
        {
            return _queryNumeric.GetAllModifiers();
        }

        #endregion

        #region 修饰符移除性能测试

        [Benchmark]
        [BenchmarkCategory("Removal")]
        public void RemoveSingleModifier()
        {
            var numeric = new Numeric(BaseValue);
            var modifier = new AdditionNumericModifier(50, new[] { "Buff" }, "Strength", 1);
            numeric.AddModifier(modifier);
            numeric.RemoveModifier(modifier);
        }

        private Numeric _numericForRemoval = null!;

        [GlobalSetup(Targets = new[] { nameof(RemoveMultipleModifiers) })]
        public void Setup_Removal()
        {
            _numericForRemoval = new Numeric(BaseValue);
            for (int i = 0; i < 100; i++)
            {
                _numericForRemoval.AddModifier(new AdditionNumericModifier(
                    10,
                    new[] { "Tag" },
                    $"Modifier{i}",
                    1
                ));
            }
        }

        [Benchmark]
        [BenchmarkCategory("Removal")]
        public void RemoveMultipleModifiers()
        {
            var numeric = new Numeric(BaseValue);
            var modifiers = new List<AdditionNumericModifier>();

            for (int i = 0; i < 50; i++)
            {
                var modifier = new AdditionNumericModifier(
                    10,
                    new[] { "Tag" },
                    $"Modifier{i}",
                    1
                );
                modifiers.Add(modifier);
                numeric.AddModifier(modifier);
            }

            // 移除一半
            for (int i = 0; i < 25; i++)
            {
                numeric.RemoveModifier(modifiers[i]);
            }
        }

        #endregion

        #region 分数修饰符性能测试

        [Benchmark]
        [BenchmarkCategory("Fraction")]
        public int AddSingleFractionModifier()
        {
            var numeric = new Numeric(BaseValue);
            numeric *= (50, FractionType.Increase);
            return numeric.FinalValue;
        }

        [Benchmark]
        [BenchmarkCategory("Fraction")]
        public int AddMultipleFractionModifiers_SameTags()
        {
            var numeric = new Numeric(BaseValue);
            numeric *= (50, FractionType.Increase, new[] { "Buff" }, "Boost1", 1);
            numeric *= (50, FractionType.Increase, new[] { "Buff" }, "Boost2", 1);
            numeric *= (50, FractionType.Increase, new[] { "Buff" }, "Boost3", 1);
            return numeric.FinalValue;
        }

        [Benchmark]
        [BenchmarkCategory("Fraction")]
        public int AddMultipleFractionModifiers_DifferentTags()
        {
            var numeric = new Numeric(BaseValue);
            numeric *= (50, FractionType.Increase, new[] { "Equipment" }, "EquipBoost", 1);
            numeric *= (50, FractionType.Increase, new[] { "Buff" }, "BuffBoost", 1);
            numeric *= (50, FractionType.Increase, new[] { "Skill" }, "SkillBoost", 1);
            return numeric.FinalValue;
        }

        #endregion

        #region 内存分配测试

        [Benchmark]
        [BenchmarkCategory("Memory")]
        public void MemoryAllocation_Creation()
        {
            var numeric = new Numeric(BaseValue);
            numeric += 50;
            numeric *= (50, FractionType.Increase);
        }

        [Benchmark]
        [BenchmarkCategory("Memory")]
        public void MemoryAllocation_Loop()
        {
            for (int i = 0; i < 100; i++)
            {
                var numeric = new Numeric(BaseValue);
                numeric += i;
                numeric *= (50, FractionType.Increase);
            }
        }

        #endregion
    }
}
