using System;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace BYWGLib.Network
{
    /// <summary>
    /// 高性能网络请求构建器
    /// 使用ArrayPool&lt;T&gt;和Span&lt;T&gt;优化内存分配
    /// </summary>
    public sealed class NetworkRequestBuilder : IDisposable
    {
        private readonly ArrayPool<byte> _arrayPool;
        private byte[] _buffer;
        private int _position;
        private bool _disposed;
        
        public NetworkRequestBuilder(ArrayPool<byte> arrayPool = null!)
        {
            _arrayPool = arrayPool ?? ArrayPool<byte>.Shared;
            _buffer = _arrayPool.Rent(1024); // 初始缓冲区大小
            _position = 0;
        }
        
        /// <summary>
        /// 写入字节
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteByte(byte value)
        {
            EnsureCapacity(1);
            _buffer[_position++] = value;
        }
        
        /// <summary>
        /// 写入字节数组
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteBytes(ReadOnlySpan<byte> bytes)
        {
            EnsureCapacity(bytes.Length);
            bytes.CopyTo(_buffer.AsSpan(_position));
            _position += bytes.Length;
        }
        
        /// <summary>
        /// 写入UInt16 (大端序)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUInt16BigEndian(ushort value)
        {
            EnsureCapacity(2);
            _buffer[_position++] = (byte)(value >> 8);
            _buffer[_position++] = (byte)value;
        }
        
        /// <summary>
        /// 写入UInt16 (小端序)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUInt16LittleEndian(ushort value)
        {
            EnsureCapacity(2);
            _buffer[_position++] = (byte)value;
            _buffer[_position++] = (byte)(value >> 8);
        }
        
        /// <summary>
        /// 写入UInt32 (大端序)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUInt32BigEndian(uint value)
        {
            EnsureCapacity(4);
            _buffer[_position++] = (byte)(value >> 24);
            _buffer[_position++] = (byte)(value >> 16);
            _buffer[_position++] = (byte)(value >> 8);
            _buffer[_position++] = (byte)value;
        }
        
        /// <summary>
        /// 写入UInt32 (小端序)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUInt32LittleEndian(uint value)
        {
            EnsureCapacity(4);
            _buffer[_position++] = (byte)value;
            _buffer[_position++] = (byte)(value >> 8);
            _buffer[_position++] = (byte)(value >> 16);
            _buffer[_position++] = (byte)(value >> 24);
        }
        
        /// <summary>
        /// 写入Int16 (大端序)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteInt16BigEndian(short value)
        {
            WriteUInt16BigEndian((ushort)value);
        }
        
        /// <summary>
        /// 写入Int16 (小端序)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteInt16LittleEndian(short value)
        {
            WriteUInt16LittleEndian((ushort)value);
        }
        
        /// <summary>
        /// 写入Int32 (大端序)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteInt32BigEndian(int value)
        {
            WriteUInt32BigEndian((uint)value);
        }
        
        /// <summary>
        /// 写入Int32 (小端序)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteInt32LittleEndian(int value)
        {
            WriteUInt32LittleEndian((uint)value);
        }
        
        /// <summary>
        /// 写入Float (大端序)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteFloatBigEndian(float value)
        {
            unsafe
            {
                WriteUInt32BigEndian(*(uint*)&value);
            }
        }
        
        /// <summary>
        /// 写入Float (小端序)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteFloatLittleEndian(float value)
        {
            unsafe
            {
                WriteUInt32LittleEndian(*(uint*)&value);
            }
        }
        
        /// <summary>
        /// 写入Double (大端序)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteDoubleBigEndian(double value)
        {
            unsafe
            {
                var bytes = *(ulong*)&value;
                WriteUInt32BigEndian((uint)(bytes >> 32));
                WriteUInt32BigEndian((uint)bytes);
            }
        }
        
        /// <summary>
        /// 写入Double (小端序)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteDoubleLittleEndian(double value)
        {
            unsafe
            {
                var bytes = *(ulong*)&value;
                WriteUInt32LittleEndian((uint)bytes);
                WriteUInt32LittleEndian((uint)(bytes >> 32));
            }
        }
        
        /// <summary>
        /// 写入字符串 (UTF-8编码)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteString(string value)
        {
            if (string.IsNullOrEmpty(value))
                return;
            
            var bytes = System.Text.Encoding.UTF8.GetBytes(value);
            WriteBytes(bytes);
        }
        
        /// <summary>
        /// 写入字符串 (UTF-8编码，带长度前缀)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteStringWithLength(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                WriteUInt16BigEndian(0);
                return;
            }
            
            var bytes = System.Text.Encoding.UTF8.GetBytes(value);
            WriteUInt16BigEndian((ushort)bytes.Length);
            WriteBytes(bytes);
        }
        
        /// <summary>
        /// 写入填充字节
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WritePadding(int count, byte value = 0)
        {
            EnsureCapacity(count);
            for (int i = 0; i < count; i++)
            {
                _buffer[_position++] = value;
            }
        }
        
        /// <summary>
        /// 写入对齐字节
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteAlignment(int alignment)
        {
            int remainder = _position % alignment;
            if (remainder != 0)
            {
                WritePadding(alignment - remainder);
            }
        }
        
        /// <summary>
        /// 获取当前位置
        /// </summary>
        public int Position => _position;
        
        /// <summary>
        /// 设置位置
        /// </summary>
        public void SetPosition(int position)
        {
            if (position < 0 || position > _position)
                throw new ArgumentOutOfRangeException(nameof(position));
            
            _position = position;
        }
        
        /// <summary>
        /// 构建最终结果
        /// </summary>
        public ReadOnlyMemory<byte> Build()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(NetworkRequestBuilder));
            
            var result = new byte[_position];
            Array.Copy(_buffer, 0, result, 0, _position);
            return result;
        }
        
        /// <summary>
        /// 构建最终结果 (使用ArrayPool)
        /// </summary>
        public (byte[] buffer, int length) BuildWithPool()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(NetworkRequestBuilder));
            
            var result = _arrayPool.Rent(_position);
            Array.Copy(_buffer, 0, result, 0, _position);
            return (result, _position);
        }
        
        /// <summary>
        /// 获取当前缓冲区内容
        /// </summary>
        public ReadOnlySpan<byte> GetCurrentBuffer()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(NetworkRequestBuilder));
            
            return _buffer.AsSpan(0, _position);
        }
        
        /// <summary>
        /// 确保缓冲区容量
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureCapacity(int requiredBytes)
        {
            if (_position + requiredBytes > _buffer.Length)
            {
                var newSize = Math.Max(_buffer.Length * 2, _position + requiredBytes);
                var newBuffer = _arrayPool.Rent(newSize);
                Array.Copy(_buffer, 0, newBuffer, 0, _position);
                _arrayPool.Return(_buffer);
                _buffer = newBuffer;
            }
        }
        
        public void Dispose()
        {
            if (!_disposed)
            {
                _arrayPool.Return(_buffer);
                _disposed = true;
            }
        }
    }
}
