using System;

namespace WFramework.CoreGameDevKit.NumericSystem
{
    /// <summary>
    /// 提供修饰符系统通用的静态验证方法。
    /// </summary>
    /// <remarks>
    /// 此类集中管理所有修饰符类型共用的输入验证逻辑，
    /// 消除代码重复并确保验证的一致性。
    /// </remarks>
    internal static class NumericValidator
    {
        /// <summary>
        /// 验证分数修饰符的分母是否有效。
        /// </summary>
        /// <param name="denominator">要验证的分母值。</param>
        /// <param name="paramName">参数名称（用于异常消息）。</param>
        /// <exception cref="ArgumentException">
        /// 当 <paramref name="denominator"/> 为零时抛出。
        /// </exception>
        /// <remarks>
        /// 分数为零会导致除零异常，因此必须在构造分数修饰符时验证分母。
        /// </remarks>
        public static void ValidateDenominator(int denominator, string? paramName = null)
        {
            if (denominator == 0)
                throw new ArgumentException("分母不能为零。", paramName ?? nameof(denominator));
        }

        /// <summary>
        /// 验证修饰符名称是否有效。
        /// </summary>
        /// <param name="name">要验证的名称。</param>
        /// <param name="paramName">参数名称（用于异常消息）。</param>
        /// <exception cref="ArgumentNullException">
        /// 当 <paramref name="name"/> 为 null 时抛出。
        /// </exception>
        /// <exception cref="ArgumentException">
        /// 当 <paramref name="name"/> 为空或仅包含空白字符时抛出。
        /// </exception>
        /// <remarks>
        /// 修饰符名称用于标识和分组，因此不能为空或空白。
        /// </remarks>
        public static void ValidateName(string name, string? paramName = null)
        {
            if (name == null)
                throw new ArgumentNullException(paramName ?? nameof(name), "名称不能为 null。");
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("名称不能为空或空白字符。", paramName ?? nameof(name));
        }

        /// <summary>
        /// 验证修饰符标签数组是否有效。
        /// </summary>
        /// <param name="tags">要验证的标签数组。</param>
        /// <param name="paramName">参数名称（用于异常消息）。</param>
        /// <exception cref="ArgumentNullException">
        /// 当 <paramref name="tags"/> 为 null 时抛出。
        /// </exception>
        /// <remarks>
        /// 标签数组不能为 null，但可以为空数组（表示不限定作用范围）。
        /// </remarks>
        public static void ValidateTags(string[] tags, string? paramName = null)
        {
            if (tags == null)
                throw new ArgumentNullException(paramName ?? nameof(tags), "标签不能为 null。");
        }

        /// <summary>
        /// 验证修饰符计数是否有效。
        /// </summary>
        /// <param name="count">要验证的计数值。</param>
        /// <param name="paramName">参数名称（用于异常消息）。</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// 当 <paramref name="count"/> 小于或等于零时抛出。
        /// </exception>
        /// <remarks>
        /// 计数必须为正数，表示修饰符至少应用一次。
        /// </remarks>
        public static void ValidateCount(int count, string? paramName = null)
        {
            if (count <= 0)
                throw new ArgumentOutOfRangeException(paramName ?? nameof(count), "计数必须大于零。");
        }

        /// <summary>
        /// 验证浮点数值是否有效。
        /// </summary>
        /// <param name="value">要验证的浮点数值。</param>
        /// <param name="paramName">参数名称（用于异常消息）。</param>
        /// <exception cref="ArgumentException">
        /// 当 <paramref name="value"/> 为 NaN、Infinity 或过大无法安全转换时抛出。
        /// </exception>
        /// <remarks>
        /// 此方法通过委托到 <see cref="FixedPoint.ValidateFloat"/> 实现。
        /// </remarks>
        public static void ValidateFloat(float value, string? paramName = null)
        {
            FixedPoint.ValidateFloat(value, paramName);
        }

        /// <summary>
        /// 验证函数参数是否为 null。
        /// </summary>
        /// <typeparam name="T">函数参数的类型。</typeparam>
        /// <param name="func">要验证的函数。</param>
        /// <param name="paramName">参数名称（用于异常消息）。</param>
        /// <exception cref="ArgumentNullException">
        /// 当 <paramref name="func"/> 为 null 时抛出。
        /// </exception>
        /// <remarks>
        /// 用于验证 CustomNumericModifier 的 intFunc 和 floatFunc 参数。
        /// </remarks>
        public static void ValidateFunc<T>(T func, string paramName) where T : Delegate
        {
            if (func == null)
                throw new ArgumentNullException(paramName, $"{paramName} 不能为 null。");
        }
    }
}
