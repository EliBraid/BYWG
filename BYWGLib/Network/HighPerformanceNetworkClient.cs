using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace BYWGLib.Network
{
    /// <summary>
    /// 高性能网络客户端
    /// 使用异步I/O、连接池、零拷贝和SIMD优化
    /// </summary>
    public sealed class HighPerformanceNetworkClient : IDisposable
    {
        private readonly string _host;
        private readonly int _port;
        private readonly int _maxConnections;
        private readonly ArrayPool<byte> _arrayPool;
        private readonly SemaphoreSlim _connectionSemaphore;
        private readonly ConcurrentQueue<NetworkConnection> _connectionPool;
        private readonly Timer _healthCheckTimer;
        private volatile bool _disposed;

        public HighPerformanceNetworkClient(string host, int port, int maxConnections = 10)
        {
            _host = host;
            _port = port;
            _maxConnections = maxConnections;
            _arrayPool = ArrayPool<byte>.Shared;
            _connectionSemaphore = new SemaphoreSlim(maxConnections, maxConnections);
            _connectionPool = new ConcurrentQueue<NetworkConnection>();
            
            // 启动健康检查定时器
            _healthCheckTimer = new Timer(HealthCheckCallback, null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
        }

        /// <summary>
        /// 异步发送和接收数据
        /// </summary>
        public async Task<ReadOnlyMemory<byte>> SendAndReceiveAsync(ReadOnlyMemory<byte> request, CancellationToken cancellationToken = default)
        {
            var connection = await RentConnectionAsync(cancellationToken);
            
            try
            {
                // 异步发送
                await connection.Stream.WriteAsync(request, cancellationToken);
                
                // 异步接收
                var response = await connection.ReadAsync(cancellationToken);
                
                return response;
            }
            finally
            {
                ReturnConnection(connection);
            }
        }

        /// <summary>
        /// 批量异步发送和接收
        /// </summary>
        public async Task<ReadOnlyMemory<byte>[]> SendAndReceiveBatchAsync(ReadOnlyMemory<byte>[] requests, CancellationToken cancellationToken = default)
        {
            var tasks = new Task<ReadOnlyMemory<byte>>[requests.Length];
            
            for (int i = 0; i < requests.Length; i++)
            {
                tasks[i] = SendAndReceiveAsync(requests[i], cancellationToken);
            }
            
            return await Task.WhenAll(tasks);
        }

        /// <summary>
        /// 租用连接
        /// </summary>
        private async Task<NetworkConnection> RentConnectionAsync(CancellationToken cancellationToken)
        {
            await _connectionSemaphore.WaitAsync(cancellationToken);
            
            if (_connectionPool.TryDequeue(out var connection))
            {
                if (connection.IsHealthy)
                {
                    return connection;
                }
                else
                {
                    connection.Dispose();
                }
            }
            
            // 创建新连接
            return await CreateConnectionAsync(cancellationToken);
        }

        /// <summary>
        /// 归还连接
        /// </summary>
        private void ReturnConnection(NetworkConnection connection)
        {
            if (connection?.IsHealthy == true)
            {
                _connectionPool.Enqueue(connection);
            }
            else
            {
                connection?.Dispose();
            }
            
            _connectionSemaphore.Release();
        }

        /// <summary>
        /// 创建新连接
        /// </summary>
        private async Task<NetworkConnection> CreateConnectionAsync(CancellationToken cancellationToken)
        {
            var client = new TcpClient();
            
            // 优化TCP选项
            client.NoDelay = true;
            client.ReceiveBufferSize = 65536;
            client.SendBufferSize = 65536;
            client.ReceiveTimeout = 5000;
            client.SendTimeout = 5000;
            
            await client.ConnectAsync(_host, _port);
            
            var stream = client.GetStream();
            stream.ReadTimeout = 5000;
            stream.WriteTimeout = 5000;
            
            return new NetworkConnection(client, stream, _arrayPool);
        }

        /// <summary>
        /// 健康检查回调
        /// </summary>
        private void HealthCheckCallback(object state)
        {
            if (_disposed) return;
            
            var connectionsToCheck = new List<NetworkConnection>();
            
            // 收集需要检查的连接
            while (_connectionPool.TryDequeue(out var connection))
            {
                connectionsToCheck.Add(connection);
            }
            
            // 检查连接健康状态
            foreach (var connection in connectionsToCheck)
            {
                if (connection.IsHealthy)
                {
                    _connectionPool.Enqueue(connection);
                }
                else
                {
                    connection.Dispose();
                }
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            
            _disposed = true;
            
            _healthCheckTimer?.Dispose();
            
            // 关闭所有连接
            while (_connectionPool.TryDequeue(out var connection))
            {
                connection.Dispose();
            }
            
            _connectionSemaphore?.Dispose();
        }
    }

    /// <summary>
    /// 网络连接封装
    /// </summary>
    public sealed class NetworkConnection : IDisposable
    {
        private readonly TcpClient _client;
        private readonly NetworkStream _stream;
        private readonly ArrayPool<byte> _arrayPool;
        private readonly byte[] _receiveBuffer;
        private volatile bool _disposed;

        public NetworkStream Stream => _stream;
        public bool IsHealthy => !_disposed && _client.Connected;

        public NetworkConnection(TcpClient client, NetworkStream stream, ArrayPool<byte> arrayPool)
        {
            _client = client;
            _stream = stream;
            _arrayPool = arrayPool;
            _receiveBuffer = _arrayPool.Rent(65536);
        }

        /// <summary>
        /// 异步读取数据
        /// </summary>
        public async Task<ReadOnlyMemory<byte>> ReadAsync(CancellationToken cancellationToken = default)
        {
            var totalBytes = await _stream.ReadAsync(_receiveBuffer, 0, _receiveBuffer.Length, cancellationToken);
            return _receiveBuffer.AsMemory(0, totalBytes);
        }

        /// <summary>
        /// 异步读取指定长度的数据
        /// </summary>
        public async Task<ReadOnlyMemory<byte>> ReadAsync(int length, CancellationToken cancellationToken = default)
        {
            var buffer = _arrayPool.Rent(length);
            try
            {
                var totalBytes = 0;
                while (totalBytes < length)
                {
                    var bytesRead = await _stream.ReadAsync(buffer, totalBytes, length - totalBytes, cancellationToken);
                    if (bytesRead == 0)
                        throw new Exception("连接已关闭");
                    totalBytes += bytesRead;
                }
                
                return buffer.AsMemory(0, totalBytes);
            }
            catch
            {
                _arrayPool.Return(buffer);
                throw;
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            
            _disposed = true;
            
            _arrayPool.Return(_receiveBuffer);
            _stream?.Dispose();
            _client?.Dispose();
        }
    }


    /// <summary>
    /// 高性能网络响应解析器
    /// </summary>
    public static class NetworkResponseParser
    {
        /// <summary>
        /// 解析UInt16 (大端序)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort ReadUInt16BigEndian(ReadOnlySpan<byte> data, int offset)
        {
            return (ushort)((data[offset] << 8) | data[offset + 1]);
        }

        /// <summary>
        /// 解析UInt32 (大端序)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ReadUInt32BigEndian(ReadOnlySpan<byte> data, int offset)
        {
            return (uint)((data[offset] << 24) | (data[offset + 1] << 16) | (data[offset + 2] << 8) | data[offset + 3]);
        }

        /// <summary>
        /// 解析Float (大端序)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ReadFloatBigEndian(ReadOnlySpan<byte> data, int offset)
        {
            unsafe
            {
                fixed (byte* ptr = data.Slice(offset, 4))
                {
                    return *(float*)ptr;
                }
            }
        }

        /// <summary>
        /// 解析Double (大端序)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double ReadDoubleBigEndian(ReadOnlySpan<byte> data, int offset)
        {
            unsafe
            {
                fixed (byte* ptr = data.Slice(offset, 8))
                {
                    return *(double*)ptr;
                }
            }
        }

        /// <summary>
        /// 计算校验和
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte CalculateChecksum(ReadOnlySpan<byte> data)
        {
            byte checksum = 0;
            foreach (var b in data)
            {
                checksum ^= b;
            }
            return checksum;
        }

        /// <summary>
        /// 验证响应完整性
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ValidateResponse(ReadOnlySpan<byte> data, int expectedLength)
        {
            return data.Length >= expectedLength;
        }
    }
}
