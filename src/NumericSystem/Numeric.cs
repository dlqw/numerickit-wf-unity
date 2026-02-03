using System;
using System.Collections.Generic;
using System.Linq;

namespace WFramework.CoreGameDevKit.NumericSystem
{
    /// <summary>
    /// 表示一个可修饰的数值，支持通过修饰符系统进行动态计算。
    /// </summary>
    /// <remarks>
    /// <para>
    /// Numeric 是数值系统的核心类，提供了一个不可变的基础值和可变的修饰符集合。
    /// 所有修饰符通过事件溯源的方式应用，确保数值变更的可追溯性和一致性。
    /// </para>
    /// <para>
    /// <strong>核心特性：</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description><strong>不可变基础值</strong>：构造时设置，只能通过 GetOriginValue() 读取</description></item>
    /// <item><description><strong>延迟计算</strong>：仅在修饰符变更时重新计算最终值</description></item>
    /// <item><description><strong>修饰符支持</strong>：支持加法、分数和自定义修饰符</description></item>
    /// <item><description><strong>定点数内部表示</strong>：确保跨平台数值一致性</description></item>
    /// </list>
    /// <para>
    /// <strong>计算流程：</strong>
    /// </para>
    /// <code>
    /// FinalValue = (((originalValue + 加法修饰符) × 分数修饰符) 应用自定义约束)
    /// </code>
    /// </remarks>
    /// <example>
    /// <code>
    /// // 创建数值并应用修饰符
    /// var health = new Numeric(100);
    /// health += 20;                                    // 添加 +20 buff
    /// health *= (150, FractionType.Increase);          // 增加 50%
    /// health += new CustomNumericModifier(x => Math.Clamp(x, 0, 200));  // 限制在 [0, 200]
    /// int result = health.FinalValue;                 // 结果：(100 + 20) × 1.5 = 180
    /// </code>
    /// </example>
    [Serializable]
    public class Numeric
    {
        /// <summary>
        /// 数值的原始基础值（只读）。
        /// </summary>
        private readonly int originalValue;

        /// <summary>
        /// 缓存的最终计算结果。
        /// </summary>
        private int finalValue;

        /// <summary>
        /// 获取应用所有修饰符后的最终整数值。
        /// </summary>
        /// <value>
        /// 应用所有修饰符后的整数值。如果修饰符已变更，会触发重新计算。
        /// </value>
        /// <remarks>
        /// 此属性会触发 <see cref="Update()"/> 方法以确保返回最新的计算结果。
        /// </remarks>
        /// <example>
        /// <code>
        /// var numeric = new Numeric(100);
        /// numeric += 20;
        /// int result = numeric.FinalValue;  // 120
        /// </code>
        /// </example>
        public int FinalValue
        {
            get
            {
                Update();
                return finalValue;
            }
        }

        /// <summary>
        /// 获取应用所有修饰符后的最终浮点数值。
        /// </summary>
        /// <value>
        /// 应用所有修饰符后的浮点数值。内部整数值通过 <see cref="FixedPoint.ToFloat"/> 转换。
        /// </value>
        /// <remarks>
        /// <para>
        /// 此属性将内部定点数整数值转换为浮点数返回。
        /// 如果使用 <see cref="Numeric(float)"/> 构造函数创建 Numeric，
        /// 此属性会返回与原始浮点数近似的结果。
        /// </para>
        /// <para>
        /// <strong>注意：</strong> 系统设计为 <c>int → int</c> 或 <c>float → float</c>，
        /// 建议保持类型一致性以避免精度损失。
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// var numeric = new Numeric(100.0f);
        /// numeric += 20.5f;
        /// float result = numeric.FinalValueF;  // 约 120.5f
        /// </code>
        /// </example>
        public float FinalValueF
        {
            get
            {
                Update();
                return finalValue.ToFloat();
            }
        }

        /// <summary>
        /// 上次计算的最终值，用于优化重复访问。
        /// </summary>
        private int lastValue;

        /// <summary>
        /// 标识是否需要重新计算最终值。
        /// </summary>
        private bool hasUpdate = true;

        /// <summary>
        /// 普通修饰符集合（加法修饰符和分数修饰符）。
        /// </summary>
        private readonly HashSet<INumericModifier> modifiers = new HashSet<INumericModifier>();

