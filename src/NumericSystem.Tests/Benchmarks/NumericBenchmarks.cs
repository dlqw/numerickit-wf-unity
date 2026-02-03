using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using WFramework.CoreGameDevKit.NumericSystem;

namespace NumericSystem.Tests.Benchmarks
{
    /// <summary>
    /// Numeric 系统性能基准测试
    /// </summary>
    /// <remarks>
    /// 运行方式：
    /// 1. 在 Visual Studio 中：右键点击项目 -> 运行基准测试
    /// 2. 使用命令行：dotnet run -c Release --project NumericSystem.Tests -- --filter *NumericBenchmarks*
    ///
    /// 注意：基准测试需要在 Release 配置下运行才能获得准确结果
    /// </remarks>
    [MemoryDiagnoser]
    [SimpleJob(warmupCount: 3, iterationCount: 10)]
    public class NumericBenchmarks
    {
        private const int BaseValue = 100;

        #region 基础操作基准测试

        [Benchmark]
        [BenchmarkCategory("Basic")]
        public void CreateNumeric()
        {
            var numeric = new Numeric(BaseValue);
        }

        [Benchmark]
        [BenchmarkCategory("Basic")]
        public void AddSingleModifier()
        {
            var numeric = new Numeric(BaseValue);
            numeric += 50;
        }

        [Benchmark]
        [BenchmarkCategory("Basic")]
        public void GetFinalValue_SingleModifier()
        {
            var numeric = new Numeric(BaseValue);
            numeric += 50;
            var value = numeric.FinalValue;
        }

        [Benchmark]
        [BenchmarkCategory("Basic")]
        public void GetFinalValue_Cached()
        {
            var numeric = new Numeric(BaseValue);
            numeric += 50;
            // 第一次计算
            var value1 = numeric.FinalValue;
            // 第二次计算（应该使用缓存）
            var value2 = numeric.FinalValue;
        }

        #endregion

        #region 大量修饰符基准测试

        private List<AdditionNumericModifier> _additionModifiers = null!;

        [GlobalSetup(Targets = new[] { nameof(AddManyModifiers_Small), nameof(AddManyModifiers_Medium), nameof(AddManyModifiers_Large) })]
        public void Setup_AddManyModifiers()
        {
            _additionModifiers = new List<AdditionNumericModifier>();
            for (int i = 0; i < 1000; i++)
            {
                _additionModifiers.Add(new AdditionNumericModifier(10, new[] { $"Tag{i % 10}" }, $"Modifier{i}", 1));
            }
        }

        [Benchmark]
        [BenchmarkCategory("Scalability")]
        public void AddManyModifiers_Small()
        {
            var numeric = new Numeric(BaseValue);
            for (int i = 0; i < 10; i++)
            {
                numeric.AddModifier(_additionModifiers[i]);
            }
        }

        [Benchmark]
        [BenchmarkCategory("Scalability")]
        public void AddManyModifiers_Medium()
        {
            var numeric = new Numeric(BaseValue);
            for (int i = 0; i < 100; i++)
            {
                numeric.AddModifier(_additionModifiers[i]);
            }
        }

        [Benchmark]
        [BenchmarkCategory("Scalability")]
        public void AddManyModifiers_Large()
        {
            var numeric = new Numeric(BaseValue);
            for (int i = 0; i < 1000; i++)
            {
                numeric.AddModifier(_additionModifiers[i]);
            }
        }

        #endregion

        #region 复杂修饰符组合基准测试

        [Benchmark]
        [BenchmarkCategory("Complex")]
        public void ComplexModifierScenario()
        {
            // 模拟游戏角色属性计算
            var health = new Numeric(100);

            // 基础属性加成
            health.AddModifier(new AdditionNumericModifier(20, new[] { "Base" }, "RaceBonus", 1));
            health.AddModifier(new AdditionNumericModifier(30, new[] { "Base" }, "ClassBonus", 1));

            // 装备加成
            health.AddModifier(new AdditionNumericModifier(50, new[] { "Equipment" }, "Armor", 1, ModifierPriority.Equipment));
            health.AddModifier(new AdditionNumericModifier(20, new[] { "Equipment" }, "Weapon", 1, ModifierPriority.Equipment));

            // Buff 加成
            health.AddModifier(new AdditionNumericModifier(30, new[] { "Buff" }, "Strength", 1, ModifierPriority.Buff));
            health.AddModifier(new AdditionNumericModifier(20, new[] { "Buff" }, "Vitality", 1, ModifierPriority.Buff));

            // 百分比加成
            health.AddModifier(new FractionNumericModifier(50, FractionType.Increase, new[] { "Equipment" }, "EquipmentMultiplier", 1));
            health.AddModifier(new FractionNumericModifier(200, FractionType.Override, new[] { "Buff" }, "BuffMultiplier", 1));

            // 约束修饰符
            health.ClampMax(500, "MaxHealthCap");

            var finalValue = health.FinalValue;
        }

