# Numeric系统类型一致性问题分析

## 问题根源

Numeric类有两种构造方式：
1. `Numeric(int value)` - 直接存储int值（普通整数）
2. `Numeric(float value)` - 转为定点数存储（value * 10000）

这导致内部表示不一致！

## 测试案例

```csharp
var numeric = new Numeric(100);      // originalValue = 100（普通整数）
numeric += 20.5f;                     // StoreValue = 205000（定点数）
// 结果：100 + 205000 = 205100 ❌ 错误！
```

## 正确的处理方式

**方案1：统一使用定点数**
- 修改int构造函数：`originalValue = value * 10000`
- 修改FinalValue：`return finalValue / 10000`
- 优点：内部表示一致
- 缺点：破坏现有行为

**方案2：禁止混合使用**
- 添加运行时检查：检测int构造+float修饰符
- 优点：保持向后兼容
- 缺点：增加复杂度

**方案3：智能转换单位**
- Apply方法检测source是否为定点数，自动转换
- 优点：透明处理
- 缺点：难以判断source的单位

## 结论

当前系统的设计存在根本缺陷：**没有统一的内部表示**。

建议：修改int构造函数，统一使用定点数表示。
