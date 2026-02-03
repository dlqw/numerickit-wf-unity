using System;
using System.Collections.Generic;
using System.Linq;

namespace WFramework.CoreGameDevKit.NumericSystem
{
    /// <summary>
    /// Numeric 系统的扩展方法，提供流畅 API 和便捷操作
    /// </summary>
    public static class NumericExtensions
    {
        #region 链式构建方法

        /// <summary>
        /// 同时添加多个加法修饰符
        /// </summary>
        /// <param name="numeric">目标 Numeric 对象</param>
        /// <param name="modifiers">要添加的修饰符集合</param>
        /// <returns>添加修饰符后的 Numeric 对象（支持链式调用）</returns>
        /// <example>
        /// <code>
        /// var health = new Numeric(100);
        /// health.AddModifiers(
        ///     new AdditionNumericModifier(10, new[] { "Equipment" }, "Armor", 1),
        ///     new AdditionNumericModifier(20, new[] { "Buff" }, "Strength", 1)
        /// );
        /// </code>
        /// </example>
        public static Numeric AddModifiers(this Numeric numeric, params AdditionNumericModifier[] modifiers)
        {
            if (numeric == null)
                throw new ArgumentNullException(nameof(numeric));
            if (modifiers == null)
                throw new ArgumentNullException(nameof(modifiers));

            foreach (var modifier in modifiers)
            {
                numeric.AddModifier(modifier);
            }

            return numeric;
        }

        /// <summary>
        /// 同时添加多个分数修饰符
        /// </summary>
        /// <param name="numeric">目标 Numeric 对象</param>
        /// <param name="modifiers">要添加的修饰符集合</param>
        /// <returns>添加修饰符后的 Numeric 对象（支持链式调用）</returns>
        public static Numeric AddModifiers(this Numeric numeric, params FractionNumericModifier[] modifiers)
        {
            if (numeric == null)
                throw new ArgumentNullException(nameof(numeric));
            if (modifiers == null)
                throw new ArgumentNullException(nameof(modifiers));

            foreach (var modifier in modifiers)
            {
                numeric.AddModifier(modifier);
            }

            return numeric;
        }

        /// <summary>
        /// 批量设置基础值和修饰符（构建器模式）
        /// </summary>
        /// <param name="baseValue">基础值</param>
        /// <param name="action">配置修饰符的 action</param>
        /// <returns>配置完成的 Numeric 对象</returns>
        /// <example>
        /// <code>
        /// var health = Numeric.Build(100, builder =>
        /// {
        ///     builder.AddEquipment(20);
        ///     builder.AddBuff(30, "Strength");
        ///     builder.BoostByPercentage(150, "Equipment");
        /// });
        /// </code>
        /// </example>
        public static Numeric Build(int baseValue, Action<NumericBuilder> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            var numeric = new Numeric(baseValue);
            var builder = new NumericBuilder(numeric);
            action(builder);
            return numeric;
        }

        #endregion

        #region 条件修饰符

        /// <summary>
        /// 条件性添加修饰符，仅当条件满足时添加
        /// </summary>
        /// <param name="numeric">目标 Numeric 对象</param>
        /// <param name="condition">条件谓词</param>
        /// <param name="modifierFactory">修饰符工厂函数</param>
        /// <returns>Numeric 对象（支持链式调用）</returns>
        /// <example>
        /// <code>
        /// health.AddIf(
        ///     h => h.FinalValue < 100,
        ///     () => new AdditionNumericModifier(10, new[] { "Buff" }, "Heal", 1)
        /// );
        /// </code>
        /// </example>
        public static Numeric AddIf(
            this Numeric numeric,
            Func<Numeric, bool> condition,
            Func<INumericModifier> modifierFactory)
        {
            if (numeric == null)
                throw new ArgumentNullException(nameof(numeric));
            if (condition == null)
                throw new ArgumentNullException(nameof(condition));
            if (modifierFactory == null)
                throw new ArgumentNullException(nameof(modifierFactory));

            if (condition(numeric))
            {
                var modifier = modifierFactory();
                if (modifier is AdditionNumericModifier addMod)
                    numeric.AddModifier(addMod);
                else if (modifier is FractionNumericModifier fracMod)
                    numeric.AddModifier(fracMod);
                else if (modifier is CustomNumericModifier customMod)
                    numeric.AddModifier(customMod);
            }

            return numeric;
        }

