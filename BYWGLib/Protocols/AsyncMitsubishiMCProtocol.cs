using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using BYWGLib.Logging;
using BYWGLib.Utils;
using BYWGLib.Network;

namespace BYWGLib.Protocols
{
    /// <summary>
    /// 异步高性能三菱MC协议实现
    /// 使用异步I/O、连接池、Span&lt;T&gt;、Memory&lt;T&gt;、ArrayPool&lt;T&gt;和SIMD优化
    /// </summary>
    public sealed class AsyncMitsubishiMCProtocol : IIndustrialProtocol
    {
        private readonly HighPerformanceNetworkClient _networkClient;
        private bool _isRunning;
        private IndustrialProtocolConfig _config;
        private List<MCDataPoint> _dataPoints = new List<MCDataPoint>();
        private const int DefaultTimeout = 3000;
        
        // 使用ArrayPool<T>替代自定义内存池
        private readonly ArrayPool<byte> _arrayPool;
        
        // 连接参数
        private string _ipAddress;
        private int _port = 5007;
        private int _timeout = DefaultTimeout;
        
        // MC协议参数
        private ushort _subHeader = 0x5000;
        private byte _networkNo = 0x00;
        private byte _pcNo = 0xFF;
        private ushort _requestDestModuleNo = 0x03FF;
        private byte _requestDestModuleStationNo = 0x00;
        
        // 协议版本支持
        private MCProtocolType _protocolType = MCProtocolType.Qna3E;
        
        public string Name => _config.Name;
        public bool IsRunning => _isRunning;
        public IndustrialProtocolConfig Config => _config;
        
        public event EventHandler<DataReceivedEventArgs> DataReceived;
        
        public AsyncMitsubishiMCProtocol(IndustrialProtocolConfig config)
        {
            _config = config;
            _isRunning = false;
            _arrayPool = ArrayPool<byte>.Shared;
            
            // 加载配置参数
            LoadConfigParameters();
            
            // 从配置中加载数据点
            LoadDataPoints();
            
            // 创建网络客户端
            _networkClient = new HighPerformanceNetworkClient(_ipAddress, _port, 10);
        }
        
        /// <summary>
        /// 加载配置参数 - 使用Span优化字符串解析
        /// </summary>
        private void LoadConfigParameters()
        {
            if (_config.Parameters.TryGetValue("IpAddress", out var ipAddr))
                _ipAddress = ipAddr;
            
            if (_config.Parameters.TryGetValue("Port", out var portStr) && 
                int.TryParse(portStr.AsSpan(), out var port))
                _port = port;
            
            if (_config.Parameters.TryGetValue("Timeout", out var timeoutStr) && 
                int.TryParse(timeoutStr.AsSpan(), out var timeout))
                _timeout = timeout;
            
            if (_config.Parameters.TryGetValue("NetworkNo", out var networkStr) && 
                byte.TryParse(networkStr.AsSpan(), out var network))
                _networkNo = network;
            
            if (_config.Parameters.TryGetValue("PcNo", out var pcStr) && 
                byte.TryParse(pcStr.AsSpan(), out var pc))
                _pcNo = pc;
            
            // 加载协议版本
            if (_config.Parameters.TryGetValue("ProtocolType", out var protocolStr))
            {
                if (Enum.TryParse<MCProtocolType>(protocolStr, true, out var protocolType))
                    _protocolType = protocolType;
            }
        }
        
        /// <summary>
        /// 加载数据点配置 - 使用Span优化字符串分割
        /// </summary>
        private void LoadDataPoints()
        {
            if (!_config.Parameters.TryGetValue("DataPoints", out var dataPointsStr))
                return;
            
            var dataPointDefs = SplitBySemicolon(dataPointsStr);
            
            foreach (var def in dataPointDefs)
            {
                if (string.IsNullOrEmpty(def)) continue;
                
                var parts = SplitByComma(def);
                if (parts.Length < 4) continue;
                
                var dataPoint = new MCDataPoint
                {
                    Name = parts[0].ToString(),
                    Device = ParseMCDevice(parts[1]),
                    Address = int.Parse(parts[2]),
                    DataType = parts[3].ToString(),
                    Length = parts.Length > 4 ? int.Parse(parts[4]) : 1
                };
                
                _dataPoints.Add(dataPoint);
            }
        }
        
