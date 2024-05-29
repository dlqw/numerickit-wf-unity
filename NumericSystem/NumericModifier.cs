using System;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Framework.NumericSystem
{
    [Serializable]
    public abstract class NumericModifier
    {
        public string[] tags = Array.Empty<string>();
        public string name = "";
        public int count = 1;

        public virtual void ApplyModifier(ref int input, int source, Numeric numeric)
        {
        }

        public virtual void ApplyModifier(ref float input, float source, Numeric numeric)
        {
        }

        public abstract bool WeakEquals(NumericModifier other);
    }

    [Serializable]
    public abstract class AdditionNumericModifier : NumericModifier
    {
    }

    [Serializable]
    public class GenericNumericModifier<T> : AdditionNumericModifier, IDescription where T : CustomDataStructure, new()
    {
        public readonly T Value;
        [ShowInInspector] public string DescribeString => $"泛型修正器:{Value}";

        public GenericNumericModifier(T value, string name = "", params string[] tags)
        {
            Value = value;
            this.name = name;
            this.tags = tags;
        }

        public GenericNumericModifier()
        {
        }

        public static implicit operator GenericNumericModifier<T>(T value)
        {
            return new GenericNumericModifier<T>(value);
        }

        public void ApplyModifier(ref T input)
        {
            var a = input as CustomDataStructure;
            var b = Value as CustomDataStructure;
            var result = a;
            for (int i = 0; i < count - 1; i++)
                result += b;
            input = (T)result;
        }

        public override bool WeakEquals(NumericModifier other)
        {
            return other is GenericNumericModifier<T> genericNumericModifier &&
                   Value.Equals(genericNumericModifier.Value) && name == genericNumericModifier.name;
        }
    }

    [Serializable]
    public class IntNumericModifier : AdditionNumericModifier, IDescription
    {
        public readonly int Value;
        [ShowInInspector] public string DescribeString => $"整型修正器:{Value}";

        public IntNumericModifier()
        {
        }

        public IntNumericModifier(int value)
        {
            Value = value;
            name = "";
            tags = Array.Empty<string>();
        }

        public IntNumericModifier(int value, string name)
        {
            Value = value;
            this.name = name;
            tags = Array.Empty<string>();
        }

        public IntNumericModifier(int value, string name = "", params string[] inputTags)
        {
            Value = value;
            this.name = name;
            tags = inputTags;
        }

        public static implicit operator IntNumericModifier(int value)
        {
            return new IntNumericModifier(value);
        }

        public static implicit operator IntNumericModifier((int, string) value)
        {
            return new IntNumericModifier(value.Item1, value.Item2);
        }

        public static implicit operator IntNumericModifier((int, string, string) value)
        {
            return new IntNumericModifier(value.Item1, value.Item2, value.Item3);
        }

        public override void ApplyModifier(ref int input, int source, Numeric numeric)
        {
            input += Value * count;
        }

        public override bool WeakEquals(NumericModifier other)
        {
            return other is IntNumericModifier intNumericModifier && Value == intNumericModifier.Value &&
                   name == intNumericModifier.name;
        }
    }

    [Serializable]
    public class FloatNumericModifier : AdditionNumericModifier, IDescription
    {
        public readonly float Value;
        [ShowInInspector] public string DescribeString => $"浮点修正器:{Value}";

        public FloatNumericModifier(float value, string name = "", params string[] tags)
        {
            Value = value;
            this.name = name;
            this.tags = tags;
        }

        public FloatNumericModifier()
        {
        }

        public static implicit operator FloatNumericModifier(float value)
        {
            return new FloatNumericModifier(value);
        }

        public override void ApplyModifier(ref float input, float source, Numeric numeric)
        {
            input += Value * count;
        }

        public override bool WeakEquals(NumericModifier other)
        {
            return other is FloatNumericModifier floatNumericModifier && Value.Equals(floatNumericModifier.Value) &&
                   name == floatNumericModifier.name;
        }
    }

    [Serializable]
    public class FixedNumericModifier : AdditionNumericModifier, IDescription
    {
        private readonly float value;
        private const uint Factor = 10000;
        private int FixedValue => (int)(value * Factor);
        [ShowInInspector] public string DescribeString => $"定点修正器:{FixedValue / (float)Factor}";

        public FixedNumericModifier(float value, string name = "", params string[] tags)
        {
            this.value = value;
            this.name = name;
            this.tags = tags;
        }

        public FixedNumericModifier()
        {
        }

        public static implicit operator FixedNumericModifier(float value)
        {
            return new FixedNumericModifier(value);
        }

        public override void ApplyModifier(ref float input, float source, Numeric numeric)
        {
            var fixedInput = (int)(input * Factor);
            fixedInput += FixedValue * count;
            input = fixedInput / (float)Factor;
        }

        public override bool WeakEquals(NumericModifier other)
        {
            return other is FixedNumericModifier fixedNumericModifier &&
                   FixedValue == fixedNumericModifier.FixedValue && name == fixedNumericModifier.name;
        }
    }

    #region 乘法修正器

    [Serializable]
    public abstract class FractionNumericModifier : NumericModifier
    {
        protected readonly int Denominator;
        protected readonly int Numerator;

        protected FractionNumericModifier(int numerator, int denominator, string name = "", params string[] tags)
        {
            Numerator = numerator;
            Denominator = denominator;
            this.name = name;
            this.tags = tags;
        }

        protected FractionNumericModifier()
        {
        }
    }

    [Serializable]
    public class OverrideFractionNumericModifier : FractionNumericModifier, IDescription
    {
        public string DescribeString => $"覆盖分数修正器:{Numerator}/{Denominator}";

        public OverrideFractionNumericModifier(int numerator, int denominator, string name = "", params string[] tag) :
            base(numerator,
                denominator, name, tag)
        {
        }

        public static implicit operator OverrideFractionNumericModifier(
            (int numerator, int denominator) value)
        {
            return new OverrideFractionNumericModifier(value.numerator, value.denominator);
        }

        public static implicit operator OverrideFractionNumericModifier(
            (int numerator, int denominator, string name) value)
        {
            return new OverrideFractionNumericModifier(value.numerator, value.denominator, value.name);
        }

        public static implicit operator OverrideFractionNumericModifier(
            (int numerator, int denominator, string name, string[] tag) value)
        {
            return new OverrideFractionNumericModifier(value.numerator, value.denominator, value.name, value.tag);
        }

        public override void ApplyModifier(ref float input, float source, Numeric numeric)
        {
            var result = 0f;
            var factor = Numerator / (float)Denominator - 1;
            if (tags.Length > 0) //如果有标签
            {
                foreach (var modifier in numeric.ModifierCollector) //遍历所有的修正器
                {
                    if (modifier is not FloatNumericModifier floatNumericModifier) continue; //如果不是浮点修正器,跳过
                    var exist = false;
                    if (floatNumericModifier.tags.Length > 0)
                    {
                        if (floatNumericModifier.tags.Any(tag => tags.Contains(tag))) //如果有相同的标签
                        {
                            exist = true;
                        }
                    }

                    if (exist)
                    {
                        result += floatNumericModifier.Value * floatNumericModifier.count * factor; //计算结果
                    }
                }
            }

            if (tags.Contains("self")) result += source * factor;

            var absResult = Mathf.Abs(result);
            var originalResult = Mathf.Abs(result);
            int isNegative = result < 0 ? -1 : 1;

            for (int i = 0; i < count - 1; i++)
            {
                absResult += originalResult * Mathf.Abs(Mathf.Pow(factor, i + 1));
            }

            input += absResult * isNegative;
        }

        public override void ApplyModifier(ref int input, int source, Numeric numeric)
        {
            var result = 0f;
            var factor = Numerator / (float)Denominator - 1;

            if (tags.Length > 0)
            {
                foreach (var modifier in numeric.ModifierCollector)
                {
                    if (modifier is not IntNumericModifier intNumericModifier) continue;
                    var exist = false;
                    if (intNumericModifier.tags.Length > 0)
                    {
                        if (intNumericModifier.tags.Any(tag => tags.Contains(tag)))
                        {
                            exist = true;
                        }
                    }

                    if (exist)
                    {
                        result += intNumericModifier.Value * intNumericModifier.count * factor;
                    }
                }
            }

            if (tags.Contains("self")) result += source * factor;

            var absResult = Mathf.Abs(result);
            var originalResult = Mathf.Abs(result);
            int isNegative = result < 0 ? -1 : 1;

            for (int i = 0; i < count - 1; i++)
            {
                absResult += originalResult * Mathf.Abs(Mathf.Pow(factor, i + 1));
            }

            input += (int)(absResult * isNegative);
        }

        public override bool WeakEquals(NumericModifier other)
        {
            return other is OverrideFractionNumericModifier overrideFractionNumericModifier &&
                   Numerator == overrideFractionNumericModifier.Numerator &&
                   Denominator == overrideFractionNumericModifier.Denominator &&
                   name == overrideFractionNumericModifier.name;
        }
    }

    [Serializable]
    public class IncreaseFractionNumericModifier : FractionNumericModifier, IDescription
    {
        public string DescribeString => $"增量分数修正器:{Numerator}/{Denominator}";

        public IncreaseFractionNumericModifier(int numerator, int denominator, string name = "", params string[] tag) :
            base(numerator,
                denominator, name, tag)
        {
        }

        public static implicit operator IncreaseFractionNumericModifier((int numerator, int denominator) value)
        {
            return new IncreaseFractionNumericModifier(value.numerator, value.denominator);
        }

        public static implicit operator IncreaseFractionNumericModifier(
            (int numerator, int denominator, string name) value)
        {
            return new IncreaseFractionNumericModifier(value.numerator, value.denominator, value.name);
        }

        public static implicit operator IncreaseFractionNumericModifier(
            (int numerator, int denominator, string name, string[] tag) value)
        {
            return new IncreaseFractionNumericModifier(value.numerator, value.denominator, value.name, value.tag);
        }

        public override void ApplyModifier(ref float input, float source, Numeric numeric)
        {
            var result = 0f;
            var factor = Numerator / (float)Denominator;

            if (tags.Length > 0)
            {
                foreach (var modifier in numeric.ModifierCollector)
                {
                    if (modifier is not FloatNumericModifier floatNumericModifier) continue;
                    var exist = false;
                    if (floatNumericModifier.tags.Length > 0)
                    {
                        if (floatNumericModifier.tags.Any(tag => tags.Contains(tag)))
                        {
                            exist = true;
                        }
                    }

                    if (exist)
                    {
                        result += floatNumericModifier.Value * floatNumericModifier.count * factor;
                    }
                }
            }

            if (tags.Contains("self")) result += source * factor;

            result *= count;

            input += result;
        }

        public override void ApplyModifier(ref int input, int source, Numeric numeric)
        {
            var result = 0;
            var factor = Numerator / (float)Denominator;

            if (tags.Length > 0)
            {
                foreach (var modifier in numeric.ModifierCollector)
                {
                    if (modifier is not IntNumericModifier intNumericModifier) continue;
                    var exist = false;
                    if (intNumericModifier.tags.Length > 0)
                    {
                        if (intNumericModifier.tags.Any(tag => tags.Contains(tag)))
                        {
                            exist = true;
                        }
                    }

                    if (exist)
                    {
                        result += (int)(intNumericModifier.Value * intNumericModifier.count * factor);
                    }
                }
            }

            if (tags.Contains("self")) result += (int)(source * factor);

            result *= count;

            input += result;
        }

        public override bool WeakEquals(NumericModifier other)
        {
            return other is IncreaseFractionNumericModifier increaseFractionNumericModifier &&
                   Numerator == increaseFractionNumericModifier.Numerator &&
                   Denominator == increaseFractionNumericModifier.Denominator &&
                   name == increaseFractionNumericModifier.name;
        }
    }

    #endregion
}