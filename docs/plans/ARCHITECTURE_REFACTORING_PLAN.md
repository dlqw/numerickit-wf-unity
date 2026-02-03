# Numeric System 架构重构计划

## 当前架构问题分析

### 1. 单一职责原则违反（SRP）
**文件**: `Numeric.cs` (~750 行)
**问题**:
- 修饰符存储管理
- 修饰符排序逻辑
- 修饰符应用逻辑
- 缓存管理
- 查询操作
- 运算符重载

**影响**: 难以测试、难以维护、难以扩展

### 2. 重复代码
**位置**: `Update()` 方法中的排序逻辑
```csharp
// 重复3次的排序代码
sortedModifiers.Sort((a, b) => {
    int priorityCompare = a.Info.Priority.CompareTo(b.Info.Priority);
    if (priorityCompare != 0) return priorityCompare;
    int nameCompare = string.Compare(a.Info.Name, b.Info.Name, StringComparison.Ordinal);
    if (nameCompare != 0) return nameCompare;
    return a.Info.Count.CompareTo(b.Info.Count);
});
```

### 3. 类型耦合
- `AddModifier()` 中大量类型判断（`is` 操作符）
- `RemoveModifier()` 中的类型转换和比较
- `GetAddModifierValue()` 硬编码了 `AdditionNumericModifier` 类型

### 4. 缺乏扩展性
- 添加新的修饰符类型需要修改 `Numeric.cs`
- 无法在运行时动态添加修饰符处理器
- 应用顺序硬编码

### 5. 测试困难
- 无法单独测试修饰符应用逻辑
- 无法 mock 修饰符存储
- 无法单独测试排序逻辑

---

## 架构重构目标

### 1. 引入策略模式
将修饰符应用逻辑从 `Numeric` 中分离，使用策略模式处理不同类型修饰符的应用。

### 2. 引入责任链模式
使用责任链模式处理修饰符的顺序应用，消除硬编码的三阶段逻辑。

### 3. 分离关注点
- **ModifierCollection**: 管理修饰符的存储和查询
- **ModifierEvaluator**: 负责修饰符的评估和应用
- **ModifierSorter**: 负责修饰符的排序
- **ModifierChain**: 负责任链的组织和执行

### 4. 提高可测试性
每个组件都可以独立测试，降低耦合度。

---

## 详细重构计划

### Phase 1: 创建基础接口和抽象（1-2天）

#### Task 1.1: 创建修饰符评估器接口
**文件**: `src/NumericSystem/Core/IModifierEvaluator.cs`
**目标**: 定义修饰符评估的标准接口
```csharp
public interface IModifierEvaluator
{
    /// <summary>
    /// 评估是否可以处理此修饰符
    /// </summary>
    bool CanEvaluate(INumericModifier modifier);

    /// <summary>
    /// 应用修饰符到当前值
    /// </summary>
    int Evaluate(int currentValue, INumericModifier modifier, Numeric context);
}
```

**验收标准**:
- [x] 接口创建完成
- [x] 包含 XML 文档注释
- [x] 单元测试通过

#### Task 1.2: 创建修饰符存储接口
**文件**: `src/NumericSystem/Core/IModifierCollection.cs`
**目标**: 抽象修饰符的存储和查询操作
```csharp
public interface IModifierCollection
{
    void Add(INumericModifier modifier);
    bool Remove(INumericModifier modifier);
    void Clear();
    IReadOnlyList<INumericModifier> GetAll();
    IReadOnlyList<T> GetAll<T>() where T : INumericModifier;
    INumericModifier? FindByName(string name);
    int GetAddModifierValue();
    int GetAddModifierValueByTag(string[] tags);
}
```

**验收标准**:
- [x] 接口创建完成
- [x] 定义所有必要的查询方法
- [x] 包含 XML 文档注释

#### Task 1.3: 创建修饰符排序器接口
**文件**: `src/NumericSystem/Core/IModifierSorter.cs`
**目标**: 抽象修饰符排序逻辑
```csharp
public interface IModifierSorter
{
    IReadOnlyList<T> Sort<T>(IReadOnlyList<T> modifiers) where T : INumericModifier;
}
```

**验收标准**:
- [x] 接口创建完成
- [x] 支持泛型排序
- [x] 包含 XML 文档注释

---

### Phase 2: 实现核心组件（3-4天）

#### Task 2.1: 实现默认修饰符评估器
**文件**: `src/NumericSystem/Core/DefaultModifierEvaluator.cs`
**目标**: 为每种修饰符类型提供评估实现
```csharp
public class DefaultModifierEvaluator : IModifierEvaluator
{
    public bool CanEvaluate(INumericModifier modifier)
    {
        return modifier != null;
    }

    public int Evaluate(int currentValue, INumericModifier modifier, Numeric context)
    {
        return modifier.Apply(currentValue)(context);
    }
}
```

