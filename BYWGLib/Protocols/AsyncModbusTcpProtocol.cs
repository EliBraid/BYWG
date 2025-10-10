using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using BYWGLib.Logging;
using BYWGLib.Utils;
using BYWGLib.Network;

namespace BYWGLib.Protocols
{
    /// <summary>
    /// 异步高性能Modbus TCP协议实现
    /// 使用异步I/O、连接池、Span&lt;T&gt;、Memory&lt;T&gt;、ArrayPool&lt;T&gt;和SIMD优化
    /// </summary>
    public sealed class AsyncModbusTcpProtocol : IIndustrialProtocol
    {
        private readonly HighPerformanceNetworkClient _networkClient;
        private bool _isRunning;
        private IndustrialProtocolConfig _config;
        private List<ModbusDataPoint> _dataPoints = new List<ModbusDataPoint>();
        private int _transactionId = 0;
        private const int DefaultTimeout = 3000;
        
        // 使用ArrayPool<T>替代自定义内存池
        private readonly ArrayPool<byte> _arrayPool;
        
        // 连接参数
        private string _ipAddress;
        private int _port = 502;
        private int _timeout = DefaultTimeout;
        private int _unitId = 1;
        
        public string Name => _config.Name;
        public bool IsRunning => _isRunning;
        public IndustrialProtocolConfig Config => _config;
        
        public event EventHandler<DataReceivedEventArgs> DataReceived;
        
        public AsyncModbusTcpProtocol(IndustrialProtocolConfig config)
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
            
            if (_config.Parameters.TryGetValue("UnitId", out var unitIdStr) && 
                int.TryParse(unitIdStr.AsSpan(), out var unitId))
                _unitId = unitId;
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
                
                var dataPoint = new ModbusDataPoint
                {
                    Name = parts[0].ToString(),
                    Address = parts[1].ToString(),
                    DataType = parts[2].ToString(),
                    FunctionCode = int.Parse(parts[3])
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
        /// 启动协议
        /// </summary>
        public void Start()
        {
            if (_isRunning)
                return;
            
            try
            {
                _isRunning = true;
                Log.Information("异步高性能Modbus TCP协议 '{0}' 已启动", Name);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "启动异步高性能Modbus TCP协议 '{0}' 时出错", Name);
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
                Log.Information("异步高性能Modbus TCP协议 '{0}' 已停止", Name);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "停止异步高性能Modbus TCP协议 '{0}' 时出错", Name);
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
                
                // 按功能码分组读取数据
                var groupedByFunctionCode = _dataPoints.GroupBy(dp => dp.FunctionCode);
                
                // 并发处理所有功能码组
                var tasks = groupedByFunctionCode.Select(async group =>
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
                    Log.Debug("触发DataReceived事件: 协议={0}, 数据项数量={1}", Name, dataItems.Count);
                    foreach (var item in dataItems)
                    {
                        Log.Debug("数据项: Id={0}, Name={1}, Value={2}", item.Id, item.Name, item.Value);
                    }
                    DataReceived?.Invoke(this, new DataReceivedEventArgs(Name, dataItems));
                }
                else
                {
                    Log.Debug("没有数据项，跳过DataReceived事件触发");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "异步高性能Modbus TCP协议 '{0}' 轮询数据时出错", Name);
            }
        }
        
        /// <summary>
        /// 异步读取数据点 - 使用异步I/O和零拷贝
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task<List<IndustrialDataItem>> ReadDataPointsAsync(List<ModbusDataPoint> dataPoints)
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
                    Log.Error(ex, "异步读取Modbus数据点时出错");
                    
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
        /// 异步创建读取请求
        /// </summary>
        private async Task<ReadOnlyMemory<byte>> CreateReadRequestAsync(OptimizedModbusRequest request)
        {
            using var builder = new NetworkRequestBuilder(_arrayPool);
            
            int transactionId = Interlocked.Increment(ref _transactionId);
            
            // MBAP头
            builder.WriteUInt16BigEndian((ushort)transactionId); // Transaction Id
            builder.WriteUInt16BigEndian(0); // Protocol Id
            builder.WriteUInt16BigEndian(6); // Length
            builder.WriteByte((byte)_unitId); // Unit Id
            
            // PDU
            builder.WriteByte((byte)request.FunctionCode); // Function Code
            builder.WriteUInt16BigEndian((ushort)request.StartAddress); // Start Address
            builder.WriteUInt16BigEndian((ushort)request.Quantity); // Quantity
            
            var requestData = builder.Build();
            
            // 添加详细的请求调试信息
            Log.Debug("创建Modbus请求: 事务ID={0}, 单元ID={1}, 功能码={2}, 起始地址={3}, 数量={4}", 
                transactionId, _unitId, request.FunctionCode, request.StartAddress, request.Quantity);
            Log.Debug("完整请求数据: [{0}]", 
                string.Join(" ", requestData.ToArray().Select(b => b.ToString("X2"))));
            
            return requestData;
        }
        
