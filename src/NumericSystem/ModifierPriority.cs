using System;

namespace WFramework.CoreGameDevKit.NumericSystem
{
    /// <summary>
    /// 定义修饰符的优先级等级，用于控制修饰符的应用顺序。
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>优先级说明：</strong>
    /// </para>
    /// <para>
    /// 数值越小，优先级越高。修饰符按优先级从小到大依次应用。
    /// </para>
    /// <para>
    /// <strong>应用顺序：</strong>
    /// </para>
    /// <list type="number">
    /// <item><description><see cref="Critical"/> (0): 关键修饰符，首先应用</description></item>
    /// <item><description><see cref="Base"/> (100): 基础属性修饰符</description></item>
    /// <item><description><see cref="Equipment"/> (200): 装备相关修饰符</description></item>
    /// <item><description><see cref="Buff"/> (300): 增益/减益效果</description></item>
    /// <item><description><see cref="Skill"/> (400): 技能效果修饰符</description></item>
    /// <item><description><see cref="Default"/> (400): 默认优先级</description></item>
    /// <item><description><see cref="Multiplier"/> (500): 乘法修饰符（百分比）</description></item>
    /// <item><description><see cref="Clamp"/> (600): 约束修饰符（限制范围）</description></item>
    /// </list>
    /// <para>
    /// <strong>示例：</strong>
    /// </para>
    /// <code>
    /// // 优先级高的先应用
    /// var health = new Numeric(100);
    /// health += new AdditionNumericModifier(50, new[] { "Equipment" }, "Armor", 1, ModifierPriority.Equipment);
    /// health += new AdditionNumericModifier(20, new[] { "Buff" }, "Strength", 1, ModifierPriority.Buff);
    /// // Armor (priority 200) 先于 Strength (priority 300) 应用
    /// </code>
    /// </remarks>
    [Serializable]
    public enum ModifierPriority : int
    {
        /// <summary>
        /// 关键优先级（0），应该最先应用的修饰符。
        /// </summary>
        /// <example>
        /// 用于系统核心的、必须首先计算的修饰符。
        /// </example>
        Critical = 0,

        /// <summary>
        /// 基础优先级（100），用于基础属性修饰符。
        /// </summary>
        /// <example>
        /// 角色的基础属性加成（如种族、职业加成）。
        /// </example>
        Base = 100,

        /// <summary>
        /// 装备优先级（200），用于装备相关的修饰符。
        /// </summary>
        /// <example>
        /// 武器、护甲、饰品等装备提供的属性加成。
        /// </example>
        Equipment = 200,

        /// <summary>
        /// 增益优先级（300），用于临时增益/减益效果。
        /// </summary>
        /// <example>
        /// Buff、Debuff、药剂效果等临时状态。
        /// </example>
        Buff = 300,

        /// <summary>
        /// 技能优先级（400），用于技能相关的修饰符。
        /// </summary>
        /// <example>
        /// 被动技能、天赋树加成等。
        /// </example>
        Skill = 400,

        /// <summary>
        /// 乘法优先级（500），用于百分比乘法修饰符。
        /// </summary>
        /// <example>
        /// FractionNumericModifier（百分比增加/减少）。
        /// </example>
        Multiplier = 500,

        /// <summary>
        /// 约束优先级（600），用于约束修饰符（如范围限制）。
        /// </summary>
        /// <example>
        /// CustomNumericModifier（clamp、min/max 限制等）。
        /// </example>
        Clamp = 600,

        /// <summary>
        /// 默认优先级（400），未指定优先级时使用。
        /// </summary>
        /// <remarks>
        /// 默认优先级低于 Multiplier，确保加法修饰符在分数修饰符之前应用。
        /// 这是最常见的应用顺序：先应用加法修饰符，再应用百分比修饰符。
        /// </remarks>
        Default = 400
    }
}
