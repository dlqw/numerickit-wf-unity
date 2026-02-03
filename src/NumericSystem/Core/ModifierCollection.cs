using System;
using System.Collections.Generic;
using System.Linq;

namespace WFramework.CoreGameDevKit.NumericSystem.Core
{
    /// <summary>
    /// 修饰符集合的默认实现，基于 HashSet 和 Dictionary 的存储结构
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>职责：</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>管理修饰符的添加、移除和清空</description></item>
    /// <item><description>提供 O(1) 时间复杂度的按名称查找</description></item>
    /// <item><description>支持修饰符合并（同名称修饰符合并 Count）</description></item>
    /// <item><description>提供按标签、类型查询修饰符的功能</description></item>
    /// </list>
    /// <para>
    /// <strong>数据结构：</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>HashSet - 存储所有修饰符，提供 O(1) 的添加和查找</description></item>
    /// <item><description>Dictionary - 按名称索引修饰符，提供 O(1) 的按名称查找</description></item>
    /// </list>
    /// <para>
    /// <strong>修饰符合并规则：</strong>
    /// </para>
    /// <list type="number">
    /// <item><description>当添加同名修饰符时，自动合并 Count</description></item>
    /// <item><description>对于加法修饰符，直接累加 Count</description></item>
    /// <item><description>对于分数修饰符，Count 合并会影响幂运算结果</description></item>
    /// </list>
    /// <para>
    /// <strong>线程安全：</strong>
    /// </para>
    /// 此实现不保证线程安全。如需线程安全，请使用外部同步机制或 <see cref="ThreadSafeNumeric"/>。
    /// </remarks>
    public sealed class ModifierCollection : IModifierCollection
    {
        /// <summary>
        /// 存储所有修饰符的集合
        /// </summary>
        private readonly HashSet<INumericModifier> modifiers;

        /// <summary>
        /// 按名称快速查找修饰符的字典
        /// </summary>
        private readonly Dictionary<string, INumericModifier> modifierLookup;

        /// <summary>
        /// 初始化 ModifierCollection 的新实例
        /// </summary>
        public ModifierCollection()
        {
            modifiers = new HashSet<INumericModifier>();
            modifierLookup = new Dictionary<string, INumericModifier>();
        }

        /// <summary>
        /// 添加一个修饰符到集合中
        /// </summary>
        /// <param name="modifier">要添加的修饰符</param>
        /// <exception cref="ArgumentNullException">当 modifier 为 null 时抛出</exception>
        /// <remarks>
        /// <para>
        /// <strong>修饰符合并规则：</strong>
        /// </para>
        /// <list type="bullet">
        /// <item><description>命名修饰符（非默认名称）：如果存在同名修饰符，合并 Count</description></item>
        /// <item><description>匿名修饰符（使用默认名称）：每个独立添加，不合并</description></item>
        /// </list>
        /// </remarks>
        public void Add(INumericModifier modifier)
        {
            if (modifier == null)
                throw new ArgumentNullException(nameof(modifier), "修饰符不能为 null。");

            var name = modifier.Info.Name;

            // 匿名修饰符（使用默认名称）不应该合并，每个都独立添加
            if (name == NumericModifierConfig.DefaultName)
            {
                modifiers.Add(modifier);
            }
            // 命名修饰符：查找同名修饰符并合并（累加Count）
            else if (modifierLookup.TryGetValue(name, out var existingModifier))
            {
                existingModifier.Info.Count += modifier.Info.Count;
            }
            else
            {
                modifiers.Add(modifier);
                modifierLookup[name] = modifier;
            }
        }

