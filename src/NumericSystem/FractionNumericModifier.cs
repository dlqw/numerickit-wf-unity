using System;
using System.Linq;

namespace WFramework.CoreGameDevKit.NumericSystem
{
    /// <summary>
    /// 分数修饰符，用于对数值进行百分比或分数形式的乘法操作
    /// </summary>
    /// <remarks>
    /// <para>支持两种操作模式：</para>
    /// <list type="bullet">
    /// <item><description><b>Increase（增量）</b>：增加指定百分比，例如 50% Increase 表示乘以 1.5</description></item>
    /// <item><description><b>Override（覆盖）</b>：替换为指定分数，例如 200% Override 表示乘以 2.0</description></item>
    /// </list>
    /// <para>通过标签系统，修饰符可以有选择地影响特定类型的加法值。</para>
    /// <para>支持 Count 属性用于叠加效果，对 Override 类型使用幂运算。</para>
    /// </remarks>
    [Serializable]
    public sealed class FractionNumericModifier : INumericModifier
    {
        private readonly int numerator;   // 分子
        private readonly int denominator; // 分母

        private readonly FractionType        type;
        public           NumericModifierInfo Info { get; }
        ModifierType INumericModifier.       Type => ModifierType.Frac;

        #region 序列化支持

        /// <summary>
        /// 获取分子（用于序列化）
        /// </summary>
        public int Numerator => numerator;

        /// <summary>
        /// 获取分母（用于序列化）
        /// </summary>
        public int Denominator => denominator;

        /// <summary>
        /// 获取分数类型（用于序列化）
        /// </summary>
        public FractionType FractionTypeValue => type;

        #endregion

        #region 构造函数和隐式转换

        public FractionNumericModifier(int numerator, int denominator, FractionType type, ModifierPriority priority = ModifierPriority.Multiplier)
        {
            if (denominator == 0)
                throw new ArgumentException(
                    $"分数修饰符的分母不能为零。（提供的值：{denominator}）",
                    nameof(denominator));

            this.numerator   = numerator;
            this.denominator = denominator;
            this.type        = type;
            Info             = new NumericModifierInfo(Array.Empty<string>(), NumericModifierConfig.DefaultName, NumericModifierConfig.DefaultCount, priority);
        }

        public FractionNumericModifier(int percent, FractionType type, ModifierPriority priority = ModifierPriority.Multiplier)
            : this(percent, 100, type, priority) { }

        public FractionNumericModifier(
            int          numerator,
            int          denominator,
            FractionType type,
            string[]     tags,
            string       name,
            int          count = 1,
            ModifierPriority priority = ModifierPriority.Multiplier)
        {
            if (denominator == 0)
                throw new ArgumentException(
                    $"分数修饰符 '{name ?? "(未命名)"}' 的分母不能为零。（提供的值：{denominator}）",
                    nameof(denominator));

            this.numerator   = numerator;
            this.denominator = denominator;
            this.type        = type;
            Info             = new NumericModifierInfo(tags, name, count, priority);
        }

        public FractionNumericModifier(int percent, FractionType type, string[] tags, string name, int count = 1, ModifierPriority priority = ModifierPriority.Multiplier)
            : this(percent, 100, type, tags, name, count, priority)
        {
        }

        public static implicit operator FractionNumericModifier((int numerator, int denominator, FractionType type) tuple)
            => new(tuple.numerator, tuple.denominator, tuple.type);

        public static implicit operator FractionNumericModifier(
            (int numerator, int denominator, FractionType type, string[] tags, string name, int count) tuple)
            => new(tuple.numerator, tuple.denominator, tuple.type, tuple.tags, tuple.name, tuple.count);

        public static implicit operator FractionNumericModifier((int percent, FractionType type) tuple)
            => new(tuple.percent, tuple.type);

        public static implicit operator FractionNumericModifier((int percent, FractionType type, string[] tags, string name, int count) tuple)
            => new(tuple.percent, tuple.type, tuple.tags, tuple.name, tuple.count);

        #endregion

