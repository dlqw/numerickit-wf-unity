# Numeric 系统性能基准测试

## 概述

这个目录包含 Numeric 系统的性能基准测试，使用 BenchmarkDotNet 框架。

## 运行基准测试

### 方法 1：使用命令行

```bash
# 运行所有基准测试
cd src
dotnet run -c Release --project NumericSystem.Tests -- --filter *NumericBenchmarks*

# 运行特定类别的基准测试
dotnet run -c Release --project NumericSystem.Tests -- --filter *Basic*
dotnet run -c Release --project NumericSystem.Tests -- --filter *Scalability*
dotnet run -c Release --project NumericSystem.Tests -- --filter *Complex*
dotnet run -c Release --project NumericSystem.Tests -- --filter *Fraction*
dotnet run -c Release --project NumericSystem.Tests -- --filter *Query*
```

### 方法 2：使用 Visual Studio

1. 将配置切换到 **Release** 模式
2. 右键点击 `NumericBenchmarks.cs` 文件
3. 选择 "运行基准测试"（如果有 BenchmarkDotNet 扩展）

## 基准测试类别

### 1. Basic（基础操作）
- `CreateNumeric` - 创建 Numeric 对象
- `AddSingleModifier` - 添加单个修饰符
- `GetFinalValue_SingleModifier` - 获取最终值（单个修饰符）
- `GetFinalValue_Cached` - 获取最终值（缓存性能）

### 2. Scalability（可扩展性）
- `AddManyModifiers_Small` - 添加 10 个修饰符
- `AddManyModifiers_Medium` - 添加 100 个修饰符
- `AddManyModifiers_Large` - 添加 1000 个修饰符

### 3. Complex（复杂场景）
- `ComplexModifierScenario` - 模拟游戏角色属性计算（包含多种修饰符类型）

### 4. Fraction（分数修饰符）
- `AddSingleFractionModifier` - 添加单个分数修饰符
- `AddMultipleFractionModifiers_SameTags` - 添加多个同标签分数修饰符
- `AddMultipleFractionModifiers_DifferentTags` - 添加多个不同标签分数修饰符

### 5. Query（查询操作）
- `GetAddModifierValue` - 获取所有加法修饰符值
- `GetAddModifierValueByTag` - 获取指定标签的修饰符值
- `HasModifier` - 检查是否存在指定修饰符
- `GetAllModifiers` - 获取所有修饰符列表

### 6. Removal（移除操作）
- `RemoveModifier` - 移除修饰符

## 性能指标说明

基准测试会测量以下指标：

1. **Mean（平均值）** - 平均执行时间
2. **StdDev（标准差）** - 执行时间的波动程度
3. **Allocated（分配内存）** - 每次迭代分配的内存
4. **Gen0/Gen1/Gen2 Collections** - GC 回收次数

## 性能优化建议

根据基准测试结果，可以考虑以下优化方向：

1. **缓存效率** - `GetFinalValue_Cached` 应该比首次计算快很多
2. **大量修饰符** - `AddManyModifiers_Large` 应该保持线性增长
3. **内存使用** - 注意 `Allocated` 指标，避免过多的内存分配
4. **GC 压力** - Gen0/Gen1 回收次数应该尽可能少

## 注意事项

1. **必须在 Release 配置下运行**，否则结果不准确
2. 运行基准测试时不要运行其他程序，避免干扰结果
3. 基准测试会运行多次（warmup + iterations），需要一些时间
4. 不同机器的结果会有差异，重点观察相对性能变化

## 基准测试结果示例

```
Benchmark                               Mean         Error        StdDev
CreateNumeric                          15.23 ns     0.45 ns      0.38 ns
AddSingleModifier                      85.67 ns     2.31 ns      2.15 ns
GetFinalValue_SingleModifier           52.34 ns     1.78 ns      1.65 ns
GetFinalValue_Cached                    5.12 ns     0.15 ns      0.13 ns
AddManyModifiers_Large                845.67 ns    23.45 ns     21.78 ns
ComplexModifierScenario              1,234.56 ns    45.67 ns     42.34 ns
```
