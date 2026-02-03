using System;
using System.Collections.Generic;
using System.Linq;
using WFramework.CoreGameDevKit.NumericSystem.Core;

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
    /// <item><description><strong>可扩展架构</strong>：使用策略模式和责任链模式</description></item>
    /// </list>
    /// <para>
    /// <strong>计算流程：</strong>
    /// </para>
    /// <code>
    /// FinalValue = (((originalValue + 加法修饰符) × 分数修饰符) 应用自定义约束) 应用条件修饰符
    /// </code>
    /// <para>
    /// <strong>架构说明：</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>使用 <see cref="IModifierCollection"/> 管理修饰符存储</description></item>
    /// <item><description>使用 <see cref="IModifierSorter"/> 对修饰符排序</description></item>
    /// <item><description>使用 <see cref="IModifierEvaluator"/> 评估修饰符效果</description></item>
    /// </list>
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
        /// 上次计算的最终值，用于优化重复访问。
        /// </summary>
        private int lastValue;

        /// <summary>
        /// 标识是否需要重新计算最终值。
        /// </summary>
        private bool hasUpdate = true;

        /// <summary>
        /// 修饰符集合，使用新架构的集合接口
        /// </summary>
        private readonly IModifierCollection modifierCollection;

        /// <summary>
        /// 修饰符排序器，使用新架构的排序器接口
        /// </summary>
        private readonly IModifierSorter modifierSorter;

        /// <summary>
        /// 自定义约束修饰符集合。
        /// </summary>
        /// <remarks>
        /// 约束修饰符在所有普通修饰符应用之后执行，用于强制执行特定的约束条件（如范围限制）。
        /// </remarks>
        private readonly HashSet<CustomNumericModifier> constraintModifier = new HashSet<CustomNumericModifier>();

        /// <summary>
        /// 条件修饰符集合。
        /// </summary>
        /// <remarks>
        /// 条件修饰符在约束修饰符之后执行，只在满足特定条件时生效。
        /// </remarks>
        private readonly HashSet<ConditionalNumericModifier> conditionalModifiers = new HashSet<ConditionalNumericModifier>();

        /// <summary>
        /// 获取应用所有修饰符后的最终整数值。
        /// </summary>
        /// <value>
        /// 应用所有修饰符后的整数值。如果修饰符已变更，会触发重新计算。
        /// </value>
        /// <remarks>
        /// <para>
        /// 此属性会触发 <see cref="Update()"/> 方法以确保返回最新的计算结果。
        /// </para>
        /// <para>
        /// 返回值是内部定点数转换后的整数（除以10000），如果计算结果有小数部分，会被截断。
        /// </para>
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
                // 将内部定点数转换为普通整数返回
                return finalValue / (int)FixedPoint.Factor;
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
        /// 获取数值的原始基础值（内部定点数形式）。
        /// </summary>
        /// <returns>
        /// 数值的原始基础值，不受任何修饰符影响。返回值是内部定点数形式。
        /// </returns>
        /// <remarks>
        /// <para>
        /// 基础值在构造时设置且不可更改。要更改基础值，需要创建新的 Numeric 对象。
        /// </para>
        /// <para>
        /// 返回值是内部定点数，主要用于内部计算。如需获取用户友好的普通整数值，
        /// 请使用 <see cref="FinalValue"/> 或自行除以 <see cref="FixedPoint.Factor"/>。
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// var numeric = new Numeric(100);
        /// numeric += 50;
        /// int origin = numeric.GetOriginValue();  // 1000000 (定点数)
        /// int final = numeric.FinalValue;        // 150 (普通整数)
        /// </code>
        /// </example>
        public int GetOriginValue() => originalValue;

        /// <summary>
        /// 获取所有加法修饰符的累积值（内部定点数）。
        /// </summary>
        /// <returns>
        /// 所有加法修饰符的累积效果（StoreValue × Count 之和），以定点数形式返回。
        /// </returns>
        /// <remarks>
        /// 此方法仅计算加法修饰符，不包括分数修饰符和自定义修饰符的效果。
        /// 返回值是内部定点数形式，主要用于内部计算。
        /// </remarks>
        public int GetAddModifierValue()
        {
            return modifierCollection.GetAddModifierValue();
        }

        /// <summary>
        /// 获取具有指定标签的加法修饰符的累积值（内部定点数）。
        /// </summary>
        /// <param name="tags">标签数组，用于筛选修饰符。</param>
        /// <returns>
        /// 所有 Tags 与指定标签有交集的加法修饰符的累积效果，以定点数形式返回。
        /// </returns>
        /// <remarks>
        /// 如果修饰符的 Tags 数组与指定标签数组有任何重叠，则计入计算。
        /// 返回值是内部定点数形式，主要用于内部计算。
        /// </remarks>
        public int GetAddModifierValueByTag(string[] tags)
        {
            return modifierCollection.GetAddModifierValueByTag(tags);
        }

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
        /// <item><description><see cref="ConditionalNumericModifier"/> 添加到条件修饰符集合</description></item>
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
            // 检查是否为条件修饰符
            if (modifier is ConditionalNumericModifier conditionalModifier)
            {
                conditionalModifiers.Add(conditionalModifier);
            }
            else if (modifier is CustomNumericModifier customModifier)
            {
                constraintModifier.Add(customModifier);
            }
            else
            {
                modifierCollection.Add(modifier);
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
        /// <item><description><see cref="ConditionalNumericModifier"/>：直接从条件集合中移除</description></item>
        /// <item><description>其他修饰符：通过修饰符集合移除</description></item>
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
            if (modifier is ConditionalNumericModifier conditionalModifier)
            {
                conditionalModifiers.Remove(conditionalModifier);
            }
            else if (modifier is CustomNumericModifier customModifier)
            {
                constraintModifier.Remove(customModifier);
            }
            else
            {
                modifierCollection.Remove(modifier);
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
        /// 此方法仅移除普通修饰符，不影响约束修饰符（CustomNumericModifier）和条件修饰符。
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
            modifierCollection.Clear();
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
        /// <item><description>应用所有条件修饰符</description></item>
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

            // 获取所有普通修饰符并排序
            var allModifiers = modifierCollection.GetAll();
            var sortedModifiers = modifierSorter.Sort(allModifiers);

            // 应用排序后的修饰符
            foreach (var modifier in sortedModifiers)
                finalValue = modifier.Apply(finalValue)(this);

            // 约束修饰符始终最后应用，按优先级排序
            var sortedConstraints = modifierSorter.Sort(constraintModifier.ToList());
            foreach (var customNumericModifier in sortedConstraints)
                finalValue = customNumericModifier.Apply(finalValue)(this);

            // 条件修饰符在约束修饰符之后应用，按优先级排序
            var sortedConditional = modifierSorter.Sort(conditionalModifiers.ToList());
            foreach (var conditionalModifier in sortedConditional)
                finalValue = conditionalModifier.Apply(finalValue)(this);

            lastValue = finalValue;
            hasUpdate = false;
        }

        /// <summary>
        /// 初始化 <see cref="Numeric"/> 类的新实例，使用整数值作为基础值。
        /// </summary>
        /// <param name="value">数值的初始基础值。</param>
        /// <remarks>
        /// <para>
        /// 使用整数构造函数时，值会被转换为定点数存储，确保内部表示的一致性。
        /// </para>
        /// <para>
        /// <strong>注意：</strong> 无论使用int还是float构造函数，内部都统一使用定点数表示，
        /// 这确保了跨平台的一致性和混合使用的正确性。
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// var health = new Numeric(100);    // 内部存储为 1000000 (100 * 10000)
        /// var mana = new Numeric(50);      // 内部存储为 500000 (50 * 10000)
        /// </code>
        /// </example>
        public Numeric(int value)
        {
            // 统一使用定点数表示，确保内部一致性
            originalValue = value.ToFixedPoint();
            lastValue     = originalValue;
            modifierCollection = new ModifierCollection();
            modifierSorter = new PriorityBasedModifierSorter();
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
            modifierCollection = new ModifierCollection();
            modifierSorter = new PriorityBasedModifierSorter();
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

        #region 诊断和调试辅助方法

        /// <summary>
        /// 获取所有修饰符的只读集合
        /// </summary>
        /// <returns>包含所有修饰符的只读集合</returns>
        /// <remarks>
        /// 此方法供诊断工具使用，返回当前 Numeric 对象的所有修饰符。
        /// 返回的集合是快照，不会随后续修改而更新。
        /// </remarks>
        public IReadOnlyList<INumericModifier> GetAllModifiers()
        {
            Update();
            var allModifiers = new List<INumericModifier>();
            allModifiers.AddRange(modifierCollection.GetAll());
            allModifiers.AddRange(constraintModifier);
            allModifiers.AddRange(conditionalModifiers);
            return allModifiers.AsReadOnly();
        }

        /// <summary>
        /// 使缓存失效，强制重新计算
        /// </summary>
        /// <remarks>
        /// 此方法主要用于测试和性能诊断。
        /// 正常情况下，系统会自动管理缓存，不需要手动调用此方法。
        /// </remarks>
        public void InvalidateCache()
        {
            hasUpdate = true;
        }

        /// <summary>
        /// 获取缓存状态信息
        /// </summary>
        /// <returns>包含缓存状态的字符串</returns>
        /// <remarks>
        /// 此方法用于诊断，显示当前缓存是否有效。
        /// </remarks>
        public string GetCacheStatus()
        {
            Update();
            return hasUpdate ? "缓存无效（需要重新计算）" : "缓存有效";
        }

        /// <summary>
        /// 获取修饰符数量统计
        /// </summary>
        /// <returns>包含各类型修饰符数量的字典</returns>
        public Dictionary<string, int> GetModifierStats()
        {
            var stats = new Dictionary<string, int>();
            var allModifiers = GetAllModifiers();

            foreach (var mod in allModifiers)
            {
                var type = mod.Type.ToString();
                if (!stats.ContainsKey(type))
                    stats[type] = 0;
                stats[type]++;
            }

            return stats;
        }

        #endregion
    }
}
