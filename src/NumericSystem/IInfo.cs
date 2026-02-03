namespace WFramework.CoreGameDevKit.NumericSystem
{
    /// <summary>
    /// 定义修饰符元数据信息的接口。
    /// </summary>
    /// <remarks>
    /// 此接口提供对修饰符信息（名称、标签、计数）的访问，
    /// 用于标识和组织修饰符实例。
    /// </remarks>
    public interface IInfo
    {
        /// <summary>
        /// 获取修饰符的元数据信息。
        /// </summary>
        /// <value>
        /// 包含修饰符的名称、标签数组和应用计数的 <see cref="NumericModifierInfo"/> 对象。
        /// </value>
        /// <remarks>
        /// <para>
        /// <strong>Name</strong>: 修饰符的唯一标识符。同名修饰符会累加 Count 而不是创建新实例。
        /// </para>
        /// <para>
        /// <strong>Tags</strong>: 标签数组，用于选择性应用修饰符。分数修饰符通过 Tags 决定影响哪些加法修饰符或基础值。
        /// </para>
        /// <para>
        /// <strong>Count</strong>: 修饰符的叠加计数，表示此修饰符被应用的次数。
        /// </para>
        /// </remarks>
        NumericModifierInfo Info { get; }
    }
}
