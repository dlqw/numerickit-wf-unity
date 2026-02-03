namespace WFramework.CoreGameDevKit.NumericSystem
{
    internal enum ModifierType
    {
        None,
        Add,
        Frac,
        Custom
    }

    public interface INumericModifier : IInfo, IApply
    {
        internal abstract ModifierType Type { get; }
    }
}