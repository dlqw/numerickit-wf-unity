namespace WFramework.CoreGameDevKit.NumericSystem
{
    /// <summary>
    /// 修饰符类型
    /// </summary>
    public enum ModifierType
    {
        /// <summary>
        /// 无类型
        /// </summary>
        None,
        /// <summary>
        /// 加法修饰符
        /// </summary>
        Add,
        /// <summary>
        /// 分数修饰符
        /// </summary>
        Frac,
        /// <summary>
        /// 自定义修饰符
        /// </summary>
        Custom
    }

    public interface INumericModifier : IInfo, IApply
    {
        internal abstract ModifierType Type { get; }
    }
}