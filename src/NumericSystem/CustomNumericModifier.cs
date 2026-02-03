using System;

namespace WFramework.CoreGameDevKit.NumericSystem
{
    public sealed class CustomNumericModifier : INumericModifier
    {
        ModifierType INumericModifier.Type => ModifierType.Custom;
        public NumericModifierInfo    Info { get; }

        private readonly Func<int, int>     intFunc;
        private readonly Func<float, float> floatFunc;


        public CustomNumericModifier(Func<int, int> intFunc)
        {
            this.intFunc = intFunc;
            Info         = NumericModifierConfig.DefaultInfo;
        }

        public CustomNumericModifier(Func<float, float> floatFunc)
        {
            this.floatFunc = floatFunc;
            Info           = NumericModifierConfig.DefaultInfo;
        }

        public CustomNumericModifier(Func<int, int> intFunc, string[] tags, string name, int count = 1)
        {
            this.intFunc = intFunc;
            Info         = new NumericModifierInfo(tags, name, count);
        }

        public CustomNumericModifier(Func<float, float> floatFunc, string[] tags, string name, int count = 1)
        {
            this.floatFunc = floatFunc;
            Info           = new NumericModifierInfo(tags, name, count);
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
                    return intFunc.Invoke(source);

                if (floatFunc != null)
                    return floatFunc.Invoke(source.ToFloat()).ToFixedPoint();

                throw new InvalidOperationException(
                    $"{nameof(CustomNumericModifier)} must have either intFunc or floatFunc configured. " +
                    "Both functions are null. This may occur if the modifier was created through " +
                    "deserialization without proper initialization.");
            };
        }
    }
}