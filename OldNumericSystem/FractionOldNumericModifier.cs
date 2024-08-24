using System;

namespace WFramework.CoreGameDevKit.OldNumericSystem
{
    [Serializable]
    [Obsolete]
    public abstract class FractionOldNumericModifier : OldNumericModifier
    {
        protected readonly int Denominator;
        protected readonly int Numerator;

        /// <summary>
        /// </summary>
        /// <param name="numerator">分子</param>
        /// <param name="denominator">分母</param>
        /// <param name="name"></param>
        /// <param name="tags"></param>
        protected FractionOldNumericModifier(int numerator, int denominator, string name = "", params string[] tags)
        {
            Numerator   = numerator;
            Denominator = denominator;
            this.name   = name;
            this.tags   = tags;
        }

        protected FractionOldNumericModifier() { }
    }
}