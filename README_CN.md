# Numeric System 中文文档

## 简介

Numeric System 是一个用于处理 gameplay 数值需求的工具集，旨在为战斗系统的数值结算提供简单、高效的解决方案。

- 基于 Event Store 的数值变更记录，可溯源，易自校验，保障原始数据安全。
- 使用定点数运算来确保多平台和设备间的数值一致性，并提高网络同步的可靠性。
- 语法简单，支持使用整数，浮点数，分数或百分数为数值添加加法，乘法或自定义修饰。

## 目录

- [Numeric System 中文文档](#numeric-system-中文文档)
  - [简介](#简介)
  - [目录](#目录)
  - [下载和部署](#下载和部署)
    - [从 github 获取](#从-github-获取)
    - [从 npm 获取](#从-npm-获取)
  - [上手指南](#上手指南)
    - [创建第一个 Numeric](#创建第一个-numeric)
    - [使用加法修正器(`AdditionNumericModifier`)为 Numeric 附加修正](#使用加法修正器additionnumericmodifier为-numeric-附加修正)
    - [获得 Numeric 的最终值](#获得-numeric-的最终值)
    - [使用乘法修正器(`FractionNumericModifier`)为 Numeric 附加修正](#使用乘法修正器fractionnumericmodifier为-numeric-附加修正)
    - [修正器的 Name, Tags 和 Count](#修正器的-name-tags-和-count)
    - [使用 Tag 部分修饰加法修正器/Numeric 基准值](#使用-tag-部分修饰加法修正器numeric-基准值)
    - [使用自定义修正器(`CustomNumericModifier`)为 Numeric 附加修正](#使用自定义修正器customnumericmodifier为-numeric-附加修正)
  - [文件路径说明](#文件路径说明)
  - [作者](#作者)
  - [版权说明](#版权说明)

## 下载和部署

### 从 github 获取

```shell
git clone git@github.com:dlqw/NumericSystem.git
```

### 从 npm 获取

```shell
npm i numericsystem
```

## 上手指南

### 创建第一个 Numeric

使用工具需要引用 `WFramework.CoreGameDevKit.NumericSystem;`

```csharp
using WFramework.CoreGameDevKit.NumericSystem;
```

您可以手动创建一个 Numeric 对象，并在其构造时传入一个整数或者浮点数。

```csharp
Numeric health = new Numeric(100);
```

这个值(上个代码段中的“100”)会作为 Numeric 的基准值，并只可读。您可以通过 `GetOriginValue()` 获取其值。如果您希望更改这个值，可以重新创建一个新的 Numeric 对象。

```csharp
// 获取原始值/基准值
var healthBasicValue = health.GetOriginValue();

// 希望改变基准值 => 重新创建一个新的 Numeric 对象
health = new Numeric(200);
```

您也可以向一个 Numeric 类型的对象赋一个整数或者浮点数以创建新的 Numeric 对象。

```csharp
Numeric health = 100;
Debug.Log(health.GetHashCode()); // 402183648

health = 100.67f;
Debug.Log(health.GetHashCode()); // 1146914344
```

应当注意到，health 指向了一个新开辟的 Numeric 对象。

### 使用加法修正器(`AdditionNumericModifier`)为 Numeric 附加修正

您可以手动创建一个加法修正器 `AdditionNumericModifier`, 修正器内部的值同样是不可修改的。您可以访问 `StoreValue` 以获悉。

```csharp
AdditionNumericModifier strongBuff = new AdditionNumericModifier(20);
var buffValue = strongBuff.StoreValue;
```

也可以使用整数或浮点数快速创建。

```csharp
AdditionNumericModifier strongBuff = 20f;
```

数值修正器（`NumericModifier`）, 可以通过 `AddModifier` 方法附加进 Numeric 对象中。也可以通过加法运算符附加。请注意下列示例。

```csharp
// Success
health.AddModifier(strongBuff);
health =  health + 20;
health += 20;
health =  health + strongBuff;
health += strongBuff;

// Error
health.AddModifier(20);
```

修正器移除的语法类似于附加，可以使用 `RemoveModifier` 方法或减法运算符。

**需要注意的其一**，请勿混用

```csharp
health += -20;
health -= 20;
```

前者的意思是为 health 附加一个值为 -20 的修正器，后者的意思为为 health 移除一个值为 20 的修正器。

**需要注意的其二**，请勿将同一个修正器在未移除的情况下多次附加进同一个 Numeric 对象。可以使用重载的加法减法运算符创建临时修正器对象。

**需要注意的其三**，请勿在同一 Numeric 中混用整数和浮点数，对于某一类型的需求，Numeric System 被设计为 `int -> int` or `float -> float`。

### 获得 Numeric 的最终值

您可以通过调用 `FinalValue` 和 `FinalValueF` 分别获取整数或浮点数的结果。

```csharp
Numeric health = 100;

health += 20.3f;

Debug.Log(health.FinalValue);
Debug.Log(health.FinalValueF);
```

### 使用乘法修正器(`FractionNumericModifier`)为 Numeric 附加修正

乘法修正器的使用相对复杂，但您仍可以通过类似的方法去构造他,并附加或移除。

```csharp
health.AddModifier(new FractionNumericModifier(1, 2, FractionNumericModifier.FractionType.Increase));
health *= (200, FractionNumericModifier.FractionType.Override);

health.RemoveModifier(new FractionNumericModifier(1, 2, FractionNumericModifier.FractionType.Increase));
health /= (200, FractionNumericModifier.FractionType.Override);
```

您可以使用 `(分子:Int,分母:Int,乘法类型:FractionNumericModifier.FractionType)来` 构建乘法修正器，直接创建对象或者使用 csharp 的元组语法都是不错的原则，前者更加清晰，后者更加方便。

下面为您介绍两种乘法修正器

```csharp
public enum FractionType
{
    Override, // 覆盖
    Increase, // 增量
}
```

增量修正器会在原有的基础上额外附加新的值，如原本的值为 100，乘法修正器为分子 2，分母 1，那么最终 z 增量的结果就是 300。而覆盖修正器的结果为 200。

公式如下:

$ Increase = (1 + (\frac{numerator}{denominator})) \times Input $
$ Override = (\frac{numerator}{denominator}) \times Input $

在乘法段最开头的程序演示中，我还使用了一个 `(Int,FractionType)` 的元组。其中 Int 代表的是百分比(Precent)。即为 $Precent = \frac{numerator}{100}$

### 修正器的 Name, Tags 和 Count

无论是何种修正器，在其初始化时都有重载包含 (`Name:string`, `Tags:string[]`, `Count:int`)
其中 `Name` 时修正器的唯一身份码，`Name` 相同的修正器被视为同一个修正器，在附加进 Numeric 时后者会在 `Count` 被累加进已经存在的同名修正器后被抛弃。

匿名修正器会被命名为 "DEFAULT MODIFIER"。

`Tags` 的作用在[后文](#使用-tag-部分修饰加法修正器numeric-基准值)中会详细介绍。

`Count` 是修正器内部处理叠加的计数器。

### 使用 Tag 部分修饰加法修正器/Numeric 基准值

Tag 只对加法修正器，乘法修正器和 Numeric 的基准值有意义。
加法修正器内的 Tags 的含义为会被哪些乘法修正器所修饰。
Numeric 同理，其 Tag 默认为也仅为 "SELF"。
乘法修正器的 Tags 表明其会对哪些加法修正器 or Numeric 起作用。

```csharp
Numeric health = 100;

health += (20, new[] { "装备" }, "明光铠", 1);
Debug.Log(health.FinalValue); // 120
health *= (120, FractionType.Override, new[] { "装备" }, "明光铠升级", 1);
Debug.Log(health.FinalValue); // 124
health *= (50, FractionType.Increase, new[] { NumericModifierConfig.TagSelf }, "升级", 1);
Debug.Log(health.FinalValue); // 174
```

### 使用自定义修正器(`CustomNumericModifier`)为 Numeric 附加修正

约束修正器在结算管线的末端被调用，他可以帮助您对某项数值进行特定的约束。以约束玩家的生命值为例。您可以创建一个 `Func<int,int>` 或 `Func<float,float>`，其传入值为目标数值的输入，输出为在您约束后的结果。如下：

```csharp
Numeric health = 100;

Func<int, int> healthLimit = value => Mathf.Clamp(value, 0, 150);

health.AddModifier(new CustomNumericModifier(healthLimit));
health -= healthLimit;
health += new CustomNumericModifier(healthLimit);
```

最终，玩家的生命值被约束在了 0 到 150 之间。自定义修正器(`CustomNumericModifier`)的创建与附加同之前的修正器大同小异，包括隐式类型转换和运算符重载 (`+`, `-`)，不再过多赘述。

如果您为一个 `Numeric` 对象同时附加了相斥的条件，最终执行结果是未定义的，请不要这么做。

## 文件路径说明

```shell
NumericSystem
├── .gitignore
├── README.md
├── package.json
├── src
│   └── NumericSystem
│       ├── FixedPoint.cs
│       ├── Numeric.cs
│       └── NumericModifier.cs
└── LICENSE
```

## 作者

[rdququ](https://github.com/dlqw)

## 版权说明

项目基于 [MIT License](./LICENSE) 发布
