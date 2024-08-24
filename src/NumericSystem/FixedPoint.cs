namespace WFramework.CoreGameDevKit.NumericSystem
{
    public static class FixedPoint
    {
        public const  uint  Factor = 10000;
        public static int   ToFixedPoint(this float value) { return (int)(value * Factor); }
        public static int   ToFixedPoint(this int   value) { return (int)(value * Factor); }
        public static float ToFloat(this      int   value) { return value / (float)Factor; }
    }
}