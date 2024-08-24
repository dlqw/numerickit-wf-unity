<h1 style="text-align:center">
  Numeric System
</h1>
<p style="text-align:center">
  HuaYe Studio rdququ
</p>

<p style="text-align:center">
<img alt="Static Badge" src="https://img.shields.io/badge/field-gameplay-red">
<img src="https://img.shields.io/badge/script-csharp-yellow">
<img src="https://img.shields.io/badge/dotnet-Standard 2.1-green">
<img src="https://img.shields.io/badge/framework-WFramework-blue">
<img alt="Static Badge" src="https://img.shields.io/badge/readme-%E4%B8%AD%E6%96%87%E6%96%87%E6%A1%A3-red?link=https%3A%2F%2Fgithub.com%2Fdlqw%2FNumericSystem%2Fblob%2Fmain%2FREADME_CN.md">
</p>

## Introduction

The Numeric System is a toolset designed to address the numerical needs of gameplay, aiming to provide a simple and efficient solution for handling combat system calculations.

- **Event Store-based Numeric Change Tracking:** Ensures traceability, easy self-verification, and security of original data.
- **Fixed-point Arithmetic:** Guarantees numerical consistency across platforms and devices, enhancing network synchronization reliability.
- **Simple Syntax:** Supports the addition of integers, floating points, fractions, or percentages to numerical values using addition, multiplication, or custom modifiers.

## Table of Contents