        /// <summary>
        /// 条件性设置值，仅当条件满足时执行操作
        /// </summary>
        /// <param name="numeric">目标 Numeric 对象</param>
        /// <param name="condition">条件谓词</param>
        /// <param name="action">要执行的操作</param>
        /// <returns>Numeric 对象（支持链式调用）</returns>
        public static Numeric DoIf(
            this Numeric numeric,
            Func<Numeric, bool> condition,
            Action<Numeric> action)
        {
            if (numeric == null)
                throw new ArgumentNullException(nameof(numeric));
            if (condition == null)
                throw new ArgumentNullException(nameof(condition));
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            if (condition(numeric))
            {
                action(numeric);
            }

            return numeric;
        }

        #endregion

        #region 便捷加法修饰符

        /// <summary>
        /// 添加装备加成（常用标签的快捷方法）
        /// </summary>
        public static Numeric AddEquipment(this Numeric numeric, int value, string name = "Equipment")
        {
            if (numeric == null)
                throw new ArgumentNullException(nameof(numeric));
            return numeric + (value, new[] { "Equipment" }, name, 1);
        }

        /// <summary>
        /// 添加 Buff 加成
        /// </summary>
        public static Numeric AddBuff(this Numeric numeric, int value, string name = "Buff")
        {
            if (numeric == null)
                throw new ArgumentNullException(nameof(numeric));
            return numeric + (value, new[] { "Buff" }, name, 1);
        }

        /// <summary>
        /// 添加 Debuff（负面效果）
        /// </summary>
        public static Numeric AddDebuff(this Numeric numeric, int value, string name = "Debuff")
        {
            if (numeric == null)
                throw new ArgumentNullException(nameof(numeric));
            return numeric + (-value, new[] { "Debuff" }, name, 1);
        }

        #endregion

        #region 便捷分数修饰符

        /// <summary>
        /// 按百分比增加（Increase 类型）
        /// </summary>
        /// <param name="numeric">目标 Numeric 对象</param>
        /// <param name="percentage">百分比（例如 150 表示 150%）</param>
        /// <param name="tags">标签数组</param>
        /// <param name="name">修饰符名称</param>
        /// <returns>Numeric 对象（支持链式调用）</returns>
        public static Numeric BoostByPercentage(
            this Numeric numeric,
            int percentage,
            string[] tags,
            string name)
        {
            if (numeric == null)
                throw new ArgumentNullException(nameof(numeric));
            return numeric * (percentage, FractionType.Increase, tags, name, 1);
        }

        /// <summary>
        /// 按百分比增加（应用于基础值）
        /// </summary>
        public static Numeric BoostBaseByPercentage(
            this Numeric numeric,
            int percentage,
            string name = "BaseBoost")
        {
            if (numeric == null)
                throw new ArgumentNullException(nameof(numeric));
            return numeric * (percentage, FractionType.Increase, new[] { NumericModifierConfig.TagSelf }, name, 1);
        }

        /// <summary>
        /// 按百分比覆盖（Override 类型）
        /// </summary>
        public static Numeric MultiplyBy(
            this Numeric numeric,
            int percentage,
            string[] tags,
            string name)
        {
            if (numeric == null)
                throw new ArgumentNullException(nameof(numeric));
            return numeric * (percentage, FractionType.Override, tags, name, 1);
        }

        #endregion

        #region 约束修饰符

        /// <summary>
        /// 设置最小值限制
        /// </summary>
        /// <param name="numeric">目标 Numeric 对象</param>
        /// <param name="minValue">最小值</param>
        /// <param name="name">修饰符名称</param>
        /// <param name="priority">修饰符优先级（默认为 Clamp）</param>
        /// <returns>Numeric 对象（支持链式调用）</returns>
        public static Numeric ClampMin(
            this Numeric numeric,
            int minValue,
            string name = "MinClamp",
            ModifierPriority priority = ModifierPriority.Clamp)
        {
            if (numeric == null)
                throw new ArgumentNullException(nameof(numeric));
            var modifier = new CustomNumericModifier(
                x => Math.Max(x, minValue),
                Array.Empty<string>(),
                name,
                1,
                priority);
            return numeric + modifier;
        }

        /// <summary>
        /// 设置最大值限制
        /// </summary>
        /// <param name="numeric">目标 Numeric 对象</param>
        /// <param name="maxValue">最大值</param>
        /// <param name="name">修饰符名称</param>
        /// <param name="priority">修饰符优先级（默认为 Clamp）</param>
        /// <returns>Numeric 对象（支持链式调用）</returns>
        public static Numeric ClampMax(
            this Numeric numeric,
            int maxValue,
            string name = "MaxClamp",
            ModifierPriority priority = ModifierPriority.Clamp)
        {
            if (numeric == null)
                throw new ArgumentNullException(nameof(numeric));
            var modifier = new CustomNumericModifier(
                x => Math.Min(x, maxValue),
                Array.Empty<string>(),
                name,
                1,
                priority);
            return numeric + modifier;
        }

