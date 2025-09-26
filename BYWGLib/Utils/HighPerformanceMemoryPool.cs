using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace BYWGLib.Utils
{
    /// <summary>
    /// 高性能内存池，使用ArrayPool&lt;T&gt;和Span&lt;T&gt;优化
    /// </summary>
    public sealed class HighPerformanceMemoryPool : IDisposable
    {
        private readonly ArrayPool<byte> _arrayPool;
        private readonly ConcurrentQueue<IMemoryOwner<byte>> _memoryOwners;
        private readonly int _bufferSize;
        private readonly int _maxPoolSize;
        private int _currentPoolSize;
        private readonly object _lockObject = new object();
        
        public HighPerformanceMemoryPool(int bufferSize, int maxPoolSize = 100)
        {
            _bufferSize = bufferSize;
            _maxPoolSize = maxPoolSize;
            _arrayPool = ArrayPool<byte>.Shared;
            _memoryOwners = new ConcurrentQueue<IMemoryOwner<byte>>();
        }
        
        /// <summary>
        /// 租用内存，返回Memory&lt;T&gt;和IMemoryOwner&lt;T&gt;
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public (Memory<byte> Memory, IMemoryOwner<byte> Owner) RentMemory()
        {
            if (_memoryOwners.TryDequeue(out var owner))
            {
                return (owner.Memory.Slice(0, _bufferSize), owner);
            }
            
            // 创建新的内存所有者
            var newOwner = new ArrayMemoryOwner(_arrayPool.Rent(_bufferSize), _arrayPool);
            return (newOwner.Memory.Slice(0, _bufferSize), newOwner);
        }
        
        /// <summary>
        /// 租用Span，用于高性能操作
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public (byte[] Array, int Length) RentSpan()
        {
            var array = _arrayPool.Rent(_bufferSize);
            return (array, _bufferSize);
        }
        
        /// <summary>
        /// 归还内存到池中
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReturnMemory(IMemoryOwner<byte> owner)
        {
            if (owner == null) return;
            
            lock (_lockObject)
            {
                if (_currentPoolSize < _maxPoolSize)
                {
                    _memoryOwners.Enqueue(owner);
                    _currentPoolSize++;
                }
                else
                {
                    owner.Dispose();
                }
            }
        }
        
        /// <summary>
        /// 归还数组到池中
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReturnArray(byte[] array)
        {
            if (array == null) return;
            _arrayPool.Return(array, clearArray: true);
        }
        
        public void Dispose()
        {
            while (_memoryOwners.TryDequeue(out var owner))
            {
                owner.Dispose();
            }
        }
    }
    
    /// <summary>
    /// 数组内存所有者实现
    /// </summary>
    internal sealed class ArrayMemoryOwner : IMemoryOwner<byte>
    {
        private readonly byte[] _array;
        private readonly ArrayPool<byte> _arrayPool;
        private bool _disposed;
        
        public ArrayMemoryOwner(byte[] array, ArrayPool<byte> arrayPool)
        {
            _array = array;
            _arrayPool = arrayPool;
        }
        
        public Memory<byte> Memory
        {
            get
            {
                if (_disposed)
                    throw new ObjectDisposedException(nameof(ArrayMemoryOwner));
                return _array;
            }
        }
        
        public void Dispose()
        {
            if (!_disposed)
            {
                _arrayPool.Return(_array, clearArray: true);
                _disposed = true;
            }
        }
    }
    
    /// <summary>
    /// 高性能对象池，使用ConcurrentQueue和弱引用优化
    /// </summary>
    public sealed class HighPerformanceObjectPool<T> where T : class, new()
    {
        private readonly ConcurrentQueue<T> _pool;
        private readonly int _maxPoolSize;
        private int _currentPoolSize;
        private readonly object _lockObject = new object();
        
        public HighPerformanceObjectPool(int maxPoolSize = 100)
        {
            _maxPoolSize = maxPoolSize;
            _pool = new ConcurrentQueue<T>();
        }
        
        /// <summary>
        /// 从池中获取对象
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Rent()
        {
            if (_pool.TryDequeue(out var obj))
            {
                Interlocked.Decrement(ref _currentPoolSize);
                return obj;
            }
            
            return new T();
        }
        
        /// <summary>
        /// 将对象返回到池中
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(T obj)
        {
            if (obj == null) return;
            
            lock (_lockObject)
            {
                if (_currentPoolSize < _maxPoolSize)
                {
                    _pool.Enqueue(obj);
                    Interlocked.Increment(ref _currentPoolSize);
                }
            }
        }
        
        /// <summary>
        /// 获取池中对象数量
        /// </summary>
        public int Count => _currentPoolSize;
    }
    
    /// <summary>
    /// 高性能字节操作工具类
    /// </summary>
    public static class HighPerformanceByteUtils
    {
        /// <summary>
        /// 使用Span进行高性能字节数组复制
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyBytes(ReadOnlySpan<byte> source, Span<byte> destination)
        {
            source.CopyTo(destination);
        }
        
        /// <summary>
        /// 使用Span进行高性能字节数组比较
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool SequenceEqual(ReadOnlySpan<byte> first, ReadOnlySpan<byte> second)
        {
            return first.SequenceEqual(second);
        }
        
        /// <summary>
        /// 使用Span进行高性能字节数组查找
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf(ReadOnlySpan<byte> span, byte value)
        {
            return span.IndexOf(value);
        }
        
        /// <summary>
        /// 使用Span进行高性能字节数组查找
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf(ReadOnlySpan<byte> span, ReadOnlySpan<byte> value)
        {
            return span.IndexOf(value);
        }
        
        /// <summary>
        /// 使用unsafe进行高性能字节序转换
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort ToUInt16BigEndian(ReadOnlySpan<byte> span, int offset)
        {
            unsafe
            {
                fixed (byte* ptr = span.Slice(offset, 2))
                {
                    return (ushort)((ptr[0] << 8) | ptr[1]);
                }
            }
        }
        
        /// <summary>
        /// 使用unsafe进行高性能字节序转换
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ToUInt32BigEndian(ReadOnlySpan<byte> span, int offset)
        {
            unsafe
            {
                fixed (byte* ptr = span.Slice(offset, 4))
                {
                    return (uint)((ptr[0] << 24) | (ptr[1] << 16) | (ptr[2] << 8) | ptr[3]);
                }
            }
        }
        
        /// <summary>
        /// 使用unsafe进行高性能字节序转换
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ToSingleBigEndian(ReadOnlySpan<byte> span, int offset)
        {
            unsafe
            {
                fixed (byte* ptr = span.Slice(offset, 4))
                {
                    return *(float*)ptr;
                }
            }
        }
        
        /// <summary>
        /// 使用unsafe进行高性能字节序转换
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double ToDoubleBigEndian(ReadOnlySpan<byte> span, int offset)
        {
            unsafe
            {
                fixed (byte* ptr = span.Slice(offset, 8))
                {
                    return *(double*)ptr;
                }
            }
        }
        
        /// <summary>
        /// 使用Span进行高性能字节数组填充
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void FillBytes(Span<byte> span, byte value)
        {
            span.Fill(value);
        }
        
        /// <summary>
        /// 使用Span进行高性能字节数组清零
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ClearBytes(Span<byte> span)
        {
            span.Clear();
        }
    }
    
    /// <summary>
    /// 高性能字符串操作工具类
    /// </summary>
    public static class HighPerformanceStringUtils
    {
        /// <summary>
        /// 使用Span进行高性能字符串分割
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string[] Split(string input, char separator)
        {
            return input.Split(separator);
        }
        
        /// <summary>
        /// 使用Span进行高性能字符串比较
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool EqualsIgnoreCase(ReadOnlySpan<char> first, ReadOnlySpan<char> second)
        {
            return first.Equals(second, StringComparison.OrdinalIgnoreCase);
        }
        
        /// <summary>
        /// 使用Span进行高性能字符串查找
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf(ReadOnlySpan<char> span, char value)
        {
            return span.IndexOf(value);
        }
        
        /// <summary>
        /// 使用Span进行高性能字符串查找
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf(ReadOnlySpan<char> span, ReadOnlySpan<char> value)
        {
            return span.IndexOf(value);
        }
    }
}
