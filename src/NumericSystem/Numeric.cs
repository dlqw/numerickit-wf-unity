using System;
using System.Collections.Generic;
using System.Linq;

namespace WFramework.CoreGameDevKit.NumericSystem
{
    [Serializable]
    public class Numeric
    {
       private readonly int originalValue;

        private int finalValue;
        
        public int FinalValue
        {
            get
            {
                Update();
                return finalValue;
            }
        }
        
        public float FinalValueF
        {
            get
            {
                Update();
                return finalValue.ToFloat();
            }
        }

        private int lastValue;
        private bool hasUpdate = true;

        private readonly HashSet<INumericModifier> modifiers = new HashSet<INumericModifier>();
        private readonly HashSet<CustomNumericModifier> constraintModifier = new HashSet<CustomNumericModifier>();

        public int GetOriginValue() => originalValue;

        public int GetAddModifierValue()
            => modifiers.Where(mod => mod.Type == ModifierType.Add)
                        .Sum(mod => mod.Info.Count * ((AdditionNumericModifier)mod).StoreValue);

        public int GetAddModifierValueByTag(string[] tags)
            => modifiers.Where(mod => mod.Type == ModifierType.Add)
                        .Where(mod => mod.Info.Tags.Intersect(tags).Any())
                        .Sum(mod => mod.Info.Count * ((AdditionNumericModifier)mod).StoreValue);

        public Numeric AddModifier(INumericModifier modifier)
        {
            if (modifier is CustomNumericModifier customModifier)
            {
                constraintModifier.Add(customModifier);
            }
            else
            {
                var existModifier = modifiers.FirstOrDefault(mod => mod.Info.Name == modifier.Info.Name);
                if (existModifier != null) existModifier.Info.Count += modifier.Info.Count;
                else modifiers.Add(modifier);
            }

            hasUpdate = true;
            return this;
        }

        public Numeric RemoveModifier(INumericModifier modifier)
        {
            if (modifier is CustomNumericModifier customModifier)
            {
                constraintModifier.Remove(customModifier);
            }
            else
            {
                var existModifier = modifiers.FirstOrDefault(mod => mod.Info.Name == modifier.Info.Name);
                if (existModifier != null)
                {
                    existModifier.Info.Count -= modifier.Info.Count;
                    if (existModifier.Info.Count <= 0) modifiers.Remove(existModifier);
                }
            }

            hasUpdate = true;
            return this;
        }

        public Numeric Clear()
        {
            modifiers.Clear();
            return this;
        }

        private void Update()
        {
            if (!hasUpdate)
            {
                finalValue = lastValue;
                return;
            }

            finalValue = originalValue;
            foreach (var modifier in modifiers) finalValue = modifier.Apply(finalValue)(this);

            foreach (var customNumericModifier in constraintModifier) finalValue = customNumericModifier.Apply(finalValue)(this);


            lastValue = finalValue;
            hasUpdate = false;
        }

        public Numeric(int value)
        {
            originalValue = value;
            lastValue     = value;
        }

        public Numeric(float value)
        {
            FixedPoint.ValidateFloat(value);
            originalValue = value.ToFixedPoint();
            lastValue     = originalValue;
        }

        public static implicit operator Numeric(int   value)   { return new Numeric(value); }
        public static implicit operator Numeric(float value)   { return new Numeric(value); }
        public static implicit operator int(Numeric   numeric) { return numeric.FinalValue; }
        public static implicit operator float(Numeric numeric) { return numeric.FinalValueF; }

        public static Numeric operator +(Numeric numeric, AdditionNumericModifier modifier) => numeric.AddModifier(modifier);

        public static Numeric operator -(Numeric numeric, AdditionNumericModifier modifier) => numeric.RemoveModifier(modifier);

        public static Numeric operator *(Numeric numeric, FractionNumericModifier modifier) => numeric.AddModifier(modifier);

        public static Numeric operator /(Numeric numeric, FractionNumericModifier modifier) => numeric.RemoveModifier(modifier);

        public static Numeric operator +(Numeric numeric, CustomNumericModifier modifier) => numeric.AddModifier(modifier);

        public static Numeric operator -(Numeric numeric, CustomNumericModifier modifier) => numeric.RemoveModifier(modifier);
    }
}