        #endregion

        #region 分数修饰符基准测试

        [Benchmark]
        [BenchmarkCategory("Fraction")]
        public void AddSingleFractionModifier()
        {
            var numeric = new Numeric(BaseValue);
            numeric *= (50, FractionType.Increase);
        }

        [Benchmark]
        [BenchmarkCategory("Fraction")]
        public void AddMultipleFractionModifiers_SameTags()
        {
            var numeric = new Numeric(BaseValue);
            numeric *= (50, FractionType.Increase, new[] { "Buff" }, "Boost1", 1);
            numeric *= (50, FractionType.Increase, new[] { "Buff" }, "Boost2", 1);
            numeric *= (50, FractionType.Increase, new[] { "Buff" }, "Boost3", 1);
        }

        [Benchmark]
        [BenchmarkCategory("Fraction")]
        public void AddMultipleFractionModifiers_DifferentTags()
        {
            var numeric = new Numeric(BaseValue);
            numeric *= (50, FractionType.Increase, new[] { "Equipment" }, "EquipBoost", 1);
            numeric *= (50, FractionType.Increase, new[] { "Buff" }, "BuffBoost", 1);
            numeric *= (50, FractionType.Increase, new[] { "Skill" }, "SkillBoost", 1);
        }

        #endregion

        #region 修饰符查询基准测试

        private Numeric _queryNumeric = null!;

        [GlobalSetup(Targets = new[] { nameof(GetAddModifierValue), nameof(GetAddModifierValueByTag), nameof(HasModifier), nameof(GetAllModifiers) })]
        public void Setup_QueryOperations()
        {
            _queryNumeric = new Numeric(BaseValue);
            _queryNumeric += (50, new[] { "Equipment" }, "Armor", 1);
            _queryNumeric += (30, new[] { "Buff" }, "Strength", 1);
            _queryNumeric += (20, new[] { "Buff" }, "Vitality", 1);
            _queryNumeric *= (50, FractionType.Increase, new[] { "Equipment" }, "EquipBoost", 1);
            _queryNumeric *= (200, FractionType.Override, new[] { "Buff" }, "BuffBoost", 1);
        }

        [Benchmark]
        [BenchmarkCategory("Query")]
        public int GetAddModifierValue()
        {
            return _queryNumeric.GetAddModifierValue();
        }

        [Benchmark]
        [BenchmarkCategory("Query")]
        public int GetAddModifierValueByTag()
        {
            return _queryNumeric.GetAddModifierValueByTag(new[] { "Buff" });
        }

        [Benchmark]
        [BenchmarkCategory("Query")]
        public bool HasModifier()
        {
            return _queryNumeric.HasModifier("Strength");
        }

        [Benchmark]
        [BenchmarkCategory("Query")]
        public IReadOnlyList<INumericModifier> GetAllModifiers()
        {
            return _queryNumeric.GetAllModifiers();
        }

        #endregion

        #region 修饰符移除基准测试

        [Benchmark]
        [BenchmarkCategory("Removal")]
        public void RemoveModifier()
        {
            var numeric = new Numeric(BaseValue);
            var modifier = new AdditionNumericModifier(50, new[] { "Buff" }, "Strength", 1);
            numeric.AddModifier(modifier);
            numeric.RemoveModifier(modifier);
        }

        #endregion
    }

    // 注意：不要在这里添加 Main 方法，避免与测试项目的 Main 方法冲突
    // 运行基准测试使用：
    // dotnet run -c Release --project NumericSystem.Tests -- --filter *NumericBenchmarks*
}