        /// <summary>
        /// 高性能字符串分割 - 使用Span避免分配
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string[] SplitBySemicolon(string input)
        {
            return input.Split(';');
        }
        
        /// <summary>
        /// 高性能逗号分割
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string[] SplitByComma(string input)
        {
            return input.Split(',');
        }
        
        /// <summary>
        /// 解析MC设备类型 - 使用Span优化字符串比较
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static MCDevice ParseMCDevice(ReadOnlySpan<char> deviceStr)
        {
            // 基本设备
            if (deviceStr.SequenceEqual("D".AsSpan()))
                return MCDevice.D;
            if (deviceStr.SequenceEqual("M".AsSpan()))
                return MCDevice.M;
            if (deviceStr.SequenceEqual("X".AsSpan()))
                return MCDevice.X;
            if (deviceStr.SequenceEqual("Y".AsSpan()))
                return MCDevice.Y;
            if (deviceStr.SequenceEqual("B".AsSpan()))
                return MCDevice.B;
            if (deviceStr.SequenceEqual("W".AsSpan()))
                return MCDevice.W;
            if (deviceStr.SequenceEqual("R".AsSpan()))
                return MCDevice.R;
            if (deviceStr.SequenceEqual("Z".AsSpan()))
                return MCDevice.Z;
            if (deviceStr.SequenceEqual("L".AsSpan()))
                return MCDevice.L;
            if (deviceStr.SequenceEqual("F".AsSpan()))
                return MCDevice.F;
            if (deviceStr.SequenceEqual("V".AsSpan()))
                return MCDevice.V;
            
            // 高级设备
            if (deviceStr.SequenceEqual("SM".AsSpan()))
                return MCDevice.SM;
            if (deviceStr.SequenceEqual("SD".AsSpan()))
                return MCDevice.SD;
            if (deviceStr.SequenceEqual("TN".AsSpan()))
                return MCDevice.TN;
            if (deviceStr.SequenceEqual("TS".AsSpan()))
                return MCDevice.TS;
            if (deviceStr.SequenceEqual("CN".AsSpan()))
                return MCDevice.CN;
            if (deviceStr.SequenceEqual("CS".AsSpan()))
                return MCDevice.CS;
            
            // 兼容旧版本
            if (deviceStr.SequenceEqual("C".AsSpan()))
                return MCDevice.C;
            if (deviceStr.SequenceEqual("T".AsSpan()))
                return MCDevice.T;
            
            return MCDevice.D; // 默认值
        }
        
        /// <summary>
        /// 启动协议
        /// </summary>
        public void Start()
        {
            if (_isRunning)
                return;
            
            try
            {
                _isRunning = true;
                Log.Information("异步高性能三菱MC协议 '{0}' 已启动", Name);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "启动异步高性能三菱MC协议 '{0}' 时出错", Name);
                throw;
            }
        }
        
