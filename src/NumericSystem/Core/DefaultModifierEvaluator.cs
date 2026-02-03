using System;

namespace WFramework.CoreGameDevKit.NumericSystem.Core
{
    /// <summary>
    /// 默认修饰符评估器，支持所有内置修饰符类型的评估
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>职责：</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>识别所有内置修饰符类型（加法、分数、自定义、条件）</description></item>
    /// <item><description>将修饰符应用到当前值，产生新值</description></item>
    /// <item><description>处理修饰符的应用函数（Apply 方法）</description></item>
    /// </list>
    /// <para>
    /// <strong>支持的修饰符类型：</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description><see cref="AdditionNumericModifier"/> - 加法修饰符</description></item>
    /// <item><description><see cref="FractionNumericModifier"/> - 分数修饰符</description></item>
    /// <item><description><see cref="CustomNumericModifier"/> - 自定义修饰符</description></item>
    /// <item><description><see cref="ConditionalNumericModifier"/> - 条件修饰符</description></item>
    /// </list>
    /// <para>
    /// <strong>扩展性：</strong>
    /// </para>
    /// 如需支持自定义修饰符类型，可以实现 <see cref="IModifierEvaluator"/> 接口
    /// 并在评估器中注册新的评估逻辑。
    /// </remarks>
    public sealed class DefaultModifierEvaluator : IModifierEvaluator
    {
        /// <summary>
        /// 评估是否可以处理此修饰符
        /// </summary>
        /// <param name="modifier">要评估的修饰符</param>
        /// <returns>
        /// 如果修饰符是内置类型（加法、分数、自定义、条件），返回 true；否则返回 false
        /// </returns>
        public bool CanEvaluate(INumericModifier modifier)
        {
            if (modifier == null)
                return false;

            return modifier switch
            {
                AdditionNumericModifier => true,
                FractionNumericModifier => true,
                CustomNumericModifier => true,
                ConditionalNumericModifier => true,
                _ => false
            };
        }

        /// <summary>
        /// 将修饰符应用到当前值
        /// </summary>
        /// <param name="currentValue">当前的计算值（内部定点数形式）</param>
        /// <param name="modifier">要应用的修饰符</param>
        /// <param name="context">Numeric 上下文，提供额外的计算信息</param>
        /// <returns>应用修饰符后的新值（内部定点数形式）</returns>
        /// <exception cref="ArgumentNullException">
        /// 当 <paramref name="modifier"/> 或 <paramref name="context"/> 为 null 时抛出
        /// </exception>
        /// <exception cref="ArgumentException">
        /// 当修饰符类型不被支持时抛出
        /// </exception>
        /// <remarks>
        /// <para>
        /// <strong>计算流程：</strong>
        /// </para>
        /// <list type="number">
        /// <item><description>验证输入参数</description></item>
        /// <item><description>获取修饰符的 Apply 函数</description></item>
        /// <item><description>执行 Apply 函数，传入当前值和上下文</description></item>
        /// <item><description>返回计算后的新值</description></item>
        /// </list>
        /// <para>
        /// <strong>性能考虑：</strong>
        /// </para>
        /// 此方法是性能关键路径，应避免不必要的内存分配和计算。
        /// </remarks>
        public int Evaluate(int currentValue, INumericModifier modifier, Numeric context)
        {
            if (modifier == null)
                throw new ArgumentNullException(nameof(modifier), "修饰符不能为 null。");
            if (context == null)
                throw new ArgumentNullException(nameof(context), "Numeric 上下文不能为 null。");

            if (!CanEvaluate(modifier))
                throw new ArgumentException(
                    $"不支持的修饰符类型：{modifier.GetType().Name}。" +
                    $"DefaultModifierEvaluator 支持的修饰符类型包括：" +
                    $"AdditionNumericModifier, FractionNumericModifier, CustomNumericModifier, ConditionalNumericModifier。",
                    nameof(modifier));

            // 获取修饰符的 Apply 函数并执行
            var applyFunc = modifier.Apply(currentValue);
            return applyFunc(context);
        }
    }
}
