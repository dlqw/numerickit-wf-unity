using System;
using Sirenix.OdinInspector;

namespace WFramework.CoreGameDevKit.OldNumericSystem
{
    [Serializable]
    [Obsolete("Obsolete")]
    public class FixedOldNumericModifier : AdditionOldNumericModifier, IDescription
    {
        private const    uint  Factor = 10000;
        private readonly float value;

        public FixedOldNumericModifier(float value, string name = "", params string[] tags)
        {
            this.value = value;
            this.name  = name;
            this.tags  = tags;
        }

        public FixedOldNumericModifier() { }

        private                  int    FixedValue     => (int)(value * Factor);
        [ShowInInspector] public string DescribeString => $"定点修正器:{FixedValue / (float)Factor}";

        public static implicit operator FixedOldNumericModifier(float value) { return new FixedOldNumericModifier(value); }

        public override void ApplyModifier(ref float input, float source, OldNumeric oldNumeric)
        {
            var fixedInput = (int)(input * Factor);
            fixedInput += FixedValue * count;
            input      =  fixedInput / (float)Factor;
        }

        public override bool WeakEquals(OldNumericModifier other)
        {
            return other is FixedOldNumericModifier fixedNumericModifier
                && FixedValue == fixedNumericModifier.FixedValue
                && name == fixedNumericModifier.name;
        }
    }
}