- [Introduction](#introduction)
- [Table of Contents](#table-of-contents)
- [Download and Deployment](#download-and-deployment)
  - [Get from GitHub](#get-from-github)
  - [Get from npm](#get-from-npm)
- [Getting Started](#getting-started)
  - [Creating the First Numeric](#creating-the-first-numeric)
  - [Attaching `AdditionNumericModifier` to Numeric](#attaching-additionnumericmodifier-to-numeric)
  - [Getting the Final Value of Numeric](#getting-the-final-value-of-numeric)
  - [Using Multiplication Modifiers(`FractionNumericModifier`)](#using-multiplication-modifiersfractionnumericmodifier)
  - [Modifiers' Name, Tags, and Count](#modifiers-name-tags-and-count)
  - [Using Tags to Partially Modify Base Values](#using-tags-to-partially-modify-base-values)
  - [Using Custom Modifiers](#using-custom-modifiers)
- [File Path Description](#file-path-description)
- [Author](#author)
- [License](#license)

## Download and Deployment

### Get from GitHub

```shell
git clone git@github.com:dlqw/NumericSystem.git

```

### Get from npm

```shell
npm i numericsystem
```

## Getting Started

### Creating the First Numeric

To use the tool, you need to reference `WFramework.CoreGameDevKit.NumericSystem;`

```csharp
using WFramework.CoreGameDevKit.NumericSystem;
```

You can manually create a Numeric object and pass an integer or floating point number to its constructor.

```csharp
Numeric health = new Numeric(100);
```

This value (100 in the example above) acts as the base value of the Numeric object and is read-only. You can retrieve its value using GetOriginValue(). To change this base value, create a new Numeric object.

```csharp
// Get original/base value
var healthBasicValue = health.GetOriginValue();

// Change the base value => Create a new Numeric object
health = new Numeric(200);
```

Alternatively, you can assign an integer or floating-point number to a Numeric object to create a new one.

```csharp
Numeric health = 100;
Debug.Log(health.GetHashCode()); // 402183648

health = 100.67f;
Debug.Log(health.GetHashCode()); // 1146914344
```

Note that in this case, health now points to a newly allocated Numeric object.

### Attaching `AdditionNumericModifier` to Numeric

You can manually create an addition modifier using `AdditionNumericModifier`. The internal value of the modifier is immutable and can be accessed through `StoreValue`.

```csharp
AdditionNumericModifier strongBuff = new AdditionNumericModifier(20);
var buffValue = strongBuff.StoreValue;
```

Alternatively, you can quickly create it using integers or floating-point numbers.

```csharp
AdditionNumericModifier strongBuff = 20f;
```

To attach a `NumericModifier` to a Numeric object, use the `AddModifier` method or the addition operator. The following examples illustrate valid usage:

```csharp
// Success
health.AddModifier(strongBuff);
health = health + 20;
health += 20;
health = health + strongBuff;
health += strongBuff;

// Error
health.AddModifier(20);
```

Removing a modifier follows similar syntax, using the `RemoveModifier` method or the subtraction operator.

**Important**: Avoid mixing these two:

```csharp
health += -20;
health -= 20;
```

The first example attaches a modifier with a value of -20, while the second removes a modifier with a value of 20.

**Important**: Do not attach the same modifier multiple times without removing it first. You can use overloaded addition and subtraction operators to create temporary modifier objects.

**Important**: Avoid mixing integers and floating-point numbers in the same Numeric object. The Numeric System is designed to work with either `int -> int` or `float -> float`, depending on your needs.

### Getting the Final Value of Numeric

You can obtain the final value by calling `FinalValue` or `FinalValueF`, which return the result as an integer or floating-point number, respectively.

```csharp
Numeric health = 100;

health += 20.3f;

Debug.Log(health.FinalValue);
Debug.Log(health.FinalValueF);
```

### Using Multiplication Modifiers(`FractionNumericModifier`)

Multiplication modifiers are slightly more complex, but you can still construct, attach, or remove them in a similar way.

```csharp
health.AddModifier(new FractionNumericModifier(1, 2, FractionNumericModifier.FractionType.Increase));
health *= (200, FractionNumericModifier.FractionType.Override);

health.RemoveModifier(new FractionNumericModifier(1, 2, FractionNumericModifier.FractionType.Increase));
health /= (200, FractionNumericModifier.FractionType.Override);
```

You can build multiplication modifiers using (`numerator:int`, `denominator:int`, `type:FractionNumericModifier.FractionType`). You can either directly create the object or use C# tuple syntax, which offers clarity or convenience, depending on your preference.

Here are two types of multiplication modifiers:

```csharp
public enum FractionType
{
    Override, // Replace the original value
    Increase, // Increment based on the original value
}
```

An increment modifier will add a new value to the original, while a replace modifier directly sets the new value. For example, if the original value is 100 and the increment modifier is `2/1`, the result will be 300, whereas the replace modifier will yield 200.

The formulas are as follows:

$$ Increase = (1 + (\frac{numerator}{denominator})) \times Input $$
$$ Override = (\frac{numerator}{denominator}) \times Input$$

### Modifiers' Name, Tags, and Count

All modifiers have overloaded constructors that include `Name:string`, `Tags:string[]`, and `Count:int`. The `Name` serves as a unique identifier. If you attach a modifier with the same `Name`, it will accumulate its Count and replace the previous one.

Anonymous modifiers are named "DEFAULT MODIFIER."

The purpose of `Tags` will be detailed in the [next section](#using-tags-to-partially-modify-base-values).

`Count` is an internal counter for stacking modifiers.

### Using Tags to Partially Modify Base Values

Tags apply to addition modifiers, multiplication modifiers, and the base value of a Numeric object. Addition modifiers with specific Tags indicate which multiplication modifiers will affect them. Similarly, Numeric base values have a default tag of "SELF".

Multiplication modifiers' Tags define which addition modifiers or base values they will affect.

```csharp
Numeric health = 100;

health += (20, new[] { "Equipment" }, "Armor", 1);
Debug.Log(health.FinalValue); // 120
health *= (120, FractionNumericModifier.FractionType.Override, new[] { "Equipment" }, "Armor Upgrade", 1);
Debug.Log(health.FinalValue); // 124
health *= (50, FractionNumericModifier.FractionType.Increase, new[] { NumericModifier.TagSelf }, "Upgrade", 1);
Debug.Log(health.FinalValue); // 174
```

### Using Custom Modifiers

Custom modifiers are invoked at the end of the calculation pipeline and can enforce specific constraints. For example, to limit a player's health, you can create a `Func<int,int>` or `Func<float,float>` that takes the target value as input and returns the constrained result.

```csharp
Numeric health = 100;

Func<int, int> healthLimit = value => Mathf.Clamp(value, 0, 150);

health.AddModifier(new CustomNumericModifier(healthLimit));
health -= healthLimit;
health += new CustomNumericModifier(healthLimit);
```

In this example, the player's health is constrained between 0 and 150. The creation and attachment of `CustomNumericModifier` are similar to other modifiers, including implicit type conversions and operator overloads (`+`, `-`).

Important: Do not attach conflicting conditions to the same `Numeric` object, as the final outcome may be undefined.

## File Path Description

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

## Author

[rdququ](https://github.com/dlqw)

## License

This project is released under the [MIT License](./LICENSE)
