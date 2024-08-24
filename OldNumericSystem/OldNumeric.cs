using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace WFramework.CoreGameDevKit.OldNumericSystem
{
    [Obsolete]
    public class OldNumeric
    {
        protected bool HasUpdate;

        [ShowInInspector] public LinkedList<OldNumericModifier> ModifierCollector = new();


        protected virtual void AddModifier(OldNumericModifier modifier)
        {
            ModifierCollector ??= new LinkedList<OldNumericModifier>();
            var isExist = false;
            foreach (var target in ModifierCollector.Where(target => target.WeakEquals(modifier)))
            {
                target.count += modifier.count;
                target.tags  =  modifier.tags;
                isExist      =  true;
                break;
            }

            if (!isExist) ModifierCollector.AddLast(modifier);
            HasUpdate = true;
            UpdateFinalValue();
        }

        protected virtual void RemoveModifier(OldNumericModifier modifier)
        {
            if (!ModifierCollector.Remove(modifier))
                foreach (var numericModifier in ModifierCollector.Where
                         (
                             numericModifier =>
                                 numericModifier.WeakEquals(modifier)
                         ))
                {
                    numericModifier.count -= 1;
                    if (numericModifier.count <= 0) ModifierCollector.Remove(numericModifier);
                    break;
                }

            HasUpdate = true;
            UpdateFinalValue();
        }

        /// <summary>
        ///     清除所有暂时的数值变化
        /// </summary>
        public void ClearModifier()
        {
            ModifierCollector.Clear();
            HasUpdate = true;
            UpdateFinalValue();
        }

        /// <summary>
        ///     清除所有包含该 Tag 的暂时的数值变化
        /// </summary>
        /// <param name="tag"></param>
        public void ClearModifier(string tag)
        {
            foreach (var modifier in ModifierCollector.Where(modifier => modifier.tags.Contains(tag))) ModifierCollector.Remove(modifier);

            HasUpdate = true;
            UpdateFinalValue();
        }

        public static OldNumeric operator +(OldNumeric oldNumeric, OldNumericModifier modifier)
        {
            oldNumeric.AddModifier(modifier);
            return oldNumeric;
        }

        public static OldNumeric operator -(OldNumeric oldNumeric, OldNumericModifier modifier)
        {
            oldNumeric.RemoveModifier(modifier);
            return oldNumeric;
        }

        public static OldNumeric operator *(OldNumeric oldNumeric, FractionOldNumericModifier modifier)
        {
            oldNumeric.AddModifier(modifier);
            return oldNumeric;
        }

        public static OldNumeric operator /(OldNumeric oldNumeric, FractionOldNumericModifier modifier)
        {
            oldNumeric.RemoveModifier(modifier);
            return oldNumeric;
        }

        protected virtual void UpdateFinalValue() { }
    }

    [Serializable]
    public class IntOldNumeric : OldNumeric
    {
        [SerializeField] private int basicValue;

        [SerializeField] private int finalValue;
        private                  int lastFinalValue;

        public IntOldNumeric(int basicValue)
        {
            this.basicValue = basicValue;
            finalValue      = basicValue;
            lastFinalValue  = basicValue;
        }

        public IntOldNumeric() { }

        public int BasicValue
        {
            get => basicValue;
            set
            {
                basicValue = value;
                HasUpdate  = true;
            }
        }

        public int FinalValue
        {
            get
            {
                UpdateFinalValue();
                return finalValue;
            }
        }

        public static implicit operator IntOldNumeric(int value) { return new IntOldNumeric(value); }

        public static IntOldNumeric operator +(IntOldNumeric oldNumeric, IntOldNumericModifier modifier)
        {
            oldNumeric.AddModifier(modifier);
            return oldNumeric;
        }

        public static IntOldNumeric operator +(IntOldNumeric oldNumeric, int value)
        {
            oldNumeric.AddModifier(new IntOldNumericModifier(value));
            return oldNumeric;
        }

        public static IntOldNumeric operator +(IntOldNumeric oldNumeric, (int, string) value)
        {
            oldNumeric.AddModifier(new IntOldNumericModifier(value.Item1, value.Item2));
            return oldNumeric;
        }

        public static IntOldNumeric operator +(IntOldNumeric oldNumeric, (int, string, string[]) value)
        {
            oldNumeric.AddModifier(new IntOldNumericModifier(value.Item1, value.Item2, value.Item3));
            return oldNumeric;
        }

        public static IntOldNumeric operator -(IntOldNumeric oldNumeric, IntOldNumericModifier modifier)
        {
            oldNumeric.RemoveModifier(modifier);
            return oldNumeric;
        }

        public static IntOldNumeric operator -(IntOldNumeric oldNumeric, int value)
        {
            oldNumeric.RemoveModifier(new IntOldNumericModifier(value));
            return oldNumeric;
        }

        public static IntOldNumeric operator -(IntOldNumeric oldNumeric, (int, string) value)
        {
            oldNumeric.RemoveModifier(new IntOldNumericModifier(value.Item1, value.Item2));
            return oldNumeric;
        }

        public static IntOldNumeric operator -(IntOldNumeric oldNumeric, (int, string, string[]) value)
        {
            oldNumeric.RemoveModifier(new IntOldNumericModifier(value.Item1, value.Item2, value.Item3));
            return oldNumeric;
        }

        public static IntOldNumeric operator *(IntOldNumeric oldNumeric, FractionOldNumericModifier modifier)
        {
            oldNumeric.AddModifier(modifier);
            return oldNumeric;
        }

        public static IntOldNumeric operator *(IntOldNumeric oldNumeric, (int, int) value)
        {
            oldNumeric.AddModifier(new OverrideFractionOldNumericModifier(value.Item1, value.Item2));
            return oldNumeric;
        }

        public static IntOldNumeric operator /(IntOldNumeric oldNumeric, FractionOldNumericModifier modifier)
        {
            oldNumeric.RemoveModifier(modifier);
            return oldNumeric;
        }

        public static IntOldNumeric operator /(IntOldNumeric oldNumeric, (int, int) value)
        {
            oldNumeric.RemoveModifier(new OverrideFractionOldNumericModifier(value.Item1, value.Item2));
            return oldNumeric;
        }

        protected override void UpdateFinalValue()
        {
            if (!HasUpdate)
            {
                finalValue = lastFinalValue;
                return;
            }

            finalValue = basicValue;
            foreach (var modifier in ModifierCollector) modifier.ApplyModifier(ref finalValue, basicValue, this);

            lastFinalValue = finalValue;
            HasUpdate      = false;
        }
    }

    [Serializable]
    public class FloatOldNumeric : OldNumeric
    {
        [SerializeField] private float basicValue;

        [SerializeField] private float finalValue;
        private                  float lastFinalValue;

        public FloatOldNumeric(float basicValue)
        {
            this.basicValue = basicValue;
            finalValue      = basicValue;
            lastFinalValue  = basicValue;
        }

        public FloatOldNumeric() { }

        public float BasicValue
        {
            get => basicValue;
            set
            {
                basicValue = value;
                HasUpdate  = true;
            }
        }

        public float FinalValue
        {
            get
            {
                UpdateFinalValue();
                return finalValue;
            }
        }

        public static implicit operator FloatOldNumeric(float value) { return new FloatOldNumeric(value); }

        public static FloatOldNumeric operator +(FloatOldNumeric oldNumeric, FloatOldNumericModifier modifier)
        {
            oldNumeric.AddModifier(modifier);
            return oldNumeric;
        }

        public static FloatOldNumeric operator +(FloatOldNumeric oldNumeric, float value)
        {
            oldNumeric.AddModifier(new FloatOldNumericModifier(value));
            return oldNumeric;
        }

        public static FloatOldNumeric operator +(FloatOldNumeric oldNumeric, (float, string) value)
        {
            oldNumeric.AddModifier(new FloatOldNumericModifier(value.Item1, value.Item2));
            return oldNumeric;
        }

        public static FloatOldNumeric operator +(FloatOldNumeric oldNumeric, (float, string, string) value)
        {
            oldNumeric.AddModifier(new FloatOldNumericModifier(value.Item1, value.Item2, value.Item3));
            return oldNumeric;
        }

        public static FloatOldNumeric operator -(FloatOldNumeric oldNumeric, FloatOldNumericModifier modifier)
        {
            oldNumeric.RemoveModifier(modifier);
            return oldNumeric;
        }

        public static FloatOldNumeric operator -(FloatOldNumeric oldNumeric, float value)
        {
            oldNumeric.RemoveModifier(new FloatOldNumericModifier(value));
            return oldNumeric;
        }

        public static FloatOldNumeric operator -(FloatOldNumeric oldNumeric, (float, string) value)
        {
            oldNumeric.RemoveModifier(new FloatOldNumericModifier(value.Item1, value.Item2));
            return oldNumeric;
        }

        public static FloatOldNumeric operator -(FloatOldNumeric oldNumeric, (float, string, string) value)
        {
            oldNumeric.RemoveModifier(new FloatOldNumericModifier(value.Item1, value.Item2, value.Item3));
            return oldNumeric;
        }

        public static FloatOldNumeric operator *(FloatOldNumeric oldNumeric, FractionOldNumericModifier modifier)
        {
            oldNumeric.AddModifier(modifier);
            return oldNumeric;
        }

        public static FloatOldNumeric operator *(FloatOldNumeric oldNumeric, (int, int) value)
        {
            oldNumeric.AddModifier(new OverrideFractionOldNumericModifier(value.Item1, value.Item2));
            return oldNumeric;
        }

        public static FloatOldNumeric operator /(FloatOldNumeric oldNumeric, FractionOldNumericModifier modifier)
        {
            oldNumeric.RemoveModifier(modifier);
            return oldNumeric;
        }

        public static FloatOldNumeric operator /(FloatOldNumeric oldNumeric, (int, int) value)
        {
            oldNumeric.RemoveModifier(new OverrideFractionOldNumericModifier(value.Item1, value.Item2));
            return oldNumeric;
        }

        protected override void UpdateFinalValue()
        {
            if (!HasUpdate)
            {
                finalValue = lastFinalValue;
                return;
            }

            finalValue = basicValue;
            foreach (var modifier in ModifierCollector) modifier.ApplyModifier(ref finalValue, basicValue, this);

            lastFinalValue = finalValue;
            HasUpdate      = false;
        }
    }

    public static class NumericExtension
    {
        public static void Modify(this OldNumeric oldNumeric, OldNumericModifier modifier) { }

        public static void Bind(this OldNumeric oldNumeric, Func<bool> condition, Action action) { }

        public static void Bind(this OldNumeric oldNumeric, Func<bool> condition, Action<int> action) { }

        public static void Bind(this OldNumeric oldNumeric, Func<bool> condition, Action<float> action) { }
    }
}