        /// <summary>
        /// 设置范围限制
        /// </summary>
        /// <param name="numeric">目标 Numeric 对象</param>
        /// <param name="minValue">最小值</param>
        /// <param name="maxValue">最大值</param>
        /// <param name="name">修饰符名称</param>
        /// <param name="priority">修饰符优先级（默认为 Clamp）</param>
        /// <returns>Numeric 对象（支持链式调用）</returns>
        public static Numeric ClampRange(
            this Numeric numeric,
            int minValue,
            int maxValue,
            string name = "RangeClamp",
            ModifierPriority priority = ModifierPriority.Clamp)
        {
            if (numeric == null)
                throw new ArgumentNullException(nameof(numeric));
            var modifier = new CustomNumericModifier(
                x => Math.Clamp(x, minValue, maxValue),
                Array.Empty<string>(),
                name,
                1,
                priority);
            return numeric + modifier;
        }

        #endregion

        #region 查询扩展方法

        /// <summary>
        /// 获取指定标签的修饰符总和
        /// </summary>
        /// <param name="numeric">目标 Numeric 对象</param>
        /// <param name="tags">要查询的标签</param>
        /// <returns>匹配标签的修饰符总值</returns>
        public static int GetTaggedModifierValue(this Numeric numeric, params string[] tags)
        {
            if (numeric == null)
                throw new ArgumentNullException(nameof(numeric));
            return numeric.GetAddModifierValueByTag(tags) / (int)FixedPoint.Factor;
        }

        /// <summary>
        /// 检查是否有指定名称的修饰符
        /// </summary>
        /// <param name="numeric">目标 Numeric 对象</param>
        /// <param name="name">修饰符名称</param>
        /// <returns>是否存在该修饰符</returns>
        public static bool HasModifier(this Numeric numeric, string name)
        {
            if (numeric == null)
                throw new ArgumentNullException(nameof(numeric));
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("名称不能为空", nameof(name));

            return numeric.GetAllModifiers().Any(m => m.Info.Name == name);
        }

        /// <summary>
        /// 获取指定名称修饰符的数量
        /// </summary>
        /// <param name="numeric">目标 Numeric 对象</param>
        /// <param name="name">修饰符名称</param>
        /// <returns>修饰符数量（通过 Count 累加）</returns>
        public static int GetModifierCount(this Numeric numeric, string name)
        {
            if (numeric == null)
                throw new ArgumentNullException(nameof(numeric));
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("名称不能为空", nameof(name));

            return numeric.GetAllModifiers()
                .Where(m => m.Info.Name == name)
                .Sum(m => m.Info.Count);
        }

        /// <summary>
        /// 获取所有指定标签的修饰符
        /// </summary>
        /// <param name="numeric">目标 Numeric 对象</param>
        /// <param name="tags">要查询的标签</param>
        /// <returns>匹配标签的修饰符列表</returns>
        public static IReadOnlyList<INumericModifier> GetModifiersByTags(
            this Numeric numeric,
            params string[] tags)
        {
            if (numeric == null)
                throw new ArgumentNullException(nameof(numeric));
            if (tags == null || tags.Length == 0)
                throw new ArgumentException("标签不能为空", nameof(tags));

            return numeric.GetAllModifiers()
                .Where(m => m.Info.Tags != null && m.Info.Tags.Any(t => tags.Contains(t)))
                .ToList()
                .AsReadOnly();
        }

        #endregion

        #region 格式化和输出

        /// <summary>
        /// 格式化输出为可读字符串
        /// </summary>
        /// <param name="numeric">目标 Numeric 对象</param>
        /// <param name="showDetails">是否显示详细信息</param>
        /// <returns>格式化的字符串</returns>
        public static string ToFormattedString(this Numeric numeric, bool showDetails = false)
        {
            if (numeric == null)
                throw new ArgumentNullException(nameof(numeric));

            if (!showDetails)
            {
                return numeric.FinalValue.ToString();
            }

            var sb = new System.Text.StringBuilder();
            sb.Append($"Value: {numeric.FinalValue}");
            sb.Append($" | Origin: {numeric.GetOriginValue() / (float)FixedPoint.Factor:F2}");

            var addValue = numeric.GetAddModifierValue();
            if (addValue > 0)
            {
                sb.Append($" | Additions: +{addValue / (float)FixedPoint.Factor:F2}");
            }

            var modifierCount = numeric.GetAllModifiers().Count;
            if (modifierCount > 0)
            {
                sb.Append($" | Modifiers: {modifierCount}");
            }

            return sb.ToString();
        }

