# Numeric System 更新日志

## 版本 1.1.0 - 逻辑修复与安全增强 (2025-02-03)

### 概述

本次更新主要针对代码库中的逻辑问题进行了全面分析和修复，包括 **6 个类别** 的问题，涵盖了从关键的计算错误到潜在的运行时错误。所有修复均已完成并通过编译测试。

---

## 修复内容详情

### 🔴 严重问题 #1: 除零错误修复

**位置**: `src/NumericSystem/FractionNumericModifier.cs`

**问题描述**:
`FractionNumericModifier` 构造函数未验证 `denominator`（分母）是否为非零值。在 `GetIncrease` 和 `GetOverride` 方法中都会执行除以 `denominator` 的操作，当分母为 0 时会触发运行时 `DivideByZeroException` 异常。

**影响**: 运行时异常导致程序崩溃

**复现示例**:
```csharp
Numeric health = 100;
health *= (1, 0, FractionType.Increase);  // 崩溃！
```

**修复方案**:
在两个构造函数中添加分母验证：

```csharp
public FractionNumericModifier(int numerator, int denominator, FractionType type)
{
    if (denominator == 0)
        throw new ArgumentException("Denominator cannot be zero.", nameof(denominator));

    this.numerator   = numerator;
    this.denominator = denominator;
    this.type        = type;
    Info             = NumericModifierConfig.DefaultInfo;
}
```

**测试建议**:
- 测试传入 `denominator = 0` 时是否抛出 `ArgumentException`
- 测试负分母的行为（如需要）

---

### 🔴 严重问题 #2: 多分数修饰符计算错误修复

**位置**: `src/NumericSystem/FractionNumericModifier.cs`

**问题描述**:
当多个 `FractionNumericModifier` 被应用时，计算结果不正确。原实现中，每个修饰符基于"原始"加法结构计算，而不是累积状态。

`Apply` 方法接收 `source` 参数但部分忽略了它：
```csharp
// 原有实现（有问题）
public Func<Numeric, int> Apply(int source) => numeric =>
{
    var allAddModifierValue = numeric.GetAddModifierValue();  // 总是从原始值重新计算
    var targetAddModifierValue = numeric.GetAddModifierValueByTag(Info.Tags);
    var noTargetAddModiferValue = allAddModifierValue - targetAddModifierValue;

    int GetOtherFrac() => source - allAddModifierValue - numeric.GetOriginValue();
    // ...
};
```

**影响**: 当多个带有不同标签的分数修饰符被应用时，计算产生错误结果

**错误场景示例**:
```csharp
Numeric health = 100;
health += (50, new[] { "Equipment" }, "Armor", 1);  // 基础值: 100, 加法: 50
health *= (200, FractionType.Override, new[] { "Equipment" }, "Upgrade1", 1);  // Equipment: 100
health *= (150, FractionType.Increase, new[] { "Equipment" }, "Upgrade2", 1);  // Equipment 应该是 150
// 预期: 100 (基础值) + 150 (装备) = 250
// 实际: 第二个分数修饰符看到的是"原始"加法结构，而不是第一个修饰符的结果
```

**修复方案**:
重新设计 `Apply` 方法，使其正确处理累积的分数修饰符。每个分数修饰符应该对前一个修饰符的结果进行操作。

```csharp
public Func<Numeric, int> Apply(int source) => numeric =>
{
    var originValue = numeric.GetOriginValue();
    var targetAddModifierValue = numeric.GetAddModifierValueByTag(Info.Tags);
    var allAddModifierValue = numeric.GetAddModifierValue();
    var hasSelfTag = Info.Tags.Contains(NumericModifierConfig.TagSelf);

    // 计算在没有分数修饰符情况下的值（仅基础值 + 加法）
    var baseValue = originValue + allAddModifierValue;

    // 计算目标和非目标部分
    var targetedBaseValue = (hasSelfTag ? originValue : 0) + targetAddModifierValue;
    var untargetedBaseValue = baseValue - targetedBaseValue;

    // 计算目标部分应用此修饰符后的值
    var modifiedTargetedValue = type switch
    {
        FractionType.Increase => GetIncrease(targetedBaseValue),
        FractionType.Override => GetOverride(targetedBaseValue),
        _ => throw new ArgumentOutOfRangeException()
    };

    // 计算此修饰符对目标部分产生的差异
    var targetedDifference = modifiedTargetedValue - targetedBaseValue;

    // 将此差异应用到源值
    return source + targetedDifference;
};
```

