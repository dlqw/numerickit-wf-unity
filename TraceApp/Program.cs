using System;
using WFramework.CoreGameDevKit.NumericSystem;

Console.WriteLine("=== LargeCount Test ===");

var numeric = new Numeric(1000000);
Console.WriteLine($"Initial: {numeric.FinalValue}");

numeric += new AdditionNumericModifier(1, Array.Empty<string>(), "Stack", 1000);
Console.WriteLine($"After adding modifier with Count=1000: {numeric.FinalValue}");
Console.WriteLine($"  GetOriginValue: {numeric.GetOriginValue()}");
Console.WriteLine($"  GetAddModifierValue: {numeric.GetAddModifierValue()}");

numeric *= (500, FractionType.Increase, Array.Empty<string>(), "BigBoost", 1);
Console.WriteLine($"After 500% Increase: {numeric.FinalValue}");
Console.WriteLine($"  Expected: 6001000");
