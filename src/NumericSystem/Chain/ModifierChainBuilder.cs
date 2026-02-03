using System;
using System.Collections.Generic;

namespace WFramework.CoreGameDevKit.NumericSystem.Core
{
    /// <summary>
    /// 修饰符责任链构建器，提供流畅的 API 来构建责任链
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>设计模式：</strong> 建造者模式（Builder Pattern）
    /// </para>
    /// <para>
    /// <strong>职责：</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>提供流畅的 API 来构建责任链</description></item>
    /// <item><description>管理责任链节点的添加顺序</description></item>
    /// <item><description>构建完整的责任链供使用</description></item>
    /// </list>
    /// <para>
    /// <strong>使用场景：</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>构建复杂的修饰符处理管道</description></item>
    /// <item><description>动态组合多个处理步骤</description></item>
    /// <item><description>创建可重用的责任链</description></item>
    /// </list>
    /// <para>
    /// <strong>线程安全：</strong>
    /// </para>
    /// 构建过程不是线程安全的，但构建完成后的责任链可以是线程安全的
    /// （只要其中的节点是线程安全的）。
    /// </remarks>
    /// <example>
    /// <code>
    /// // 示例 1: 构建简单的责任链
    /// var chain = new ModifierChainBuilder()
    ///     .AddNode((value, modifier, context) =>
    ///     {
    ///         Console.WriteLine($"Processing {modifier.Info.Name}");
    ///         return value;
    ///     })
    ///     .AddNode((value, modifier, context) =>
    ///     {
    ///         return value + 100;
    ///     })
    ///     .Build();
    ///
    /// // 示例 2: 使用预定义的节点
    /// var validator = new ValidationNode();
    /// var processor = new ProcessorNode();
    ///
    /// var chain = new ModifierChainBuilder()
    ///     .AddNode(validator)
    ///     .AddNode(processor)
    ///     .Build();
    ///
    /// // 示例 3: 构建带条件的链
    /// var chain = new ModifierChainBuilder()
    ///     .AddNode((value, modifier, context) =>
    ///     {
    ///         if (modifier.Info.Priority < ModifierPriority.Equipment)
    ///             return value;
    ///         return value;
    ///     })
    ///     .AddEvaluator()  // 添加默认评估器
    ///     .Build();
    /// </code>
    /// </example>
    public sealed class ModifierChainBuilder
    {
        private readonly List<Func<int, INumericModifier, Numeric, int>> nodeFunctions;
        private readonly List<IModifierChainNode> nodes;

        /// <summary>
        /// 初始化 ModifierChainBuilder 的新实例
        /// </summary>
        public ModifierChainBuilder()
        {
            nodeFunctions = new List<Func<int, INumericModifier, Numeric, int>>();
            nodes = new List<IModifierChainNode>();
        }

        /// <summary>
        /// 添加一个处理函数作为责任链节点
        /// </summary>
        /// <param name="processFunc">处理修饰符的函数</param>
        /// <returns>当前构建器实例，支持链式调用</returns>
        /// <exception cref="ArgumentNullException">当 processFunc 为 null 时抛出</exception>
        /// <remarks>
        /// 处理函数会被包装成 <see cref="GenericChainNode"/> 添加到责任链中。
        /// </remarks>
        public ModifierChainBuilder AddNode(Func<int, INumericModifier, Numeric, int> processFunc)
        {
            if (processFunc == null)
                throw new ArgumentNullException(nameof(processFunc), "处理函数不能为 null。");

            nodeFunctions.Add(processFunc);
            return this;
        }

        /// <summary>
        /// 添加一个责任链节点
        /// </summary>
        /// <param name="node">要添加的责任链节点</param>
        /// <returns>当前构建器实例，支持链式调用</returns>
        /// <exception cref="ArgumentNullException">当 node 为 null 时抛出</exception>
        public ModifierChainBuilder AddNode(IModifierChainNode node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node), "责任链节点不能为 null。");

            nodes.Add(node);
            return this;
        }

        /// <summary>
        /// 添加默认的修饰符评估器节点
        /// </summary>
        /// <returns>当前构建器实例，支持链式调用</returns>
        /// <remarks>
        /// 此方法创建一个使用 <see cref="DefaultModifierEvaluator"/> 的节点，
        /// 用于评估修饰符并应用其效果。
        /// </remarks>
        public ModifierChainBuilder AddEvaluator()
        {
            var evaluator = new DefaultModifierEvaluator();
            return AddNode((value, modifier, context) =>
            {
                if (evaluator.CanEvaluate(modifier))
                    return evaluator.Evaluate(value, modifier, context);
                return value;
            });
        }

        /// <summary>
        /// 构建责任链
        /// </summary>
        /// <returns>构建完成的第一个责任链节点</returns>
        /// <remarks>
        /// <para>
        /// <strong>构建顺序：</strong>
        /// </para>
        /// <list type="number">
        /// <item><description>先添加函数节点</description></item>
        /// <item><description>再添加自定义节点</description></item>
        /// <item><description>按照添加顺序从后向前连接</description></item>
        /// </list>
        /// <para>
        /// <strong>责任链执行顺序：</strong>
        /// </para>
        /// 先添加的节点先执行，后添加的节点后执行。
        /// </remarks>
        public IModifierChainNode Build()
        {
            IModifierChainNode current = null;

            // 先添加自定义节点
            foreach (var node in nodes)
            {
                current = ChainNode(current, node);
            }

            // 再添加函数节点
            foreach (var func in nodeFunctions)
            {
                current = ChainNode(current, func);
            }

            return current ?? throw new InvalidOperationException(
                "责任链不能为空。请至少添加一个节点或处理函数。");
        }

        /// <summary>
        /// 创建或连接责任链节点
        /// </summary>
        /// <param name="current">当前节点</param>
        /// <param name="node">要连接的节点</param>
        /// <returns>连接后的新节点</returns>
        private static IModifierChainNode ChainNode(IModifierChainNode current, IModifierChainNode node)
        {
            if (current == null)
                return node;

            // 创建一个包装节点，先执行当前逻辑，再传递给下一个节点
            return new GenericChainNode(
                (value, modifier, context) =>
                {
                    // 先执行下一个节点（因为是从后向前构建的）
                    var result = node.Process(value, modifier, context);
                    // 再执行当前节点
                    return current.Process(result, modifier, context);
                });
        }

        /// <summary>
        /// 创建或连接函数节点
        /// </summary>
        /// <param name="current">当前节点</param>
        /// <param name="func">处理函数</param>
        /// <returns>连接后的新节点</returns>
        private static IModifierChainNode ChainNode(
            IModifierChainNode current,
            Func<int, INumericModifier, Numeric, int> func)
        {
            var newNode = new GenericChainNode(func);
            return ChainNode(current, newNode);
        }

        /// <summary>
        /// 重置构建器，清除所有已添加的节点
        /// </summary>
        /// <returns>当前构建器实例，支持链式调用</returns>
        public ModifierChainBuilder Reset()
        {
            nodeFunctions.Clear();
            nodes.Clear();
            return this;
        }
    }
}