**子任务**:
- [ ] 实现基础评估器
- [ ] 处理 `AdditionNumericModifier`
- [ ] 处理 `FractionNumericModifier`
- [ ] 处理 `CustomNumericModifier`
- [ ] 处理 `ConditionalNumericModifier`

**验收标准**:
- [x] 所有修饰符类型正确处理
- [x] 单元测试覆盖所有分支
- [x] 性能不低于原实现

#### Task 2.2: 实现修饰符排序器
**文件**: `src/NumericSystem/Core/PriorityBasedModifierSorter.cs`
**目标**: 提供基于优先级的排序实现
```csharp
public class PriorityBasedModifierSorter : IModifierSorter
{
    public IReadOnlyList<T> Sort<T>(IReadOnlyList<T> modifiers)
    {
        var sorted = new List<T>(modifiers);
        sorted.Sort((a, b) => {
            int priorityCompare = a.Info.Priority.CompareTo(b.Info.Priority);
            if (priorityCompare != 0) return priorityCompare;
            int nameCompare = string.Compare(a.Info.Name, b.Info.Name, StringComparison.Ordinal);
            if (nameCompare != 0) return nameCompare;
            return a.Info.Count.CompareTo(b.Info.Count);
        });
        return sorted.AsReadOnly();
    }
}
```

**验收标准**:
- [x] 排序逻辑与原实现一致
- [x] 单元测试通过
- [x] 性能不低于原实现

#### Task 2.3: 实现修饰符集合
**文件**: `src/NumericSystem/Core/ModifierCollection.cs`
**目标**: 管理修饰符的存储、添加、移除和查询

**子任务**:
- [ ] 实现基础存储（HashSet + Dictionary）
- [ ] 实现修饰符合并逻辑
- [ ] 实现修饰符移除逻辑
- [ ] 实现查询方法
- [ ] 添加单元测试

**验收标准**:
- [x] 所有存储操作正常工作
- [x] 查询性能不低于原实现
- [x] 单元测试通过

---

### Phase 3: 引入责任链模式（2-3天）

#### Task 3.1: 创建责任链节点接口
**文件**: `src/NumericSystem/Chain/IModifierChainNode.cs`
```csharp
public interface IModifierChainNode
{
    /// <summary>
    /// 处理修饰符应用
    /// </summary>
    int Process(int currentValue, Numeric context);

    /// <summary>
    /// 设置下一个节点
    /// </summary>
    IModifierChainNode SetNext(IModifierChainNode next);
}
```

#### Task 3.2: 实现通用责任链节点
**文件**: `src/NumericSystem/Chain/GenericChainNode.cs`
```csharp
public class GenericChainNode : IModifierChainNode
{
    private readonly IModifierCollection _collection;
    private readonly IModifierSorter _sorter;
    private readonly IModifierEvaluator _evaluator;
    private IModifierChainNode? _next;

    public GenericChainNode(
        IModifierCollection collection,
        IModifierSorter sorter,
        IModifierEvaluator evaluator)
    {
        _collection = collection;
        _sorter = sorter;
        _evaluator = evaluator;
    }

    public int Process(int currentValue, Numeric context)
    {
        var modifiers = _collection.GetAll();
        var sorted = _sorter.Sort(modifiers);

        foreach (var modifier in sorted)
        {
            currentValue = _evaluator.Evaluate(currentValue, modifier, context);
        }

        return _next != null ? _next.Process(currentValue, context) : currentValue;
    }

    public IModifierChainNode SetNext(IModifierChainNode next)
    {
        _next = next;
        return next;
    }
}
```

**验收标准**:
- [x] 责任链节点正确工作
- [x] 支持链式调用
- [x] 单元测试通过

#### Task 3.3: 创建责任链构建器
**文件**: `src/NumericSystem/Chain/ModifierChainBuilder.cs`
```csharp
public class ModifierChainBuilder
{
    private readonly List<IModifierChainNode> _nodes = new();

    public ModifierChainBuilder AddNode(IModifierChainNode node)
    {
        _nodes.Add(node);
        return this;
    }

    public IModifierChainNode Build()
    {
        if (_nodes.Count == 0)
            throw new InvalidOperationException("至少需要一个节点");

        for (int i = 0; i < _nodes.Count - 1; i++)
        {
            _nodes[i].SetNext(_nodes[i + 1]);
        }

        return _nodes[0];
    }
}
```

**验收标准**:
- [x] 构建器正确组装责任链
- [x] 流畅 API 设计
- [x] 单元测试通过

