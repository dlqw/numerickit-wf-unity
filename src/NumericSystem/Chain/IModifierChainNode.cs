using System;

namespace WFramework.CoreGameDevKit.NumericSystem.Core
{
    /// <summary>
    /// 修饰符责任链节点接口，定义了责任链中单个节点的行为
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>设计模式：</strong> 责任链模式（Chain of Responsibility Pattern）
    /// </para>
    /// <para>
    /// <strong>职责：</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>定义处理修饰符的统一接口</description></item>
    /// <item><description>支持将处理责任传递给下一个节点</description></item>
    /// <item><description>允许动态构建处理管道</description></item>
    /// </list>
    /// <para>
    /// <strong>应用场景：</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>修饰符的验证和过滤</description></item>
    /// <item><description>修饰符的转换和处理</description></item>
    /// <item><description>修饰符的日志记录和诊断</description></item>
    /// </list>
    /// <para>
    /// <strong>扩展性：</strong>
    /// </para>
    /// 可以通过实现此接口来创建自定义的链节点，插入到处理管道中。
    /// </remarks>
    /// <example>
    /// <code>
    /// // 示例：创建一个验证节点
    /// public class ValidationNode : IModifierChainNode
    /// {
    ///     private readonly IModifierChainNode next;
    ///
    ///     public ValidationNode(IModifierChainNode next = null)
    ///     {
    ///         this.next = next;
    ///     }
    ///
    ///     public int Process(int value, INumericModifier modifier, Numeric context)
    ///     {
    ///         if (modifier == null)
    ///             throw new ArgumentNullException(nameof(modifier));
    ///
    ///         // 处理当前节点的逻辑
    ///         var processedValue = value + 10;
    ///
    ///         // 传递给下一个节点
    ///         return next?.Process(processedValue, modifier, context) ?? processedValue;
    ///     }
    /// }
    /// </code>
    /// </example>
    public interface IModifierChainNode
    {
        /// <summary>
        /// 处理修饰符，并返回处理后的值
        /// </summary>
        /// <param name="value">当前的计算值（内部定点数形式）</param>
        /// <param name="modifier">要处理的修饰符</param>
        /// <param name="context">Numeric 上下文，提供额外的计算信息</param>
        /// <returns>处理后的值（内部定点数形式）</returns>
        /// <remarks>
        /// <para>
        /// <strong>处理流程：</strong>
        /// </para>
        /// <list type="number">
        /// <item><description>执行当前节点的处理逻辑</description></item>
        /// <item><description>如果有下一个节点，将结果传递给下一个节点处理</description></item>
        /// <item><description>如果没有下一个节点，返回当前处理结果</description></item>
        /// </list>
        /// <para>
        /// <strong>注意事项：</strong>
        /// </para>
        /// <list type="bullet">
        /// <item><description>此方法应该是纯函数，不应该修改修饰符或上下文的状态</description></item>
        /// <item><description>应该处理可能的边界情况（如溢出、除零等）</description></item>
        /// </list>
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// 当 <paramref name="modifier"/> 或 <paramref name="context"/> 为 null 时抛出
        /// </exception>
        int Process(int value, INumericModifier modifier, Numeric context);
    }
}