        /// <summary>
        /// 输出到控制台（用于调试）
        /// </summary>
        /// <param name="numeric">目标 Numeric 对象</param>
        /// <param name="label">可选标签</param>
        public static void Dump(this Numeric numeric, string label = "Numeric")
        {
            if (numeric == null)
                throw new ArgumentNullException(nameof(numeric));

            Console.WriteLine($"{label}: {numeric.ToFormattedString(showDetails: true)}");
        }

        #endregion

        #region 条件修饰符扩展

        /// <summary>
        /// 添加条件加法修饰符
        /// </summary>
        /// <param name="numeric">目标 Numeric 对象</param>
        /// <param name="condition">生效条件</param>
        /// <param name="value">加法值</param>
        /// <param name="name">修饰符名称</param>
        /// <param name="priority">优先级</param>
        /// <returns>Numeric 对象（支持链式调用）</returns>
        public static Numeric AddIf(
            this Numeric numeric,
            Func<Numeric, bool> condition,
            int value,
            string name = "ConditionalAdd",
            ModifierPriority priority = ModifierPriority.Default)
        {
            if (numeric == null)
                throw new ArgumentNullException(nameof(numeric));
            if (condition == null)
                throw new ArgumentNullException(nameof(condition));

            var modifier = ConditionalNumericModifier.ConditionalAdd(
                new PredicateCondition(condition),
                value,
                name,
                1,
                priority);
            numeric.AddModifier(modifier);
            return numeric;
        }

        /// <summary>
        /// 添加条件加法修饰符
        /// </summary>
        public static Numeric AddIf(
            this Numeric numeric,
            ICondition condition,
            int value,
            string name = "ConditionalAdd",
            ModifierPriority priority = ModifierPriority.Default)
        {
            if (numeric == null)
                throw new ArgumentNullException(nameof(numeric));
            if (condition == null)
                throw new ArgumentNullException(nameof(condition));

            var modifier = ConditionalNumericModifier.ConditionalAdd(condition, value, name, 1, priority);
            numeric.AddModifier(modifier);
            return numeric;
        }

        /// <summary>
        /// 添加条件加法修饰符
        /// </summary>
        public static Numeric AddIf(
            this Numeric numeric,
            ConditionBuilder conditionBuilder,
            int value,
            string name = "ConditionalAdd",
            ModifierPriority priority = ModifierPriority.Default)
        {
            if (numeric == null)
                throw new ArgumentNullException(nameof(numeric));
            if (conditionBuilder == null)
                throw new ArgumentNullException(nameof(conditionBuilder));

            return numeric.AddIf(conditionBuilder.Build(), value, name, priority);
        }

        /// <summary>
        /// 添加条件分数修饰符
        /// </summary>
        /// <param name="numeric">目标 Numeric 对象</param>
        /// <param name="condition">生效条件</param>
        /// <param name="percent">百分比（例如 150 表示 150%）</param>
        /// <param name="type">分数类型</param>
        /// <param name="name">修饰符名称</param>
        /// <param name="priority">优先级</param>
        /// <returns>Numeric 对象（支持链式调用）</returns>
        public static Numeric MultiplyIf(
            this Numeric numeric,
            ICondition condition,
            int percent,
            FractionType type,
            string name = "ConditionalFraction",
            ModifierPriority priority = ModifierPriority.Multiplier)
        {
            if (numeric == null)
                throw new ArgumentNullException(nameof(numeric));
            if (condition == null)
                throw new ArgumentNullException(nameof(condition));

            var modifier = ConditionalNumericModifier.ConditionalFraction(condition, percent, type, name, 1, priority);
            numeric.AddModifier(modifier);
            return numeric;
        }

        /// <summary>
        /// 添加条件分数修饰符
        /// </summary>
        public static Numeric MultiplyIf(
            this Numeric numeric,
            ConditionBuilder conditionBuilder,
            int percent,
            FractionType type,
            string name = "ConditionalFraction",
            ModifierPriority priority = ModifierPriority.Multiplier)
        {
            if (numeric == null)
                throw new ArgumentNullException(nameof(numeric));
            if (conditionBuilder == null)
                throw new ArgumentNullException(nameof(conditionBuilder));

            return numeric.MultiplyIf(conditionBuilder.Build(), percent, type, name, priority);
        }

