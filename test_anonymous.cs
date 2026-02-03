using WFramework.CoreGameDevKit.NumericSystem;

var numeric = new Numeric(100);
Console.WriteLine($"Initial: {numeric.FinalValue}");

numeric += 10;
Console.WriteLine($"After += 10: {numeric.FinalValue}");

numeric += 20;
Console.WriteLine($"After += 20: {numeric.FinalValue}");

numeric += 30;
Console.WriteLine($"After += 30: {numeric.FinalValue}");

Console.WriteLine($"Expected: 160, Actual: {numeric.FinalValue}");