        /// <summary>
        /// 自定义约束修饰符集合。
        /// </summary>
        /// <remarks>
        /// 约束修饰符在所有普通修饰符应用之后执行，用于强制执行特定的约束条件（如范围限制）。
        /// </remarks>
        private readonly HashSet<CustomNumericModifier> constraintModifier = new HashSet<CustomNumericModifier>();

        /// <summary>
        /// 获取数值的原始基础值。
        /// </summary>
        /// <returns>
        /// 数值的原始基础值，不受任何修饰符影响。
        /// </returns>
        /// <remarks>
        /// 基础值在构造时设置且不可更改。要更改基础值，需要创建新的 Numeric 对象。
        /// </remarks>
        /// <example>
        /// <code>
        /// var numeric = new Numeric(100);
        /// numeric += 50;
        /// int origin = numeric.GetOriginValue();  // 100（仍然返回原始值）
        /// int final = numeric.FinalValue;        // 150
        /// </code>
        /// </example>
        public int GetOriginValue() => originalValue;

        /// <summary>
        /// 获取所有加法修饰符的累积值。
        /// </summary>
        /// <returns>
        /// 所有加法修饰符的累积效果（StoreValue × Count 之和）。
        /// </returns>
        /// <remarks>
        /// 此方法仅计算加法修饰符，不包括分数修饰符和自定义修饰符的效果。
        /// </remarks>
        public int GetAddModifierValue()
            => modifiers.Where(mod => mod.Type == ModifierType.Add)
                        .Sum(mod => mod.Info.Count * ((AdditionNumericModifier)mod).StoreValue);

        /// <summary>
        /// 获取具有指定标签的加法修饰符的累积值。
        /// </summary>
        /// <param name="tags">标签数组，用于筛选修饰符。</param>
        /// <returns>
        /// 所有 Tags 与指定标签有交集的加法修饰符的累积效果。
        /// </returns>
        /// <remarks>
        /// 如果修饰符的 Tags 数组与指定标签数组有任何重叠，则计入计算。
        /// </remarks>
        public int GetAddModifierValueByTag(string[] tags)
            => modifiers.Where(mod => mod.Type == ModifierType.Add)
                        .Where(mod => mod.Info.Tags.Intersect(tags).Any())
                        .Sum(mod => mod.Info.Count * ((AdditionNumericModifier)mod).StoreValue);

        /// <summary>
        /// 添加一个修饰符到此 Numeric 对象。
        /// </summary>
        /// <param name="modifier">要添加的修饰符。</param>
        /// <returns>
        /// 此 Numeric 对象（支持链式调用）。
        /// </returns>
        /// <remarks>
        /// <para>
        /// <strong>添加规则：</strong>
        /// </para>
        /// <list type="bullet">
        /// <item><description><see cref="CustomNumericModifier"/> 添加到约束修饰符集合，可以存在多个同名实例</description></item>
        /// <item><description>其他修饰符添加到普通修饰符集合</description></item>
        /// <item><description>如果已存在同名修饰符，则累加 Count 而不是创建新实例</description></item>
        /// </list>
        /// </remarks>
        /// <example>
        /// <code>
        /// var numeric = new Numeric(100);
        /// numeric += 20;                    // 使用运算符
        /// numeric.AddModifier(new AdditionNumericModifier(30));  // 使用方法
        /// </code>
        /// </example>
        public Numeric AddModifier(INumericModifier modifier)
        {
            if (modifier is CustomNumericModifier customModifier)
            {
                constraintModifier.Add(customModifier);
            }
            else
            {
                var existModifier = modifiers.FirstOrDefault(mod => mod.Info.Name == modifier.Info.Name);
                if (existModifier != null) existModifier.Info.Count += modifier.Info.Count;
                else modifiers.Add(modifier);
            }

            hasUpdate = true;
            return this;
        }

        /// <summary>
        /// 从此 Numeric 对象移除一个修饰符。
        /// </summary>
        /// <param name="modifier">要移除的修饰符。</param>
        /// <returns>
        /// 此 Numeric 对象（支持链式调用）。
        /// </returns>
        /// <remarks>
        /// <para>
        /// <strong>移除规则：</strong>
        /// </para>
        /// <list type="bullet">
        /// <item><description><see cref="CustomNumericModifier"/>：直接从约束集合中移除</description></item>
        /// <item><description>其他修饰符：减少同名修饰符的 Count</description></item>
        /// <item><description>当 Count 减至 0 或以下时，从集合中移除修饰符</description></item>
        /// </list>
        /// </remarks>
        /// <example>
        /// <code>
        /// var numeric = new Numeric(100);
        /// numeric += 20;
        /// numeric -= 20;                   // 使用运算符
        /// numeric.RemoveModifier(new AdditionNumericModifier(20));  // 使用方法
        /// </code>
        /// </example>
        public Numeric RemoveModifier(INumericModifier modifier)
        {
            if (modifier is CustomNumericModifier customModifier)
            {
                constraintModifier.Remove(customModifier);
            }
            else
            {
                var existModifier = modifiers.FirstOrDefault(mod => mod.Info.Name == modifier.Info.Name);
                if (existModifier != null)
                {
                    existModifier.Info.Count -= modifier.Info.Count;
                    if (existModifier.Info.Count <= 0) modifiers.Remove(existModifier);
                }
            }

            hasUpdate = true;
            return this;
        }

