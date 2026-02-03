using System.Collections.Generic;
using System.Linq;

namespace WFramework.CoreGameDevKit.NumericSystem.Core
{
    /// <summary>
    /// 修饰符排序器接口，用于定义修饰符的排序规则
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>职责：</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>定义修饰符的排序规则</description></item>
    /// <item><description>支持稳定的排序（相同优先级的修饰符保持原有顺序）</description></item>
    /// <item><description>可配置的排序策略</description></item>
    /// </list>
    /// <para>
    /// <strong>排序规则：</strong>
    /// </para>
    /// <list type="number">
    /// <item><description>按优先级（Priority）升序排列（数值越小优先级越高）</description></item>
    /// <item><description>优先级相同按名称（Name）字母序排列</description></item>
    /// <item><description>名称相同按计数（Count）升序排列</description></item>
    /// </list>
    /// <para>
    /// <strong>设计模式：</strong> 策略模式
    /// </para>
    /// <para>
    /// <strong>扩展性：</strong> 可以通过实现此接口来创建自定义的排序策略，
    /// 例如按标签分组排序、按修饰符类型排序等。
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // 示例：创建一个自定义排序器
    /// public class TagBasedModifierSorter : IModifierSorter
    /// {
    ///     public IReadOnlyList&lt;T&gt; Sort&lt;T&gt;(IReadOnlyList&lt;T&gt; modifiers)
    ///     {
    ///         var sorted = modifiers.ToList();
    ///         sorted.Sort((a, b) => a.Info.Tags[0].CompareTo(b.Info.Tags[0]));
    ///         return sorted.AsReadOnly();
    ///     }
    /// }
    /// </code>
    /// </example>
    public interface IModifierSorter
    {
        /// <summary>
        /// 对修饰符列表进行排序
        /// </summary>
        /// <typeparam name="T">修饰符类型，必须实现 <see cref="INumericModifier"/></typeparam>
        /// <param name="modifiers">要排序的修饰符列表</param>
        /// <returns>排序后的修饰符列表</returns>
        /// <remarks>
        /// <para>
        /// <strong>排序规则：</strong>
        /// </para>
        /// <list type="number">
        /// <item><description>按 <see cref="NumericModifierInfo.Priority"/> 升序</description></item>
        /// <item><description>按 <see cref="NumericModifierInfo.Name"/> 字母序</description></item>
        /// <item><description>按 <see cref="NumericModifierInfo.Count"/> 升序</description></item>
        /// </list>
        /// <para>
        /// <strong>稳定性保证：</strong>
        /// </para>
        /// <list type="bullet">
        /// <item><description>相同排序键的修饰符保持原有相对顺序</description></item>
        /// <item><description>多次调用应该返回一致的结果</description></item>
        /// </list>
        /// </remarks>
        /// <exception cref="ArgumentNullException">当 modifiers 为 null 时抛出</exception>
        IReadOnlyList<T> Sort<T>(IReadOnlyList<T> modifiers) where T : INumericModifier;
    }
}