        /// <summary>
        /// 添加条件修饰符（通用方法）
        /// </summary>
        /// <param name="numeric">目标 Numeric 对象</param>
        /// <param name="condition">生效条件</param>
        /// <param name="modifier">要条件应用的修饰符</param>
        /// <param name="name">修饰符名称</param>
        /// <param name="priority">优先级</param>
        /// <returns>Numeric 对象（支持链式调用）</returns>
        public static Numeric AddConditionalModifier(
            this Numeric numeric,
            ICondition condition,
            INumericModifier modifier,
            string name = "Conditional",
            ModifierPriority priority = ModifierPriority.Default)
        {
            if (numeric == null)
                throw new ArgumentNullException(nameof(numeric));
            if (condition == null)
                throw new ArgumentNullException(nameof(condition));
            if (modifier == null)
                throw new ArgumentNullException(nameof(modifier));

            var conditionalModifier = new ConditionalNumericModifier(condition, modifier, name, 1, priority);
            numeric.AddModifier(conditionalModifier);
            return numeric;
        }

        /// <summary>
        /// 添加条件修饰符（通用方法）- ConditionBuilder 重载
        /// </summary>
        public static Numeric AddConditionalModifier(
            this Numeric numeric,
            ConditionBuilder conditionBuilder,
            INumericModifier modifier,
            string name = "Conditional",
            ModifierPriority priority = ModifierPriority.Default)
        {
            if (numeric == null)
                throw new ArgumentNullException(nameof(numeric));
            if (conditionBuilder == null)
                throw new ArgumentNullException(nameof(conditionBuilder));
            if (modifier == null)
                throw new ArgumentNullException(nameof(modifier));

            return numeric.AddConditionalModifier(conditionBuilder.Build(), modifier, name, priority);
        }

        #endregion
    }

    /// <summary>
    /// Numeric 构建器，用于流畅配置 Numeric 对象
    /// </summary>
    public sealed class NumericBuilder
    {
        private readonly Numeric _numeric;

        internal NumericBuilder(Numeric numeric)
        {
            _numeric = numeric ?? throw new ArgumentNullException(nameof(numeric));
        }

        /// <summary>
        /// 添加装备加成
        /// </summary>
        public NumericBuilder AddEquipment(int value, string name = "Equipment")
        {
            _numeric.AddModifier(new AdditionNumericModifier(value, new[] { "Equipment" }, name, 1, ModifierPriority.Equipment));
            return this;
        }

        /// <summary>
        /// 添加 Buff
        /// </summary>
        public NumericBuilder AddBuff(int value, string name = "Buff")
        {
            _numeric.AddModifier(new AdditionNumericModifier(value, new[] { "Buff" }, name, 1, ModifierPriority.Buff));
            return this;
        }

        /// <summary>
        /// 添加 Debuff
        /// </summary>
        public NumericBuilder AddDebuff(int value, string name = "Debuff")
        {
            _numeric.AddModifier(new AdditionNumericModifier(-value, new[] { "Debuff" }, name, 1, ModifierPriority.Buff));
            return this;
        }

        /// <summary>
        /// 按百分比提升
        /// </summary>
        public NumericBuilder BoostByPercentage(int percentage, string[] tags, string name)
        {
            _numeric.AddModifier(new FractionNumericModifier(percentage, FractionType.Increase, tags, name, 1, ModifierPriority.Multiplier));
            return this;
        }

        /// <summary>
        /// 提升基础值
        /// </summary>
        public NumericBuilder BoostBase(int percentage, string name = "BaseBoost")
        {
            _numeric.AddModifier(new FractionNumericModifier(percentage, FractionType.Increase, new[] { NumericModifierConfig.TagSelf }, name, 1, ModifierPriority.Multiplier));
            return this;
        }

        /// <summary>
        /// 设置最小值限制
        /// </summary>
        public NumericBuilder WithMinLimit(int minValue, string name = "MinClamp")
        {
            _numeric.ClampMin(minValue, name);
            return this;
        }

        /// <summary>
        /// 设置最大值限制
        /// </summary>
        public NumericBuilder WithMaxLimit(int maxValue, string name = "MaxClamp")
        {
            _numeric.ClampMax(maxValue, name);
            return this;
        }

        /// <summary>
        /// 添加自定义约束
        /// </summary>
        public NumericBuilder WithConstraint(Func<int, int> func, string name = "Constraint")
        {
            _numeric.AddModifier(new CustomNumericModifier(func, Array.Empty<string>(), name, 1, ModifierPriority.Clamp));
            return this;
        }

        /// <summary>
        /// 构建并返回 Numeric 对象
        /// </summary>
        public Numeric Build()
        {
            return _numeric;
        }
    }
}
