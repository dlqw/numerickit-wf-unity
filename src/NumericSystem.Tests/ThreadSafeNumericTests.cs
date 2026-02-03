using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using WFramework.CoreGameDevKit.NumericSystem;

namespace NumericSystem.Tests
{
    /// <summary>
    /// ThreadSafeNumeric 线程安全测试
    /// </summary>
    public class ThreadSafeNumericTests
    {
        [Fact]
        public void Constructor_ShouldCreateValidInstance()
        {
            // Act
            var safeNumeric = new ThreadSafeNumeric(100);

            // Assert
            Assert.Equal(100, safeNumeric.FinalValue);
        }

        [Fact]
        public void AddModifier_ShouldBeThreadSafe()
        {
            // Arrange
            var safeNumeric = new ThreadSafeNumeric(100);

            // Act
            safeNumeric += new AdditionNumericModifier(20, new[] { "Test" }, "Mod1", 1);

            // Assert
            Assert.Equal(120, safeNumeric.FinalValue);
        }

        [Fact]
        public void RemoveModifier_ShouldBeThreadSafe()
        {
            // Arrange
            var safeNumeric = new ThreadSafeNumeric(100);
            var modifier = new AdditionNumericModifier(20, new[] { "Test" }, "Mod1", 1);
            safeNumeric += modifier;

            // Act
            safeNumeric -= modifier;

            // Assert
            Assert.Equal(100, safeNumeric.FinalValue);
        }

        [Fact]
        public void Read_ShouldReturnConsistentValue()
        {
            // Arrange
            var safeNumeric = new ThreadSafeNumeric(100);

            // Act
            var value1 = safeNumeric.FinalValue;
            var value2 = safeNumeric.FinalValue;

            // Assert
            Assert.Equal(value1, value2);
        }

        [Fact]
        public void ReadMethod_ShouldProvideAccessToNumeric()
        {
            // Arrange
            var safeNumeric = new ThreadSafeNumeric(100);
            safeNumeric += (20, new[] { "Equipment" }, "Armor", 1);

            // Act
            var result = safeNumeric.Read(numeric => numeric.FinalValue);

            // Assert
            Assert.Equal(120, result);
        }

        [Fact]
        public void WriteMethod_ShouldModifyNumeric()
        {
            // Arrange
            var safeNumeric = new ThreadSafeNumeric(100);

            // Act
            safeNumeric.Write(numeric =>
            {
                numeric += (10, Array.Empty<string>(), "Test", 1);
            });

            // Assert
            Assert.Equal(110, safeNumeric.FinalValue);
        }

        [Fact]
        public void ConcurrentReads_ShouldNotBlock()
        {
            // Arrange
            var safeNumeric = new ThreadSafeNumeric(100);
            safeNumeric += (20, new[] { "Equipment" }, "Armor", 1);

            // 预先计算 FinalValue，确保修饰符已应用
            var expectedValue = safeNumeric.FinalValue;
            Assert.Equal(120, expectedValue); // 验证预期值

            var successCount = 0;
            var failCount = 0;
            var lockObj = new object();
            var tasks = new Task[10];

            // Act - 启动多个并发读操作
            for (int i = 0; i < tasks.Length; i++)
            {
                tasks[i] = Task.Run(() =>
                {
                    var value = safeNumeric.FinalValue;
                    lock (lockObj)
                    {
                        if (value == 120)
                        {
                            successCount++;
                        }
                        else
                        {
                            failCount++;
                        }
                    }
                });
            }

            Task.WaitAll(tasks);

            // Assert - 所有读操作都应该成功完成
            Assert.Equal(10, successCount);
            Assert.Equal(0, failCount);
        }

        [Fact]
        public void ConcurrentWrites_ShouldBeSerialized()
        {
            // Arrange
            var safeNumeric = new ThreadSafeNumeric(100);
            var writeCount = 0;
            var tasks = new Task[5];

            // Act - 启动多个并发写操作
            for (int i = 0; i < tasks.Length; i++)
            {
                var index = i;
                tasks[index] = Task.Run(() =>
                {
                    safeNumeric.Write(numeric =>
                    {
                        numeric += (10, Array.Empty<string>(), $"Task{index}", 1);
                        Interlocked.Increment(ref writeCount);
                    });
                });
            }

            Task.WaitAll(tasks);

            // Assert - 所有写操作都应该成功完成
            Assert.Equal(5, writeCount);
            Assert.Equal(150, safeNumeric.FinalValue); // 100 + 10*5
        }

