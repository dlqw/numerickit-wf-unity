using System;

namespace WFramework.CoreGameDevKit.NumericSystem
{
    /// <summary>
    /// 条件修饰符，只在指定条件满足时才应用包装的修饰符
    /// </summary>
    /// <remarks>
    /// <para>
    /// ConditionalNumericModifier 是一个装饰器模式的实现，它包装另一个修饰符，
    /// 并根据条件的评估结果决定是否应用该修饰符。
    /// </para>
    /// <para>
    /// <strong>工作原理：</strong>
    /// </para>
    /// <list type="number">
    /// <item><description>每次计算时，先评估条件是否满足</description></item>
    /// <item><description>如果条件满足，应用包装的修饰符</description></item>
    /// <item><description>如果条件不满足，返回原始值（不应用修饰符）</description></item>
    /// </list>
    /// <para>
    /// <strong>注意事项：</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>条件修饰符作为 CustomModifier 的一种特殊形式，在约束阶段之前应用</description></item>
    /// <item><description>条件是在每次 Update() 时动态评估的，因此值的变化会立即反映</description></item>
    /// <item><description>避免循环依赖：条件不应该依赖于它所修饰的值</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // 示例 1: 生命值低于 30% 时获得 +50 防御
    /// var health = new Numeric(100);
    /// var defense = new Numeric(50);
    ///
    /// var lowHpCondition = ConditionBuilder.Where(h => h.FinalValue < 30);
    /// var emergencyShield = new ConditionalNumericModifier(
    ///     lowHpCondition,
    ///     new AdditionNumericModifier(50, new[] { "Defense" }, "EmergencyShield", 1),
    ///     "LowHP_Shield"
    /// );
    /// defense.AddModifier(emergencyShield);
    ///
    /// // 示例 2: 组合条件 - 生命值低且是夜间时
    /// var complexCondition = ConditionBuilder
    ///     .Where(h => h.FinalValue < 50)
    ///     .And(h => IsNightTime);  // 假设有 IsNightTime() 函数
    /// </code>
    /// </example>
    public sealed class ConditionalNumericModifier : INumericModifier
    {
        private readonly ICondition condition;
        private readonly INumericModifier wrappedModifier;

        ModifierType INumericModifier.Type => ModifierType.Custom;

        /// <summary>
        /// 获取修饰符信息
        /// </summary>
        public NumericModifierInfo Info { get; }

        /// <summary>
        /// 获取条件
        /// </summary>
        public ICondition Condition => condition;

        /// <summary>
        /// 获取包装的修饰符
        /// </summary>
        public INumericModifier WrappedModifier => wrappedModifier;

        /// <summary>
        /// 初始化 ConditionalNumericModifier 的新实例
        /// </summary>
        /// <param name="condition">评估条件</param>
        /// <param name="wrappedModifier">要包装的修饰符</param>
        /// <param name="name">修饰符名称</param>
        /// <param name="count">计数（用于叠加）</param>
        /// <param name="priority">优先级</param>
        /// <exception cref="ArgumentNullException">当 condition 或 wrappedModifier 为 null 时抛出</exception>
        public ConditionalNumericModifier(
            ICondition condition,
            INumericModifier wrappedModifier,
            string name = "Conditional",
            int count = 1,
            ModifierPriority priority = ModifierPriority.Default)
        {
            this.condition = condition ?? throw new ArgumentNullException(nameof(condition));
            this.wrappedModifier = wrappedModifier ?? throw new ArgumentNullException(nameof(wrappedModifier));
            Info = new NumericModifierInfo(Array.Empty<string>(), name, count, priority);
        }

        /// <summary>
        /// 应用修饰符（评估条件后决定是否应用包装的修饰符）
        /// </summary>
        /// <param name="source">源值</param>
        /// <returns>应用修饰符后的函数</returns>
        public Func<Numeric, int> Apply(int source)
        {
            return numeric =>
            {
                // 评估条件
                bool conditionMet = condition.Evaluate(numeric);

                if (!conditionMet)
                {
                    // 条件不满足，返回原始值
                    return source;
                }

                // 条件满足，应用包装的修饰符
                // 注意：条件修饰符本身不修改 source，由包装的修饰符决定如何修改
                var applyFunc = wrappedModifier.Apply(source);
                return applyFunc(numeric);
            };
        }

        /// <summary>
        /// 创建条件加法修饰符的便捷方法
        /// </summary>
        public static ConditionalNumericModifier ConditionalAdd(
            ICondition condition,
            int value,
            string name = "ConditionalAdd",
            int count = 1,
            ModifierPriority priority = ModifierPriority.Default)
        {
            var addition = new AdditionNumericModifier(value, Array.Empty<string>(), name + "_Inner", count, priority);
            return new ConditionalNumericModifier(condition, addition, name, count, priority);
        }

        /// <summary>
        /// 创建条件分数修饰符的便捷方法
        /// </summary>
        public static ConditionalNumericModifier ConditionalFraction(
            ICondition condition,
            int percent,
            FractionType type,
            string name = "ConditionalFraction",
            int count = 1,
            ModifierPriority priority = ModifierPriority.Multiplier)
        {
            var fraction = new FractionNumericModifier(percent, type, Array.Empty<string>(), name + "_Inner", count, priority);
            return new ConditionalNumericModifier(condition, fraction, name, count, priority);
        }
    }
}
