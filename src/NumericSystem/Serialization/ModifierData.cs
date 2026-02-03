using System;
using System.Collections.Generic;

namespace WFramework.CoreGameDevKit.NumericSystem.Serialization
{
    /// <summary>
    /// 修饰符数据传输对象（DTO），用于序列化
    /// </summary>
    [Serializable]
    public sealed class ModifierData
    {
        public ModifierType Type;
        public string[] Tags = Array.Empty<string>();
        public string Name = string.Empty;
        public int Count;
        public ModifierPriority Priority;

        // AdditionModifier 特有字段
        public int StoreValue;

        // FractionModifier 特有字段
        public int Numerator;
        public int Denominator;
        public FractionType FractionType;

        /// <summary>
        /// 创建 AdditionModifier 数据
        /// </summary>
        public static ModifierData CreateAddition(int storeValue, string[] tags, string name, int count, ModifierPriority priority)
        {
            return new ModifierData
            {
                Type = ModifierType.Add,
                StoreValue = storeValue,
                Tags = tags,
                Name = name,
                Count = count,
                Priority = priority
            };
        }

        /// <summary>
        /// 创建 FractionModifier 数据
        /// </summary>
        public static ModifierData CreateFraction(int numerator, int denominator, FractionType fractionType, string[] tags, string name, int count, ModifierPriority priority)
        {
            return new ModifierData
            {
                Type = ModifierType.Frac,
                Numerator = numerator,
                Denominator = denominator,
                FractionType = fractionType,
                Tags = tags,
                Name = name,
                Count = count,
                Priority = priority
            };
        }
    }

    /// <summary>
    /// Numeric 序列化数据
    /// </summary>
    [Serializable]
    public sealed class NumericData
    {
        public int OriginValue;
        public ModifierData[] Modifiers;

        public NumericData(int originValue, ModifierData[] modifiers)
        {
            OriginValue = originValue;
            Modifiers = modifiers ?? Array.Empty<ModifierData>();
        }
    }
}
