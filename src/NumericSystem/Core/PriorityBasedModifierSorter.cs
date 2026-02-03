using System;
using System.Collections.Generic;
using System.Linq;

namespace WFramework.CoreGameDevKit.NumericSystem.Core
{
    /// <summary>
    /// 基于优先级的修饰符排序器，按照修饰符的优先级、名称和计数进行排序
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>排序规则：</strong>
    /// </para>
    /// <list type="number">
    /// <item><description>优先级（Priority）升序排列（数值越小优先级越高）</description></item>
    /// <item><description>优先级相同按名称（Name）字母序排列</description></item>
    /// <item><description>名称相同按计数（Count）升序排列</description></item>
    /// </list>
    /// <para>
    /// <strong>稳定性保证：</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>使用 LINQ 的 OrderBy 方法，保证排序稳定性</description></item>
    /// <item><description>相同排序键的修饰符保持原有相对顺序</description></item>
    /// <item><description>多次调用返回一致的结果</description></item>
    /// </list>
    /// <para>
    /// <strong>设计模式：</strong> 策略模式
    /// </para>
    /// <para>
    /// <strong>扩展性：</strong>
    /// </para>
    /// 如需不同的排序策略，可以实现 <see cref="IModifierSorter"/> 接口创建自定义排序器。
    /// </remarks>
    /// <example>
    /// <code>
    /// // 示例：创建排序器并排序修饰符
    /// var sorter = new PriorityBasedModifierSorter();
    /// var sortedModifiers = sorter.Sort(modifiers);
    ///
    /// // 示例：在 Numeric 类中使用
    /// var numeric = new Numeric(100);
    /// numeric += (50, new[] { "Base" }, "RaceBonus", 1, ModifierPriority.Base);
    /// numeric += (30, new[] { "Equipment" }, "Armor", 1, ModifierPriority.Equipment);
    /// // 排序后的应用顺序：Base (100) → Equipment (200)
    /// </code>
    /// </example>
    public sealed class PriorityBasedModifierSorter : IModifierSorter
    {
        /// <summary>
        /// 对修饰符列表进行排序
        /// </summary>
        /// <typeparam name="T">修饰符类型，必须实现 <see cref="INumericModifier"/></typeparam>
        /// <param name="modifiers">要排序的修饰符列表</param>
        /// <returns>排序后的修饰符列表</returns>
        /// <exception cref="ArgumentNullException">当 modifiers 为 null 时抛出</exception>
        /// <remarks>
        /// <para>
        /// <strong>排序规则：</strong>
        /// </para>
        /// <list type="number">
        /// <item><description>按 <see cref="NumericModifierInfo.Priority"/> 升序（数值越小越优先）</description></item>
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
        /// <para>
        /// <strong>性能考虑：</strong>
        /// </para>
        /// 此方法使用 LINQ 的 OrderBy，时间复杂度为 O(n log n)。
        /// </remarks>
        public IReadOnlyList<T> Sort<T>(IReadOnlyList<T> modifiers) where T : INumericModifier
        {
            if (modifiers == null)
                throw new ArgumentNullException(nameof(modifiers), "修饰符列表不能为 null。");

            // 使用 LINQ 的 OrderBy 进行稳定排序
            // OrderBy 使用稳定排序算法，保持相同键的元素原有顺序
            var sorted = modifiers
                .OrderBy(m => m.Info.Priority)
                .ThenBy(m => m.Info.Name)
                .ThenBy(m => m.Info.Count)
                .ToList();

            return sorted.AsReadOnly();
        }
    }
}
