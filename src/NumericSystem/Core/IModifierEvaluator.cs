using System;

namespace WFramework.CoreGameDevKit.NumericSystem.Core
{
    /// <summary>
    /// 修饰符评估器接口，用于定义如何应用修饰符到数值
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>职责：</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>评估是否可以处理特定类型的修饰符</description></item>
    /// <item><description>将修饰符应用到当前值，产生新值</description></item>
    /// </list>
    /// <para>
    /// <strong>设计模式：</strong> 策略模式
    /// </para>
    /// <para>
    /// <strong>扩展性：</strong> 可以通过实现此接口来创建自定义的修饰符评估逻辑，
    /// 无需修改核心 Numeric 类。
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // 示例：实现一个自定义评估器
    /// public class CustomModifierEvaluator : IModifierEvaluator
    /// {
    ///     public bool CanEvaluate(INumericModifier modifier) =>
    ///         modifier is ICustomModifier;
    ///
    ///     public int Evaluate(int currentValue, INumericModifier modifier, Numeric context)
    ///     {
    ///         return ((ICustomModifier)modifier).ApplyCustom(currentValue);
    ///     }
    /// }
    /// </code>
    /// </example>
    public interface IModifierEvaluator
    {
        /// <summary>
        /// 评估是否可以处理此修饰符
        /// </summary>
        /// <param name="modifier">要评估的修饰符</param>
        /// <returns>如果此评估器可以处理该修饰符，返回 true；否则返回 false</returns>
        /// <remarks>
        /// 此方法用于判断评估器是否适用于给定的修饰符类型。
        /// 可以通过类型检查、接口检查或其他条件来确定。
        /// </remarks>
        bool CanEvaluate(INumericModifier modifier);

        /// <summary>
        /// 将修饰符应用到当前值
        /// </summary>
        /// <param name="currentValue">当前的计算值</param>
        /// <param name="modifier">要应用的修饰符</param>
        /// <param name="context">Numeric 上下文，提供额外的计算信息</param>
        /// <returns>应用修饰符后的新值</returns>
        /// <remarks>
        /// <para>
        /// <strong>计算流程：</strong>
        /// </para>
        /// <list type="number">
        /// <item><description>接收当前的计算值</description></item>
        /// <item><description>根据修饰符的类型和属性进行计算</description></item>
        /// <item><description>返回新的计算值</description></item>
        /// </list>
        /// <para>
        /// <strong>注意事项：</strong>
        /// </para>
        /// <list type="bullet">
        /// <item><description>此方法应该是纯函数，不应该修改修饰符或上下文的状态</description></item>
        /// <item><description>应该处理可能的边界情况（如溢出、除零等）</description></item>
        /// <item><description>性能敏感，应该避免不必要的内存分配</description></item>
        /// </list>
        /// </remarks>
        int Evaluate(int currentValue, INumericModifier modifier, Numeric context);
    }
}
