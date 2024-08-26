using System;

namespace WFramework.CoreGameDevKit.NumericSystem
{
    public interface IApply
    {
        Func<Numeric, int> Apply(int source);
    }
}