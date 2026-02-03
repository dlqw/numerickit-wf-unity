using System;
using static WFramework.CoreGameDevKit.NumericSystem.NumericModifierConfig;

namespace WFramework.CoreGameDevKit.NumericSystem
{
    public sealed class AdditionNumericModifier : INumericModifier
    {
        public readonly int StoreValue;

        ModifierType INumericModifier.Type => ModifierType.Add;

        public NumericModifierInfo Info { get; }

        public Func<Numeric, int> Apply(int source) { return _ => source + StoreValue * Info.Count; }

        #region 构造函数和隐式转换

        public AdditionNumericModifier(int value)
        {
            StoreValue = value;
            Info       = DefaultInfo;
        }

        public AdditionNumericModifier(float value)
        {
            NumericValidator.ValidateFloat(value);
            StoreValue = value.ToFixedPoint();
            Info       = DefaultInfo;
        }

        public AdditionNumericModifier(int value, string[] tags, string name, int count = 1)
        {
            StoreValue = value;
            Info       = new NumericModifierInfo(tags, name, count);
        }

        public AdditionNumericModifier(float value, string[] tags, string name, int count = 1)
        {
            NumericValidator.ValidateFloat(value);
            StoreValue = value.ToFixedPoint();
            Info       = new NumericModifierInfo(tags, name, count);
        }

        public static implicit operator AdditionNumericModifier(int value) => new(value);

        public static implicit operator AdditionNumericModifier(float value) => new(value);

        public static implicit operator AdditionNumericModifier((int value, string[] tags, string name, int count) tuple)
            => new(tuple.value, tuple.tags, tuple.name, tuple.count);

        public static implicit operator AdditionNumericModifier((float value, string[] tags, string name, int count) tuple)
            => new(tuple.value, tuple.tags, tuple.name, tuple.count);

        #endregion
    }
}