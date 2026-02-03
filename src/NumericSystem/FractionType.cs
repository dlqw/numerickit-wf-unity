namespace WFramework.CoreGameDevKit.NumericSystem
{
    /// <summary>
    /// 定义分数修饰符的计算类型。
    /// </summary>
    /// <remarks>
    /// <para>
    /// FractionType 决定了分数修饰符如何影响目标值：
    /// </para>
    /// <list type="table">
    /// <listheader>
    /// <term>类型</term>
    /// <description>说明</description>
    /// <description>公式</description>
    /// </listheader>
    /// <item>
    /// <term><see cref="Override"/></term>
    /// <description>覆盖模式：直接将目标值替换为分数计算结果</description>
    /// <description>result = (numerator/denominator) × value</description>
    /// </item>
    /// <item>
    /// <term><see cref="Increase"/></term>
    /// <description>增量模式：在原值基础上额外增加分数计算的结果</description>
    /// <description>result = (1 + numerator/denominator) × value</description>
    /// </item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // 覆盖模式：200% 表示翻倍
    /// numeric *= (200, FractionType.Override);  // 100 → 200
    ///
    /// // 增量模式：50% 表示增加一半
    /// numeric *= (50, FractionType.Increase);    // 100 → 150
    /// </code>
    /// </example>
    public enum FractionType
    {
        /// <summary>
        /// 覆盖模式：将目标值替换为分数计算的结果。
        /// </summary>
        /// <example>
        /// 如果分子为 200，分母为 100（即 200%），则将目标值乘以 2。
        /// </example>
        Override,

        /// <summary>
        /// 增量模式：在原值基础上增加分数计算的结果。
        /// </summary>
        /// <example>
        /// 如果分子为 50，分母为 100（即 50%），则将目标值乘以 1.5（原值 + 50%）。
        /// </example>
        Increase,
    }
}
