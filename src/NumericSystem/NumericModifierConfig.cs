using System;

namespace WFramework.CoreGameDevKit.NumericSystem
{
    /// <summary>
    /// 提供数值修饰符系统的配置常量和默认值。
    /// </summary>
    /// <remarks>
    /// 此类包含修饰符系统使用的标准配置值，包括标签常量、默认名称和默认计数。
    /// </remarks>
    public static class NumericModifierConfig
    {
        /// <summary>
        /// 表示影响基础值本身的特殊标签。
        /// </summary>
        /// <remarks>
        /// <para>
        /// 当分数修饰符的 Tags 数组包含 "SELF" 时，该修饰符会影响 Numeric 的基础值（OriginValue）。
        /// </para>
        /// <para>
        /// 基础值默认具有 "SELF" 标签，因此带有此标签的分数修饰符可以直接对基础值进行乘法运算。
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// // 创建影响基础值的修饰符
        /// numeric *= (150, FractionType.Increase, new[] { NumericModifierConfig.TagSelf }, "Boost", 1);
        /// // 结果：基础值 100 × 1.5 = 150
        /// </code>
        /// </example>
        public const string TagSelf      = "SELF";

        /// <summary>
        /// 匿名修饰符的默认名称。
        /// </summary>
        /// <remarks>
        /// 当创建修饰符时不指定名称，则使用此默认名称。
        /// 多个匿名修饰符会被视为同名修饰符，会累加 Count 而不是独立应用。
        /// </remarks>
        public const string DefaultName  = "DEFAULT MODIFIER";

        /// <summary>
        /// 修饰符的默认计数。
        /// </summary>
        /// <remarks>
        /// 新创建的修饰符默认 Count 为 1，表示该修饰符应用一次。
        /// </remarks>
        public const int    DefaultCount = 1;

        /// <summary>
        /// 获取默认的修饰符信息。
        /// </summary>
        /// <value>
        /// 一个 <see cref="NumericModifierInfo"/> 对象，包含空的 Tags 数组、默认名称和默认计数。
        /// </value>
        /// <remarks>
        /// 此默认信息用于匿名修饰符，或作为构造函数的默认参数。
        /// </remarks>
        public static NumericModifierInfo DefaultInfo => new(Array.Empty<string>(), DefaultName, DefaultCount);
    }
}
