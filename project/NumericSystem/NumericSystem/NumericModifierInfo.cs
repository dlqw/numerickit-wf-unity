namespace WFramework.CoreGameDevKit.NumericSystem
{
    public record NumericModifierInfo
    {
        public readonly string[] Tags;
        public readonly string   Name;

        public int Count;

        public NumericModifierInfo(string[] tags, string name, int count)
        {
            Tags  = tags;
            Name  = name;
            Count = count;
        }
    }
}