        /// <summary>
        /// 移除此 Numeric 对象的所有修饰符。
        /// </summary>
        /// <returns>
        /// 此 Numeric 对象（支持链式调用）。
        /// </returns>
        /// <remarks>
        /// 此方法仅移除普通修饰符，不影响约束修饰符（CustomNumericModifier）。
        /// </remarks>
        /// <example>
        /// <code>
        /// var numeric = new Numeric(100);
        /// numeric += 20;
        /// numeric *= (150, FractionType.Increase);
        /// numeric.Clear();  // 移除所有修饰符，恢复为基础值
        /// int result = numeric.FinalValue;  // 100
        /// </code>
        /// </example>
        public Numeric Clear()
        {
            modifiers.Clear();
            return this;
        }

        /// <summary>
        /// 更新最终值的计算结果。
        /// </summary>
        /// <remarks>
        /// <para>
        /// <strong>计算流程：</strong>
        /// </para>
        /// <list type="number">
        /// <item><description>如果 <see cref="hasUpdate"/> 为 false，直接返回缓存值</description></item>
        /// <item><description>从原始值开始</description></item>
        /// <item><description>依次应用所有普通修饰符（加法修饰符、分数修饰符）</description></item>
        /// <item><description>应用所有约束修饰符</description></item>
        /// <item><description>缓存结果并重置 <see cref="hasUpdate"/> 标志</description></item>
        /// </list>
        /// </remarks>
        private void Update()
        {
            if (!hasUpdate)
            {
                finalValue = lastValue;
                return;
            }

            finalValue = originalValue;
            foreach (var modifier in modifiers) finalValue = modifier.Apply(finalValue)(this);

            foreach (var customNumericModifier in constraintModifier) finalValue = customNumericModifier.Apply(finalValue)(this);


            lastValue = finalValue;
            hasUpdate = false;
        }

        /// <summary>
        /// 初始化 <see cref="Numeric"/> 类的新实例，使用整数值作为基础值。
        /// </summary>
        /// <param name="value">数值的初始基础值。</param>
        /// <remarks>
        /// 使用整数构造函数时，值直接存储而不进行定点数转换。
        /// </remarks>
        /// <example>
        /// <code>
        /// var health = new Numeric(100);
        /// var mana = new Numeric(50);
        /// </code>
        /// </example>
        public Numeric(int value)
        {
            originalValue = value;
            lastValue     = value;
        }

        /// <summary>
        /// 初始化 <see cref="Numeric"/> 类的新实例，使用浮点数值作为基础值。
        /// </summary>
        /// <param name="value">数值的初始基础值。</param>
        /// <exception cref="ArgumentException">
        /// 当 <paramref name="value"/> 为 NaN 或 Infinity 时抛出。
        /// </exception>
        /// <remarks>
        /// <para>
        /// 使用浮点数构造函数时，值会通过 <see cref="FixedPoint.ToFixedPoint"/> 转换为定点数存储，
        /// 确保后续计算的一致性。
        /// </para>
        /// <para>
        /// <strong>建议：</strong> 保持类型一致性，如果在同一 Numeric 实例中使用浮点数修饰符，
        /// 建议使用浮点数构造函数。
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// var damage = new Numeric(123.45f);
        /// </code>
        /// </example>
        public Numeric(float value)
        {
            NumericValidator.ValidateFloat(value);
            originalValue = value.ToFixedPoint();
            lastValue     = originalValue;
        }

        /// <summary>
        /// 将整数值隐式转换为 <see cref="Numeric"/> 对象。
        /// </summary>
        /// <param name="value">要转换的整数值。</param>
        /// <returns>
        /// 一个新的 <see cref="Numeric"/> 对象，基础值为 <paramref name="value"/>。
        /// </returns>
        public static implicit operator Numeric(int   value)   { return new Numeric(value); }

