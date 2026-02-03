using Xunit;
using Xunit.Abstractions;
using WFramework.CoreGameDevKit.NumericSystem;

namespace NumericSystem.Tests
{
    /// <summary>
    /// DiagnosticHelper 诊断工具测试
    /// </summary>
    public class DiagnosticHelperTests
    {
        private readonly ITestOutputHelper _output;

        public DiagnosticHelperTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void GetDiagnostics_ShouldReturnValidInfo()
        {
            // Arrange
            var numeric = new Numeric(100);
            numeric += (20, new[] { "Equipment" }, "Armor", 1);
            numeric *= (150, FractionType.Increase, new[] { "Equipment" }, "Upgrade", 1);

            // Act
            var diagnostics = DiagnosticHelper.GetDiagnostics(numeric);

            // Assert
            Assert.Contains("Numeric 诊断信息", diagnostics);
            Assert.Contains("原始值", diagnostics);
            Assert.Contains("最终值", diagnostics);
            Assert.Contains("修饰符统计", diagnostics);
            _output.WriteLine(diagnostics);
        }

        [Fact]
        public void GetModifierTree_ShouldReturnTreeStructure()
        {
            // Arrange
            var numeric = new Numeric(100);
            numeric += (20, new[] { "Equipment" }, "Armor", 1);
            numeric += (30, new[] { "Buff" }, "Strength", 1);

            // Act
            var tree = DiagnosticHelper.GetModifierTree(numeric);

            // Assert
            Assert.Contains("修饰符树", tree);
            Assert.Contains("├─", tree);
            Assert.Contains("Armor", tree);
            Assert.Contains("Strength", tree);
            _output.WriteLine(tree);
        }

        [Fact]
        public void GetCalculationTrace_ShouldShowCalculationSteps()
        {
            // Arrange
            var numeric = new Numeric(100);
            numeric += (20, new[] { "Equipment" }, "Armor", 1);
            numeric += (30, new[] { "Buff" }, "Strength", 1);

            // Act
            var trace = DiagnosticHelper.GetCalculationTrace(numeric);

            // Assert
            Assert.Contains("原始值", trace);
            Assert.Contains("加法修饰符总和", trace);
            Assert.Contains("最终值", trace);
            _output.WriteLine(trace);
        }

        [Fact]
        public void AnalyzePerformance_ShouldReturnPerformanceReport()
        {
            // Arrange
            var numeric = new Numeric(100);
            numeric += (10, Array.Empty<string>(), "Test", 1);
            numeric *= (120, FractionType.Increase, Array.Empty<string>(), "Boost", 1);

            // Act
            var report = DiagnosticHelper.AnalyzePerformance(numeric, iterations: 1000);

            // Assert
            Assert.Contains("性能分析", report);
            Assert.Contains("迭代次数", report);
            Assert.Contains("平均耗时", report);
            Assert.Contains("性能评估", report);
            _output.WriteLine(report);
        }

        [Fact]
        public void ExportToJson_ShouldReturnValidJson()
        {
            // Arrange
            var numeric = new Numeric(100);
            numeric += (20, new[] { "Equipment" }, "Armor", 1);

            // Act
            var json = DiagnosticHelper.ExportToJson(numeric);

            // Assert
            Assert.Contains("{", json);
            Assert.Contains("originValue", json);
            Assert.Contains("finalValue", json);
            Assert.Contains("modifiers", json);
            _output.WriteLine(json);
        }

        [Fact]
        public void GetAllModifiers_ShouldReturnAllModifiers()
        {
            // Arrange
            var numeric = new Numeric(100);
            numeric += (20, new[] { "Equipment" }, "Armor", 1);
            numeric += (30, new[] { "Buff" }, "Strength", 1);
            numeric *= (150, FractionType.Increase, Array.Empty<string>(), "Boost", 1);

            // Act
            var modifiers = numeric.GetAllModifiers();

            // Assert
            Assert.Equal(3, modifiers.Count);
            var addCount = modifiers.Count(m => m.Info.Name == "Armor" || m.Info.Name == "Strength");
            var fracCount = modifiers.Count(m => m.Info.Name == "Boost");
            Assert.Equal(2, addCount);
            Assert.Equal(1, fracCount);
        }

        [Fact]
        public void GetModifierStats_ShouldReturnCorrectStats()
        {
            // Arrange
            var numeric = new Numeric(100);
            numeric += (20, new[] { "Equipment" }, "Armor", 1);
            numeric += (30, new[] { "Buff" }, "Strength", 1);
            numeric *= (150, FractionType.Increase, Array.Empty<string>(), "Boost", 1);

            // Act
            var stats = numeric.GetModifierStats();

            // Assert
            Assert.Equal(3, stats.Values.Sum());
            // 验证有不同类型的修饰符
            Assert.True(stats.Values.Count >= 2);
        }

        [Fact]
        public void InvalidateCache_ShouldForceRecalculation()
        {
            // Arrange
            var numeric = new Numeric(100);
            numeric += (20, Array.Empty<string>(), "Test", 1);
            var firstValue = numeric.FinalValue;

            // Act
            numeric.InvalidateCache();
            numeric += (10, Array.Empty<string>(), "Test2", 1);
            var secondValue = numeric.FinalValue;

            // Assert
            Assert.Equal(130, secondValue);
        }

        [Fact]
        public void GetCacheStatus_ShouldReturnStatusString()
        {
            // Arrange
            var numeric = new Numeric(100);

            // Act
            var status = numeric.GetCacheStatus();

            // Assert
            Assert.NotNull(status);
            Assert.Contains("缓存", status);
        }
    }
}
