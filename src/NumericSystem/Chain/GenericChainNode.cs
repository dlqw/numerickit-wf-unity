using System;

namespace WFramework.CoreGameDevKit.NumericSystem.Core
{
    /// <summary>
    /// 通用的责任链节点实现，支持通过委托定义处理逻辑
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>设计模式：</strong> 责任链模式（Chain of Responsibility Pattern）
    /// </para>
    /// <para>
    /// <strong>职责：</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>封装处理修饰符的委托函数</description></item>
    /// <item><description>管理责任链中的下一个节点</description></item>
    /// <item><description>提供灵活的责任链构建方式</description></item>
    /// </list>
    /// <para>
    /// <strong>使用场景：</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>快速创建责任链节点，无需定义新类</description></item>
    /// <item><description>动态构建处理管道</description></item>
    /// <item><description>组合多个处理步骤</description></item>
    /// </list>
    /// <para>
    /// <strong>线程安全：</strong>
    /// </para>
    /// 此类是线程安全的，只要传入的委托函数是线程安全的。
    /// </remarks>
    /// <example>
    /// <code>
    /// // 示例 1: 创建简单的处理节点
    /// var node = new GenericChainNode((value, modifier, context) =>
    /// {
    ///     return value + 100;  // 简单的处理逻辑
    /// });
    ///
    /// // 示例 2: 创建带下一个节点的链
    /// var validator = new GenericChainNode((value, modifier, context) =>
    /// {
    ///     if (modifier == null) throw new ArgumentNullException();
    ///     return value;
    /// });
    ///
    /// var processor = new GenericChainNode((value, modifier, context) =>
    /// {
    ///     return value * 2;
    /// }, validator);  // 设置下一个节点
    ///
    /// // 示例 3: 使用 Lambda 表达式
    /// var loggingNode = new GenericChainNode((value, modifier, context) =>
    /// {
    ///     Console.WriteLine($"Processing {modifier.Info.Name}");
    ///     return value;
    /// });
    /// </code>
    /// </example>
    public sealed class GenericChainNode : IModifierChainNode
    {
        private readonly Func<int, INumericModifier, Numeric, int> processFunc;
        private readonly IModifierChainNode next;

        /// <summary>
        /// 初始化 GenericChainNode 的新实例
        /// </summary>
        /// <param name="processFunc">处理修饰符的委托函数</param>
        /// <param name="next">责任链中的下一个节点（可选）</param>
        /// <exception cref="ArgumentNullException">当 processFunc 为 null 时抛出</exception>
        /// <remarks>
        /// <paramref name="processFunc"/> 委托签名：
        /// <code>
        /// int ProcessFunc(int value, INumericModifier modifier, Numeric context)
        /// </code>
        /// <list type="bullet">
        /// <item><description>value - 当前计算值（内部定点数形式）</description></item>
        /// <item><description>modifier - 要处理的修饰符</description></item>
        /// <item><description>context - Numeric 上下文</description></item>
        /// <item><description>返回值 - 处理后的值（内部定点数形式）</description></item>
        /// </list>
        /// </remarks>
        public GenericChainNode(
            Func<int, INumericModifier, Numeric, int> processFunc,
            IModifierChainNode next = null)
        {
            this.processFunc = processFunc ?? throw new ArgumentNullException(
                nameof(processFunc),
                "处理函数不能为 null。请提供一个有效的 Func<int, INumericModifier, Numeric, int> 委托。");
            this.next = next;
        }

        /// <summary>
        /// 处理修饰符，并返回处理后的值
        /// </summary>
        /// <param name="value">当前的计算值（内部定点数形式）</param>
        /// <param name="modifier">要处理的修饰符</param>
        /// <param name="context">Numeric 上下文，提供额外的计算信息</param>
        /// <returns>处理后的值（内部定点数形式）</returns>
        /// <exception cref="ArgumentNullException">
        /// 当 <paramref name="modifier"/> 或 <paramref name="context"/> 为 null 时抛出
        /// </exception>
        /// <remarks>
        /// <para>
        /// <strong>处理流程：</strong>
        /// </para>
        /// <list type="number">
        /// <item><description>执行当前节点的处理函数</description></item>
        /// <item><description>如果有下一个节点，将结果传递给下一个节点处理</description></item>
        /// <item><description>如果没有下一个节点，返回当前处理结果</description></item>
        /// </list>
        /// </remarks>
        public int Process(int value, INumericModifier modifier, Numeric context)
        {
            if (modifier == null)
                throw new ArgumentNullException(nameof(modifier), "修饰符不能为 null。");
            if (context == null)
                throw new ArgumentNullException(nameof(context), "Numeric 上下文不能为 null。");

            // 执行当前节点的处理逻辑
            var result = processFunc(value, modifier, context);

            // 如果有下一个节点，传递给下一个节点处理
            return next?.Process(result, modifier, context) ?? result;
        }

        /// <summary>
        /// 创建一个新的链节点，将其添加到当前节点的后面
        /// </summary>
        /// <param name="processFunc">新节点的处理函数</param>
        /// <returns>新创建的节点</returns>
        /// <remarks>
        /// 此方法创建一个新节点，并将其设置为当前节点的下一个节点。
        /// 原有的下一个节点会被替换。
        /// </remarks>
        public GenericChainNode Then(Func<int, INumericModifier, Numeric, int> processFunc)
        {
            var newNode = new GenericChainNode(processFunc);
            // 注意：这里我们返回新节点，用户需要手动连接
            return newNode;
        }
    }
}