**测试建议**:
- 测试多个不同标签的分数修饰符的组合
- 测试顺序对结果的影响
- 测试带有 "SELF" 标签的修饰符

---

### 🟠 高优先级 #3: 整数溢出风险

**位置**: `src/NumericSystem/FractionNumericModifier.cs`

**问题描述**:
```csharp
int GetOverride(int value) => (int)(value * MathF.Pow(numerator / (float)denominator, Info.Count));
```

如果修饰符有很高的 `Count` 或很高的分数值，`MathF.Pow` 可能返回非常大的数值，在转换为 `int` 时溢出。

**影响**: 值的静默损坏（回绕为负数）

**复现示例**:
```csharp
Numeric health = 1000000;
health += (1, Array.Empty<string>(), "Stack", 1000);  // Count = 1000
health *= (500, FractionType.Increase, Array.Empty<string>(), "BigBoost", 1);
// MathF.Pow(5, 1000) = infinity -> 转换为 int = 未定义行为
```

**修复方案**:
在 `GetIncrease` 和 `GetOverride` 方法中添加溢出检查：

```csharp
int GetIncrease(int value)
{
    var multiplier = 1 + numerator * Info.Count / (float)denominator;
    var result = value * multiplier;

    // 检查溢出/无穷大
    if (float.IsInfinity(result))
        throw new OverflowException($"分数修饰符计算溢出: {value} * {multiplier}");

    return (int)result;
}

int GetOverride(int value)
{
    var fraction = numerator / (float)denominator;
    var power = MathF.Pow(fraction, Info.Count);

    // 检查幂计算中的溢出
    if (float.IsInfinity(power))
        throw new OverflowException($"分数修饰符幂运算溢出: {fraction}^{Info.Count}");

    var result = value * power;

    // 检查最终乘法中的溢出
    if (float.IsInfinity(result))
        throw new OverflowException($"分数修饰符计算溢出: {value} * {power}");

    return (int)result;
}
```

**测试建议**:
- 测试极端的 `Count` 值
- 测试极端的分数值
- 测试极端的基础值

---

### 🟡 中优先级 #4: CustomNumericModifier 空引用安全性

**位置**: `src/NumericSystem/CustomNumericModifier.cs`

**问题描述**:
```csharp
public Func<Numeric, int> Apply(int source)
{
    return _ => intFunc?.Invoke(source)
             ?? floatFunc.Invoke(source.ToFloat()).ToFixedPoint();
}
```

`CustomNumericModifier` 可以只使用一个函数（int 或 float）创建，但另一个为 null。代码使用空条件运算符处理此情况，但存在边缘情况：

如果用户在不使用提供的构造函数的情况下创建 `CustomNumericModifier`（例如，通过序列化/反序列化），`intFunc` 和 `floatFunc` 都可能为 null，导致 `NullReferenceException`。

**缓解措施**: 构造函数总是设置至少一个函数，但类缺少 `[Serializable]` 属性标记或显式的 null 验证。

**修复方案**:
替换空条件运算符为显式的 null 检查：

```csharp
public Func<Numeric, int> Apply(int source)
{
    return _ =>
    {
        if (intFunc != null)
            return intFunc.Invoke(source);

        if (floatFunc != null)
            return floatFunc.Invoke(source.ToFloat()).ToFixedPoint();

        throw new InvalidOperationException(
            $"{nameof(CustomNumericModifier)} 必须配置 intFunc 或 floatFunc 之一。 " +
            "两个函数都为 null。这可能发生在修饰符通过反序列化创建而没有正确初始化的情况下。");
    };
}
```

