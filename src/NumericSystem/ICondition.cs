using System;

namespace WFramework.CoreGameDevKit.NumericSystem
{
    /// <summary>
    /// 条件接口，用于定义修饰符的生效条件
    /// </summary>
    /// <remarks>
    /// <para>
    /// 条件修饰符允许修饰符只在特定条件下生效，提供了强大的灵活性。
    /// </para>
    /// <para>
    /// <strong>应用场景：</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>生命值低于 30% 时触发额外防御</description></item>
    /// <item><description>只有当装备了特定武器时才生效</description></item>
    /// <item><description>在夜间/白天有不同的属性加成</description></item>
    /// <item><description>Buff/Debuff 在特定条件下叠加或抵消</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // 创建条件：生命值低于 30%
    /// var lowHealthCondition = new PredicateCondition(numeric => numeric.FinalValue < 30);
    ///
    /// // 创建条件修饰符：低生命值时 +50 防御
    /// var modifier = new ConditionalNumericModifier(
    ///     lowHealthCondition,
    ///     new AdditionNumericModifier(50, new[] { "Defense" }, "EmergencyShield", 1)
    /// );
    /// </code>
    /// </example>
    public interface ICondition
    {
        /// <summary>
        /// 评估条件是否满足
        /// </summary>
        /// <param name="numeric">要评估的 Numeric 对象</param>
        /// <returns>如果条件满足返回 true，否则返回 false</returns>
        bool Evaluate(Numeric numeric);
    }

    /// <summary>
    /// 基于谓词的条件实现
    /// </summary>
    /// <remarks>
    /// 提供最灵活的条件定义方式，可以使用任何谓词函数。
    /// </remarks>
    public sealed class PredicateCondition : ICondition
    {
        private readonly Func<Numeric, bool> predicate;

        /// <summary>
        /// 初始化 PredicateCondition 的新实例
        /// </summary>
        /// <param name="predicate">用于评估条件的谓词函数</param>
        /// <exception cref="ArgumentNullException">当 predicate 为 null 时抛出</exception>
        public PredicateCondition(Func<Numeric, bool> predicate)
        {
            this.predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
        }

        /// <summary>
        /// 评估条件是否满足
        /// </summary>
        public bool Evaluate(Numeric numeric) => predicate(numeric);
    }

    /// <summary>
    /// 组合条件类型
    /// </summary>
    public enum ConditionOperator
    {
        /// <summary>
        /// 逻辑与：所有条件都必须满足
        /// </summary>
        And,

        /// <summary>
        /// 逻辑或：至少有一个条件满足
        /// </summary>
        Or,

        /// <summary>
        /// 逻辑非：条件必须不满足
        /// </summary>
        Not
    }

    /// <summary>
    /// 组合条件实现
    /// </summary>
    /// <remarks>
    /// 允许通过逻辑运算符组合多个条件，创建复杂的条件表达式。
    /// </remarks>
    public sealed class CompositeCondition : ICondition
    {
        private readonly ICondition[] conditions;
        private readonly ConditionOperator op;

        /// <summary>
        /// 初始化 CompositeCondition 的新实例
        /// </summary>
        /// <param name="op">逻辑运算符</param>
        /// <param name="conditions">要组合的条件数组</param>
        /// <exception cref="ArgumentNullException">当 conditions 为 null 时抛出</exception>
        /// <exception cref="ArgumentException">当 conditions 为空或 Not 运算符的条件数量不为 1 时抛出</exception>
        public CompositeCondition(ConditionOperator op, params ICondition[] conditions)
        {
            if (conditions == null)
                throw new ArgumentNullException(nameof(conditions));
            if (conditions.Length == 0)
                throw new ArgumentException("至少需要一个条件。", nameof(conditions));
            if (op == ConditionOperator.Not && conditions.Length != 1)
                throw new ArgumentException("Not 运算符只能有一个条件。", nameof(conditions));

            this.op = op;
            this.conditions = conditions;
        }

        /// <summary>
        /// 评估组合条件是否满足
        /// </summary>
        public bool Evaluate(Numeric numeric)
        {
            return op switch
            {
                ConditionOperator.And => Array.TrueForAll(conditions, c => c.Evaluate(numeric)),
                ConditionOperator.Or => Array.Exists(conditions, c => c.Evaluate(numeric)),
                ConditionOperator.Not => !conditions[0].Evaluate(numeric),
                _ => throw new ArgumentOutOfRangeException($"未知的条件运算符: {op}")
            };
        }
    }

    /// <summary>
    /// 条件构建器，提供流畅的条件构建 API
    /// </summary>
    public sealed class ConditionBuilder
    {
        private readonly ICondition condition;

        internal ConditionBuilder(ICondition condition)
        {
            this.condition = condition ?? throw new ArgumentNullException(nameof(condition));
        }

        /// <summary>
        /// 创建基于谓词的条件
        /// </summary>
        public static ConditionBuilder Where(Func<Numeric, bool> predicate)
        {
            return new ConditionBuilder(new PredicateCondition(predicate));
        }

        /// <summary>
        /// 逻辑与：当前条件和另一个条件都必须满足
        /// </summary>
        public ConditionBuilder And(ICondition other)
        {
            return new ConditionBuilder(new CompositeCondition(ConditionOperator.And, condition, other));
        }

        /// <summary>
        /// 逻辑与：当前条件和另一个谓词都必须满足
        /// </summary>
        public ConditionBuilder And(Func<Numeric, bool> other)
        {
            return And(new PredicateCondition(other));
        }

        /// <summary>
        /// 逻辑或：当前条件或另一个条件至少有一个满足
        /// </summary>
        public ConditionBuilder Or(ICondition other)
        {
            return new ConditionBuilder(new CompositeCondition(ConditionOperator.Or, condition, other));
        }

        /// <summary>
        /// 逻辑或：当前条件或另一个谓词至少有一个满足
        /// </summary>
        public ConditionBuilder Or(Func<Numeric, bool> other)
        {
            return Or(new PredicateCondition(other));
        }

        /// <summary>
        /// 逻辑非：当前条件必须不满足
        /// </summary>
        public ConditionBuilder Not()
        {
            return new ConditionBuilder(new CompositeCondition(ConditionOperator.Not, condition));
        }

        /// <summary>
        /// 构建并返回条件
        /// </summary>
        public ICondition Build()
        {
            return condition;
        }
    }
}
