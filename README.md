<h1 align="center">
  Numeric System
</h1>
<p align="center">
  HuaYe Studio rdququ
</p>

<p align="center">
<img alt="Static Badge" src="https://img.shields.io/badge/field-gameplay-red">
<img src="https://img.shields.io/badge/script-csharp-yellow">
<img src="https://img.shields.io/badge/dotnet-Standard 2.1-green">
<img src="https://img.shields.io/badge/framework-WFramework-blue">
<img alt="Static Badge" src="https://img.shields.io/badge/tests-129%20passed-success">
<img alt="Static Badge" src="https://img.shields.io/badge/version-1.2.0-blue">
<img alt="Static Badge" src="https://img.shields.io/badge/readme-%E4%B8%AD%E6%96%87%E6%96%87%E6%A1%A3-red?style=flat&link=https%3A%2F%2Fgithub.com%2Fdlqw%2Fnumerickit-wf-unity%2Fblob%2Fmain%2FREADME_CN.md">
</p>

## Introduction

The Numeric System is a powerful and flexible toolset designed to address the numerical needs of gameplay, aiming to provide a simple and efficient solution for handling combat system calculations.

- **Event Store-based Numeric Change Tracking:** Ensures traceability, easy self-verification, and security of original data.
- **Fixed-point Arithmetic:** Guarantees numerical consistency across platforms and devices, enhancing network synchronization reliability.
- **Simple Syntax:** Supports the addition of integers, floating points, fractions, or percentages to numerical values using addition, multiplication, or custom modifiers.
- **Extensible Architecture:** Support for custom modifiers, conditional modifiers, and modifier priorities.
- **Thread-Safe Operations:** Optional thread-safe wrapper for multi-threaded scenarios.
- **Serialization Support:** Built-in support for modifier serialization/deserialization.

## Changelog

### Version 1.2.0 - Advanced Features and Performance (2025-02-03)

This major release adds advanced features while maintaining 100% backward compatibility:

**New Features:**
- ✨ **Modifier Priority System** - Control modifier application order with fine-grained priorities
- ✨ **Conditional Modifiers** - Apply modifiers based on dynamic conditions using predicate conditions
- ✨ **Serialization Support** - Serialize and deserialize modifiers for save/load functionality
- ✨ **Thread-Safe Wrapper** - Thread-safe `Numeric` operations for multi-threaded environments
- ✨ **Performance Benchmarks** - Comprehensive benchmark suite using BenchmarkDotNet
- ✨ **Diagnostic Tools** - Enhanced debugging and diagnostic capabilities
- ✨ **Fluent API** - Rich extension methods and builder patterns for better developer experience

**Improvements:**
- 🚀 Increased test coverage from 117 to **129 tests** (100% pass rate)
- 🚀 Enhanced error messages and validation
- 🚀 Improved caching mechanism
- 🚀 Better performance for modifier queries

**Documentation:**
- 📚 Complete XML documentation coverage
- 📚 Performance benchmark documentation
- 📚 Architecture refactoring plan

For detailed information, see [changelogs/1.2.0.md](./changelogs/1.2.0.md)

### Version 1.1.0 - Logic Fixes and Security Enhancements (2025-02-03)

- **Fixed Division by Zero**: Added validation in `FractionNumericModifier` constructor
- **Fixed Multi-Fraction Modifier Bug**: Redesigned the `Apply` method
- **Added Overflow Protection**: Prevented silent value corruption
- **Improved CustomNumericModifier Safety**: Enhanced null checking

For detailed information, see [changelogs/1.1.0_CN.md](./changelogs/1.1.0_CN.md)

## Table of Contents

