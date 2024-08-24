using System;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace WFramework.CoreGameDevKit.NumericSystem
{
    [ShowInInspector]
    public record NumericModifierInfo
    {
        public readonly string[] Tags;
        public readonly string   Name;

        public int Count;

        public NumericModifierInfo(string[] tags, string name, int count)
        {
            Tags  = tags;
            Name  = name;
            Count = count;
        }
    }

    [ShowInInspector]
    public class NumericModifier
    {
        public const string TagSelf      = "SELF";
        public const string DefaultName  = "DEFAULT MODIFIER";
        public const int    DefaultCount = 1;

        private static readonly NumericModifierInfo DefaultInfo = new(Array.Empty<string>(), DefaultName, DefaultCount);

        private readonly NumericModifierInfo info;

        public NumericModifierInfo Info => info ?? DefaultInfo;

        public virtual Func<Numeric, int> Apply(int source) { return _ => source; }

        #region 构造函数

        protected NumericModifier() { info = DefaultInfo; }

        protected NumericModifier(NumericModifierInfo newInfo) { info = newInfo; }

        #endregion
    }

    [ShowInInspector]
    public sealed class AdditionNumericModifier : NumericModifier
    {
        public readonly int StoreValue;

        public override Func<Numeric, int> Apply(int source) { return _ => source + StoreValue * Info.Count; }

        #region 构造函数和隐式转换

        public AdditionNumericModifier(int   value) { StoreValue = value; }
        public AdditionNumericModifier(float value) { StoreValue = value.ToFixedPoint(); }

        public AdditionNumericModifier(int value, string[] tags, string name, int count = 1) : base(new NumericModifierInfo(tags, name, count))
        {
            StoreValue = value;
        }

        public AdditionNumericModifier(float value, string[] tags, string name, int count = 1) : base(new NumericModifierInfo(tags, name, count))
        {
            StoreValue = value.ToFixedPoint();
        }

        public static implicit operator AdditionNumericModifier(int value) => new(value);

        public static implicit operator AdditionNumericModifier(float value) => new(value);

        public static implicit operator AdditionNumericModifier((int value, string[] tags, string name, int count) tuple)
            => new(tuple.value, tuple.tags, tuple.name, tuple.count);

        public static implicit operator AdditionNumericModifier((float value, string[] tags, string name, int count) tuple)
            => new(tuple.value, tuple.tags, tuple.name, tuple.count);

        #endregion
    }

    [ShowInInspector]
    public sealed class FractionNumericModifier : NumericModifier
    {
        public enum FractionType
        {
            Override, // 覆盖
            Increase, // 增量
        }

        [ShowInInspector] private readonly int numerator;   // 分子
        [ShowInInspector] private readonly int denominator; // 分母

        private readonly FractionType type;

        #region 构造函数和隐式转换

        public FractionNumericModifier(int numerator, int denominator, FractionType type)
        {
            this.numerator   = numerator;
            this.denominator = denominator;
            this.type        = type;
        }

        public FractionNumericModifier(int precent, FractionType type) : this(precent, 100, type) { }

        public FractionNumericModifier(
            int          numerator,
            int          denominator,
            FractionType type,
            string[]     tags,
            string       name,
            int          count = 1) : base(new NumericModifierInfo(tags, name, count))
        {
            this.numerator   = numerator;
            this.denominator = denominator;
            this.type        = type;
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

        public override Func<Numeric, int> Apply(int source) => numeric =>
        {
            var targetAddModfierValue   = numeric.GetAddModfierValueByTag(Info.Tags);
            var noTargetAddModiferValue = numeric.GetAddModfierValue() - targetAddModfierValue;
            return type switch
            {
                FractionType.Increase => GetIncrease(targetAddModfierValue + GetOrigin(numeric)) + noTargetAddModiferValue,
                FractionType.Override => GetOverride(targetAddModfierValue + GetOrigin(numeric)) + noTargetAddModiferValue,
                _                     => throw new ArgumentOutOfRangeException()
            };
        };

        int GetOrigin(Numeric numeric) => Info.Tags.Contains(TagSelf) ? numeric.GetOriginValue() : 0;
        int GetIncrease(int   value)   => (int)(value * (1 + numerator * Info.Count / (float)denominator));
        int GetOverride(int   value)   => (int)(value * Mathf.Pow(numerator / (float)denominator, p: Info.Count));
    }

    [ShowInInspector]
    public sealed class CustomNumericModifier : NumericModifier
    {
        private readonly Func<int, int>     intFunc;
        private readonly Func<float, float> floatFunc;


        public CustomNumericModifier(Func<int, int>     intFunc) { this.intFunc     = intFunc; }
        public CustomNumericModifier(Func<float, float> floatFunc) { this.floatFunc = floatFunc; }

        public static implicit operator CustomNumericModifier(Func<int, int>     intFunc)   => new(intFunc);
        public static implicit operator CustomNumericModifier(Func<float, float> floatFunc) => new(floatFunc);

        public override Func<Numeric, int> Apply(int source)
        {
            return _ => intFunc?.Invoke(source)
                     ?? floatFunc.Invoke(source.ToFloat())
                                 .ToFixedPoint();
        }
    }
}