        /// <summary>
        /// 将浮点数值隐式转换为 <see cref="Numeric"/> 对象。
        /// </summary>
        /// <param name="value">要转换的浮点数值。</param>
        /// <returns>
        /// 一个新的 <see cref="Numeric"/> 对象，基础值为 <paramref name="value"/>。
        /// </returns>
        public static implicit operator Numeric(float value)   { return new Numeric(value); }

        /// <summary>
        /// 将 <see cref="Numeric"/> 对象隐式转换为整数值。
        /// </summary>
        /// <param name="numeric">要转换的 Numeric 对象。</param>
        /// <returns>
        /// 应用了所有修饰符后的最终整数值（<see cref="FinalValue"/>）。
        /// </returns>
        public static implicit operator int(Numeric   numeric) { return numeric.FinalValue; }

        /// <summary>
        /// 将 <see cref="Numeric"/> 对象隐式转换为浮点数值。
        /// </summary>
        /// <param name="numeric">要转换的 Numeric 对象。</param>
        /// <returns>
        /// 应用了所有修饰符后的最终浮点数值（<see cref="FinalValueF"/>）。
        /// </returns>
        public static implicit operator float(Numeric numeric) { return numeric.FinalValueF; }

        /// <summary>
        /// 加法运算符：添加一个加法修饰符到 Numeric 对象。
        /// </summary>
        /// <param name="numeric">目标 Numeric 对象。</param>
        /// <param name="modifier">要添加的加法修饰符。</param>
        /// <returns>
        /// 添加修饰符后的 <paramref name="numeric"/> 对象（支持链式调用）。
        /// </returns>
        public static Numeric operator +(Numeric numeric, AdditionNumericModifier modifier) => numeric.AddModifier(modifier);

        /// <summary>
        /// 减法运算符：从 Numeric 对象移除一个加法修饰符。
        /// </summary>
        /// <param name="numeric">目标 Numeric 对象。</param>
        /// <param name="modifier">要移除的加法修饰符。</param>
        /// <returns>
        /// 移除修饰符后的 <paramref name="numeric"/> 对象（支持链式调用）。
        /// </returns>
        public static Numeric operator -(Numeric numeric, AdditionNumericModifier modifier) => numeric.RemoveModifier(modifier);

        /// <summary>
        /// 乘法运算符：添加一个分数修饰符到 Numeric 对象。
        /// </summary>
        /// <param name="numeric">目标 Numeric 对象。</param>
        /// <param name="modifier">要添加的分数修饰符。</param>
        /// <returns>
        /// 添加修饰符后的 <paramref name="numeric"/> 对象（支持链式调用）。
        /// </returns>
        public static Numeric operator *(Numeric numeric, FractionNumericModifier modifier) => numeric.AddModifier(modifier);

        /// <summary>
        /// 除法运算符：从 Numeric 对象移除一个分数修饰符。
        /// </summary>
        /// <param name="numeric">目标 Numeric 对象。</param>
        /// <param name="modifier">要移除的分数修饰符。</param>
        /// <returns>
        /// 移除修饰符后的 <paramref name="numeric"/> 对象（支持链式调用）。
        /// </returns>
        public static Numeric operator /(Numeric numeric, FractionNumericModifier modifier) => numeric.RemoveModifier(modifier);

        /// <summary>
        /// 加法运算符：添加一个自定义修饰符到 Numeric 对象。
        /// </summary>
        /// <param name="numeric">目标 Numeric 对象。</param>
        /// <param name="modifier">要添加的自定义修饰符。</param>
        /// <returns>
        /// 添加修饰符后的 <paramref name="numeric"/> 对象（支持链式调用）。
        /// </returns>
        public static Numeric operator +(Numeric numeric, CustomNumericModifier modifier) => numeric.AddModifier(modifier);

        /// <summary>
        /// 减法运算符：从 Numeric 对象移除一个自定义修饰符。
        /// </summary>
        /// <param name="numeric">目标 Numeric 对象。</param>
        /// <param name="modifier">要移除的自定义修饰符。</param>
        /// <returns>
        /// 移除修饰符后的 <paramref name="numeric"/> 对象（支持链式调用）。
        /// </returns>
        public static Numeric operator -(Numeric numeric, CustomNumericModifier modifier) => numeric.RemoveModifier(modifier);
    }
}
