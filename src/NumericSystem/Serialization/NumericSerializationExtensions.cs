using System;
using System.Collections.Generic;
using System.Linq;

namespace WFramework.CoreGameDevKit.NumericSystem.Serialization
{
    /// <summary>
    /// Numeric 序列化扩展方法
    /// </summary>
    public static class NumericSerializationExtensions
    {
        /// <summary>
        /// 将 Numeric 序列化为 NumericData
        /// </summary>
        /// <param name="numeric">要序列化的 Numeric 对象</param>
        /// <returns>包含所有可序列化修饰符的 NumericData</returns>
        /// <remarks>
        /// 注意：CustomNumericModifier 和 ConditionalNumericModifier 包含不可序列化的委托类型，
        /// 因此不会被包含在序列化结果中。
        /// </remarks>
        public static NumericData Serialize(this Numeric numeric)
        {
            if (numeric == null)
                throw new ArgumentNullException(nameof(numeric));

            var modifierList = new List<ModifierData>();

            // 序列化 AdditionNumericModifier
            foreach (var modifier in numeric.GetModifiers<AdditionNumericModifier>())
            {
                modifierList.Add(ModifierData.CreateAddition(
                    modifier.StoreValue,
                    modifier.Info.Tags,
                    modifier.Info.Name,
                    modifier.Info.Count,
                    modifier.Info.Priority
                ));
            }

            // 序列化 FractionNumericModifier
            foreach (var modifier in numeric.GetModifiers<FractionNumericModifier>())
            {
                // 使用反射或公共接口获取私有字段
                var data = CreateFractionData(modifier);
                modifierList.Add(data);
            }

            return new NumericData(numeric.GetOriginValue(), modifierList.ToArray());
        }

        /// <summary>
        /// 从 NumericData 反序列化为 Numeric
        /// </summary>
        /// <param name="data">要反序列化的 NumericData</param>
        /// <returns>恢复后的 Numeric 对象</returns>
        public static Numeric Deserialize(this NumericData data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            // 将固定点值转换回浮点数，因为 Numeric 构造函数会再次转换为固定点
            var originFloat = data.OriginValue.ToFloat();
            var numeric = new Numeric(originFloat);

            foreach (var modifierData in data.Modifiers)
            {
                var modifier = CreateModifier(modifierData);
                if (modifier != null)
                {
                    numeric.AddModifier(modifier);
                }
            }

            return numeric;
        }

        /// <summary>
        /// 从 ModifierData 创建 INumericModifier
        /// </summary>
        private static INumericModifier CreateModifier(ModifierData data)
        {
            return data.Type switch
            {
                ModifierType.Add => new AdditionNumericModifier(
                    data.StoreValue.ToFloat(),
                    data.Tags,
                    data.Name,
                    data.Count,
                    data.Priority
                ),
                ModifierType.Frac => new FractionNumericModifier(
                    data.Numerator,
                    data.Denominator,
                    data.FractionType,
                    data.Tags,
                    data.Name,
                    data.Count,
                    data.Priority
                ),
                _ => throw new NotSupportedException($"不支持的修饰符类型：{data.Type}")
            };
        }

        /// <summary>
        /// 从 FractionNumericModifier 创建 ModifierData
        /// </summary>
        private static ModifierData CreateFractionData(FractionNumericModifier modifier)
        {
            // 使用公共属性获取序列化所需的数据
            return ModifierData.CreateFraction(
                modifier.Numerator,
                modifier.Denominator,
                modifier.FractionTypeValue,
                modifier.Info.Tags,
                modifier.Info.Name,
                modifier.Info.Count,
                modifier.Info.Priority
            );
        }
    }

    /// <summary>
    /// 获取特定类型的修饰符的扩展方法
    /// </summary>
    public static class NumericModifierExtensions
    {
        /// <summary>
        /// 获取所有指定类型的修饰符
        /// </summary>
        /// <typeparam name="T">修饰符类型</typeparam>
        /// <param name="numeric">Numeric 对象</param>
        /// <returns>匹配的修饰符集合</returns>
        public static IEnumerable<T> GetModifiers<T>(this Numeric numeric) where T : INumericModifier
        {
            return numeric.GetAllModifiers().OfType<T>();
        }
    }
}
