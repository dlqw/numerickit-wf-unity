# Numeric System 中文文档

<p align="center">
<img alt="Static Badge" src="https://img.shields.io/badge/field-gameplay-red">
<img src="https://img.shields.io/badge/script-csharp-yellow">
<img src="https://img.shields.io/badge/dotnet-Standard 2.1-green">
<img src="https://img.shields.io/badge/framework-WFramework-blue">
<img alt="Static Badge" src="https://img.shields.io/badge/tests-129%20passed-success">
<img alt="Static Badge" src="https://img.shields.io/badge/version-1.2.0-blue">
</p>

## 简介

Numeric System 是一个功能强大、灵活性高的工具集，旨在为战斗系统的数值结算提供简单、高效的解决方案。

- **基于 Event Store 的数值变更记录** - 可溯源，易自校验，保障原始数据安全
- **定点数运算** - 确保多平台和设备间的数值一致性，提高网络同步可靠性
- **语法简单** - 支持整数、浮点数、分数或百分比的加法、乘法修饰
- **可扩展架构** - 支持自定义修饰符、条件修饰符和修饰符优先级
- **线程安全操作** - 为多线程环境提供线程安全包装
- **序列化支持** - 内置修饰符序列化/反序列化功能

## 更新日志

### 版本 1.2.0 - 高级功能与性能优化 (2025-02-03)

此主要版本添加了高级功能，同时保持 100% 向后兼容：

**新功能：**
- ✨ **修饰符优先级系统** - 使用细粒度优先级控制修饰符应用顺序
- ✨ **条件修饰符** - 基于动态条件应用修饰符（谓词条件）
- ✨ **序列化支持** - 支持修饰符的序列化/反序列化，用于存档/读档
- ✨ **线程安全包装** - 为多线程环境提供线程安全的 `Numeric` 操作
- ✨ **性能基准测试** - 使用 BenchmarkDotNet 的全面基准测试套件
- ✨ **诊断工具** - 增强调试和诊断功能
- ✨ **流畅 API** - 丰富的扩展方法和构建器模式，提升开发体验

**改进：**
- 🚀 测试覆盖率从 117 增加到 **129 个测试**（100% 通过率）
- 🚀 增强错误消息和验证
- 🚀 改进缓存机制
- 🚀 提升修饰符查询性能

**文档：**
- 📚 完整的 XML 文档注释覆盖
- 📚 性能基准测试文档
- 📚 架构重构计划

详细信息请参阅 [CHANGELOG.md](./CHANGELOG.md)

### 版本 1.1.0 - 逻辑修复与安全增强 (2025-02-03)

- **修复除零错误**: 在 `FractionNumericModifier` 构造函数中添加验证
- **修复多分数修饰符错误**: 重新设计 `Apply` 方法
- **添加溢出保护**: 防止极端计算导致的静默值损坏
- **改进 CustomNumericModifier 安全性**: 增强空值检查

详细信息请参阅 [changelogs/1.1.0_CN.md](./changelogs/1.1.0_CN.md)

## 目录

