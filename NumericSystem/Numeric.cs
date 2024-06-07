using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Framework.Utility.NumericSystem
{
    public class Numeric
    {
        [ShowInInspector] public LinkedList<NumericModifier> ModifierCollector = new();
        protected bool HasUpdate;

        protected virtual void AddModifier(NumericModifier modifier)
        {
            ModifierCollector ??= new LinkedList<NumericModifier>();
            var isExist = false;
            foreach (var target in ModifierCollector.Where(target => target.WeakEquals(modifier)))
            {
                target.count += modifier.count;
                target.tags = modifier.tags;
                isExist = true;
                break;
            }

            if (!isExist) ModifierCollector.AddLast(modifier);
            HasUpdate = true;
            UpdateFinalValue();
        }

        protected virtual void RemoveModifier(NumericModifier modifier)
        {
            if (!ModifierCollector.Remove(modifier))
            {
                foreach (var numericModifier in ModifierCollector.Where(numericModifier =>
                             numericModifier.WeakEquals(modifier)))
                {
                    numericModifier.count -= 1;
                    if (numericModifier.count <= 0) ModifierCollector.Remove(numericModifier);
                    break;
                }
            }

            HasUpdate = true;
            UpdateFinalValue();
        }

        /// <summary>
        /// 清除所有暂时的数值变化
        /// </summary>
        public void ClearModifier()
        {
            ModifierCollector.Clear();
            HasUpdate = true;
            UpdateFinalValue();
        }

        /// <summary>
        /// 清除所有包含该 Tag 的暂时的数值变化
        /// </summary>
        /// <param name="tag"></param>
        public void ClearModifier(string tag)
        {
            foreach (var modifier in ModifierCollector.Where(modifier => modifier.tags.Contains(tag)))
            {
                ModifierCollector.Remove(modifier);
            }

            HasUpdate = true;
            UpdateFinalValue();
        }

        public static Numeric operator +(Numeric numeric, NumericModifier modifier)
        {
            numeric.AddModifier(modifier);
            return numeric;
        }

        public static Numeric operator -(Numeric numeric, NumericModifier modifier)
        {
            numeric.RemoveModifier(modifier);
            return numeric;
        }

        public static Numeric operator *(Numeric numeric, FractionNumericModifier modifier)
        {
            numeric.AddModifier(modifier);
            return numeric;
        }

        public static Numeric operator /(Numeric numeric, FractionNumericModifier modifier)
        {
            numeric.RemoveModifier(modifier);
            return numeric;
        }

        protected virtual void UpdateFinalValue()
        {
        }
    }

    [Serializable]
    public class IntNumeric : Numeric
    {
        [SerializeField] private int basicValue;

        public int BasicValue
        {
            get => basicValue;
            set
            {
                basicValue = value;
                HasUpdate = true;
            }
        }

        [SerializeField] private int finalValue;
        private int lastFinalValue;

        public int FinalValue
        {
            get
            {
                UpdateFinalValue();
                return finalValue;
            }
        }

        public IntNumeric(int basicValue)
        {
            this.basicValue = basicValue;
            finalValue = basicValue;
            lastFinalValue = basicValue;
        }

        public IntNumeric()
        {
        }

        public static implicit operator IntNumeric(int value)
        {
            return new IntNumeric(value);
        }

        public static IntNumeric operator +(IntNumeric numeric, IntNumericModifier modifier)
        {
            numeric.AddModifier(modifier);
            return numeric;
        }

        public static IntNumeric operator +(IntNumeric numeric, int value)
        {
            numeric.AddModifier(new IntNumericModifier(value));
            return numeric;
        }

        public static IntNumeric operator +(IntNumeric numeric, (int, string) value)
        {
            numeric.AddModifier(new IntNumericModifier(value.Item1, value.Item2));
            return numeric;
        }

        public static IntNumeric operator +(IntNumeric numeric, (int, string, string[]) value)
        {
            numeric.AddModifier(new IntNumericModifier(value.Item1, value.Item2, value.Item3));
            return numeric;
        }

        public static IntNumeric operator -(IntNumeric numeric, IntNumericModifier modifier)
        {
            numeric.RemoveModifier(modifier);
            return numeric;
        }

        public static IntNumeric operator -(IntNumeric numeric, int value)
        {
            numeric.RemoveModifier(new IntNumericModifier(value));
            return numeric;
        }

        public static IntNumeric operator -(IntNumeric numeric, (int, string) value)
        {
            numeric.RemoveModifier(new IntNumericModifier(value.Item1, value.Item2));
            return numeric;
        }

        public static IntNumeric operator -(IntNumeric numeric, (int, string, string[]) value)
        {
            numeric.RemoveModifier(new IntNumericModifier(value.Item1, value.Item2, value.Item3));
            return numeric;
        }

        public static IntNumeric operator *(IntNumeric numeric, FractionNumericModifier modifier)
        {
            numeric.AddModifier(modifier);
            return numeric;
        }

        public static IntNumeric operator *(IntNumeric numeric, (int, int) value)
        {
            numeric.AddModifier(new OverrideFractionNumericModifier(value.Item1, value.Item2));
            return numeric;
        }

        public static IntNumeric operator /(IntNumeric numeric, FractionNumericModifier modifier)
        {
            numeric.RemoveModifier(modifier);
            return numeric;
        }

        public static IntNumeric operator /(IntNumeric numeric, (int, int) value)
        {
            numeric.RemoveModifier(new OverrideFractionNumericModifier(value.Item1, value.Item2));
            return numeric;
        }

        protected override void UpdateFinalValue()
        {
            if (!HasUpdate)
            {
                finalValue = lastFinalValue;
                return;
            }

            finalValue = basicValue;
            foreach (var modifier in ModifierCollector)
            {
                modifier.ApplyModifier(ref finalValue, basicValue, this);
            }

            lastFinalValue = finalValue;
            HasUpdate = false;
        }
    }

    [Serializable]
    public class FloatNumeric : Numeric
    {
        [SerializeField] private float basicValue;

        public float BasicValue
        {
            get => basicValue;
            set
            {
                basicValue = value;
                HasUpdate = true;
            }
        }

        [SerializeField] private float finalValue;
        private float lastFinalValue;

        public float FinalValue
        {
            get
            {
                UpdateFinalValue();
                return finalValue;
            }
        }

        public FloatNumeric(float basicValue)
        {
            this.basicValue = basicValue;
            finalValue = basicValue;
            lastFinalValue = basicValue;
        }

        public FloatNumeric()
        {
        }

        public static implicit operator FloatNumeric(float value)
        {
            return new FloatNumeric(value);
        }

        public static FloatNumeric operator +(FloatNumeric numeric, FloatNumericModifier modifier)
        {
            numeric.AddModifier(modifier);
            return numeric;
        }

        public static FloatNumeric operator +(FloatNumeric numeric, float value)
        {
            numeric.AddModifier(new FloatNumericModifier(value));
            return numeric;
        }

        public static FloatNumeric operator +(FloatNumeric numeric, (float, string) value)
        {
            numeric.AddModifier(new FloatNumericModifier(value.Item1, value.Item2));
            return numeric;
        }

        public static FloatNumeric operator +(FloatNumeric numeric, (float, string, string) value)
        {
            numeric.AddModifier(new FloatNumericModifier(value.Item1, value.Item2, value.Item3));
            return numeric;
        }

        public static FloatNumeric operator -(FloatNumeric numeric, FloatNumericModifier modifier)
        {
            numeric.RemoveModifier(modifier);
            return numeric;
        }

        public static FloatNumeric operator -(FloatNumeric numeric, float value)
        {
            numeric.RemoveModifier(new FloatNumericModifier(value));
            return numeric;
        }

        public static FloatNumeric operator -(FloatNumeric numeric, (float, string) value)
        {
            numeric.RemoveModifier(new FloatNumericModifier(value.Item1, value.Item2));
            return numeric;
        }

        public static FloatNumeric operator -(FloatNumeric numeric, (float, string, string) value)
        {
            numeric.RemoveModifier(new FloatNumericModifier(value.Item1, value.Item2, value.Item3));
            return numeric;
        }

        public static FloatNumeric operator *(FloatNumeric numeric, FractionNumericModifier modifier)
        {
            numeric.AddModifier(modifier);
            return numeric;
        }

        public static FloatNumeric operator *(FloatNumeric numeric, (int, int) value)
        {
            numeric.AddModifier(new OverrideFractionNumericModifier(value.Item1, value.Item2));
            return numeric;
        }

        public static FloatNumeric operator /(FloatNumeric numeric, FractionNumericModifier modifier)
        {
            numeric.RemoveModifier(modifier);
            return numeric;
        }

        public static FloatNumeric operator /(FloatNumeric numeric, (int, int) value)
        {
            numeric.RemoveModifier(new OverrideFractionNumericModifier(value.Item1, value.Item2));
            return numeric;
        }

        protected override void UpdateFinalValue()
        {
            if (!HasUpdate)
            {
                finalValue = lastFinalValue;
                return;
            }

            finalValue = basicValue;
            foreach (var modifier in ModifierCollector)
            {
                modifier.ApplyModifier(ref finalValue, basicValue, this);
            }

            lastFinalValue = finalValue;
            HasUpdate = false;
        }
    }

    [Serializable]
    public class GenericNumeric<T> : Numeric where T : CustomDataStructure, new()
    {
        [SerializeField] private T basicValue;

        public T BasicValue
        {
            get => basicValue;
            set
            {
                basicValue = value;
                HasUpdate = true;
            }
        }

        [SerializeField] private T finalValue;
        private T lastFinalValue;

        public T FinalValue
        {
            get
            {
                UpdateFinalValue();
                return finalValue;
            }
        }

        public GenericNumeric(T basicValue)
        {
            this.basicValue = basicValue;
            finalValue = basicValue;
            lastFinalValue = basicValue;
        }

        public GenericNumeric()
        {
        }

        public static implicit operator GenericNumeric<T>(T value)
        {
            return new GenericNumeric<T>(value);
        }

        public static GenericNumeric<T> operator +(GenericNumeric<T> numeric, GenericNumericModifier<T> modifier)
        {
            numeric.AddModifier(modifier);
            return numeric;
        }

        public static GenericNumeric<T> operator +(GenericNumeric<T> numeric, T value)
        {
            numeric.AddModifier(new GenericNumericModifier<T>(value));
            return numeric;
        }

        public static GenericNumeric<T> operator +(GenericNumeric<T> numeric, (T, string) value)
        {
            numeric.AddModifier(new GenericNumericModifier<T>(value.Item1, value.Item2));
            return numeric;
        }

        public static GenericNumeric<T> operator +(GenericNumeric<T> numeric, (T, string, string[]) value)
        {
            numeric.AddModifier(new GenericNumericModifier<T>(value.Item1, value.Item2, value.Item3));
            return numeric;
        }

        public static GenericNumeric<T> operator -(GenericNumeric<T> numeric, GenericNumericModifier<T> modifier)
        {
            numeric.RemoveModifier(modifier);
            return numeric;
        }

        public static GenericNumeric<T> operator -(GenericNumeric<T> numeric, T value)
        {
            numeric.RemoveModifier(new GenericNumericModifier<T>(value));
            return numeric;
        }

        public static GenericNumeric<T> operator -(GenericNumeric<T> numeric, (T, string) value)
        {
            numeric.RemoveModifier(new GenericNumericModifier<T>(value.Item1, value.Item2));
            return numeric;
        }

        public static GenericNumeric<T> operator -(GenericNumeric<T> numeric, (T, string, string[]) value)
        {
            numeric.RemoveModifier(new GenericNumericModifier<T>(value.Item1, value.Item2, value.Item3));
            return numeric;
        }

        public static GenericNumeric<T> operator *(GenericNumeric<T> numeric, FractionNumericModifier modifier)
        {
            numeric.AddModifier(modifier);
            return numeric;
        }

        public static GenericNumeric<T> operator *(GenericNumeric<T> numeric, (int, int) value)
        {
            numeric.AddModifier(new OverrideFractionNumericModifier(value.Item1, value.Item2));
            return numeric;
        }

        public static GenericNumeric<T> operator /(GenericNumeric<T> numeric, FractionNumericModifier modifier)
        {
            numeric.RemoveModifier(modifier);
            return numeric;
        }

        public static GenericNumeric<T> operator /(GenericNumeric<T> numeric, (int, int) value)
        {
            numeric.RemoveModifier(new OverrideFractionNumericModifier(value.Item1, value.Item2));
            return numeric;
        }

        protected override void UpdateFinalValue()
        {
            if (!HasUpdate)
            {
                finalValue = lastFinalValue;
                return;
            }

            finalValue = basicValue;
            foreach (var modifier in ModifierCollector)
            {
                (modifier as GenericNumericModifier<T>)?.ApplyModifier(ref finalValue);
            }

            lastFinalValue = finalValue;
            HasUpdate = false;
        }
    }
}