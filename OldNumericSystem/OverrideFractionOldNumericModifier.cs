using System;
using System.Linq;
using UnityEngine;

namespace WFramework.CoreGameDevKit.OldNumericSystem
{
    [Serializable]
    [Obsolete("Obsolete")]
    public class OverrideFractionOldNumericModifier : FractionOldNumericModifier, IDescription
    {
        /// <summary>
        /// </summary>
        /// <param name="numerator">分子</param>
        /// <param name="denominator">分母</param>
        /// <param name="name"></param>
        /// <param name="tag"></param>
        public OverrideFractionOldNumericModifier(int numerator, int denominator, string name = "", params string[] tag) :
            base
            (
                numerator,
                denominator, name, tag
            )
        {
        }

        public string DescribeString => $"覆盖分数修正器:{Numerator}/{Denominator}";

        public static implicit operator OverrideFractionOldNumericModifier((int numerator, int denominator) value)
        {
            return new OverrideFractionOldNumericModifier(value.numerator, value.denominator);
        }

        public static implicit operator OverrideFractionOldNumericModifier((int numerator, int denominator, string name) value)
        {
            return new OverrideFractionOldNumericModifier(value.numerator, value.denominator, value.name);
        }

        public static implicit operator OverrideFractionOldNumericModifier(
            (int numerator, int denominator, string name, string[] tag) value)
        {
            return new OverrideFractionOldNumericModifier(value.numerator, value.denominator, value.name, value.tag);
        }

        public override void ApplyModifier(ref float input, float source, OldNumeric oldNumeric)
        {
            var result = 0f;
            var factor = Numerator / (float)Denominator - 1;
            if (tags.Length > 0)                                    //如果有标签
                foreach (var modifier in oldNumeric.ModifierCollector) //遍历所有的修正器
                {
                    if (modifier is not FloatOldNumericModifier floatNumericModifier) continue; //如果不是浮点修正器,跳过
                    var exist = false;
                    if (floatNumericModifier.tags.Length > 0)
                        if (floatNumericModifier.tags.Any(tag => tags.Contains(tag))) //如果有相同的标签
                            exist = true;

                    if (exist) result += floatNumericModifier.Value * floatNumericModifier.count * factor; //计算结果
                }

            if (tags.Contains("self")) result += source * factor;

            var absResult      = Mathf.Abs(result);
            var originalResult = Mathf.Abs(result);
            var isNegative     = result < 0 ? -1 : 1;

            for (var i = 0; i < count - 1; i++) absResult += originalResult * Mathf.Abs(Mathf.Pow(factor, i + 1));

            input += absResult * isNegative;
        }

        public override void ApplyModifier(ref int input, int source, OldNumeric oldNumeric)
        {
            var result = 0f;
            var factor = Numerator / (float)Denominator - 1;

            if (tags.Length > 0)
                foreach (var modifier in oldNumeric.ModifierCollector)
                {
                    if (modifier is not IntOldNumericModifier intNumericModifier) continue;
                    var exist = false;
                    if (intNumericModifier.tags.Length > 0)
                        if (intNumericModifier.tags.Any(tag => tags.Contains(tag)))
                            exist = true;

                    if (exist) result += intNumericModifier.value * intNumericModifier.count * factor;
                }

            if (tags.Contains("self")) result += source * factor;

            var absResult      = Mathf.Abs(result);
            var originalResult = Mathf.Abs(result);
            var isNegative     = result < 0 ? -1 : 1;

            for (var i = 0; i < count - 1; i++) absResult += originalResult * Mathf.Abs(Mathf.Pow(factor, i + 1));

            input += (int)(absResult * isNegative);
        }

        public override bool WeakEquals(OldNumericModifier other)
        {
            return other is OverrideFractionOldNumericModifier overrideFractionNumericModifier
                && Numerator == overrideFractionNumericModifier.Numerator
                && Denominator == overrideFractionNumericModifier.Denominator
                && name == overrideFractionNumericModifier.name;
        }
    }
}