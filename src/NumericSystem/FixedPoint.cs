using System;

namespace WFramework.CoreGameDevKit.NumericSystem
{
    public static class FixedPoint
    {
        public const uint Factor = 10000;

        public static int ToFixedPoint(this float value)
        {
            ValidateFloat(value);
            return (int)(value * Factor);
        }

        public static int ToFixedPoint(this int value) { return (int)(value * Factor); }

        public static float ToFloat(this int value) { return value / (float)Factor; }

        /// <summary>
        /// Validates that a float value is not NaN or Infinity.
        /// Throws ArgumentException if validation fails.
        /// </summary>
        /// <param name="value">要验证的浮点值</param>
        /// <param name="paramName">参数名称（可选）</param>
        /// <exception cref="ArgumentException">当值为 NaN、Infinity 或超出定点数范围时抛出</exception>
        public static void ValidateFloat(float value, string? paramName = null)
        {
            if (float.IsNaN(value))
            {
                throw new ArgumentException(
                    $"数值 {value} 不是有效数字（NaN）。请提供一个有效的浮点数。",
                    paramName ?? nameof(value));
            }

            if (float.IsInfinity(value))
            {
                throw new ArgumentException(
                    $"数值 {value} 超出了浮点数的表示范围（Infinity）。" +
                    $"请使用更小的值或将数值拆分为多个计算。",
                    paramName ?? nameof(value));
            }

            // 添加范围检查，防止转换后溢出
            var maxValue = float.MaxValue / Factor;
            if (MathF.Abs(value) > maxValue)
            {
                throw new ArgumentException(
                    $"数值 {value} 过大，无法安全转换为定点数。" +
                    $"定点数精度因子为 {Factor}，最大安全值为 {maxValue:F2}。" +
                    $"建议：减小数值范围或使用 decimal 类型进行中间计算。",
                    paramName ?? nameof(value));
            }
        }
    }
}