        /// <summary>
        /// 应用修饰符到指定的数值
        /// </summary>
        /// <param name="source">源值（可能已被之前的修饰符修改）</param>
        /// <returns>应用修饰符后的值</returns>
        /// <remarks>
        /// <para>应用逻辑：</para>
        /// <list type="number">
        /// <item><description>空标签：仅影响基础值，不影响加法修饰符值</description></item>
        /// <item><description>SELF 标签：影响基础值和匹配标签的加法值</description></item>
        /// <item><description>其他标签：仅影响匹配标签的加法值</description></item>
        /// </list>
        /// <para>处理多个分数修饰符的组合：</para>
        /// <list type="bullet">
        /// <item><description>非目标部分在所有修饰符中保持不变</description></item>
        /// <item><description>目标部分被所有修饰符依次应用</description></item>
        /// <item><description>从源值中减去非目标部分得到当前目标值</description></item>
        /// </list>
        /// </remarks>
        public Func<Numeric, int> Apply(int source) => numeric =>
        {
            ValidateInputs(numeric);

            // 获取基础数值信息
            var context = CalculateModifierContext(numeric);

            // 根据标签类型应用修饰符
            return context.HasEmptyTags
                ? ApplyToEmptyTags(source, context)
                : ApplyWithTags(source, context);
        };

        /// <summary>
        /// 验证输入参数
        /// </summary>
        private void ValidateInputs(Numeric numeric)
        {
            if (numeric == null)
            {
                throw new ArgumentNullException(
                    nameof(numeric),
                    $"应用分数修饰符 '{Info.Name}' 时失败：Numeric 对象不能为 null。");
            }

            if (Info.Tags == null)
            {
                throw new InvalidOperationException(
                    $"分数修饰符 '{Info.Name}' 的 Tags 属性未正确初始化。Tags 不能为 null。" +
                    $"这可能表示修饰符在反序列化或创建过程中出现了问题。");
            }
        }

        /// <summary>
        /// 计算修饰符上下文信息
        /// </summary>
        private ModifierContext CalculateModifierContext(Numeric numeric)
        {
            var originValue = numeric.GetOriginValue();
            var targetAddModifierValue = numeric.GetAddModifierValueByTag(Info.Tags);
            var allAddModifierValue = numeric.GetAddModifierValue();

            var hasSelfTag = Info.Tags.Contains(NumericModifierConfig.TagSelf);
            var hasEmptyTags = Info.Tags.Length == 0;
            var baseValue = originValue + allAddModifierValue;
            var targetedBaseValue = (hasSelfTag ? originValue : 0) + targetAddModifierValue;
            var untargetedBaseValue = baseValue - targetedBaseValue;

            return new ModifierContext
            {
                OriginValue = originValue,
                AllAddModifierValue = allAddModifierValue,
                TargetedBaseValue = targetedBaseValue,
                UntargetedBaseValue = untargetedBaseValue,
                HasEmptyTags = hasEmptyTags,
                HasSelfTag = hasSelfTag
            };
        }

        /// <summary>
        /// 应用空标签修饰符（仅影响基础值）
        /// </summary>
        private int ApplyToEmptyTags(int source, ModifierContext context)
        {
            var modifiedOrigin = ApplyFractionType(context.OriginValue);
            return modifiedOrigin + context.AllAddModifierValue;
        }

        /// <summary>
        /// 应用带标签的修饰符
        /// </summary>
        private int ApplyWithTags(int source, ModifierContext context)
        {
            // 全部是目标值
            if (context.UntargetedBaseValue == 0)
            {
                return ApplyFractionType(source);
            }

            // 全部是非目标值
            if (context.TargetedBaseValue == 0)
            {
                return source;
            }

            // 混合情况：分离目标和非目标部分
            return ApplyToMixedTarget(source, context);
        }

        /// <summary>
        /// 应用到混合目标（既有目标值也有非目标值）
        /// </summary>
        private int ApplyToMixedTarget(int source, ModifierContext context)
        {
            // 从源值中提取当前目标值
            // 关键洞察：分数修饰符只影响目标部分，非目标部分保持不变
            // 因此：source = untargetedBase + currentTargeted
            var currentTargeted = source - context.UntargetedBaseValue;

            // 对目标值应用修饰符
            var modifiedTargeted = ApplyFractionType(currentTargeted);

            // 重构结果：非目标 + 修改后的目标
            return context.UntargetedBaseValue + modifiedTargeted;
        }

