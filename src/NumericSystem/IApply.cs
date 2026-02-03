using System;

namespace WFramework.CoreGameDevKit.NumericSystem
{
    /// <summary>
    /// 定义修饰符应用行为的接口。
    /// </summary>
    /// <remarks>
    /// 此接口提供了修饰符的应用逻辑，每个修饰符通过 Apply 方法返回一个函数，
    /// 该函数接收当前的累积值并返回应用修饰符后的新值。
    /// </remarks>
    public interface IApply
    {
        /// <summary>
        /// 获取修饰符的应用函数。
        /// </summary>
        /// <param name="source">
        /// 应用此修饰符之前的累积值。这个值已经包含了所有之前应用的修饰符的效果。
        /// </param>
        /// <returns>
        /// 一个函数，接收 Numeric 对象并返回应用此修饰符后的最终整数值。
        /// </returns>
        /// <remarks>
        /// <para>
        /// Apply 方法返回一个延迟执行的函数，该函数会在 <see cref="Numeric.Update()"/> 时被调用。
        /// 这种设计允许修饰符链式组合，每个修饰符都可以基于之前的修饰符结果进行计算。
        /// </para>
        /// <para>
        /// 对于不同的修饰符类型：
        /// <list type="bullet">
        /// <item><description><see cref="AdditionNumericModifier"/>: 返回 source + StoreValue * Count</description></item>
        /// <item><description><see cref="FractionNumericModifier"/>: 根据 <see cref="FractionType"/> 对目标部分应用乘法</description></item>
        /// <item><description><see cref="CustomNumericModifier"/>: 应用自定义的转换函数</description></item>
        /// </list>
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// // 加法修饰符的 Apply 实现
        /// public Func{Numeric, int} Apply(int source) => _ => source + StoreValue * Info.Count;
        /// </code>
        /// </example>
        Func<Numeric, int> Apply(int source);
    }
}