- [Introduction](#introduction)
- [Table of Contents](#table-of-contents)
- [Changelog](#changelog)
- [Download and Deployment](#download-and-deployment)
  - [Get from GitHub](#get-from-github)
  - [Get from npm](#get-from-npm)
- [Getting Started](#getting-started)
  - [Creating the First Numeric](#creating-the-first-numeric)
  - [Attaching Modifiers](#attaching-modifiers)
  - [Getting the Final Value](#getting-the-final-value)
  - [Using Multiplication Modifiers](#using-multiplication-modifiersfractionnumericmodifier)
- [Advanced Features](#advanced-features)
  - [Modifier Priority System](#modifier-priority-system)
  - [Conditional Modifiers](#conditional-modifiers)
  - [Serialization](#serialization)
  - [Thread-Safe Operations](#thread-safe-operations)
  - [Performance Benchmarks](#performance-benchmarks)
- [API Reference](#api-reference)
- [File Path Description](#file-path-description)
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

This value (100 in the example above) acts as the base value of the Numeric object and is read-only. You can retrieve its value using `GetOriginValue()`. To change this base value, create a new Numeric object.

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

### Attaching Modifiers

#### Addition Modifiers

You can use operators or explicit methods:

```csharp
// Using operators (recommended)
health += 20;
health -= 10;

// Using explicit methods
health.AddModifier(new AdditionNumericModifier(20));
health.RemoveModifier(new AdditionNumericModifier(10));
```

#### Multiplication Modifiers

```csharp
// Percentage increase
health *= (150, FractionType.Increase);  // +50%

// Percentage override
health *= (200, FractionType.Override);  // ×2.0

// Remove modifiers
health /= (150, FractionType.Increase);
```

### Getting the Final Value

```csharp
Numeric health = 100;
health += 20.3f;

Debug.Log(health.FinalValue);   // 120 (int)
Debug.Log(health.FinalValueF); // 120.3f (float)
```

### Using Modifiers with Tags and Names

```csharp
// Create named modifier with tags
health += (20, new[] { "Equipment" }, "Armor", 1);
health *= (120, FractionType.Override, new[] { "Equipment" }, "ArmorUpgrade", 1);
health *= (50, FractionType.Increase, new[] { "Buff" }, "StrengthBoost", 1);
```

### Using Custom Modifiers

Custom modifiers are invoked at the end of the calculation pipeline and can enforce specific constraints.

```csharp
Numeric health = 100;

// Clamp health between 0 and 150
health.ClampMax(150, "MaxHealthCap");
health.ClampMin(0, "MinHealthCap");

// Or use custom function
Func<int, int> healthLimit = value => Mathf.Clamp(value, 0, 150);
health.AddModifier(new CustomNumericModifier(healthLimit));
```

## Advanced Features

### Modifier Priority System

Control the order in which modifiers are applied using priorities:

```csharp
var health = new Numeric(100);

// Add modifiers with different priorities
health += (50, new[] { "Base" }, "RaceBonus", 1, ModifierPriority.Base);      // 100
health += (30, new[] { "Base" }, "ClassBonus", 1, ModifierPriority.Base);     // 100
health += (50, new[] { "Equipment" }, "Armor", 1, ModifierPriority.Equipment); // 200
health += (30, new[] { "Buff" }, "Strength", 1, ModifierPriority.Buff);       // 300
health *= (50, FractionType.Increase, Array.Empty<string>(), "Multiplier", 1, ModifierPriority.Multiplier); // 500

// Application order: Base → Equipment → Buff → Multiplier
```

**Priority Levels:**
- `Critical` (0) - Highest priority
- `Base` (100) - Base attributes
- `Equipment` (200) - Equipment modifiers
- `Buff` (300) - Buff/Debuff effects
- `Skill` (400) - Skill bonuses
- `Default` (400) - Default priority
- `Multiplier` (500) - Percentage modifiers
- `Clamp` (600) - Constraint modifiers

### Conditional Modifiers

Apply modifiers based on dynamic conditions:

```csharp
var health = new Numeric(100);

// Condition: Health below 30%
var lowHpCondition = ConditionBuilder.Where(h => h.FinalValue < 30);
var emergencyShield = ConditionalNumericModifier.ConditionalAdd(
    lowHpCondition,
    50,
    "EmergencyShield"
);
health.AddModifier(emergencyShield);

// Complex conditions with AND/OR/NOT
var complexCondition = ConditionBuilder
    .Where(h => h.FinalValue < 50)
    .And(h => h.GetAddModifierValueByTag(new[] { "Buff" }) < 1000000)
    .Build();

health.AddConditionalModifier(
    complexCondition,
    new AdditionNumericModifier(30, Array.Empty<string>(), "ComplexBonus")
);
```

### Serialization

Save and load modifier states:

```csharp
var health = new Numeric(100);
health += 50;
health *= (150, FractionType.Increase);

// Serialize
var data = health.Serialize();

// Deserialize
var restored = data.Deserialize();
Assert.Equal(health.FinalValue, restored.FinalValue);
```

**Supported Modifiers:**
- ✅ `AdditionNumericModifier`
- ✅ `FractionNumericModifier`
- ⚠️ `CustomNumericModifier` (contains delegates)
- ⚠️ `ConditionalNumericModifier` (contains delegates)

### Thread-Safe Operations

For multi-threaded scenarios, use `ThreadSafeNumeric`:

```csharp
var safeHealth = new ThreadSafeNumeric(100);

// Thread-safe operations
safeHealth += 50;
safeHealth.AddModifier(new FractionNumericModifier(150, FractionType.Increase));

// Thread-safe read
var value = safeHealth.FinalValue;

// Thread-safe operations with callbacks
safeHealth.Read(numeric =>
{
    Debug.Log($"Health: {numeric.FinalValue}");
    Debug.Log($"Modifiers: {numeric.GetAllModifiers().Count}");
});

safeHealth.Write(numeric =>
{
    numeric += 20;
});
```

### Performance Benchmarks

Run benchmarks to measure performance:

```bash
cd src
dotnet run -c Release --project NumericSystem.Tests -- --filter *NumericBenchmarks*
```

Benchmark categories:
- **Basic** - Creation, modification, calculation
- **Scalability** - Large modifier counts (10, 100, 1000)
- **Complex** - Real-world scenarios
- **Fraction** - Fraction modifier performance
- **Query** - Lookup operations

See [src/NumericSystem.Tests/Benchmarks/README.md](./src/NumericSystem.Tests/Benchmarks/README.md) for details.

## API Reference

### Fluent API

```csharp
// Builder pattern
var health = Numeric.Build(100, builder =>
{
    builder.AddEquipment(50, "Armor");
    builder.AddBuff(30, "Strength");
    builder.BoostBase(150, "BaseBoost");
    builder.WithMaxLimit(500, "MaxHP");
});

// Extension methods
health.AddEquipment(20, "Helmet");
health.AddBuff(10, "Potion");
health.ClampMax(300);

// Conditional extensions
health.AddIf(h => h.FinalValue < 100, 50, "EmergencyHeal");
health.MultiplyIf(
    ConditionBuilder.Where(h => h.FinalValue > 200).Build(),
    150,
    FractionType.Increase
);
```

### Diagnostic Tools

```csharp
var health = new Numeric(100);
health += 50;
health *= (150, FractionType.Increase);

// Get modifier statistics
var stats = health.GetModifierStats();
foreach (var stat in stats)
{
    Debug.Log($"{stat.Key}: {stat.Value}");
}

// Dump detailed information
health.Dump("Player Health");

// Check cache status
Debug.Log(health.GetCacheStatus());
```

## File Path Description

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
│       ├── Core/                          # Core abstractions
│       ├── Chain/                         # Responsibility chain pattern
│       ├── Serialization/                 # Serialization support
│       ├── FixedPoint.cs
│       ├── Numeric.cs
│       ├── NumericModifier*.cs            # Modifier implementations
│       ├── NumericExtensions.cs           # Extension methods
│       ├── ThreadSafeNumeric.cs           # Thread-safe wrapper
│       └── DiagnosticHelper.cs            # Diagnostic tools
│   └── NumericSystem.Tests
│       ├── Benchmarks/                    # Performance benchmarks
│       ├── *Tests.cs                      # Unit tests
└── LICENSE
```

## Testing

The project includes comprehensive unit tests:

```bash
cd src
dotnet test
```

**Test Coverage:**
- 129 tests (100% pass rate)
- All major features covered
- Edge cases and error handling

## License

This project is released under the [MIT License](./LICENSE)
