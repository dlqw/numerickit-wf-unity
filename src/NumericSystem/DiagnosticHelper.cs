using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WFramework.CoreGameDevKit.NumericSystem
{
    /// <summary>
    /// 数值系统诊断和调试辅助工具
    /// </summary>
    /// <remarks>
    /// 提供以下功能：
    /// <list type="bullet">
    /// <item><description>显示 Numeric 对象的完整状态</description></item>
    /// <item><description>列出所有修饰符的详细信息</description></item>
    /// <item><description>可视化修饰符的计算过程</description></item>
    /// <item><description>性能分析和诊断</description></item>
    /// </list>
    /// </remarks>
    public static class DiagnosticHelper
    {
        /// <summary>
        /// 获取 Numeric 对象的详细诊断信息
        /// </summary>
        /// <param name="numeric">要诊断的 Numeric 对象</param>
        /// <returns>格式化的诊断信息字符串</returns>
        public static string GetDiagnostics(Numeric numeric)
        {
            if (numeric == null)
                throw new ArgumentNullException(nameof(numeric));

            var sb = new StringBuilder();
            sb.AppendLine("=== Numeric 诊断信息 ===");
            sb.AppendLine();

            // 基础信息
            sb.AppendLine("【基础信息】");
            sb.AppendLine($"  原始值（内部定点数）: {numeric.GetOriginValue()}");
            sb.AppendLine($"  原始值（用户可见）: {numeric.GetOriginValue() / (float)FixedPoint.Factor:F2}");
            sb.AppendLine($"  最终值（用户可见）: {numeric.FinalValue}");
            sb.AppendLine($"  最终值（内部定点数）: {numeric.GetFinalValueFixed()}");
            sb.AppendLine();

            // 修饰符统计
            var allModifiers = numeric.GetAllModifiers();
            var modifierCount = allModifiers.Count;
            var modifierBreakdown = allModifiers.GroupBy(m => m.Type)
                                           .Select(g => new { Type = g.Key, Count = g.Count() })
                                           .OrderByDescending(x => x.Count);

            sb.AppendLine($"【修饰符统计】");
            sb.AppendLine($"  总数: {modifierCount}");
            foreach (var item in modifierBreakdown)
            {
                sb.AppendLine($"  {item.Type}: {item.Count}");
            }
            sb.AppendLine();

            // 加法修饰符详情
            var addModifiers = allModifiers.OfType<AdditionNumericModifier>().ToList();
            if (addModifiers.Any())
            {
                sb.AppendLine("【加法修饰符】");
                foreach (var mod in addModifiers)
                {
                    sb.AppendLine($"  - {mod.Info.Name} (Count: {mod.Info.Count})");
                    sb.AppendLine($"    值（内部）: {mod.StoreValue}");
                    sb.AppendLine($"    值（用户）: {mod.StoreValue / (float)FixedPoint.Factor:F2}");
                    sb.AppendLine($"    标签: [{string.Join(", ", mod.Info.Tags)}]");
                }
                sb.AppendLine();
            }

            // 分数修饰符详情
            var fracModifiers = allModifiers.OfType<FractionNumericModifier>().ToList();
            if (fracModifiers.Any())
            {
                sb.AppendLine("【分数修饰符】");
                foreach (var mod in fracModifiers)
                {
                    sb.AppendLine($"  - {mod.Info.Name} (Count: {mod.Info.Count})");
                    sb.AppendLine($"    类型: {GetFractionTypeDisplayName(mod)}");
                    sb.AppendLine($"    分数: {GetFractionDisplay(mod)}");
                    sb.AppendLine($"    标签: [{string.Join(", ", mod.Info.Tags)}]");
                }
                sb.AppendLine();
            }

            // 自定义修饰符详情
            var customModifiers = allModifiers.OfType<CustomNumericModifier>().ToList();
            if (customModifiers.Any())
            {
                sb.AppendLine("【自定义修饰符】");
                foreach (var mod in customModifiers)
                {
                    sb.AppendLine($"  - {mod.Info.Name} (Count: {mod.Info.Count})");
                    sb.AppendLine($"    标签: [{string.Join(", ", mod.Info.Tags)}]");
                }
                sb.AppendLine();
            }

            // 计算跟踪
            sb.AppendLine("【计算路径】");
            sb.AppendLine(GetCalculationTrace(numeric));

            return sb.ToString();
        }

        /// <summary>
        /// 获取修饰符的可视化树状结构
        /// </summary>
        /// <param name="numeric">要可视化的 Numeric 对象</param>
        /// <returns>树状结构的字符串表示</returns>
        public static string GetModifierTree(Numeric numeric)
        {
            if (numeric == null)
                throw new ArgumentNullException(nameof(numeric));

            var sb = new StringBuilder();
            sb.AppendLine("=== 修饰符树 ===");
            sb.AppendLine();

            PrintNode(sb, numeric, "├─", "", 0);

            return sb.ToString();
        }

        private static void PrintNode(StringBuilder sb, Numeric numeric, string prefix, string indent, int depth)
        {
            if (depth > 10) // 防止无限递归
            {
                sb.AppendLine($"{indent}{prefix}... (maximum depth reached)");
                return;
            }

            // 显示当前节点
            var value = numeric.FinalValue;
            var origin = numeric.GetOriginValue() / (float)FixedPoint.Factor;
            sb.AppendLine($"{indent}{prefix} 值: {value} (原始: {origin:F2})");

            // 获取修饰符并显示
            var modifiers = numeric.GetAllModifiers().ToList();
            if (modifiers.Any())
            {
                var newIndent = indent + "│  ";
                for (int i = 0; i < modifiers.Count; i++)
                {
                    var isLast = i == modifiers.Count - 1;
                    var newPrefix = isLast ? "└─" : "├─";
                    var mod = modifiers[i];

                    sb.AppendLine($"{newIndent}{newPrefix} {GetModifierDisplay(mod)}");
                }
            }
        }

        /// <summary>
        /// 获取计算跟踪信息
        /// </summary>
        /// <param name="numeric">要跟踪的 Numeric 对象</param>
        /// <returns>计算跟踪的字符串表示</returns>
        public static string GetCalculationTrace(Numeric numeric)
        {
            if (numeric == null)
                throw new ArgumentNullException(nameof(numeric));

            var sb = new StringBuilder();

            var origin = numeric.GetOriginValue();
            var addValue = numeric.GetAddModifierValue();

            sb.AppendLine($"  1. 原始值: {origin / (float)FixedPoint.Factor:F2}");

            if (addValue > 0)
            {
                sb.AppendLine($"  2. 加法修饰符总和: +{addValue / (float)FixedPoint.Factor:F2}");

                // 按标签分组显示
                var addModifiers = numeric.GetAllModifiers().OfType<AdditionNumericModifier>();
                var byTag = addModifiers.GroupBy(m => string.Join(",", m.Info.Tags ?? Array.Empty<string>()));
                foreach (var group in byTag)
                {
                    var tagName = string.IsNullOrEmpty(group.Key) ? "(无标签)" : group.Key;
                    var sum = group.Sum(m => m.StoreValue * m.Info.Count);
                    sb.AppendLine($"     - [{tagName}]: +{sum / (float)FixedPoint.Factor:F2}");
                }
            }

            var final = numeric.FinalValue;
            sb.AppendLine($"  3. 最终值: {final}");

            return sb.ToString();
        }

        /// <summary>
        /// 分析性能瓶颈
        /// </summary>
        /// <param name="numeric">要分析的 Numeric 对象</param>
        /// <param name="iterations">测试迭代次数</param>
        /// <returns>性能分析报告</returns>
        public static string AnalyzePerformance(Numeric numeric, int iterations = 10000)
        {
            if (numeric == null)
                throw new ArgumentNullException(nameof(numeric));

            if (iterations <= 0)
                throw new ArgumentException("迭代次数必须大于零", nameof(iterations));

            var sb = new StringBuilder();
            sb.AppendLine("=== 性能分析 ===");
            sb.AppendLine();

            // 预热
            for (int i = 0; i < 100; i++)
            {
                _ = numeric.FinalValue;
            }

            // 测试 FinalValue 计算性能
            var start = DateTime.UtcNow;
            for (int i = 0; i < iterations; i++)
            {
                numeric.InvalidateCache();
                _ = numeric.FinalValue;
            }
            var end = DateTime.UtcNow;

            var duration = end - start;
            var avgMicroseconds = duration.TotalMilliseconds * 1000 / iterations;

            sb.AppendLine($"迭代次数: {iterations:N0}");
            sb.AppendLine($"总耗时: {duration.TotalMilliseconds:F2} ms");
            sb.AppendLine($"平均耗时: {avgMicroseconds:F2} μs/次");
            sb.AppendLine();

            // 性能评估
            if (avgMicroseconds < 1)
            {
                sb.AppendLine("性能评估: 优秀 (< 1 μs)");
            }
            else if (avgMicroseconds < 10)
            {
                sb.AppendLine("性能评估: 良好 (1-10 μs)");
            }
            else if (avgMicroseconds < 100)
            {
                sb.AppendLine($"性能评估: 可接受 (10-100 μs)");
            }
            else
            {
                sb.AppendLine($"性能评估: 需要优化 (> 100 μs)");
            }

            sb.AppendLine();
            sb.AppendLine("建议:");
            var modifierCount = numeric.GetAllModifiers().Count();
            if (modifierCount > 20)
            {
                sb.AppendLine("  - 考虑减少修饰符数量（当前: " + modifierCount + "）");
            }
            if (avgMicroseconds > 10)
            {
                sb.AppendLine("  - 考虑使用缓存或索引优化");
            }

            return sb.ToString();
        }

        /// <summary>
        /// 导出 Numeric 状态为 JSON 格式
        /// </summary>
        /// <param name="numeric">要导出的 Numeric 对象</param>
        /// <returns>JSON 格式的状态字符串</returns>
        public static string ExportToJson(Numeric numeric)
        {
            if (numeric == null)
                throw new ArgumentNullException(nameof(numeric));

            var sb = new StringBuilder();
            sb.AppendLine("{");
            sb.AppendLine($"  \"originValue\": {numeric.GetOriginValue()},");
            sb.AppendLine($"  \"finalValue\": {numeric.FinalValue},");
            sb.AppendLine($"  \"modifierCount\": {numeric.GetAllModifiers().Count()},");
            sb.AppendLine("  \"modifiers\": [");

            var modifiers = numeric.GetAllModifiers().ToList();
            for (int i = 0; i < modifiers.Count; i++)
            {
                var mod = modifiers[i];
                sb.Append("    {");
                sb.Append($"\"type\": \"{mod.Type}\",");
                sb.Append($"\"name\": \"{mod.Info.Name}\",");
                sb.Append($"\"count\": {mod.Info.Count},");
                sb.Append($"\"tags\": [\"{string.Join("\", \"", mod.Info.Tags ?? Array.Empty<string>())}\"]");

                if (i < modifiers.Count - 1)
                {
                    sb.AppendLine("},");
                }
                else
                {
                    sb.AppendLine();
                }
            }

            sb.AppendLine("  ]");
            sb.AppendLine("}");

            return sb.ToString();
        }

        #region 辅助方法

        private static int GetFinalValueFixed(this Numeric numeric)
        {
            // 通过反射或公共API获取内部finalValue
            // 这里使用计算后的结果
            return numeric.FinalValue * (int)FixedPoint.Factor;
        }

        private static string GetFractionTypeDisplayName(FractionNumericModifier mod)
        {
            // 简化实现：返回通用类型名称
            // 完整实现需要使用反射或公共API来获取类型信息
            return "Fraction";
        }

        private static string GetFractionDisplay(FractionNumericModifier mod)
        {
            // 简化实现：返回通用分数显示
            // 完整实现需要使用反射或公共API来获取分子分母
            return "N/A";
        }

        private static string GetModifierDisplay(INumericModifier mod)
        {
            var sb = new StringBuilder();
            sb.Append($"{mod.Info.Name} ({mod.Type})");
            if (mod.Info.Count > 1)
            {
                sb.Append($" ×{mod.Info.Count}");
            }
            return sb.ToString();
        }

        #endregion
    }
}
