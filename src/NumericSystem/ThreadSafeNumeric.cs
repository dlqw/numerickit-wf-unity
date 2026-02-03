using System;
using System.Collections.Generic;
using System.Threading;

namespace WFramework.CoreGameDevKit.NumericSystem
{
    /// <summary>
    /// 线程安全的数值包装类，提供对 Numeric 对象的并发访问支持
    /// </summary>
    /// <remarks>
    /// <para>适用场景：</para>
    /// <list type="bullet">
    /// <item><description>多线程环境下的属性访问</description></item>
    /// <item><description>异步加载资源时的数值更新</description></item>
    /// <item><description>跨线程共享的游戏状态</description></item>
    /// </list>
    /// <para><strong>性能考虑：</strong></para>
    /// <list type="bullet">
    /// <item><description>使用 ReaderWriterLockSlim 优化读取性能</description></item>
    /// <item><description>读操作可以并发，写操作互斥</description></item>
    /// <item><description>在单线程环境下会有轻微性能开销</description></item>
    /// </list>
    /// <para><strong>注意：</strong> 如果只在单线程（Unity 主线程）中使用，
    /// 直接使用 <see cref="Numeric"/> 类即可，无需使用此类。</para>
    /// </remarks>
    public class ThreadSafeNumeric
    {
        private readonly Numeric _numeric;
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        /// <summary>
        /// 初始化 ThreadSafeNumeric 的新实例
        /// </summary>
        /// <param name="value">初始值</param>
        public ThreadSafeNumeric(int value)
        {
            _numeric = new Numeric(value);
        }

        /// <summary>
        /// 初始化 ThreadSafeNumeric 的新实例
        /// </summary>
        /// <param name="value">初始值</param>
        public ThreadSafeNumeric(float value)
        {
            _numeric = new Numeric(value);
        }

        /// <summary>
        /// 获取或设置最终值
        /// </summary>
        public int FinalValue
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    return _numeric.FinalValue;
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
            set
            {
                _numeric.InvalidateCache();
                // 注意：这里只是设置缓存失效，实际值由内部修饰符决定
                // 这个属性 setter 主要用于兼容性，实际修改应该通过修饰符
            }
        }

        /// <summary>
        /// 获取最终浮点值
        /// </summary>
        public float FinalValueF
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    return _numeric.FinalValueF;
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
        }

        /// <summary>
        /// 获取原始值
        /// </summary>
        public int GetOriginValue()
        {
            _lock.EnterReadLock();
            try
            {
                return _numeric.GetOriginValue();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// 获取所有修饰符的快照
        /// </summary>
        public IReadOnlyList<INumericModifier> GetAllModifiers()
        {
            _lock.EnterReadLock();
            try
            {
                return _numeric.GetAllModifiers();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// 添加修饰符（线程安全）
        /// </summary>
        public ThreadSafeNumeric AddModifier(INumericModifier modifier)
        {
            if (modifier == null)
                throw new ArgumentNullException(nameof(modifier));

            _lock.EnterWriteLock();
            try
            {
                _numeric.AddModifier(modifier);
            }
            finally
            {
                _lock.ExitWriteLock();
            }

            return this;
        }

        /// <summary>
        /// 移除修饰符（线程安全）
        /// </summary>
        public ThreadSafeNumeric RemoveModifier(INumericModifier modifier)
        {
            if (modifier == null)
                throw new ArgumentNullException(nameof(modifier));

            _lock.EnterWriteLock();
            try
            {
                _numeric.RemoveModifier(modifier);
            }
            finally
            {
                _lock.ExitWriteLock();
            }

            return this;
        }

        /// <summary>
        /// 清空所有修饰符（线程安全）
        /// </summary>
        public ThreadSafeNumeric Clear()
        {
            _lock.EnterWriteLock();
            try
            {
                _numeric.Clear();
            }
            finally
            {
                _lock.ExitWriteLock();
            }

            return this;
        }

        #region 运算符重载支持

        public static ThreadSafeNumeric operator +(ThreadSafeNumeric numeric, AdditionNumericModifier modifier)
            => numeric.AddModifier(modifier);

        public static ThreadSafeNumeric operator -(ThreadSafeNumeric numeric, AdditionNumericModifier modifier)
            => numeric.RemoveModifier(modifier);

        public static ThreadSafeNumeric operator *(ThreadSafeNumeric numeric, FractionNumericModifier modifier)
            => numeric.AddModifier(modifier);

        public static ThreadSafeNumeric operator /(ThreadSafeNumeric numeric, FractionNumericModifier modifier)
            => numeric.RemoveModifier(modifier);

        public static ThreadSafeNumeric operator +(ThreadSafeNumeric numeric, CustomNumericModifier modifier)
            => numeric.AddModifier(modifier);

        public static ThreadSafeNumeric operator -(ThreadSafeNumeric numeric, CustomNumericModifier modifier)
            => numeric.RemoveModifier(modifier);

        #endregion

        /// <summary>
        /// 获取底层的 Numeric 对象（用于高级操作）
        /// </summary>
        /// <remarks>
        /// <para><strong>警告：</strong> 返回的 Numeric 对象不是线程安全的。
        /// 如果需要在多线程环境下使用，必须通过此 ThreadSafeNumeric 包装器进行操作。</para>
        /// <para>此方法主要用于与现有 API 兼容或执行不需要线程安全的批量操作。</para>
        /// </remarks>
        public Numeric GetUnsafeNumeric()
        {
            _lock.EnterReadLock();
            try
            {
                return _numeric;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// 执行读操作（自动加锁）
        /// </summary>
        /// <param name="action">要执行的操作</param>
        /// <remarks>
        /// 此方法确保在读操作期间持有读锁，防止并发写入。
        /// 对于简单的值访问（如 FinalValue），无需使用此方法。
        /// </remarks>
        public void Read(Action<Numeric> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            _lock.EnterReadLock();
            try
            {
                action(_numeric);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// 执行读操作并返回结果（自动加锁）
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="func">要执行的函数</param>
        /// <returns>函数的返回值</returns>
        public T Read<T>(Func<Numeric, T> func)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            _lock.EnterReadLock();
            try
            {
                return func(_numeric);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// 执行写操作（自动加锁）
        /// </summary>
        /// <param name="action">要执行的操作</param>
        /// <remarks>
        /// 此方法确保在写操作期间持有写锁，防止并发访问。
        /// 所有修改操作（添加/移除修饰符）都应该通过此方法或相应的运算符进行。
        /// </remarks>
        public void Write(Action<Numeric> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            _lock.EnterWriteLock();
            try
            {
                action(_numeric);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// 执行写操作并返回结果（自动加锁）
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="func">要执行的函数</param>
        /// <returns>函数的返回值</returns>
        public T Write<T>(Func<Numeric, T> func)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            _lock.EnterWriteLock();
            try
            {
                return func(_numeric);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
    }
}
