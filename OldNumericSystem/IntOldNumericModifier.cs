using System;
using Sirenix.OdinInspector;

namespace WFramework.CoreGameDevKit.OldNumericSystem
{
    [Serializable]
    [Obsolete("Obsolete")]
    public class IntOldNumericModifier : AdditionOldNumericModifier, IDescription
    {
        public int value;

        public IntOldNumericModifier() { }

        public IntOldNumericModifier(int value)
        {
            this.value = value;
            name       = "";
            tags       = Array.Empty<string>();
        }

        public IntOldNumericModifier(int value, string name)
        {
            this.value = value;
            this.name  = name;
            tags       = Array.Empty<string>();
        }

        public IntOldNumericModifier(int value, string name = "", params string[] inputTags)
        {
            this.value = value;
            this.name  = name;
            tags       = inputTags;
        }

        [ShowInInspector] public string DescribeString => $"整型修正器:{value}";

        public static implicit operator IntOldNumericModifier(int value) => new(value);

        public static implicit operator IntOldNumericModifier((int, string) value) => new(value.Item1, value.Item2);

        public static implicit operator IntOldNumericModifier((int, string, string) value)
        {
            return new IntOldNumericModifier(value.Item1, value.Item2, value.Item3);
        }

        public override void ApplyModifier(ref int input, int source, OldNumeric oldNumeric) { input += value * count; }

        public override bool WeakEquals(OldNumericModifier other)
        {
            return other is IntOldNumericModifier intNumericModifier
                && value == intNumericModifier.value
                && name == intNumericModifier.name;
        }
    }
}