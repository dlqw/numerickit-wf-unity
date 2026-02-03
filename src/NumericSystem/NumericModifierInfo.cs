using System;

namespace WFramework.CoreGameDevKit.NumericSystem
{
    [Serializable]
    public record NumericModifierInfo
    {
        public readonly string[] Tags;
        public readonly string   Name;

        public int Count;
        public ModifierPriority Priority;

        public NumericModifierInfo(string[] tags, string name, int count, ModifierPriority priority = ModifierPriority.Default)
        {
            if (tags == null)
                throw new ArgumentNullException(nameof(tags), "标签不能为 null。");
            if (name == null)
                throw new ArgumentNullException(nameof(name), "名称不能为 null。");
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("名称不能为空或空白字符。", nameof(name));
            if (count <= 0)
                throw new ArgumentOutOfRangeException(nameof(count), "计数必须大于零。");

            Tags     = tags;
            Name     = name;
            Count    = count;
            Priority = priority;
        }

        /// <summary>
        /// 修饰符的比较键，用于优先级排序。
        /// </summary>
        /// <remarks>
        /// 优先级相同的情况下，按名称和计数排序以保证稳定排序。
        /// </remarks>
        public (ModifierPriority priority, string name, int count) SortKey => (Priority, Name, Count);
    }
}