        [Fact]
        public void MixedReadWrite_ShouldBeConsistent()
        {
            // Arrange
            var safeNumeric = new ThreadSafeNumeric(100);
            var exceptions = new System.Collections.Concurrent.ConcurrentBag<Exception>();
            var tasks = new Task[20];

            // Act - 混合读写操作
            for (int i = 0; i < tasks.Length; i++)
            {
                var index = i;
                tasks[i] = Task.Run(() =>
                {
                    try
                    {
                        if (index % 2 == 0)
                        {
                            // 读操作
                            var _ = safeNumeric.FinalValue;
                        }
                        else
                        {
                            // 写操作
                            safeNumeric.Write(numeric =>
                            {
                                numeric += (5, Array.Empty<string>(), $"Op{index}", 1);
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(ex);
                    }
                });
            }

            Task.WaitAll(tasks);

            // Assert
            Assert.Empty(exceptions);
            Assert.Equal(150, safeNumeric.FinalValue); // 100 + 5*10
        }

        [Fact]
        public void GetUnsafeNumeric_ShouldReturnWrappedNumeric()
        {
            // Arrange
            var safeNumeric = new ThreadSafeNumeric(100);

            // Act
            var numeric = safeNumeric.GetUnsafeNumeric();

            // Assert
            Assert.NotNull(numeric);
            Assert.Equal(100, numeric.FinalValue);
        }

        [Fact]
        public void Clear_ShouldRemoveAllModifiers()
        {
            // Arrange
            var safeNumeric = new ThreadSafeNumeric(100);
            safeNumeric += (20, new[] { "Test" }, "Mod1", 1);
            safeNumeric += (30, new[] { "Test" }, "Mod2", 1);

            // Act
            safeNumeric.Clear();

            // Assert
            Assert.Equal(100, safeNumeric.FinalValue);
            Assert.Empty(safeNumeric.GetAllModifiers());
        }

        [Fact]
        public void GetAllModifiers_ShouldReturnSnapshot()
        {
            // Arrange
            var safeNumeric = new ThreadSafeNumeric(100);
            safeNumeric += (20, new[] { "Equipment" }, "Armor", 1);

            // Act
            var modifiers = safeNumeric.GetAllModifiers();

            // Assert
            Assert.Single(modifiers);
            Assert.Equal("Armor", modifiers[0].Info.Name);
        }

        [Fact]
        public void StressTest_ConcurrentOperations_ShouldMaintainConsistency()
        {
            // Arrange
            var safeNumeric = new ThreadSafeNumeric(100);
            var exceptions = new System.Collections.Concurrent.ConcurrentBag<Exception>();
            var tasks = new Task[50];
            var random = new Random();

            // Act - 大量并发操作
            for (int i = 0; i < tasks.Length; i++)
            {
                tasks[i] = Task.Run(() =>
                {
                    try
                    {
                        for (int j = 0; j < 10; j++)
                        {
                            var operation = random.Next(3);
                            switch (operation)
                            {
                                case 0: // 读
                                    _ = safeNumeric.FinalValue;
                                    break;
                                case 1: // 加法
                                    safeNumeric += (1, Array.Empty<string>(), "Op", 1);
                                    break;
                                case 2: // 分数
                                    safeNumeric *= (105, FractionType.Increase, Array.Empty<string>(), "Boost", 1);
                                    break;
                            }

                            // 小延迟模拟真实使用
                            Thread.Sleep(random.Next(5));
                        }
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(ex);
                    }
                });
            }

            Task.WaitAll(tasks);

            // Assert
            Assert.Empty(exceptions);
            // 最终值应该合理（100 + 某些加法和乘法）
            var finalValue = safeNumeric.FinalValue;
            Assert.True(finalValue >= 100, $"Final value {finalValue} should be >= 100");
        }
    }
}
