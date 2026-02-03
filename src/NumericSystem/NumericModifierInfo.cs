using System;

namespace WFramework.CoreGameDevKit.NumericSystem
{
    public record NumericModifierInfo
    {
        public readonly string[] Tags;
        public readonly string   Name;

        public int Count;

        public NumericModifierInfo(string[] tags, string name, int count)
        {
            if (tags == null)
                throw new ArgumentNullException(nameof(tags), "标签不能为 null。");
            if (name == null)
                throw new ArgumentNullException(nameof(name), "名称不能为 null。");
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("名称不能为空或空白字符。", nameof(name));
            if (count <= 0)
                throw new ArgumentOutOfRangeException(nameof(count), "计数必须大于零。");

            Tags  = tags;
            Name  = name;
            Count = count;
        }
    }
}