using System;

namespace WFramework.CoreGameDevKit.NumericSystem
{
    /// <summary>
    /// 提供定点数运算的工具方法，用于确保跨平台的数值一致性。
    /// </summary>
    /// <remarks>
    /// <para>
    /// 定点数（Fixed-Point）是一种用整数表示小数的方法，通过乘以一个固定的精度因子来存储浮点数值。
    /// 本系统使用的精度因子为 10,000（10^4），提供 4 位小数精度。
    /// </para>
    /// <para>
    /// <strong>为什么使用定点数：</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>跨平台一致性：避免不同平台浮点运算的微小差异</description></item>
    /// <item><description>网络同步：整数在网络传输中更可靠</description></item>
    /// <item><description>确定性计算：避免浮点数精度问题导致的计算错误</description></item>
    /// </list>
    /// <para>
    /// <strong>内部表示：</strong>
    /// </para>
    /// <list type="table">
    /// <listheader><term>浮点值</term><description>内部整数值</description><description>说明</description></listheader>
    /// <item><term>1.0</term><description>10000</description><description>1.0 × 10000</description></item>
    /// <item><term>0.5</term><description>5000</description><description>0.5 × 10000</description></item>
    /// <item><term>123.4567</term><description>1234567</description><description>123.4567 × 10000（取整）</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // 将浮点数转换为定点数
    /// float floatValue = 100.5f;
    /// int fixedPointValue = floatValue.ToFixedPoint();  // 结果：1005000
    ///
    /// // 将定点数转换回浮点数
    /// float result = fixedPointValue.ToFloat();  // 结果：100.5
    /// </code>
    /// </example>
    public static class FixedPoint
    {
        /// <summary>
        /// 定点数的精度因子。
        /// </summary>
        /// <value>
        /// 固定值 10,000（10^4），提供 4 位小数精度。
        /// </value>
        /// <remarks>
        /// 所有浮点数乘以此因子后转换为整数存储，除以此因子后还原为浮点数。
        /// </remarks>
        public const uint Factor = 10000;

        /// <summary>
        /// 将浮点数值转换为定点数表示。
        /// </summary>
        /// <param name="value">要转换的浮点数值。</param>
        /// <returns>
        /// 定点数整数值，等于 <paramref name="value"/> × <see cref="Factor"/>。
        /// </returns>
        /// <remarks>
        /// 此方法会自动调用 <see cref="ValidateFloat"/> 进行输入验证。
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// 当 <paramref name="value"/> 为 NaN 或 Infinity 时抛出。
        /// </exception>
        /// <example>
        /// <code>
        /// float value = 123.456f;
        /// int fixed = value.ToFixedPoint();  // 1234560
        /// </code>
        /// </example>
        public static int ToFixedPoint(this float value)
        {
            ValidateFloat(value);
            return (int)(value * Factor);
        }

        /// <summary>
        /// 将整数值转换为定点数表示。
        /// </summary>
        /// <param name="value">要转换的整数值。</param>
        /// <returns>
        /// 定点数整数值，等于 <paramref name="value"/> × <see cref="Factor"/>。
        /// </returns>
        /// <example>
        /// <code>
        /// int value = 100;
        /// int fixed = value.ToFixedPoint();  // 1000000
        /// </code>
        /// </example>
        public static int ToFixedPoint(this int value) { return (int)(value * Factor); }

        /// <summary>
        /// 将定点数整数值转换回浮点数。
        /// </summary>
        /// <param name="value">定点数整数值。</param>
        /// <returns>
        /// 还原的浮点数值，等于 <paramref name="value"/> ÷ <see cref="Factor"/>。
        /// </returns>
        /// <example>
        /// <code>
        /// int fixed = 1005000;
        /// float value = fixed.ToFloat();  // 100.5
        /// </code>
        /// </example>
        public static float ToFloat(this int value) { return value / (float)Factor; }

        /// <summary>
        /// 验证浮点值是否为有效的数值（非 NaN，非 Infinity，在可表示范围内）。
        /// </summary>
        /// <param name="value">要验证的浮点数值。</param>
        /// <param name="paramName">参数名称（用于异常消息）。</param>
        /// <exception cref="ArgumentException">
        /// 当 <paramref name="value"/> 为 NaN、Infinity 或过大无法安全转换时抛出。
        /// </exception>
        /// <remarks>
        /// 此方法防止无效的浮点值进入系统，确保计算的确定性和正确性。
        /// </remarks>
        public static void ValidateFloat(float value, string? paramName = null)
        {
            if (float.IsNaN(value))
                throw new ArgumentException("值不能为 NaN。", paramName ?? nameof(value));

            if (float.IsInfinity(value))
                throw new ArgumentException("值不能为 Infinity。", paramName ?? nameof(value));

            // 添加范围检查，防止转换后溢出
            if (MathF.Abs(value) > float.MaxValue / Factor)
                throw new ArgumentException($"值 {value} 过大，无法安全转换为定点数。", paramName ?? nameof(value));
        }
    }
}
