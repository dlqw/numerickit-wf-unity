namespace WFramework.CoreGameDevKit.NumericSystem
{
    /// <summary>
    /// 定义修饰符的类型标识。
    /// </summary>
    /// <remarks>
    /// 此枚举用于内部区分不同类型的修饰符，以便系统正确处理和应用修饰符。
    /// </remarks>
    internal enum ModifierType
    {
        /// <summary>
        /// 无类型（未使用）。
        /// </summary>
        None,

        /// <summary>
        /// 加法修饰符类型。
        /// </summary>
        Add,

        /// <summary>
        /// 分数修饰符类型。
        /// </summary>
        Frac,

        /// <summary>
        /// 自定义修饰符类型。
        /// </summary>
        Custom
    }

    /// <summary>
    /// 数值修饰符的基础接口，定义了所有修饰符必须实现的成员。
    /// </summary>
    /// <remarks>
    /// <para>
    /// INumericModifier 接口继承自 IInfo 和 IApply，提供了修饰符的元数据和应用行为。
    /// 所有修饰符类型（AdditionNumericModifier、FractionNumericModifier、CustomNumericModifier）都实现此接口。
    /// </para>
    /// <para>
    /// 修饰符通过 Name 属性进行标识，同名修饰符会累加 Count 属性而不是创建新实例。
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // 创建并应用修饰符
    /// INumericModifier modifier = new AdditionNumericModifier(10, new[] { "Equipment" }, "Armor", 1);
    /// numeric.AddModifier(modifier);
    /// </code>
    /// </example>
    public interface INumericModifier : IInfo, IApply
    {
        /// <summary>
        /// 获取修饰符的类型标识。
        /// </summary>
        /// <value>
        /// 修饰符类型，用于内部区分不同类型的修饰符（加法、分数、自定义等）。
        /// </value>
        internal abstract ModifierType Type { get; }
    }
}
