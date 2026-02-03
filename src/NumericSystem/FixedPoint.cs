using System;

namespace WFramework.CoreGameDevKit.NumericSystem
{
    public static class FixedPoint
    {
        public const uint Factor = 10000;

        public static int ToFixedPoint(this float value)
        {
            ValidateFloat(value);
            return (int)(value * Factor);
        }

        public static int ToFixedPoint(this int value) { return (int)(value * Factor); }

        public static float ToFloat(this int value) { return value / (float)Factor; }

        /// <summary>
        /// Validates that a float value is not NaN or Infinity.
        /// Throws ArgumentException if validation fails.
        /// </summary>
        public static void ValidateFloat(float value, string? paramName = null)
        {
            if (float.IsNaN(value))
                throw new ArgumentException("Value cannot be NaN.", paramName ?? nameof(value));

            if (float.IsInfinity(value))
                throw new ArgumentException("Value cannot be Infinity.", paramName ?? nameof(value));
        }
    }
}