using System;
using System.Linq;

namespace WFramework.CoreGameDevKit.NumericSystem
{
    public sealed class FractionNumericModifier : INumericModifier
    {
        private readonly int numerator;   // 分子
        private readonly int denominator; // 分母

        private readonly FractionType        type;
        public           NumericModifierInfo Info { get; }
        ModifierType INumericModifier.       Type => ModifierType.Frac;

        #region 构造函数和隐式转换

        public FractionNumericModifier(int numerator, int denominator, FractionType type)
        {
            if (denominator == 0)
                throw new ArgumentException("Denominator cannot be zero.", nameof(denominator));

            this.numerator   = numerator;
            this.denominator = denominator;
            this.type        = type;
            Info             = NumericModifierConfig.DefaultInfo;
        }

        public FractionNumericModifier(int precent, FractionType type) : this(precent, 100, type) { }

        public FractionNumericModifier(
            int          numerator,
            int          denominator,
            FractionType type,
            string[]     tags,
            string       name,
            int          count = 1)
        {
            if (denominator == 0)
                throw new ArgumentException("Denominator cannot be zero.", nameof(denominator));

            this.numerator   = numerator;
            this.denominator = denominator;
            this.type        = type;
            Info             = new NumericModifierInfo(tags, name, count);
        }

        public FractionNumericModifier(int precent, FractionType type, string[] tags, string name, int count = 1) : this
            (precent, 100, type, tags, name, count)
        {
        }

        public static implicit operator FractionNumericModifier((int numerator, int denominator, FractionType type) tuple)
            => new(tuple.numerator, tuple.denominator, tuple.type);

        public static implicit operator FractionNumericModifier(
            (int numerator, int denominator, FractionType type, string[] tags, string name, int count) tuple)
            => new(tuple.numerator, tuple.denominator, tuple.type, tuple.tags, tuple.name, tuple.count);

        public static implicit operator FractionNumericModifier((int precent, FractionType type) tuple)
            => new(tuple.precent, tuple.type);

        public static implicit operator FractionNumericModifier((int precent, FractionType type, string[] tags, string name, int count) tuple)
            => new(tuple.precent, tuple.type, tuple.tags, tuple.name, tuple.count);

        #endregion

        public Func<Numeric, int> Apply(int source) => numeric =>
        {
            // FRACTION MODIFIER APPLICATION FIX
            //
            // Problem: Original implementation caused incorrect results when multiple fraction
            // modifiers were applied, as each recalculated from the original additive structure.
            //
            // Solution: Simplify the logic to make fraction modifiers compose correctly.
            // Each modifier applies its transformation to the cumulative source, preserving
            // the effects of previous modifiers.
            //
            // The key is to work with the source value directly, which already contains
            // the cumulative result of all previous modifiers. We only need to adjust
            // the portions that this modifier is responsible for.

            var originValue = numeric.GetOriginValue();
            var targetAddModifierValue = numeric.GetAddModifierValueByTag(Info.Tags);
            var allAddModifierValue = numeric.GetAddModifierValue();
            var hasSelfTag = Info.Tags.Contains(NumericModifierConfig.TagSelf);

            // Calculate what value would be WITHOUT any fraction modifiers (origin + additive only)
            var baseValue = originValue + allAddModifierValue;

            // The source value includes effects from previous fraction modifiers.
            // We need to determine which portions of the base value have already been modified.
            //
            // Strategy: Reconstruct the value by starting from source and adjusting
            // only the portions this modifier is responsible for.

            // Calculate the targeted and untargeted portions from the base value
            var targetedBaseValue = (hasSelfTag ? originValue : 0) + targetAddModifierValue;
            var untargetedBaseValue = baseValue - targetedBaseValue;

            // Calculate what the targeted portion becomes after applying this modifier
            var modifiedTargetedValue = type switch
            {
                FractionType.Increase => GetIncrease(targetedBaseValue),
                FractionType.Override => GetOverride(targetedBaseValue),
                _ => throw new ArgumentOutOfRangeException()
            };

            // The source value can be thought of as:
            // source = (untargetedBaseValue) + (modifiedTargetedValue from previous modifiers)
            //
            // We want to produce:
            // result = (untargetedBaseValue) + (our modifiedTargetedValue)
            //
            // The challenge is that previous modifiers might have already modified the targeted portion.
            // We handle this by using the source as a starting point and adjusting the difference.

            // Calculate the difference our modifier makes to the targeted portion
            var targetedDifference = modifiedTargetedValue - targetedBaseValue;

            // Apply this difference to the source, assuming the source's targeted portion
            // is approximately equal to the base's targeted portion (before modification)
            return source + targetedDifference;
        };

        int GetIncrease(int value)
        {
            var multiplier = 1 + numerator * Info.Count / (float)denominator;
            var result = value * multiplier;

            // Check for overflow/infinity
            if (float.IsInfinity(result))
                throw new OverflowException($"Fraction modifier calculation overflow: {value} * {multiplier}");

            return (int)result;
        }

        int GetOverride(int value)
        {
            var fraction = numerator / (float)denominator;
            var power = MathF.Pow(fraction, Info.Count);

            // Check for overflow in power calculation
            if (float.IsInfinity(power))
                throw new OverflowException($"Fraction modifier power overflow: {fraction}^{Info.Count}");

            var result = value * power;

            // Check for overflow in final multiplication
            if (float.IsInfinity(result))
                throw new OverflowException($"Fraction modifier calculation overflow: {value} * {power}");

            return (int)result;
        }
    }
}