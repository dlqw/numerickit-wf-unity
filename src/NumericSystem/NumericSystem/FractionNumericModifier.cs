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
            var allAddModifierValue      = numeric.GetAddModifierValue();
            var targetAddModifierValue   = numeric.GetAddModifierValueByTag(Info.Tags);
            var noTargetAddModiferValue = allAddModifierValue - targetAddModifierValue;
            return type switch
            {
                FractionType.Increase => GetIncrease(targetAddModifierValue) + noTargetAddModiferValue + GetOrigin(GetIncrease) + GetOtherFrac(),
                FractionType.Override => GetOverride(targetAddModifierValue) + noTargetAddModiferValue + GetOrigin(GetOverride) + GetOtherFrac(),
                _                     => throw new ArgumentOutOfRangeException()
            };

            int GetOrigin(Func<int, int> func)
                => Info.Tags.Contains(NumericModifierConfig.TagSelf) ? func(numeric.GetOriginValue()) : numeric.GetOriginValue();

            int GetOtherFrac() => source - allAddModifierValue - numeric.GetOriginValue();
        };

        int GetIncrease(int value) => (int)(value * (1 + numerator * Info.Count / (float)denominator));
        int GetOverride(int value) => (int)(value * MathF.Pow(numerator / (float)denominator, Info.Count));
    }
}