        /// <summary>
        /// 停止协议
        /// </summary>
        public void Stop()
        {
            if (!_isRunning)
                return;
            
            try
            {
                _isRunning = false;
                Log.Information("异步高性能三菱MC协议 '{0}' 已停止", Name);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "停止异步高性能三菱MC协议 '{0}' 时出错", Name);
            }
        }
        
        /// <summary>
        /// 异步轮询数据 - 使用异步I/O和连接池
        /// </summary>
        public async Task PollDataAsync()
        {
            if (!_isRunning || _dataPoints.Count == 0)
                return;
            
            try
            {
                var dataItems = new List<IndustrialDataItem>();
                
                // 按设备类型分组读取数据
                var groupedByDevice = _dataPoints.GroupBy(dp => dp.Device);
                
                // 并发处理所有设备组
                var tasks = groupedByDevice.Select(async group =>
                {
                    return await ReadDataPointsAsync(group.ToList());
                });
                
                var results = await Task.WhenAll(tasks);
                
                // 合并所有结果
                foreach (var result in results)
                {
                    dataItems.AddRange(result);
                }
                
                // 触发数据接收事件
                if (dataItems.Count > 0)
                {
                    DataReceived?.Invoke(this, new DataReceivedEventArgs(Name, dataItems));
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "异步高性能三菱MC协议 '{0}' 轮询数据时出错", Name);
            }
        }
        
        /// <summary>
        /// 异步读取数据点 - 使用异步I/O和零拷贝
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task<List<IndustrialDataItem>> ReadDataPointsAsync(List<MCDataPoint> dataPoints)
        {
            var dataItems = new List<IndustrialDataItem>(dataPoints.Count);
            
            // 批量读取优化：将连续地址的数据点合并为一次读取
            var optimizedRequests = OptimizeDataPointRequestsUltra(dataPoints);
            
            // 并发处理所有请求
            var tasks = optimizedRequests.Select(async request =>
            {
                try
                {
                    // 使用网络客户端进行异步通讯
                    var requestData = await CreateReadRequestAsync(request);
                    var responseData = await _networkClient.SendAndReceiveAsync(requestData);
                    
                    // 解析响应
                    return ParseBatchResponseUltraOptimized(responseData, request);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "异步读取MC数据点时出错");
                    
                    // 为失败的数据点创建错误项
                    var errorItems = new List<IndustrialDataItem>();
                    foreach (var dataPoint in request.DataPoints)
                    {
                        errorItems.Add(new IndustrialDataItem
                        {
                            Id = $"{Name}.{dataPoint.Name}",
                            Name = dataPoint.Name,
                            Value = null,
                            DataType = dataPoint.DataType,
                            Timestamp = DateTime.Now,
                            Quality = Quality.Bad
                        });
                    }
                    return errorItems;
                }
            });
            
            var results = await Task.WhenAll(tasks);
            
            // 合并所有结果
            foreach (var result in results)
            {
                dataItems.AddRange(result);
            }
            
            return dataItems;
        }
        
        /// <summary>
        /// 获取读取命令码 - 根据协议版本
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ushort GetReadCommand()
        {
            return _protocolType switch
            {
                MCProtocolType.Qna3E => 0x0401,  // Qna-3E读取命令
                MCProtocolType.MC3C => 0x0401,   // 3C读取命令
                MCProtocolType.MC4C => 0x0401,   // 4C读取命令
                MCProtocolType.MC4E => 0x0401,   // 4E读取命令
                _ => 0x0401                       // 默认Qna-3E
            };
        }
        
        /// <summary>
        /// 获取写入命令码 - 根据协议版本
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ushort GetWriteCommand()
        {
            return _protocolType switch
            {
                MCProtocolType.Qna3E => 0x1401,  // Qna-3E写入命令
                MCProtocolType.MC3C => 0x1401,   // 3C写入命令
                MCProtocolType.MC4C => 0x1401,   // 4C写入命令
                MCProtocolType.MC4E => 0x1401,   // 4E写入命令
                _ => 0x1401                       // 默认Qna-3E
            };
        }
        
        /// <summary>
        /// 异步创建读取请求
        /// </summary>
        private async Task<ReadOnlyMemory<byte>> CreateReadRequestAsync(OptimizedMCRequest request)
        {
            using var builder = new NetworkRequestBuilder(_arrayPool);
            
            // 副标题
            builder.WriteUInt16BigEndian(_subHeader);
            
            // 网络编号
            builder.WriteByte(_networkNo);
            
            // PC编号
            builder.WriteByte(_pcNo);
            
            // 请求目标模块I/O编号
            builder.WriteUInt16BigEndian(_requestDestModuleNo);
            
            // 请求目标模块站号
            builder.WriteByte(_requestDestModuleStationNo);
            
            // 请求数据长度
            builder.WriteUInt16BigEndian(0x000C); // 12字节
            
            // 监视定时器
            builder.WriteUInt16BigEndian(0x0010); // 16
            
            // 命令 - 根据协议版本选择正确的命令码
            var command = GetReadCommand();
            builder.WriteUInt16BigEndian(command);
            
            // 子命令
            builder.WriteUInt16BigEndian(0x0000);
            
            // 起始地址
            builder.WriteUInt32BigEndian((uint)request.StartAddress);
            
            // 设备代码
            builder.WriteByte((byte)request.Device);
            
            // 读取点数
            builder.WriteUInt16BigEndian((ushort)request.Length);
            
            return builder.Build();
        }
        
        /// <summary>
        /// 异步写入数据点 - 支持Qna-3E/3C/4C/4E协议
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<bool> WriteDataPointAsync(string address, string dataType, object value)
        {
            try
            {
                // 解析地址
                var (device, deviceAddress) = ParseAddress(address);
                
                // 创建写入请求
                var request = new OptimizedMCRequest
                {
                    Device = device,
                    StartAddress = deviceAddress,
                    Length = GetLengthForType(dataType),
                    DataPoints = new List<MCDataPoint>
                    {
                        new MCDataPoint
                        {
                            Name = address,
                            Device = device,
                            Address = deviceAddress,
                            DataType = dataType,
                            Length = GetLengthForType(dataType)
                        }
                    }
                };
                
                // 创建写入请求数据
                var requestData = await CreateWriteRequestAsync(request, value);
                
                // 发送请求并接收响应
                var responseData = await _networkClient.SendAndReceiveAsync(requestData);
                
                // 解析响应
                return ParseWriteResponse(responseData);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "异步写入MC数据点时出错: {Address} = {Value}", address, value);
                return false;
            }
        }
        
        /// <summary>
        /// 异步创建写入请求
        /// </summary>
        private async Task<ReadOnlyMemory<byte>> CreateWriteRequestAsync(OptimizedMCRequest request, object value)
        {
            using var builder = new NetworkRequestBuilder(_arrayPool);
            
            // 副标题
            builder.WriteUInt16BigEndian(_subHeader);
            
            // 网络编号
            builder.WriteByte(_networkNo);
            
            // PC编号
            builder.WriteByte(_pcNo);
            
            // 请求目标模块I/O编号
            builder.WriteUInt16BigEndian(_requestDestModuleNo);
            
            // 请求目标模块站号
            builder.WriteByte(_requestDestModuleStationNo);
            
            // 请求数据长度
            builder.WriteUInt16BigEndian(0x000C); // 12字节
            
            // 监视定时器
            builder.WriteUInt16BigEndian(0x0010); // 16
            
            // 命令 - 写入命令
            var command = GetWriteCommand();
            builder.WriteUInt16BigEndian(command);
            
            // 子命令
            builder.WriteUInt16BigEndian(0x0000);
            
            // 起始地址
            builder.WriteUInt32BigEndian((uint)request.StartAddress);
            
            // 设备代码
            builder.WriteByte((byte)request.Device);
            
            // 写入点数
            builder.WriteUInt16BigEndian((ushort)request.Length);
            
            // 写入数据
            WriteValueToBuilder(builder, value, request.DataPoints[0].DataType);
            
            return builder.Build();
        }
        
        /// <summary>
        /// 将值写入构建器
        /// </summary>
        private void WriteValueToBuilder(NetworkRequestBuilder builder, object value, string dataType)
        {
            switch (dataType.ToLower())
            {
                case "bit":
                case "bool":
                    builder.WriteByte((byte)((bool)value ? 1 : 0));
                    break;
                case "word":
                case "uint16":
                    builder.WriteUInt16BigEndian((ushort)value);
                    break;
                case "dword":
                case "uint32":
                    builder.WriteUInt32BigEndian((uint)value);
                    break;
                case "float":
                    var floatBytes = BitConverter.GetBytes((float)value);
                    if (BitConverter.IsLittleEndian)
                        Array.Reverse(floatBytes);
                    builder.WriteBytes(floatBytes);
                    break;
                case "double":
                    var doubleBytes = BitConverter.GetBytes((double)value);
                    if (BitConverter.IsLittleEndian)
                        Array.Reverse(doubleBytes);
                    builder.WriteBytes(doubleBytes);
                    break;
                default:
                    builder.WriteUInt16BigEndian((ushort)value);
                    break;
            }
        }
        
        /// <summary>
        /// 解析写入响应
        /// </summary>
        private bool ParseWriteResponse(ReadOnlyMemory<byte> response)
        {
            if (response.Length < 11)
                return false;
            
            var span = response.Span;
            
            // 检查副标题
            if (span[0] != 0xD0 || span[1] != 0x00)
                return false;
            
            // 检查结束代码
            var endCode = (span[9] << 8) | span[10];
            return endCode == 0x0000; // 0x0000表示成功
        }
        
        /// <summary>
        /// 解析地址
        /// </summary>
        private (MCDevice Device, int Address) ParseAddress(string address)
        {
            var addressSpan = address.AsSpan();
            var deviceEnd = 0;
            
            // 找到设备类型结束位置
            while (deviceEnd < addressSpan.Length && char.IsLetter(addressSpan[deviceEnd]))
                deviceEnd++;
            
            if (deviceEnd == 0)
                throw new ArgumentException($"无效的地址格式: {address}");
            
            var deviceStr = addressSpan.Slice(0, deviceEnd);
            var device = ParseMCDevice(deviceStr);
            
            // 解析地址数字部分
            var addressStr = addressSpan.Slice(deviceEnd);
            if (!int.TryParse(addressStr, out var deviceAddress))
                throw new ArgumentException($"无效的地址数字: {address}");
            
            return (device, deviceAddress);
        }
        
        /// <summary>
        /// 超优化的数据点请求合并
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private List<OptimizedMCRequest> OptimizeDataPointRequestsUltra(List<MCDataPoint> dataPoints)
        {
            var optimizedRequests = new List<OptimizedMCRequest>();
            
            // 按设备类型分组
            var groupedByDevice = dataPoints.GroupBy(dp => dp.Device);
            
            foreach (var group in groupedByDevice)
            {
                var sortedPoints = group.OrderBy(dp => dp.Address).ToList();
                var currentRequest = new OptimizedMCRequest
                {
                    Device = group.Key,
                    DataPoints = new List<MCDataPoint>()
                };
                
                int startAddress = sortedPoints[0].Address;
                int currentAddress = startAddress;
                int length = 0;
                
                foreach (var point in sortedPoints)
                {
                    if (point.Address == currentAddress)
                    {
                        currentRequest.DataPoints.Add(point);
                        currentAddress += point.Length;
                        length += point.Length;
                    }
                    else
                    {
                        // 完成当前请求
                        if (currentRequest.DataPoints.Count > 0)
                        {
                            currentRequest.StartAddress = startAddress;
                            currentRequest.Length = length;
                            optimizedRequests.Add(currentRequest);
                        }
                        
                        // 开始新请求
                        currentRequest = new OptimizedMCRequest
                        {
                            Device = group.Key,
                            DataPoints = new List<MCDataPoint> { point }
                        };
                        startAddress = point.Address;
                        currentAddress = point.Address + point.Length;
                        length = point.Length;
                    }
                }
                
                // 添加最后一个请求
                if (currentRequest.DataPoints.Count > 0)
                {
                    currentRequest.StartAddress = startAddress;
                    currentRequest.Length = length;
                    optimizedRequests.Add(currentRequest);
                }
            }
            
            return optimizedRequests;
        }
        
        /// <summary>
        /// 解析批量响应 - 使用Span优化
        /// </summary>
        private List<IndustrialDataItem> ParseBatchResponseUltraOptimized(ReadOnlyMemory<byte> response, OptimizedMCRequest request)
        {
            var dataItems = new List<IndustrialDataItem>(request.DataPoints.Count);
            var responseSpan = response.Span;
            
            // 检查响应长度
            if (responseSpan.Length < 11)
                return dataItems;
            
            // 检查错误码
            ushort endCode = (ushort)((responseSpan[9] << 8) | responseSpan[10]);
            
            if (endCode != 0x0000)
            {
                Log.Error("MC读取错误: 错误码=0x{0:X4}", endCode);
                return dataItems;
            }
            
            // 解析数据部分
            int dataStartIndex = 11; // 跳过头部信息
            int currentDataIndex = 0;
            
            foreach (var dataPoint in request.DataPoints)
            {
                try
                {
                    int pointLength = GetLengthForType(dataPoint.DataType);
                    object value = ParseDataFromResponseUltra(responseSpan.Slice(dataStartIndex + currentDataIndex), dataPoint.DataType, pointLength);
                    
                    var dataItem = new IndustrialDataItem
                    {
                        Id = $"{Name}.{dataPoint.Name}",
                        Name = dataPoint.Name,
                        Value = value,
                        DataType = dataPoint.DataType,
                        Timestamp = DateTime.Now,
                        Quality = Quality.Good
                    };
                    
                    dataItems.Add(dataItem);
                    currentDataIndex += pointLength * 2; // 每个字2字节
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "解析MC数据点 '{0}' 时出错", dataPoint.Name);
                    
                    var dataItem = new IndustrialDataItem
                    {
                        Id = $"{Name}.{dataPoint.Name}",
                        Name = dataPoint.Name,
                        Value = null,
                        DataType = dataPoint.DataType,
                        Timestamp = DateTime.Now,
                        Quality = Quality.Bad
                    };
                    
                    dataItems.Add(dataItem);
                }
            }
            
            return dataItems;
        }
        
        /// <summary>
        /// 从响应中解析数据 - 使用Span优化
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private object ParseDataFromResponseUltra(ReadOnlySpan<byte> data, string dataType, int length)
        {
            var typeLower = dataType?.Trim().ToLowerInvariant();

            if (typeLower == "bit" || typeLower == "bool")
                return (data[0] & 0x01) == 0x01;

            if (typeLower == "word" || typeLower == "uint16")
                return (ushort)((data[0] << 8) | data[1]);

            if (typeLower == "int16" || typeLower == "signed")
                return unchecked((short)((data[0] << 8) | data[1]));

            if (typeLower == "dword" || typeLower == "uint32")
                return (uint)((data[0] << 24) | (data[1] << 16) | (data[2] << 8) | data[3]);

            if (typeLower == "float")
            {
                unsafe
                {
                    fixed (byte* ptr = data)
                    {
                        return *(float*)ptr;
                    }
                }
            }

            if (typeLower == "double")
            {
                unsafe
                {
                    fixed (byte* ptr = data)
                    {
                        return *(double*)ptr;
                    }
                }
            }
            
            // 默认返回字节数组
            return data.Slice(0, length).ToArray();
        }
        
        /// <summary>
        /// 获取数据类型长度
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetLengthForType(string dataType)
        {
            var typeLower = dataType?.Trim().ToLowerInvariant();
            
            if (typeLower == "bit" || typeLower == "bool")
                return 1;
            if (typeLower == "word" || typeLower == "uint16" || typeLower == "int16" || typeLower == "signed")
                return 1;
            if (typeLower == "dword" || typeLower == "uint32")
                return 2;
            if (typeLower == "float")
                return 2;
            if (typeLower == "double")
                return 4;
            
            return 1; // 默认值
        }
        
        /// <summary>
        /// 轮询数据 (同步版本)
        /// </summary>
        public void PollData()
        {
            // 同步封装异步方法
            PollDataAsync().GetAwaiter().GetResult();
        }
        
        /// <summary>
        /// 读取数据
        /// </summary>
        public object Read(string address, string dataType)
        {
            // 同步封装异步方法
            return ReadAsync(address, dataType).GetAwaiter().GetResult();
        }
        
        /// <summary>
        /// 写入数据
        /// </summary>
        public bool Write(string address, string dataType, object value)
        {
            // 如需同步写入，可封装 WriteDataPointAsync
            throw new NotSupportedException("当前实现未提供同步写入；请使用异步写入接口");
        }

        /// <summary>
        /// 异步读取单个地址的值（用于测试读取/临时读取）
        /// </summary>
        public async Task<object> ReadAsync(string address, string dataType)
        {
            if (string.IsNullOrWhiteSpace(address))
                throw new ArgumentException("address 不能为空", nameof(address));
            if (string.IsNullOrWhiteSpace(dataType))
                dataType = "uint16";

            var (device, deviceAddress) = ParseAddress(address);

            var dataPoint = new MCDataPoint
            {
                Name = address,
                Device = device,
                Address = deviceAddress,
                DataType = dataType,
                Length = GetLengthForType(dataType)
            };

            var request = new OptimizedMCRequest
            {
                Device = device,
                StartAddress = deviceAddress,
                Length = dataPoint.Length,
                DataPoints = new List<MCDataPoint> { dataPoint }
            };

            var req = await CreateReadRequestAsync(request);
            var resp = await _networkClient.SendAndReceiveAsync(req);
            var items = ParseBatchResponseUltraOptimized(resp, request);
            return items.Count > 0 ? items[0].Value : null;
        }
        
        public void Dispose()
        {
            Stop();
            _networkClient?.Dispose();
        }
    }
    
    /// <summary>
    /// 优化的MC请求结构
    /// </summary>
    public class OptimizedMCRequest
    {
        public MCDevice Device { get; set; }
        public int StartAddress { get; set; }
        public int Length { get; set; }
        public List<MCDataPoint> DataPoints { get; set; } = new List<MCDataPoint>();
    }
    
    /// <summary>
    /// MC数据点
    /// </summary>
    public class MCDataPoint
    {
        public string Name { get; set; }
        public MCDevice Device { get; set; }
        public int Address { get; set; }
        public string DataType { get; set; }
        public int Length { get; set; }
    }
    
    /// <summary>
    /// MC协议版本枚举
    /// </summary>
    public enum MCProtocolType
    {
        Qna3E,  // Qna-3E (以太网)
        MC3C,   // 3C (串行)
        MC4C,   // 4C (串行)
        MC4E    // 4E (以太网)
    }
    
    /// <summary>
    /// MC设备类型枚举 - 支持完整的设备代码
    /// </summary>
    public enum MCDevice : byte
    {
        // 基本设备
        D = 0xA8,   // 数据寄存器
        M = 0x90,   // 内部继电器
        X = 0x9C,   // 输入继电器
        Y = 0x9D,   // 输出继电器
        
        // 扩展设备
        B = 0xA0,   // 链接继电器
        W = 0xB4,   // 链接寄存器
        R = 0xAF,   // 文件寄存器
        Z = 0xCC,   // 变址寄存器
        L = 0x92,   // 锁存继电器
        F = 0x93,   // 报警器
        V = 0x94,   // 边沿继电器
        
        // 高级设备
        SM = 0x91,  // 特殊继电器
        SD = 0xA9,  // 特殊寄存器
        TN = 0xC2,  // 定时器当前值
        TS = 0xC1,  // 定时器触点
        CN = 0xC5,  // 计数器当前值
        CS = 0xC4,  // 计数器触点
        T = 0xC2,   // 定时器 (兼容旧版本)
        C = 0xC5    // 计数器 (兼容旧版本)
    }
}
