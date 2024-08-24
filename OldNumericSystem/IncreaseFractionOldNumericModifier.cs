using System;
using System.Linq;

namespace WFramework.CoreGameDevKit.OldNumericSystem
{
    [Serializable]
    [Obsolete("Obsolete")]
    public class IncreaseFractionOldNumericModifier : FractionOldNumericModifier, IDescription
    {
        /// <summary>
        /// </summary>
        /// <param name="numerator">分子</param>
        /// <param name="denominator">分母</param>
        /// <param name="name"></param>
        /// <param name="tag"></param>
        public IncreaseFractionOldNumericModifier(int numerator, int denominator, string name = "", params string[] tag) :
            base
            (
                numerator,
                denominator, name, tag
            )
        {
        }

        public string DescribeString => $"增量分数修正器:{Numerator}/{Denominator}";

        public static implicit operator IncreaseFractionOldNumericModifier((int numerator, int denominator) value)
        {
            return new IncreaseFractionOldNumericModifier(value.numerator, value.denominator);
        }

        public static implicit operator IncreaseFractionOldNumericModifier((int numerator, int denominator, string name) value)
        {
            return new IncreaseFractionOldNumericModifier(value.numerator, value.denominator, value.name);
        }

        public static implicit operator IncreaseFractionOldNumericModifier(
            (int numerator, int denominator, string name, string[] tag) value)
        {
            return new IncreaseFractionOldNumericModifier(value.numerator, value.denominator, value.name, value.tag);
        }

        public override void ApplyModifier(ref float input, float source, OldNumeric oldNumeric)
        {
            var result = 0f;
            var factor = Numerator / (float)Denominator;

            if (tags.Length > 0)
                foreach (var modifier in oldNumeric.ModifierCollector)
                {
                    if (modifier is not FloatOldNumericModifier floatNumericModifier) continue;
                    var exist = false;
                    if (floatNumericModifier.tags.Length > 0)
                        if (floatNumericModifier.tags.Any(tag => tags.Contains(tag)))
                            exist = true;

                    if (exist) result += floatNumericModifier.Value * floatNumericModifier.count * factor;
                }

            if (tags.Contains("self")) result += source * factor;

            result *= count;

            input += result;
        }

        public override void ApplyModifier(ref int input, int source, OldNumeric oldNumeric)
        {
            var result = 0;
            var factor = Numerator / (float)Denominator;

            if (tags.Length > 0)
                foreach (var modifier in oldNumeric.ModifierCollector)
                {
                    if (modifier is not IntOldNumericModifier intNumericModifier) continue;
                    var exist = false;
                    if (intNumericModifier.tags.Length > 0)
                        if (intNumericModifier.tags.Any(tag => tags.Contains(tag)))
                            exist = true;

                    if (exist) result += (int)(intNumericModifier.value * intNumericModifier.count * factor);
                }

            if (tags.Contains("self")) result += (int)(source * factor);

            result *= count;

            input += result;
        }

        public override bool WeakEquals(OldNumericModifier other)
        {
            return other is IncreaseFractionOldNumericModifier increaseFractionNumericModifier
                && Numerator == increaseFractionNumericModifier.Numerator
                && Denominator == increaseFractionNumericModifier.Denominator
                && name == increaseFractionNumericModifier.name;
        }
    }
}