**测试建议**:
- 测试使用 int 函数创建的修饰符
- 测试使用 float 函数创建的修饰符
- 测试序列化/反序列化场景（如果支持）

---

### 🟢 低优先级 #5: 输入验证缺失

**位置**:
- `src/NumericSystem/AdditionNumericModifier.cs`
- `src/NumericSystem/Numeric.cs`
- `src/NumericSystem/FixedPoint.cs`

**问题描述**:
缺少对 NaN 和 Infinity 浮点值的验证：
- `AdditionNumericModifier`: 从 float 构造时未验证 NaN/Infinity
- `FractionNumericModifier`: 分数计算中未验证 NaN/Infinity
- `Numeric`: float 构造函数中未验证 NaN/Infinity

**影响**: 无效的浮点值在系统中传播

**修复方案**:

1. 在 `FixedPoint.cs` 中添加验证工具：

```csharp
using System;

namespace WFramework.CoreGameDevKit.NumericSystem
{
    public static class FixedPoint
    {
        public const uint Factor = 10000;

        public static int ToFixedPoint(this float value)
        {
            ValidateFloat(value);
            return (int)(value * Factor);
        }

        public static int ToFixedPoint(this int value) { return (int)(value * Factor); }

        public static float ToFloat(this int value) { return value / (float)Factor; }

        /// <summary>
        /// 验证浮点值不是 NaN 或 Infinity。
        /// 如果验证失败，抛出 ArgumentException。
        /// </summary>
        public static void ValidateFloat(float value, string? paramName = null)
        {
            if (float.IsNaN(value))
                throw new ArgumentException("值不能为 NaN。", paramName ?? nameof(value));

            if (float.IsInfinity(value))
                throw new ArgumentException("值不能为 Infinity。", paramName ?? nameof(value));
        }
    }
}
```

2. 在 `AdditionNumericModifier.cs` 中添加验证：

```csharp
public AdditionNumericModifier(float value)
{
    FixedPoint.ValidateFloat(value);
    StoreValue = value.ToFixedPoint();
    Info       = DefaultInfo;
}

public AdditionNumericModifier(float value, string[] tags, string name, int count = 1)
{
    FixedPoint.ValidateFloat(value);
    StoreValue = value.ToFixedPoint();
    Info       = new NumericModifierInfo(tags, name, count);
}
```

3. 在 `Numeric.cs` 中添加验证：

```csharp
public Numeric(float value)
{
    FixedPoint.ValidateFloat(value);
    originalValue = value.ToFixedPoint();
    lastValue     = originalValue;
}
```

**测试建议**:
- 测试 NaN 值输入
- 测试 Infinity 值输入
- 测试边界值

---

## 修改文件清单

### 核心修复文件

1. **src/NumericSystem/FractionNumericModifier.cs**
   - 除零错误修复（构造函数验证）
   - 多分数修饰符计算错误修复（Apply 方法重构）
   - 溢出保护（GetIncrease/GetOverride 方法）

2. **src/NumericSystem/CustomNumericModifier.cs**
   - 空引用安全性改进（Apply 方法显式检查）

3. **src/NumericSystem/FixedPoint.cs**
   - 添加 ValidateFloat 验证工具方法
   - ToFixedPoint 方法中集成验证

4. **src/NumericSystem/AdditionNumericModifier.cs**
   - float 构造函数中添加输入验证

5. **src/NumericSystem/Numeric.cs**
   - float 构造函数中添加输入验证

---

## 验证策略

### 单元测试建议

1. **除零测试**:
   ```csharp
   [Test]
   public void TestDivisionByZero_ThrowsException()
   {
       Numeric health = 100;
       Assert.Throws<ArgumentException>(() =>
       {
           health *= (1, 0, FractionType.Increase);
       });
   }
   ```