---

### Phase 4: 重构 Numeric 类（2-3天）

#### Task 4.1: 创建新的 Numeric 实现
**文件**: `src/NumericSystem/NumericV2.cs` (临时)
**目标**: 使用新架构重新实现 Numeric

**变更**:
```csharp
public class Numeric
{
    private readonly IModifierCollection _modifiers;
    private readonly IModifierCollection _constraints;
    private readonly IModifierCollection _conditionals;
    private readonly IModifierChainNode _chain;
    private readonly int _originalValue;

    // 构造函数使用依赖注入
    public Numeric(int value,
                  IModifierCollection? modifiers = null,
                  IModifierCollection? constraints = null,
                  IModifierCollection? conditionals = null,
                  IModifierChainNode? chain = null)
    {
        _originalValue = value.ToFixedPoint();
        _modifiers = modifiers ?? new ModifierCollection();
        _constraints = constraints ?? new ModifierCollection();
        _conditionals = conditionals ?? new ModifierCollection();

        // 构建默认责任链
        _chain = chain ?? BuildDefaultChain();
    }

    private IModifierChainNode BuildDefaultChain()
    {
        return new ModifierChainBuilder()
            .AddNode(new GenericChainNode(_modifiers, new PriorityBasedModifierSorter(), new DefaultModifierEvaluator()))
            .AddNode(new GenericChainNode(_constraints, new PriorityBasedModifierSorter(), new DefaultModifierEvaluator()))
            .AddNode(new GenericChainNode(_conditionals, new PriorityBasedModifierSorter(), new DefaultModifierEvaluator()))
            .Build();
    }

    private int Update()
    {
        if (!_hasUpdate) return _cachedValue;

        _cachedValue = _chain.Process(_originalValue, this);
        _hasUpdate = false;
        return _cachedValue;
    }
}
```

**验收标准**:
- [x] 所有现有单元测试通过
- [x] 性能不低于原实现
- [x] 保持向后兼容的 API

#### Task 4.2: 迁移运算符和扩展方法
**目标**: 确保所有现有 API 继续工作

**子任务**:
- [ ] 运算符重载
- [ ] 扩展方法
- [ ] 属性访问器
- [ ] 隐式转换

**验收标准**:
- [x] 所有运算符正常工作
- [x] 所有扩展方法正常工作
- [x] 向后兼容性测试通过

---

### Phase 5: 测试和验证（2-3天）

#### Task 5.1: 创建单元测试
**文件**: `src/NumericSystem.Tests/Core/`
**目标**: 为所有新组件创建单元测试

**测试列表**:
- [ ] `DefaultModifierEvaluatorTests.cs`
- [ ] `PriorityBasedModifierSorterTests.cs`
- [ ] `ModifierCollectionTests.cs`
- [ ] `GenericChainNodeTests.cs`
- [ ] `ModifierChainBuilderTests.cs`
- [ ] `NumericV2Tests.cs` (功能对比测试)

**验收标准**:
- [x] 测试覆盖率 > 90%
- [x] 所有测试通过
- [x] 性能基准测试通过

#### Task 5.2: 性能对比测试
**文件**: `src/NumericSystem.Tests/Benchmarks/ArchitectureComparisonBenchmarks.cs`
**目标**: 确保新架构性能不降低

**测试场景**:
- [ ] 创建 1000 个修饰符
- [ ] 添加/移除修饰符
- [ ] 计算 FinalValue
- [ ] 缓存命中/未命中
- [ ] 内存使用

**验收标准**:
- [x] 性能差异 < 5%
- [x] 内存分配持平或更少
- [x] GC 压力持平或更少

#### Task 5.3: 集成测试
**目标**: 确保整个系统正常工作

**测试场景**:
- [ ] 复杂修饰符组合
- [ ] 条件修饰符
- [ ] 序列化/反序列化
- [ ] 线程安全
- [ ] 边界情况

**验收标准**:
- [x] 所有 129 个现有测试通过
- [x] 新集成测试通过
- [x] 无回归问题

---

### Phase 6: 文档和清理（1-2天）

#### Task 6.1: 更新 XML 文档注释
**目标**: 为所有新组件添加完整的文档

**子任务**:
- [ ] 所有公共接口
- [ ] 所有公共类
- [ ] 所有公共方法
- [ ] 示例代码

**验收标准**:
- [x] 文档完整性 100%
- [x] 包含使用示例
- [x] 生成文档网站无警告

#### Task 6.2: 创建迁移指南
**文件**: `docs/MIGRATION_GUIDE.md`
**内容**:
- [ ] 架构变更说明
- [ ] API 变更列表
- [ ] 迁移步骤
- [ ] 常见问题
- [ ] 性能对比

