using System;

namespace WFramework.CoreGameDevKit.NumericSystem
{
    /// <summary>
    /// 自定义数值修饰符，允许使用自定义函数对数值进行任意转换
    /// </summary>
    /// <remarks>
    /// <para>CustomNumericModifier 提供了最大的灵活性，允许开发者定义自己的数值转换逻辑。</para>
    /// <para>支持两种函数类型：</para>
    /// <list type="bullet">
    /// <item><description><b>Func&lt;int, int&gt;</b> - 接收普通整数，返回普通整数（系统会自动进行定点数转换）</description></item>
    /// <item><description><b>Func&lt;float, float&gt;</b> - 接收浮点数，返回浮点数（系统会自动进行定点数转换）</description></item>
    /// </list>
    /// <para>典型应用场景：</para>
    /// <list type="bullet">
    /// <item><description>数值限制（clamp）：确保数值在指定范围内</description></item>
    /// <item><description>条件计算：根据数值大小应用不同的规则</description></item>
    /// <item><description>复杂公式：实现系统不支持的数学运算</description></item>
    /// <item><description>数值取整：向上/向下/四舍五入到指定精度</description></item>
    /// </list>
    /// <para>注意：CustomNumericModifier 在修饰符管道中最后应用，可用于实现最终约束。</para>
    /// </remarks>
    public sealed class CustomNumericModifier : INumericModifier
    {
        ModifierType INumericModifier.Type => ModifierType.Custom;
        public NumericModifierInfo    Info { get; }

        private readonly Func<int, int>?     intFunc;
        private readonly Func<float, float>? floatFunc;


        public CustomNumericModifier(Func<int, int> intFunc, ModifierPriority priority = ModifierPriority.Clamp)
        {
            this.intFunc = intFunc ?? throw new ArgumentNullException(
                nameof(intFunc),
                "无法创建 CustomNumericModifier：intFunc 参数不能为 null。" +
                "请提供一个有效的 Func<int, int> 委托。");
            Info = new NumericModifierInfo(Array.Empty<string>(), NumericModifierConfig.DefaultName, NumericModifierConfig.DefaultCount, priority);
        }

        public CustomNumericModifier(Func<float, float> floatFunc, ModifierPriority priority = ModifierPriority.Clamp)
        {
            this.floatFunc = floatFunc ?? throw new ArgumentNullException(
                nameof(floatFunc),
                "无法创建 CustomNumericModifier：floatFunc 参数不能为 null。" +
                "请提供一个有效的 Func<float, float> 委托。");
            Info = new NumericModifierInfo(Array.Empty<string>(), NumericModifierConfig.DefaultName, NumericModifierConfig.DefaultCount, priority);
        }

        public CustomNumericModifier(Func<int, int> intFunc, string[] tags, string name, int count = 1, ModifierPriority priority = ModifierPriority.Clamp)
        {
            this.intFunc = intFunc ?? throw new ArgumentNullException(
                nameof(intFunc),
                $"无法创建名为 '{name}' 的 CustomNumericModifier：intFunc 参数不能为 null。" +
                "请提供一个有效的 Func<int, int> 委托。");
            Info = new NumericModifierInfo(tags, name, count, priority);
        }

        public CustomNumericModifier(Func<float, float> floatFunc, string[] tags, string name, int count = 1, ModifierPriority priority = ModifierPriority.Clamp)
        {
            this.floatFunc = floatFunc ?? throw new ArgumentNullException(
                nameof(floatFunc),
                $"无法创建名为 '{name}' 的 CustomNumericModifier：floatFunc 参数不能为 null。" +
                "请提供一个有效的 Func<float, float> 委托。");
            Info = new NumericModifierInfo(tags, name, count, priority);
        }

        public static implicit operator CustomNumericModifier(Func<int, int>     intFunc)   => new(intFunc);
        public static implicit operator CustomNumericModifier(Func<float, float> floatFunc) => new(floatFunc);

        public static implicit operator CustomNumericModifier((Func<int, int> intFunc, string[] tags, string name, int count) tuple)
            => new(tuple.intFunc, tuple.tags, tuple.name, tuple.count);

        public static implicit operator CustomNumericModifier((Func<float, float> floatFunc, string[] tags, string name, int count) tuple)
            => new(tuple.floatFunc, tuple.tags, tuple.name, tuple.count);

        public Func<Numeric, int> Apply(int source)
        {
            return _ =>
            {
                if (intFunc != null)
                {
                    // 将内部定点数转换为普通整数传给函数，然后再转回定点数
                    int normalValue = source / (int)FixedPoint.Factor;
                    int result = intFunc.Invoke(normalValue);
                    return result.ToFixedPoint();
                }

                if (floatFunc != null)
                    return floatFunc.Invoke(source.ToFloat()).ToFixedPoint();

                throw new InvalidOperationException(
                    $"CustomNumericModifier '{Info.Name}' 的状态无效：" +
                    "intFunc 和 floatFunc 都为 null。" +
                    "此修饰符未正确初始化，无法执行。" +
                    Environment.NewLine +
                    "可能的原因：" + Environment.NewLine +
                    "1. 修饰符通过反序列化创建，但未正确初始化委托函数" + Environment.NewLine +
                    "2. 构造函数被绕过，直接设置了 null 值" + Environment.NewLine +
                    "3. 内部状态被意外修改" + Environment.NewLine +
                    Environment.NewLine +
                    "解决方法：确保使用标准的构造函数创建 CustomNumericModifier，" +
                    "并提供 intFunc 或 floatFunc 中的至少一个。");
            };
        }
    }
}