using System;
using Sirenix.OdinInspector;

namespace WFramework.CoreGameDevKit.OldNumericSystem
{
    [Serializable]
    [Obsolete("Obsolete")]
    public class FloatOldNumericModifier : AdditionOldNumericModifier, IDescription
    {
        public readonly float Value;

        public FloatOldNumericModifier(float value, string name = "", params string[] tags)
        {
            Value     = value;
            this.name = name;
            this.tags = tags;
        }

        public FloatOldNumericModifier() { }

        [ShowInInspector] public string DescribeString => $"浮点修正器:{Value}";

        public static implicit operator FloatOldNumericModifier(float value) { return new FloatOldNumericModifier(value); }

        public override void ApplyModifier(ref float input, float source, OldNumeric oldNumeric) { input += Value * count; }

        public override bool WeakEquals(OldNumericModifier other)
        {
            return other is FloatOldNumericModifier floatNumericModifier
                && Value.Equals(floatNumericModifier.Value)
                && name == floatNumericModifier.name;
        }
    }
}