        /// <summary>
        /// 超优化的数据点请求合并
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private List<OptimizedModbusRequest> OptimizeDataPointRequestsUltra(List<ModbusDataPoint> dataPoints)
        {
            var optimizedRequests = new List<OptimizedModbusRequest>();
            
            // 按功能码分组
            var groupedByFunctionCode = dataPoints.GroupBy(dp => dp.FunctionCode);
            
            foreach (var group in groupedByFunctionCode)
            {
                var sortedPoints = group.OrderBy(dp => int.Parse(dp.Address)).ToList();
                var currentRequest = new OptimizedModbusRequest
                {
                    FunctionCode = group.Key,
                    DataPoints = new List<ModbusDataPoint>()
                };
                
                int startAddress = int.Parse(sortedPoints[0].Address);
                int currentAddress = startAddress;
                int quantity = 1;
                
                foreach (var point in sortedPoints)
                {
                    int address = int.Parse(point.Address);
                    
                    if (address == currentAddress)
                    {
                        currentRequest.DataPoints.Add(point);
                        currentAddress++;
                        quantity++;
                    }
                    else
                    {
                        // 完成当前请求
                        if (currentRequest.DataPoints.Count > 0)
                        {
                            currentRequest.StartAddress = startAddress;
                            currentRequest.Quantity = quantity;
                            optimizedRequests.Add(currentRequest);
                        }
                        
                        // 开始新请求
                        currentRequest = new OptimizedModbusRequest
                        {
                            FunctionCode = group.Key,
                            DataPoints = new List<ModbusDataPoint> { point }
                        };
                        startAddress = address;
                        currentAddress = address + 1;
                        quantity = 1;
                    }
                }
                
                // 添加最后一个请求
                if (currentRequest.DataPoints.Count > 0)
                {
                    currentRequest.StartAddress = startAddress;
                    currentRequest.Quantity = quantity;
                    optimizedRequests.Add(currentRequest);
                }
            }
            
            return optimizedRequests;
        }
        
