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
            // 统一使用定点数表示，确保与Numeric内部表示一致
            StoreValue = value.ToFixedPoint();
            Info       = DefaultInfo;
        }

        public AdditionNumericModifier(float value)
        {
            FixedPoint.ValidateFloat(value);
            StoreValue = value.ToFixedPoint();
            Info       = DefaultInfo;
        }

        public AdditionNumericModifier(int value, string[] tags, string name, int count = 1)
        {
            // 统一使用定点数表示，确保与Numeric内部表示一致
            StoreValue = value.ToFixedPoint();
            Info       = new NumericModifierInfo(tags, name, count);
        }

        public AdditionNumericModifier(float value, string[] tags, string name, int count = 1)
        {
            FixedPoint.ValidateFloat(value);
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