- [简介](#简介)
- [更新日志](#更新日志)
- [下载和部署](#下载和部署)
  - [从 GitHub 获取](#从-github-获取)
  - [从 npm 获取](#从-npm-获取)
- [快速开始](#快速开始)
  - [创建第一个 Numeric](#创建第一个-numeric)
  - [附加修饰符](#附加修饰符)
  - [获取最终值](#获取最终值)
  - [使用乘法修饰符](#使用乘法修饰符)
- [高级功能](#高级功能)
  - [修饰符优先级系统](#修饰符优先级系统)
  - [条件修饰符](#条件修饰符)
  - [序列化](#序列化)
  - [线程安全操作](#线程安全操作)
  - [性能基准测试](#性能基准测试)
- [API 参考](#api-参考)
- [文件路径说明](#文件路径说明)
- [版权说明](#版权说明)

## 下载和部署

### 从 GitHub 获取

```shell
git clone git@github.com:dlqw/NumericSystem.git
```

### 从 npm 获取

```shell
npm i numericsystem
```

## 快速开始

### 创建第一个 Numeric

引用命名空间：

```csharp
using WFramework.CoreGameDevKit.NumericSystem;
```

创建 Numeric 对象：

```csharp
Numeric health = new Numeric(100);
```

此值（100）作为基础值是只读的。可以通过 `GetOriginValue()` 获取原始值：

```csharp
var basicValue = health.GetOriginValue();
```

### 附加修饰符

#### 加法修饰符

```csharp
// 使用运算符（推荐）
health += 20;
health -= 10;

// 使用显式方法
health.AddModifier(new AdditionNumericModifier(20));
health.RemoveModifier(new AdditionNumericModifier(10));
```

#### 乘法修饰符

```csharp
// 百分比增加
health *= (150, FractionType.Increase);  // +50%

// 百分比覆盖
health *= (200, FractionType.Override);  // ×2.0

// 移除修饰符
health /= (150, FractionType.Increase);
```

### 获取最终值

```csharp
Numeric health = 100;
health += 20.3f;

Debug.Log(health.FinalValue);   // 120 (int)
Debug.Log(health.FinalValueF); // 120.3f (float)
```

### 使用标签和名称

```csharp
// 创建带标签和名称的修饰符
health += (20, new[] { "Equipment" }, "Armor", 1);
health *= (120, FractionType.Override, new[] { "Equipment" }, "ArmorUpgrade", 1);
health *= (50, FractionType.Increase, new[] { "Buff" }, "StrengthBoost", 1);
```

### 使用自定义修饰符

```csharp
Numeric health = 100;

// 限制生命值范围
health.ClampMax(150, "MaxHealthCap");
health.ClampMin(0, "MinHealthCap");

// 或使用自定义函数
Func<int, int> healthLimit = value => Mathf.Clamp(value, 0, 150);
health.AddModifier(new CustomNumericModifier(healthLimit));
```

## 高级功能

### 修饰符优先级系统

使用优先级控制修饰符的应用顺序：

```csharp
var health = new Numeric(100);

// 添加不同优先级的修饰符
health += (50, new[] { "Base" }, "RaceBonus", 1, ModifierPriority.Base);      // 100
health += (30, new[] { "Base" }, "ClassBonus", 1, ModifierPriority.Base);     // 100
health += (50, new[] { "Equipment" }, "Armor", 1, ModifierPriority.Equipment); // 200
health += (30, new[] { "Buff" }, "Strength", 1, ModifierPriority.Buff);       // 300
health *= (50, FractionType.Increase, Array.Empty<string>(), "Multiplier", 1, ModifierPriority.Multiplier); // 500

// 应用顺序：Base → Equipment → Buff → Multiplier
```

**优先级级别：**
- `Critical` (0) - 最高优先级
- `Base` (100) - 基础属性
- `Equipment` (200) - 装备修饰符
- `Buff` (300) - Buff/Debuff 效果
- `Skill` (400) - 技能加成
- `Default` (400) - 默认优先级
- `Multiplier` (500) - 百分比修饰符
- `Clamp` (600) - 约束修饰符

### 条件修饰符

基于动态条件应用修饰符：

```csharp
var health = new Numeric(100);

// 条件：生命值低于 30%
var lowHpCondition = ConditionBuilder.Where(h => h.FinalValue < 30);
var emergencyShield = ConditionalNumericModifier.ConditionalAdd(
    lowHpCondition,
    50,
    "EmergencyShield"
);
health.AddModifier(emergencyShield);

// 使用 AND/OR/NOT 的复杂条件
var complexCondition = ConditionBuilder
    .Where(h => h.FinalValue < 50)
    .And(h => h.GetAddModifierValueByTag(new[] { "Buff" }) < 1000000)
    .Build();

health.AddConditionalModifier(
    complexCondition,
    new AdditionNumericModifier(30, Array.Empty<string>(), "ComplexBonus")
);
```

### 序列化

保存和加载修饰符状态：

```csharp
var health = new Numeric(100);
health += 50;
health *= (150, FractionType.Increase);

// 序列化
var data = health.Serialize();

// 反序列化
var restored = data.Deserialize();
Assert.Equal(health.FinalValue, restored.FinalValue);
```

**支持的修饰符：**
- ✅ `AdditionNumericModifier`
- ✅ `FractionNumericModifier`
- ⚠️ `CustomNumericModifier`（包含委托）
- ⚠️ `ConditionalNumericModifier`（包含委托）

### 线程安全操作

多线程场景使用 `ThreadSafeNumeric`：

```csharp
var safeHealth = new ThreadSafeNumeric(100);

// 线程安全操作
safeHealth += 50;
safeHealth.AddModifier(new FractionNumericModifier(150, FractionType.Increase));

// 线程安全读取
var value = safeHealth.FinalValue;

// 使用回调的线程安全操作
safeHealth.Read(numeric =>
{
    Debug.Log($"生命值: {numeric.FinalValue}");
    Debug.Log($"修饰符: {numeric.GetAllModifiers().Count}");
});

safeHealth.Write(numeric =>
{
    numeric += 20;
});
```

### 性能基准测试

运行基准测试以测量性能：

```bash
cd src
dotnet run -c Release --project NumericSystem.Tests -- --filter *NumericBenchmarks*
```

基准测试类别：
- **Basic** - 创建、修改、计算
- **Scalability** - 大量修饰符（10、100、1000）
- **Complex** - 真实场景
- **Fraction** - 分数修饰符性能
- **Query** - 查询操作

详见 [src/NumericSystem.Tests/Benchmarks/README.md](./src/NumericSystem.Tests/Benchmarks/README.md)

## API 参考

### 流畅 API

```csharp
// 构建器模式
var health = Numeric.Build(100, builder =>
{
    builder.AddEquipment(50, "Armor");
    builder.AddBuff(30, "Strength");
    builder.BoostBase(150, "BaseBoost");
    builder.WithMaxLimit(500, "MaxHP");
});

// 扩展方法
health.AddEquipment(20, "Helmet");
health.AddBuff(10, "Potion");
health.ClampMax(300);

// 条件扩展
health.AddIf(h => h.FinalValue < 100, 50, "EmergencyHeal");
health.MultiplyIf(
    ConditionBuilder.Where(h => h.FinalValue > 200).Build(),
    150,
    FractionType.Increase
);
```

### 诊断工具

```csharp
var health = new Numeric(100);
health += 50;
health *= (150, FractionType.Increase);

// 获取修饰符统计
var stats = health.GetModifierStats();
foreach (var stat in stats)
{
    Debug.Log($"{stat.Key}: {stat.Value}");
}

// 输出详细信息
health.Dump("玩家生命值");

// 检查缓存状态
Debug.Log(health.GetCacheStatus());
```

## 文件路径说明

```shell
NumericSystem
├── .gitignore
├── README.md
├── README_CN.md
├── CHANGELOG.md
├── ARCHITECTURE_REFACTORING_PLAN.md
├── package.json
├── src
│   └── NumericSystem
│       ├── Core/                          # 核心抽象
│       ├── Chain/                         # 责任链模式
│       ├── Serialization/                 # 序列化支持
│       ├── FixedPoint.cs
│       ├── Numeric.cs
│       ├── NumericModifier*.cs            # 修饰符实现
│       ├── NumericExtensions.cs           # 扩展方法
│       ├── ThreadSafeNumeric.cs           # 线程安全包装
│       └── DiagnosticHelper.cs            # 诊断工具
│   └── NumericSystem.Tests
│       ├── Benchmarks/                    # 性能基准测试
│       └── *Tests.cs                      # 单元测试
└── LICENSE
```

## 测试

项目包含全面的单元测试：

```bash
cd src
dotnet test
```

**测试覆盖：**
- 129 个测试（100% 通过率）
- 覆盖所有主要功能
- 边界情况和错误处理

## 版权说明

本项目采用 [MIT License](./LICENSE) 发布