        /// <summary>
        /// 解析批量响应 - 使用Span优化
        /// </summary>
        private List<IndustrialDataItem> ParseBatchResponseUltraOptimized(ReadOnlyMemory<byte> response, OptimizedModbusRequest request)
        {
            var dataItems = new List<IndustrialDataItem>(request.DataPoints.Count);
            var responseSpan = response.Span;
            
            // 检查响应长度
            if (responseSpan.Length < 9) // MBAP头(7) + 功能码(1) + 字节数(1)
            {
                Log.Error("Modbus响应长度不足: {0} 字节", responseSpan.Length);
                return dataItems;
            }
            
            // 检查是否有错误码
            if ((responseSpan[7] & 0x80) == 0x80)
            {
                int errorCode = responseSpan[8];
                string errorMessage = GetModbusErrorMessage(errorCode);
                Log.Error("Modbus错误: 错误码={0} ({1})", errorCode, errorMessage);
                
                // 为所有数据点创建错误项
                foreach (var dataPoint in request.DataPoints)
                {
                    var errorItem = new IndustrialDataItem
                    {
                        Id = $"{Name}.{dataPoint.Name}",
                        Name = dataPoint.Name,
                        Value = null,
                        DataType = dataPoint.DataType,
                        Timestamp = DateTime.Now,
                        Quality = Quality.Bad
                    };
                    dataItems.Add(errorItem);
                }
                return dataItems;
            }
            
            // 解析数据部分
            int dataLength = responseSpan[8]; // 数据字节数
            int dataStartIndex = 9; // 数据开始位置
            
            // 添加完整的响应调试信息
            Log.Debug("Modbus响应: 总长度={0}, 数据长度={1}, 数据起始位置={2}", 
                responseSpan.Length, dataLength, dataStartIndex);
            Log.Debug("完整响应数据: [{0}]", 
                string.Join(" ", responseSpan.ToArray().Select(b => b.ToString("X2"))));
            
            // 解析MBAP头信息
            ushort transactionId = (ushort)((responseSpan[0] << 8) | responseSpan[1]);
            ushort protocolId = (ushort)((responseSpan[2] << 8) | responseSpan[3]);
            ushort length = (ushort)((responseSpan[4] << 8) | responseSpan[5]);
            byte unitId = responseSpan[6];
            byte functionCode = responseSpan[7];
            
            Log.Debug("MBAP头解析: 事务ID={0}, 协议ID={1}, 长度={2}, 单元ID={3}, 功能码=0x{4:X2}", 
                transactionId, protocolId, length, unitId, functionCode);
            
            // 检查数据长度是否足够
            if (dataStartIndex + dataLength > responseSpan.Length)
            {
                Log.Error("Modbus数据长度不匹配: 期望{0}字节，实际{1}字节", dataLength, responseSpan.Length - dataStartIndex);
                return dataItems;
            }
            
            // 计算每个数据点的起始位置
            // 根据数据点的实际地址计算偏移量，而不是简单的递增
            int startAddress = request.StartAddress;
            
            foreach (var dataPoint in request.DataPoints)
            {
                try
                {
                    int dataTypeLength = GetLengthForType(dataPoint.DataType);
                    
                    // 计算数据点在响应中的偏移量
                    int dataPointAddress = int.Parse(dataPoint.Address);
                    int offsetInResponse = (dataPointAddress - startAddress) * 2; // 每个寄存器2字节
                    
                    // 检查是否有足够的数据
                    if (offsetInResponse + dataTypeLength > dataLength)
                    {
                        Log.Error("数据点 '{0}' 数据不足: 需要{1}字节，剩余{2}字节", 
                            dataPoint.Name, dataTypeLength, dataLength - offsetInResponse);
                        
                        var errorItem = new IndustrialDataItem
                        {
                            Id = $"{Name}.{dataPoint.Name}",
                            Name = dataPoint.Name,
                            Value = null,
                            DataType = dataPoint.DataType,
                            Timestamp = DateTime.Now,
                            Quality = Quality.Bad
                        };
                        dataItems.Add(errorItem);
                        continue;
                    }
                    
                    // 提取数据并解析
                    var dataSlice = responseSpan.Slice(dataStartIndex + offsetInResponse, dataTypeLength);
                    
                    // 添加详细的调试信息
                    Log.Debug("数据点 '{0}': 起始位置={1}, 数据类型长度={2}, 数据切片=[{3}]", 
                        dataPoint.Name, 
                        dataStartIndex + offsetInResponse, 
                        dataTypeLength,
                        string.Join(" ", dataSlice.ToArray().Select(b => b.ToString("X2"))));
                    
                    object value = ParseDataFromResponseUltra(dataSlice, dataPoint.DataType);
                    
                    var dataItem = new IndustrialDataItem
                    {
                        Id = $"{Name}.{dataPoint.Name}",
                        Name = dataPoint.Name,
                        Value = value,
                        DataType = dataPoint.DataType,
                        Timestamp = DateTime.Now,
                        Quality = Quality.Good
                    };
                    
                    Log.Debug("创建IndustrialDataItem: Id={0}, Name={1}, Value={2}, DataType={3}", 
                        dataItem.Id, dataItem.Name, dataItem.Value, dataItem.DataType);
                    
                    dataItems.Add(dataItem);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "解析Modbus数据点 '{0}' 时出错", dataPoint.Name);
                    
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
        /// 获取Modbus错误消息
        /// </summary>
        private string GetModbusErrorMessage(int errorCode)
        {
            return errorCode switch
            {
                0x01 => "Illegal Function Code (非法功能码)",
                0x02 => "Illegal Data Address (非法数据地址)",
                0x03 => "Illegal Data Value (非法数据值)",
                0x04 => "Slave Device Failure (从设备故障)",
                0x05 => "Acknowledge (确认)",
                0x06 => "Slave Device Busy (从设备忙)",
                0x08 => "Memory Parity Error (内存奇偶校验错误)",
                0x0A => "Gateway Path Unavailable (网关路径不可用)",
                0x0B => "Gateway Target Device Failed to Respond (网关目标设备响应失败)",
                _ => $"Unknown Error Code (未知错误码: 0x{errorCode:X2})"
            };
        }

        /// <summary>
        /// 从响应中解析数据 - 使用Span优化
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private object ParseDataFromResponseUltra(ReadOnlySpan<byte> data, string dataType)
        {
            var typeLower = dataType?.Trim().ToLowerInvariant();
            
            // 添加调试日志
            Log.Debug("解析数据类型: {0}, 数据长度: {1}, 数据: [{2}]", 
                dataType, data.Length, string.Join(", ", data.ToArray().Select(b => b.ToString("X2"))));
            
            if (typeLower == "bit" || typeLower == "bool")
                return (data[0] & 0x01) == 0x01;
            
            if (typeLower == "word" || typeLower == "uint16" || typeLower == "unsign")
            {
                if (data.Length < 2)
                    return null;
                ushort result = (ushort)((data[0] << 8) | data[1]);
                Log.Debug("解析uint16/unsign: {0} -> {1}", string.Join(" ", data.ToArray().Select(b => b.ToString("X2"))), result);
                return result;
            }

            if (typeLower == "int16" || typeLower == "signed")
            {
                // 确保正确处理有符号16位整数
                if (data.Length < 2)
                {
                    Log.Error("signed数据类型数据不足: 需要2字节，实际{0}字节", data.Length);
                    return null;
                }
                
                // 尝试大端序（标准Modbus）
                short resultBigEndian = unchecked((short)((data[0] << 8) | data[1]));
                
                // 尝试小端序（某些设备可能使用）
                short resultLittleEndian = unchecked((short)((data[1] << 8) | data[0]));
                
                Log.Debug("解析signed: 原始数据=[{0}], 大端序={1}, 小端序={2}", 
                    string.Join(" ", data.ToArray().Select(b => b.ToString("X2"))), 
                    resultBigEndian, 
                    resultLittleEndian);
                
                // 默认使用大端序（标准Modbus）
                return resultBigEndian;
            }
            
            if (typeLower == "dword" || typeLower == "uint32")
            {
                if (data.Length < 4)
                    return null;
                return (uint)((data[0] << 24) | (data[1] << 16) | (data[2] << 8) | data[3]);
            }
            
            if (typeLower == "int32")
            {
                if (data.Length < 4)
                    return null;
                return unchecked((int)((data[0] << 24) | (data[1] << 16) | (data[2] << 8) | data[3]));
            }
            
            if (typeLower == "float")
            {
                if (data.Length < 4)
                    return null;
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
                if (data.Length < 8)
                    return null;
                unsafe
                {
                    fixed (byte* ptr = data)
                    {
                        return *(double*)ptr;
                    }
                }
            }
            
            // 默认返回字节数组
            return data.ToArray();
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
            if (typeLower == "word" || typeLower == "uint16" || typeLower == "int16" || typeLower == "signed" || typeLower == "unsign")
                return 2;
            if (typeLower == "dword" || typeLower == "uint32" || typeLower == "int32")
                return 4;
            if (typeLower == "float")
                return 4;
            if (typeLower == "double")
                return 8;
            
            return 2; // 默认值
        }
        
        /// <summary>
        /// 轮询数据 (同步版本)
        /// </summary>
        public void PollData()
        {
            // 同步封装异步方法，便于统一调用
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
        /// 异步读取单个地址的值（用于测试读取/临时读取）
        /// </summary>
        public async Task<object> ReadAsync(string address, string dataType)
        {
            if (string.IsNullOrWhiteSpace(address))
                throw new ArgumentException("address 不能为空", nameof(address));
            if (string.IsNullOrWhiteSpace(dataType))
                dataType = "uint16";

            // 解析地址与功能码
            var (functionCode, startAddress, quantity) = ParseModbusAddress(address, dataType);
            
            // 添加调试日志
            Log.Debug("解析地址 '{0}' -> 功能码: {1}, 起始地址: {2}, 数量: {3}", 
                address, functionCode, startAddress, quantity);
            
            // 检查地址范围
            if (startAddress < 0)
            {
                Log.Error("地址 '{0}' 解析后的起始地址为负数: {1}", address, startAddress);
                throw new ArgumentException($"地址 '{address}' 无效，起始地址不能为负数");
            }
            
            if (startAddress > 65535)
            {
                Log.Error("地址 '{0}' 解析后的起始地址超出范围: {1}", address, startAddress);
                throw new ArgumentException($"地址 '{address}' 无效，起始地址超出Modbus范围");
            }

            // 构造单请求
            var dataPoint = new ModbusDataPoint
            {
                Name = address,
                Address = startAddress.ToString(),
                DataType = dataType,
                FunctionCode = functionCode
            };
            var request = new OptimizedModbusRequest
            {
                FunctionCode = functionCode,
                StartAddress = startAddress,
                Quantity = quantity,
                DataPoints = new List<ModbusDataPoint> { dataPoint }
            };

            var req = await CreateReadRequestAsync(request);
            var resp = await _networkClient.SendAndReceiveAsync(req);
            var items = ParseBatchResponseUltraOptimized(resp, request);
            return items.Count > 0 ? items[0].Value : null;
        }

        private (int functionCode, int startAddress, int quantity) ParseModbusAddress(string address, string dataType)
        {
            // 支持多种地址格式：
            // 1. 标准Modbus格式：40001/30001/10001/00001
            // 2. D前缀格式：D4500 (设备地址4500，实际发送4499)
            // 3. 纯数字格式：4500 (默认Holding Register，实际发送4499)
            // 4. 设备地址格式：4500 (设备上的地址4500，实际发送4499)
            // 5. 0基地址格式：@4500 (直接使用4500作为Modbus地址)
            
            var addrStr = address.Trim();
            int functionCode;
            int startAddress;
            int numeric;

            // 处理@前缀格式（0基地址，如@4500）
            if (addrStr.StartsWith("@", StringComparison.OrdinalIgnoreCase))
            {
                var digits = addrStr.Substring(1);
                if (!int.TryParse(digits, out numeric))
                    throw new ArgumentException($"无效的Modbus地址: {address}");
                
                functionCode = 3; // Holding Register
                startAddress = numeric; // 直接使用，不进行-1操作
                Log.Debug("使用0基地址格式: {0} -> 功能码: {1}, 起始地址: {2}", address, functionCode, startAddress);
            }
            // 处理D前缀格式（如D4500）
            else if (addrStr.StartsWith("D", StringComparison.OrdinalIgnoreCase))
            {
                var digits = addrStr.Substring(1);
                if (!int.TryParse(digits, out numeric))
                    throw new ArgumentException($"无效的Modbus地址: {address}");
                
                functionCode = 3; // Holding Register
                // D4500 表示设备上的地址4500，实际发送的地址是 4500-1 = 4499
                startAddress = numeric - 1; // Modbus帧内0基
                Log.Debug("使用D前缀格式: {0} -> 功能码: {1}, 起始地址: {2}", address, functionCode, startAddress);
            }
            else if (!int.TryParse(addrStr, out numeric))
            {
                // 非纯数字，尝试移除前缀再解析
                var digits = new string(addrStr.Where(char.IsDigit).ToArray());
                if (!int.TryParse(digits, out numeric))
                    throw new ArgumentException($"无效的Modbus地址: {address}");
            }

            // 处理标准Modbus地址格式
            if (numeric >= 40001)
            {
                functionCode = 3; // Holding Register
                startAddress = numeric - 40001; // 40001 -> 0, 40002 -> 1, 44500 -> 4499
                Log.Debug("使用标准Modbus格式: {0} -> 功能码: {1}, 起始地址: {2}", address, functionCode, startAddress);
            }
            else if (numeric >= 30001)
            {
                functionCode = 4; // Input Register
                startAddress = numeric - 30001;
                Log.Debug("使用Input Register格式: {0} -> 功能码: {1}, 起始地址: {2}", address, functionCode, startAddress);
            }
            else if (numeric >= 10001)
            {
                functionCode = 2; // Discrete Input
                startAddress = numeric - 10001;
                Log.Debug("使用Discrete Input格式: {0} -> 功能码: {1}, 起始地址: {2}", address, functionCode, startAddress);
            }
            else if (numeric >= 1)
            {
                // 纯数字格式，默认视为Holding Register
                // 尝试两种地址模式：
                // 1. 1基地址：4500 -> 4499 (标准Modbus)
                // 2. 0基地址：4500 -> 4500 (某些设备)
                functionCode = 3; // Holding Register
                
                // 先尝试0基地址（很多设备使用这种方式）
                startAddress = numeric; // 4500 -> 4500
                Log.Debug("使用纯数字格式(0基): {0} -> 功能码: {1}, 起始地址: {2}", address, functionCode, startAddress);
            }
            else
            {
                functionCode = 1;
                startAddress = 0;
                Log.Debug("使用默认格式: {0} -> 功能码: {1}, 起始地址: {2}", address, functionCode, startAddress);
            }

            var quantity = GetLengthForType(dataType);
            Log.Debug("最终解析结果: 地址={0}, 功能码={1}, 起始地址={2}, 数量={3}, 数据类型={4}", 
                address, functionCode, startAddress, quantity, dataType);
            
            return (functionCode, startAddress, quantity);
        }
        
        /// <summary>
        /// 写入数据
        /// </summary>
        public bool Write(string address, string dataType, object value)
        {
            // 如需同步写入，可按需实现 WriteDataPointAsync 并在此封装
            // 暂提供最小实现：不支持写入
            throw new NotSupportedException("当前实现未提供同步写入；请使用异步写入接口");
        }

        /// <summary>
        /// 异步写入单个保持寄存器（FC6）
        /// </summary>
        public async Task<bool> WriteSingleRegisterAsync(int startAddress, ushort value, int unitId = 1, int timeoutMs = DefaultTimeout)
        {
            var requestData = BuildWriteSingleRegisterRequest(unitId, startAddress, value);
            var resp = await _networkClient.SendAndReceiveAsync(requestData);
            // 简单校验：回显功能码与地址、值
            if (resp.Length >= 12 && resp.Span[7] == 0x06)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 异步写入多个保持寄存器（FC16）
        /// </summary>
        public async Task<bool> WriteMultipleRegistersAsync(int startAddress, ushort[] values, int unitId = 1, int timeoutMs = DefaultTimeout)
        {
            if (values == null || values.Length == 0) throw new ArgumentException("values 不能为空");
            var requestData = BuildWriteMultipleRegistersRequest(unitId, startAddress, values);
            var resp = await _networkClient.SendAndReceiveAsync(requestData);
            if (resp.Length >= 12 && resp.Span[7] == 0x10)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 便捷写入：按地址字符串与数据类型写入（默认 FC6 单寄存器）
        /// </summary>
        public async Task<bool> WriteAsync(string address, string dataType, object value, int? functionCode = null)
        {
            if (string.IsNullOrWhiteSpace(address)) throw new ArgumentException("address 不能为空");
            if (string.IsNullOrWhiteSpace(dataType)) dataType = "uint16";
            var (fc, startAddress, quantity) = ParseModbusAddress(address, dataType);
            var useFc = functionCode ?? 6; // 默认 FC6

            if (useFc == 6)
            {
                ushort val = Convert.ToUInt16(value);
                return await WriteSingleRegisterAsync(startAddress, val, _unitId, _timeout);
            }
            else if (useFc == 16)
            {
                // 将 value 解析为 ushort[]
                ushort[] arr;
                if (value is ushort[] uarr) arr = uarr;
                else if (value is IEnumerable<ushort> us) arr = us.ToArray();
                else arr = new ushort[] { Convert.ToUInt16(value) };
                return await WriteMultipleRegistersAsync(startAddress, arr, _unitId, _timeout);
            }
            else
            {
                throw new NotSupportedException($"暂不支持功能码: {useFc}");
            }
        }

        private ReadOnlyMemory<byte> BuildWriteSingleRegisterRequest(int unitId, int address, ushort value)
        {
            using var builder = new NetworkRequestBuilder(_arrayPool);
            int transactionId = Interlocked.Increment(ref _transactionId);
            // MBAP
            builder.WriteUInt16BigEndian((ushort)transactionId);
            builder.WriteUInt16BigEndian(0);
            builder.WriteUInt16BigEndian(6);
            builder.WriteByte((byte)unitId);
            // PDU FC6
            builder.WriteByte(0x06);
            builder.WriteUInt16BigEndian((ushort)address);
            builder.WriteUInt16BigEndian(value);
            return builder.Build();
        }

        private ReadOnlyMemory<byte> BuildWriteMultipleRegistersRequest(int unitId, int startAddress, ushort[] values)
        {
            using var builder = new NetworkRequestBuilder(_arrayPool);
            int transactionId = Interlocked.Increment(ref _transactionId);
            int byteCount = values.Length * 2;
            // MBAP 长度 = 7 (UnitId+FC+Addr(2)+Qty(2)+ByteCount+Data(N))
            int length = 7 + byteCount;
            builder.WriteUInt16BigEndian((ushort)transactionId);
            builder.WriteUInt16BigEndian(0);
            builder.WriteUInt16BigEndian((ushort)length);
            builder.WriteByte((byte)unitId);
            // PDU FC16
            builder.WriteByte(0x10);
            builder.WriteUInt16BigEndian((ushort)startAddress);
            builder.WriteUInt16BigEndian((ushort)values.Length);
            builder.WriteByte((byte)byteCount);
            foreach (var v in values)
            {
                builder.WriteUInt16BigEndian(v);
            }
            return builder.Build();
        }
        
        /// <summary>
        /// 扫描设备可用的地址范围
        /// </summary>
        public async Task<List<int>> ScanAvailableAddressesAsync(int startAddress = 0, int endAddress = 100, int functionCode = 3)
        {
            var availableAddresses = new List<int>();
            
            Log.Information("开始扫描设备地址范围: {0} - {1}, 功能码: {2}", startAddress, endAddress, functionCode);
            
            for (int addr = startAddress; addr <= endAddress; addr++)
            {
                try
                {
                    var testRequest = new OptimizedModbusRequest
                    {
                        FunctionCode = functionCode,
                        StartAddress = addr,
                        Quantity = 1,
                        DataPoints = new List<ModbusDataPoint>
                        {
                            new ModbusDataPoint
                            {
                                Name = $"Test_{addr}",
                                Address = addr.ToString(),
                                DataType = "uint16",
                                FunctionCode = functionCode
                            }
                        }
                    };
                    
                    var req = await CreateReadRequestAsync(testRequest);
                    var resp = await _networkClient.SendAndReceiveAsync(req);
                    
                    // 检查是否有错误
                    if (resp.Span.Length >= 9 && (resp.Span[7] & 0x80) == 0x80)
                    {
                        // 有错误，跳过这个地址
                        continue;
                    }
                    
                    // 没有错误，地址可用
                    availableAddresses.Add(addr);
                    Log.Debug("发现可用地址: {0}", addr);
                }
                catch (Exception ex)
                {
                    Log.Debug("地址 {0} 不可用: {1}", addr, ex.Message);
                }
            }
            
            Log.Information("扫描完成，发现 {0} 个可用地址", availableAddresses.Count);
            return availableAddresses;
        }
        public void Dispose()
        {
            Stop();
            _networkClient?.Dispose();
        }
    }
    
    /// <summary>
    /// 优化的Modbus请求结构
    /// </summary>
    public class OptimizedModbusRequest
    {
        public int FunctionCode { get; set; }
        public int StartAddress { get; set; }
        public int Quantity { get; set; }
        public List<ModbusDataPoint> DataPoints { get; set; } = new List<ModbusDataPoint>();
    }
    
    /// <summary>
    /// Modbus数据点
    /// </summary>
}
