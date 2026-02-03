using System;

namespace WFramework.CoreGameDevKit.NumericSystem
{
    /// <summary>
    /// 表示数值修饰符的元数据信息。
    /// </summary>
    /// <remarks>
    /// <para>
    /// 此 record 类型存储修饰符的标识和配置信息，包括标签数组、名称和应用计数。
    /// 所有修饰符都通过此信息进行标识和分组。
    /// </para>
    /// <para>
    /// <strong>同名修饰符行为：</strong> 当添加一个与现有修饰符同名的新修饰符时，
    /// 系统会累加 Count 属性而不是创建新实例。这允许同一修饰符效果叠加。
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // 创建带元数据的修饰符
    /// var info = new NumericModifierInfo(
    ///     new[] { "Equipment", "Rare" },  // Tags
    ///     "DragonArmor",                  // Name
    ///     1                               // Count
    /// );
    /// </code>
    /// </example>
    /// <seealso cref="INumericModifier"/>
    /// <seealso cref="NumericModifierConfig"/>
    public record NumericModifierInfo
    {
        /// <summary>
        /// 获取修饰符的标签数组。
        /// </summary>
        /// <value>
        /// 字符串数组，用于标识修饰符作用的范围。分数修饰符通过 Tags 决定影响哪些加法修饰符或基础值。
        /// </value>
        /// <remarks>
        /// <para>
        /// 标签系统工作原理：
        /// </para>
        /// <list type="bullet">
        /// <item><description>加法修饰符的 Tags：声明自己属于哪些标签</description></item>
        /// <item><description>分数修饰符的 Tags：声明自己影响哪些标签的值</description></item>
        /// <item><description>Numeric 基础值默认具有 "SELF" 标签（<see cref="NumericModifierConfig.TagSelf"/>）</description></item>
        /// <item><description>空数组表示影响所有内容（包括基础值和所有加法修饰符）</description></item>
        /// </list>
        /// </remarks>
        public readonly string[] Tags;

        /// <summary>
        /// 获取修饰符的唯一名称。
        /// </summary>
        /// <value>
        /// 修饰符的标识符，用于区分不同的修饰符实例。
        /// </value>
        /// <remarks>
        /// <para>
        /// 同名修饰符会累加 Count 而不是创建新实例。
        /// 这通常用于同一效果的多次叠加（如多层 buff）。
        /// </para>
        /// <para>
        /// 匿名修饰符使用 <see cref="NumericModifierConfig.DefaultName"/>（"DEFAULT MODIFIER"）。
        /// </para>
        /// </remarks>
        public readonly string   Name;

        /// <summary>
        /// 获取或设置修饰符的应用计数。
        /// </summary>
        /// <value>
        /// 表示此修饰符被应用的次数。默认值为 1。
        /// </value>
        /// <remarks>
        /// <para>
        /// Count 用于控制修饰符的叠加效果：
        /// </para>
        /// <list type="bullet">
        /// <item><description>对于加法修饰符：最终效果 = StoreValue × Count</description></item>
        /// <item><description>对于分数修饰符：最终效果 = fraction^Count（幂运算）</description></item>
        /// </list>
        /// </remarks>
        public int Count;

        /// <summary>
        /// 初始化 <see cref="NumericModifierInfo"/> 的新实例。
        /// </summary>
        /// <param name="tags">修饰符的标签数组，用于标识作用范围。</param>
        /// <param name="name">修饰符的唯一名称，不能为 null 或空白。</param>
        /// <param name="count">修饰符的应用计数，必须大于零。</param>
        /// <exception cref="ArgumentNullException">
        /// 当 <paramref name="tags"/> 或 <paramref name="name"/> 为 null 时抛出。
        /// </exception>
        /// <exception cref="ArgumentException">
        /// 当 <paramref name="name"/> 为空或仅包含空白字符时抛出。
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// 当 <paramref name="count"/> 小于或等于零时抛出。
        /// </exception>
        public NumericModifierInfo(string[] tags, string name, int count)
        {
            if (tags == null)
                throw new ArgumentNullException(nameof(tags), "标签不能为 null。");
            if (name == null)
                throw new ArgumentNullException(nameof(name), "名称不能为 null。");
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("名称不能为空或空白字符。", nameof(name));
            if (count <= 0)
                throw new ArgumentOutOfRangeException(nameof(count), "计数必须大于零。");

            Tags  = tags;
            Name  = name;
            Count = count;
        }
    }
}