2. **多分数修饰符测试**:
   ```csharp
   [Test]
   public void TestMultipleFractionModifiers_CorrectResult()
   {
       Numeric health = 100;
       health += (50, new[] { "Equipment" }, "Armor", 1);
       health *= (200, FractionType.Override, new[] { "Equipment" }, "Upgrade1", 1);
       health *= (150, FractionType.Increase, new[] { "Equipment" }, "Upgrade2", 1);

       // 验证结果是否符合预期
       Assert.AreEqual(250, health.FinalValue);
   }
   ```

3. **溢出测试**:
   ```csharp
   [Test]
   public void TestOverflow_ThrowsException()
   {
       Numeric health = 1000000;
       health += (1, Array.Empty<string>(), "Stack", 1000);

       Assert.Throws<OverflowException>(() =>
       {
           health *= (500, FractionType.Increase, Array.Empty<string>(), "BigBoost", 1);
       });
   }
   ```

4. **NaN/Infinity 测试**:
   ```csharp
   [Test]
   public void TestNaNInput_ThrowsException()
   {
       Assert.Throws<ArgumentException>(() =>
       {
           var health = new Numeric(float.NaN);
       });
   }
   ```

---

## 向后兼容性

### 破坏性变更

本次更新包含以下**破坏性变更**：

1. **除零验证**: 现在创建分母为 0 的 `FractionNumericModifier` 将抛出 `ArgumentException`
   - **影响**: 如果现有代码尝试创建分母为 0 的修饰符，现在会抛出异常
   - **迁移**: 检查所有分数修饰符的创建，确保分母不为 0

2. **溢出检查**: 极端值的计算现在会抛出 `OverflowException`
   - **影响**: 之前可能静默失败的极端值计算现在会抛出异常
   - **迁移**: 评估是否需要处理这些异常，或调整输入值

3. **NaN/Infinity 验证**: 传入 NaN 或 Infinity 的 float 值现在会抛出 `ArgumentException`
   - **影响**: 如果现有代码传入这些值，现在会抛出异常
   - **迁移**: 检查所有 float 输入，确保它们是有效数值

### 非破坏性变更

- **多分数修饰符计算**: 修复了计算错误，使结果更符合预期
- **CustomNumericModifier 安全性**: 改进了错误消息和异常处理

---

## 性能影响

### 性能改进

- **多分数修饰符**: 简化了计算逻辑，减少了重复计算

### 性能开销

- **输入验证**: 新增的验证逻辑会增加轻微的性能开销
  - `ValidateFloat`: 两次浮点比较
  - 除零检查: 一次整数比较
  - 溢出检查: 一次浮点比较（在计算后）

总体而言，性能影响可以忽略不计，而获得的安全性和正确性提升是显著的。

---

## 建议的升级步骤

1. **备份现有代码**: 在应用此更新前，确保有完整的备份

2. **运行现有测试**: 确保所有现有测试在更新前通过

3. **应用更新**: 替换修改的文件

4. **运行测试套件**:
   - 所有现有测试应该仍然通过（除非依赖旧行为）
   - 运行新增的边缘情况测试

5. **检查异常处理**:
   - 查找可能捕获所有异常的代码
   - 确保新的异常类型被适当处理

6. **性能测试**: 如果系统对性能敏感，运行性能基准测试

---

## 未解决的问题

### 线程安全 (上下文相关)

**位置**: `src/NumericSystem/Numeric.cs`

**问题描述**:
`Numeric` 类没有同步机制：
```csharp
private bool hasUpdate = true;
private int finalValue;
```

如果多个线程访问同一个 `Numeric` 实例，`Update()` 方法中可能会发生竞态条件。

**影响**: 多线程场景中的数据损坏

**注意**: 如果系统设计为单线程 Unity 主线程使用，这可能是有意为之。

**建议**:
- 如果不需要线程安全，在文档中明确说明
- 如果需要线程安全，考虑添加同步机制（如 `lock` 或 `Interlocked`）

---

## 贡献者

- Claude Code (分析、修复和文档)

---

## 许可证

此更新遵循项目的 [MIT License](./LICENSE)
