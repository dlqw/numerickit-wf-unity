# Numeric System 更新日志

## [1.2.0] - 2025-02-03

### 新增功能 ✨

#### 修饰符优先级系统
- 实现了完整的修饰符优先级系统
- 支持自定义优先级（Critical、Base、Equipment、Buff、Skill、Multiplier、Clamp 等）
- 修饰符按优先级从小到大依次应用
- 同优先级修饰符按名称和计数排序确保稳定性

#### 条件修饰符系统
- 实现了条件修饰符（`ConditionalNumericModifier`）
- 支持基于 `Numeric` 状态的条件评估
- 提供多种条件组合方式（AND、OR、NOT）
- 提供流畅的 `ConditionBuilder` API
- 支持条件加法和分数修饰符

#### 修饰符序列化支持
- 实现了修饰符序列化系统
- 支持 `AdditionNumericModifier` 和 `FractionNumericModifier` 的序列化
- 提供了 `NumericData` 和 `ModifierData` DTO
- 支持完整的序列化/反序列化循环

#### 线程安全支持
- 实现了 `ThreadSafeNumeric` 包装类
- 使用 `ReaderWriterLockSlim` 优化并发读性能
- 支持线程安全的修饰符添加、移除和查询
- 提供安全的读写操作 API

#### 性能基准测试
- 使用 BenchmarkDotNet 创建了完整的性能测试套件
- 覆盖基础操作、可扩展性、复杂场景、分数修饰符、查询操作等多个维度
- 提供详细的性能指标（均值、标准差、内存分配、GC 压力）

#### 调试和诊断工具
- 实现了 `DiagnosticHelper` 辅助类
- 提供详细的修饰符信息输出
- 支持缓存状态查询
- 提供修饰符统计功能

#### 扩展方法和流畅 API
- 提供了丰富的扩展方法
- 支持链式构建器（`NumericBuilder`）
- 提供便捷的条件修饰符 API
- 支持约束修饰符（`ClampMin`、`ClampMax`、`ClampRange`）

### 改进 🚀

#### 测试覆盖
- 从 117 个测试增加到 **129 个测试** ✅
- 所有测试通过（100% 通过率）
- 新增条件修饰符测试（10 个）
- 新增修饰符优先级测试（8 个）
- 新增序列化测试（14 个）
- 新增线程安全测试（7 个）

#### 代码质量
- 完整的 XML 文档注释覆盖
- 改进的错误消息和异常处理
- 增强的输入验证
- 优化的缓存机制

#### 性能优化
- 优化了修饰符查询性能
- 改进了缓存失效策略
- 减少了不必要的计算

### 修复 🐛

#### 逻辑修复
- 修复了 `FractionNumericModifier` 在多个修饰符组合时的计算问题
- 修复了修饰符排序的稳定性问题
- 修复了缓存失效的边界情况

#### 类型安全
- 将 `ModifierType` 改为 `public` 以支持序列化
- 为所有修饰符类添加 `[Serializable]` 属性
- 为 `NumericModifierInfo` record 添加序列化支持

### 文档 📚

- 新增性能基准测试使用文档
- 更新了 API 文档
- 添加了架构重构计划文档
- 完善了代码注释

### 技术细节 🔧

#### 修饰符应用流程
```
原始值
  ↓
[优先级排序]
  ↓
普通修饰符（Addition + Fraction）
  ↓
约束修饰符（Custom）
  ↓
条件修饰符（Conditional）
  ↓
最终值
```

#### 新增文件
```
src/NumericSystem/
├── ConditionalNumericModifier.cs      # 条件修饰符
├── ICondition.cs                      # 条件接口和实现
├── ModifierPriority.cs                # 优先级枚举
├── NumericExtensions.cs               # 扩展方法
├── ThreadSafeNumeric.cs               # 线程安全包装
├── DiagnosticHelper.cs                # 诊断工具
└── Serialization/                     # 序列化支持
    ├── ModifierData.cs
    └── NumericSerializationExtensions.cs

src/NumericSystem.Tests/
├── ConditionalModifierTests.cs        # 条件修饰符测试
├── ModifierPriorityTests.cs           # 优先级测试
├── NumericSerializationTests.cs      # 序列化测试
├── ThreadSafeNumericTests.cs          # 线程安全测试
├── DiagnosticHelperTests.cs           # 诊断工具测试
└── Benchmarks/                        # 性能基准测试
    ├── NumericBenchmarks.cs
    └── README.md
```

### 兼容性 ⚠️

- 保持向后兼容
- 所有现有 API 继续工作
- 推荐使用新的流畅 API 以获得更好的开发体验

### 迁移指南

从 1.1.0 升级到 1.2.0 无需任何代码更改，所有现有代码继续工作。

可选：使用新的优先级系统和条件修饰符来增强功能。

---

## [1.1.0] - 2025-02-03

### 关键 Bug 修复
- 修复除零错误
- 修复多分数修饰符计算错误
- 添加溢出保护
- 改进输入验证

详细信息见 [changelogs/1.1.0_CN.md](./1.1.0_CN.md)

---

## [1.0.0] - 初始版本

### 基础功能
- 定点数运算
- 加法修饰符
- 分数修饰符
- 自定义修饰符
- 标签系统
- 运算符重载