        /// <summary>
        /// 从集合中移除一个修饰符
        /// </summary>
        /// <param name="modifier">要移除的修饰符</param>
        /// <returns>如果成功移除返回 true；如果未找到返回 false</returns>
        /// <exception cref="ArgumentNullException">当 modifier 为 null 时抛出</exception>
        /// <remarks>
        /// <para>
        /// <strong>移除规则：</strong>
        /// </para>
        /// <list type="bullet">
        /// <item><description>命名修饰符：按名称查找并移除</description></item>
        /// <item><description>匿名修饰符（使用默认名称）：按类型和值查找并移除</description></item>
        /// </list>
        /// </remarks>
        public bool Remove(INumericModifier modifier)
        {
            if (modifier == null)
                throw new ArgumentNullException(nameof(modifier), "修饰符不能为 null。");

            var name = modifier.Info.Name;

            // 命名修饰符：从查找字典中移除
            if (modifierLookup.TryGetValue(name, out var existingModifier))
            {
                // 减少计数
                existingModifier.Info.Count -= modifier.Info.Count;
                if (existingModifier.Info.Count <= 0)
                {
                    // Count 减至 0 或以下时，完全移除
                    modifiers.Remove(existingModifier);
                    modifierLookup.Remove(name);
                }
                return true;
            }

            // 匿名修饰符（使用默认名称）：按类型和值查找并移除
            INumericModifier? toRemove = null;
            foreach (var mod in modifiers)
            {
                if (mod.Info.Name == NumericModifierConfig.DefaultName &&
                    mod.Type == modifier.Type)
                {
                    if (modifier.Type == ModifierType.Add &&
                        mod is AdditionNumericModifier addMod &&
                        modifier is AdditionNumericModifier addInput &&
                        addMod.StoreValue == addInput.StoreValue)
                    {
                        toRemove = mod;
                        break;
                    }
                    else if (modifier.Type == ModifierType.Frac &&
                             mod is FractionNumericModifier fracMod &&
                             modifier is FractionNumericModifier fracInput &&
                             fracMod.Info.Tags == fracInput.Info.Tags)
                    {
                        toRemove = mod;
                        break;
                    }
                }
            }

            if (toRemove != null)
            {
                modifiers.Remove(toRemove);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 清空集合中的所有修饰符
        /// </summary>
        public void Clear()
        {
            modifiers.Clear();
            modifierLookup.Clear();
        }

        /// <summary>
        /// 获取所有修饰符的只读集合
        /// </summary>
        /// <returns>包含所有修饰符的只读集合</returns>
        public IReadOnlyList<INumericModifier> GetAll()
        {
            return modifiers.ToList().AsReadOnly();
        }

        /// <summary>
        /// 获取指定类型的所有修饰符
        /// </summary>
        /// <typeparam name="T">修饰符类型</typeparam>
        /// <returns>匹配类型的修饰符集合</returns>
        public IReadOnlyList<T> GetAll<T>() where T : INumericModifier
        {
            return modifiers
                .OfType<T>()
                .ToList()
                .AsReadOnly();
        }

        /// <summary>
        /// 根据名称查找修饰符
        /// </summary>
        /// <param name="name">修饰符名称</param>
        /// <returns>找到的修饰符，如果未找到返回 null</returns>
        /// <exception cref="ArgumentNullException">当 name 为 null 时抛出</exception>
        public INumericModifier? FindByName(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name), "修饰符名称不能为 null。");

            modifierLookup.TryGetValue(name, out var modifier);
            return modifier;
        }

        /// <summary>
        /// 获取所有加法修饰符的累积值（内部定点数形式）
        /// </summary>
        /// <returns>所有加法修饰符的累积值</returns>
        /// <remarks>
        /// 计算公式：Σ(修饰符.StoreValue × 修饰符.Info.Count)
        /// </remarks>
        public int GetAddModifierValue()
        {
            int sum = 0;
            foreach (var mod in modifiers)
            {
                if (mod.Type == ModifierType.Add)
                {
                    sum += mod.Info.Count * ((AdditionNumericModifier)mod).StoreValue;
                }
            }
            return sum;
        }

        /// <summary>
        /// 获取具有指定标签的加法修饰符的累积值（内部定点数形式）
        /// </summary>
        /// <param name="tags">标签数组，用于筛选修饰符</param>
        /// <returns>匹配标签的修饰符累积值</returns>
        /// <exception cref="ArgumentNullException">当 tags 为 null 时抛出</exception>
        /// <remarks>
        /// <para>
        /// <strong>筛选规则：</strong>
        /// </para>
        /// <list type="bullet">
        /// <item><description>如果修饰符的 Tags 与提供的 tags 数组有交集，则计入累积值</description></item>
        /// <item><description>空 tags 数组会匹配所有加法修饰符</description></item>
        /// </list>
        /// </remarks>
        public int GetAddModifierValueByTag(string[] tags)
        {
            if (tags == null)
                throw new ArgumentNullException(nameof(tags), "标签数组不能为 null。");

            // 空标签数组：匹配所有加法修饰符
            if (tags.Length == 0)
                return GetAddModifierValue();

            int sum = 0;
            foreach (var mod in modifiers)
            {
                if (mod.Type == ModifierType.Add && HasMatchingTag(mod.Info.Tags, tags))
                {
                    sum += mod.Info.Count * ((AdditionNumericModifier)mod).StoreValue;
                }
            }
            return sum;
        }

        /// <summary>
        /// 检查修饰符的标签是否与目标标签匹配
        /// </summary>
        /// <param name="modifierTags">修饰符的标签数组</param>
        /// <param name="targetTags">目标标签数组</param>
        /// <returns>如果有交集返回 true，否则返回 false</returns>
        private static bool HasMatchingTag(string[] modifierTags, string[] targetTags)
        {
            foreach (var targetTag in targetTags)
            {
                foreach (var modifierTag in modifierTags)
                {
                    if (modifierTag == targetTag)
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 获取集合中的修饰符数量
        /// </summary>
        public int Count => modifiers.Count;

        /// <summary>
        /// 检查集合是否为空
        /// </summary>
        public bool IsEmpty => modifiers.Count == 0;
    }
}