        /// <summary>
        /// 根据分数类型应用相应的计算
        /// </summary>
        private int ApplyFractionType(int value)
        {
            return type switch
            {
                FractionType.Increase => GetIncrease(value),
                FractionType.Override => GetOverride(value),
                _ => throw new ArgumentOutOfRangeException(nameof(type), "不支持的分数类型。")
            };
        }

        /// <summary>
        /// 修饰符计算上下文
        /// </summary>
        private struct ModifierContext
        {
            public int OriginValue;
            public int AllAddModifierValue;
            public int TargetedBaseValue;
            public int UntargetedBaseValue;
            public bool HasEmptyTags;
            public bool HasSelfTag;
        }

        /// <summary>
        /// 应用增量类型修饰符（增加指定百分比）
        /// </summary>
        /// <param name="value">要修改的值</param>
        /// <returns>增加后的值</returns>
        /// <remarks>
        /// Increase 表示"增加 N%"，例如：
        /// - 50% Increase 表示乘以 1.5（值 + 50%）
        /// - 100% Increase 表示乘以 2.0（值 + 100%）
        /// - 150% Increase 表示乘以 2.5（值 + 150%）
        /// </remarks>
        private int GetIncrease(int value)
        {
            var multiplier = CalculateIncreaseMultiplier();
            var result = value * multiplier;
            ValidateNoOverflow(result);

            return (int)result;
        }

        /// <summary>
        /// 计算增量类型的乘数
        /// </summary>
        private float CalculateIncreaseMultiplier()
        {
            return 1 + numerator * Info.Count / (float)denominator;
        }

        /// <summary>
        /// 应用覆盖类型修饰符（替换为指定的分数）
        /// </summary>
        /// <param name="value">要修改的值</param>
        /// <returns>覆盖后的值</returns>
        /// <remarks>
        /// Override 表示"替换为 N%"，例如：
        /// - 200% Override 表示乘以 2.0
        /// - 50% Override 表示乘以 0.5
        /// - 对于 Count > 1，使用幂运算：fraction^Count
        /// </remarks>
        private int GetOverride(int value)
        {
            var fraction = numerator / (float)denominator;
            var power = CalculateOverridePower(fraction);
            var result = value * power;
            ValidateNoOverflow(result);

            return (int)result;
        }

        /// <summary>
        /// 计算覆盖类型的幂
        /// </summary>
        private float CalculateOverridePower(float fraction)
        {
            if (float.IsInfinity(fraction))
            {
                throw new OverflowException(
                    $"分数修饰符 '{Info.Name}' 的分数值溢出。" +
                    $"分子：{numerator}，分母：{denominator}，计算结果：{fraction}");
            }

            var power = MathF.Pow(fraction, Info.Count);

            if (float.IsInfinity(power))
            {
                throw new OverflowException(
                    $"分数修饰符 '{Info.Name}' 的幂运算溢出。" +
                    $"分数：{fraction}（{numerator}/{denominator}），指数（Count）：{Info.Count}，" +
                    $"计算结果：{fraction}^{Info.Count} 超过了浮点数范围。");
            }

            return power;
        }

        /// <summary>
        /// 验证计算结果没有溢出
        /// </summary>
        private void ValidateNoOverflow(float result)
        {
            if (float.IsInfinity(result))
            {
                var operation = type == FractionType.Increase ? "增量" : "覆盖";
                var calculation = type == FractionType.Increase
                    ? $"1 + ({numerator} × {Info.Count} / {denominator})"
                    : $"({numerator}/{denominator})^{Info.Count}";

                throw new OverflowException(
                    $"分数修饰符 '{Info.Name}' 的{operation}计算溢出。" +
                    $"计算公式：{calculation}。" +
                    $"计算结果超过了 float 类型的最大值。");
            }
        }
    }
}