#### Task 6.3: 清理旧代码
**子任务**:
- [ ] 删除重复代码
- [ ] 统一命名风格
- [ ] 移除未使用的方法
- [ ] 优化 using 语句

**验收标准**:
- [x] 代码审查通过
- [x] 无编译警告
- [x] 符合编码规范

---

### Phase 7: 高级功能扩展（可选，3-4天）

#### Task 7.1: 插件式修饰符系统
**目标**: 支持运行时动态注册新的修饰符类型

**文件**:
- `src/NumericSystem/Plugin/IModifierPlugin.cs`
- `src/NumericSystem/Plugin/ModifierPluginRegistry.cs`

#### Task 7.2: 自定义评估器
**目标**: 允许用户自定义修饰符评估逻辑

**文件**:
- `src/NumericSystem/Custom/ICustomModifierEvaluator.cs`
- `src/NumericSystem/Custom/CustomEvaluatorRegistry.cs`

#### Task 7.3: 事件系统
**目标**: 在修饰符应用时触发事件

**文件**:
- `src/NumericSystem/Events/ModifierEventArgs.cs`
- `src/NumericSystem/Events/ModifierEventHandler.cs`

---

## 风险和缓解措施

### 风险 1: 性能下降
**概率**: 中
**影响**: 高
**缓解**:
- 在每个阶段进行性能基准测试
- 保留优化空间
- 必要时使用缓存

### 风险 2: API 破坏性变更
**概率**: 低
**影响**: 高
**缓解**:
- 保持向后兼容
- 使用渐进式迁移
- 提供迁移工具

### 风险 3: 测试覆盖不足
**概率**: 中
**影响**: 中
**缓解**:
- TDD 方法
- 每个组件独立测试
- 集成测试覆盖主要场景

### 风险 4: 时间超支
**概率**: 中
**影响**: 中
**缓解**:
- 分阶段实施
- 每个阶段都有可交付成果
- 必要时可以暂停

---

## 时间估算

| 阶段 | 任务数 | 预计时间 | 累计时间 |
|------|--------|----------|----------|
| Phase 1 | 3 | 1-2 天 | 1-2 天 |
| Phase 2 | 3 | 3-4 天 | 4-6 天 |
| Phase 3 | 3 | 2-3 天 | 6-9 天 |
| Phase 4 | 2 | 2-3 天 | 8-12 天 |
| Phase 5 | 3 | 2-3 天 | 10-15 天 |
| Phase 6 | 3 | 1-2 天 | 11-17 天 |
| Phase 7 | 3 | 3-4 天 | 14-21 天 |

**总计**: 约 2-3 周（全职开发）

---

## 成功标准

### 功能性
- [x] 所有现有单元测试通过（129/129）
- [x] 所有现有功能正常工作
- [x] 向后兼容性 100%

### 质量性
- [x] 代码覆盖率 > 90%
- [x] 无编译警告
- [x] 通过代码审查

### 性能性
- [x] 性能不低于原实现（±5%）
- [x] 内存分配持平或更少
- [x] GC 压力持平或更少

### 可维护性
- [x] 单一职责原则
- [x] 开闭原则
- [x] 依赖倒置原则
- [x] 接口隔离原则

---

## 建议的实施顺序

### 优先级 P0（必须）
- Phase 1: 创建基础接口和抽象
- Phase 2: 实现核心组件
- Phase 3: 引入责任链模式
- Phase 4: 重构 Numeric 类
- Phase 5: 测试和验证

### 优先级 P1（推荐）
- Phase 6: 文档和清理

### 优先级 P2（可选）
- Phase 7: 高级功能扩展

---

## 下一步行动

1. **评审本计划** - 与团队讨论架构重构的必要性和范围
2. **创建分支** - 创建 `refactor/architecture-v2` 分支
3. **开始 Phase 1** - 从创建基础接口开始
4. **持续集成** - 每个阶段完成后合并到主分支
5. **文档更新** - 同步更新设计文档

---

## 附录

### A. 相关设计模式
- **策略模式**: IModifierEvaluator
- **责任链模式**: IModifierChainNode
- **构建器模式**: ModifierChainBuilder
- **外观模式**: Numeric（简化后的 API）

### B. 参考资源
- 《设计模式：可复用面向对象软件的基础》
- 《重构：改善既有代码的设计》
- 《Clean Architecture》
- BenchmarkDotNet 文档

### C. 关键指标
- 当前代码行数: ~750 行（Numeric.cs）
- 目标代码行数: ~300 行（Numeric.cs） + 分离的组件
- 当前测试数量: 129
- 目标测试数量: 150+
