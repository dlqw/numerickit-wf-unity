using System.Collections.Generic;

namespace WFramework.CoreGameDevKit.NumericSystem.Core
{
    /// <summary>
    /// 修饰符集合接口，用于管理修饰符的存储和查询
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>职责：</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>修饰符的添加、移除和清空</description></item>
    /// <item><description>修饰符的查询（按名称、类型、标签等）</description></item>
    /// <item><description>修饰符的合并逻辑（同名称修饰符合并 Count）</description></item>
    /// </list>
    /// <para>
    /// <strong>线程安全：</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>此接口本身不保证线程安全</description></item>
    /// <item><description>实现类应该根据使用场景提供适当的同步机制</description></item>
    /// <item><description>对于多线程环境，请使用 ThreadSafeNumeric</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var collection = new ModifierCollection();
    /// collection.Add(modifier);
    /// var allModifiers = collection.GetAll();
    /// </code>
    /// </example>
    public interface IModifierCollection
    {
        /// <summary>
        /// 添加一个修饰符到集合中
        /// </summary>
        /// <param name="modifier">要添加的修饰符</param>
        void Add(INumericModifier modifier);

        /// <summary>
        /// 从集合中移除一个修饰符
        /// </summary>
        /// <param name="modifier">要移除的修饰符</param>
        /// <returns>如果成功移除返回 true；如果未找到返回 false</returns>
        bool Remove(INumericModifier modifier);

        /// <summary>
        /// 清空集合中的所有修饰符
        /// </summary>
        void Clear();

        /// <summary>
        /// 获取所有修饰符的只读集合
        /// </summary>
        /// <returns>包含所有修饰符的只读集合</returns>
        IReadOnlyList<INumericModifier> GetAll();

        /// <summary>
        /// 获取指定类型的所有修饰符
        /// </summary>
        /// <typeparam name="T">修饰符类型</typeparam>
        /// <returns>匹配类型的修饰符集合</returns>
        IReadOnlyList<T> GetAll<T>() where T : INumericModifier;

        /// <summary>
        /// 根据名称查找修饰符
        /// </summary>
        /// <param name="name">修饰符名称</param>
        /// <returns>找到的修饰符，如果未找到返回 null</returns>
        INumericModifier? FindByName(string name);

        /// <summary>
        /// 获取所有加法修饰符的累积值（内部定点数形式）
        /// </summary>
        /// <returns>所有加法修饰符的累积值</returns>
        int GetAddModifierValue();

        /// <summary>
        /// 获取具有指定标签的加法修饰符的累积值（内部定点数形式）
        /// </summary>
        /// <param name="tags">标签数组，用于筛选修饰符</param>
        /// <returns>匹配标签的修饰符累积值</returns>
        int GetAddModifierValueByTag(string[] tags);

        /// <summary>
        /// 获取集合中的修饰符数量
        /// </summary>
        int Count { get; }

        /// <summary>
        /// 检查集合是否为空
        /// </summary>
        bool IsEmpty { get; }
    }
}
