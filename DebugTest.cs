using WFramework.CoreGameDevKit.NumericSystem;

var numeric = new Numeric(100);
Console.WriteLine($"Initial: {numeric.FinalValue}");

numeric.AddModifier(new AdditionNumericModifier(50));
Console.WriteLine($"After +50: {numeric.FinalValue}");

numeric.Clear();
numeric = new Numeric(100);
numeric.AddModifier(new FractionNumericModifier(200, 100, FractionType.Override));
Console.WriteLine($"After 200% Override: {numeric.FinalValue}");
Console.WriteLine($"Modifier Count: {numeric.GetAddModifierValue()}");
