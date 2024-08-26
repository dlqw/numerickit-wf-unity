using System;

namespace WFramework.CoreGameDevKit.NumericSystem
{
    public static class NumericModifierConfig
    {
        public const string TagSelf      = "SELF";
        public const string DefaultName  = "DEFAULT MODIFIER";
        public const int    DefaultCount = 1;

        public static NumericModifierInfo DefaultInfo => new(Array.Empty<string>(), DefaultName, DefaultCount);
    }
}