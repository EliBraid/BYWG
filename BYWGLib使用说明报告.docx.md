# BYWGLib 使用说明报告

**项目名称**: BYWGLib 高性能工业协议通信库  
**版本**: v1.0.0.0  
**目标框架**: .NET 8.0  
**许可证**: MIT License  
**文档日期**: 2025年9月27日  

---

## 1. 概述

### 1.1 产品介绍

BYWGLib是一个专为Modbus协议通信设计的高性能C#库，支持Modbus TCP和RTU协议，采用零依赖设计，性能媲美C++/C。

### 1.2 核心特性

#### 🚀 性能特性
- **零依赖**: 不依赖任何第三方库，完全自实现
- **高性能**: 使用unsafe代码、内存池、零拷贝技术
- **低延迟**: 优化的网络通信和数据处理
- **内存优化**: 对象池和内存池减少GC压力
- **批量处理**: 智能合并连续地址的读取请求

#### 📋 支持的协议
- **Modbus TCP** - 高性能TCP实现，支持以太网通信
- **Modbus RTU** - 优化的串口通信，支持RS485/RS232

### 1.3 技术规格

| 项目 | 规格 |
|------|------|
| 目标框架 | .NET 8.0 |
| 语言版本 | C# 12.0 |
| 支持平台 | Windows, Linux, macOS |
| 内存使用 | 优化的内存管理 |
| 并发支持 | 高并发异步操作 |
| 连接池 | 智能连接池管理 |
| 协议支持 | Modbus TCP, Modbus RTU |

---

## 2. 快速开始

### 2.1 安装和引用

#### 项目引用
```xml
<ProjectReference Include="BYWGLib\BYWGLib.csproj" />
```

#### 命名空间引用
```csharp
using BYWGLib;
using BYWGLib.Protocols;
using BYWGLib.Logging;
```

### 2.2 基本使用示例

#### 创建协议配置
```csharp
var config = new IndustrialProtocolConfig
{
    Name = "ModbusTCP_Device1",
    Type = "ModbusTCP",
    Parameters = new Dictionary<string, string>
    {
        { "Host", "192.168.1.100" },
        { "Port", "502" },
        { "Timeout", "5000" },
        { "UnitId", "1" }
    }
};
```

#### 创建协议实例
```csharp
// Modbus TCP协议
var modbusTcp = new ModbusTcpProtocol(config);

// Modbus RTU协议
var rtuConfig = new IndustrialProtocolConfig
{
    Name = "ModbusRTU_Device1",
    Type = "ModbusRTU",
    Parameters = new Dictionary<string, string>
    {
        { "Port", "COM1" },
        { "BaudRate", "9600" },
        { "DataBits", "8" },
        { "StopBits", "1" },
        { "Parity", "None" },
        { "UnitId", "1" }
    }
};
var modbusRtu = new ModbusRtuProtocol(rtuConfig);
```

#### 基本操作
```csharp
// 启动协议
protocol.Start();

// 读取数据
var value = await protocol.ReadAsync("D4500", "signed");

// 写入数据
await protocol.WriteAsync("D4500", "signed", 123);

// 停止协议
protocol.Stop();
```

---

## 3. 详细使用指南

### 3.1 协议管理器

#### 基本使用
```csharp
var manager = new ProtocolManager();
manager.Initialize();

// 添加协议
manager.AddProtocol(config);

// 启动所有协议
manager.StartAllProtocols();

// 开始轮询
manager.StartPolling();
```

#### 事件处理
```csharp
// 订阅数据变化事件
manager.DataChanged += (sender, e) =>
{
    foreach (var item in e.DataItems)
    {
        Console.WriteLine($"数据变化: {item.Id} = {item.Value}");
    }
};
```

### 3.2 数据点管理

#### 创建数据点
```csharp
// 创建单个数据点
var dataPoint = new ModbusDataPoint("Temperature", "D4500", 3, "signed");

// 创建批量数据点
var dataPoints = new List<ModbusDataPoint>
{
    new ModbusDataPoint("Temp1", "D4500", 3, "signed"),
    new ModbusDataPoint("Temp2", "D4501", 3, "signed"),
    new ModbusDataPoint("Temp3", "D4502", 3, "signed")
};
```

#### 批量读取
```csharp
var results = await protocol.ReadBatchAsync(dataPoints);
foreach (var result in results)
{
    Console.WriteLine($"{result.Address} = {result.Value}");
}
```

### 3.3 事件处理

#### 数据接收事件
```csharp
// 订阅数据接收事件
protocol.DataReceived += (sender, e) =>
{
    Console.WriteLine($"接收到数据: {e.ProtocolName}");
    foreach (var item in e.DataItems)
    {
        Console.WriteLine($"  {item.Id} = {item.Value}");
    }
};
```

---

## 4. 配置参数

### 4.1 Modbus TCP配置

| 参数 | 说明 | 默认值 | 示例 |
|------|------|--------|------|
| Host | 设备IP地址 | localhost | 192.168.1.100 |
| Port | 端口号 | 502 | 502 |
| Timeout | 超时时间(ms) | 5000 | 3000 |
| UnitId | 单元ID | 1 | 1 |
| MaxConnections | 最大连接数 | 100 | 50 |

#### 配置示例
```csharp
var config = new IndustrialProtocolConfig
{
    Name = "ModbusTCP_Device",
    Type = "ModbusTCP",
    Parameters = new Dictionary<string, string>
    {
        { "Host", "192.168.1.100" },
        { "Port", "502" },
        { "Timeout", "5000" },
        { "UnitId", "1" },
        { "MaxConnections", "50" }
    }
};
```

### 4.2 Modbus RTU配置

| 参数 | 说明 | 默认值 | 示例 |
|------|------|--------|------|
| Port | 串口名称 | COM1 | COM1 |
| BaudRate | 波特率 | 9600 | 9600 |
| DataBits | 数据位 | 8 | 8 |
| StopBits | 停止位 | 1 | 1 |
| Parity | 校验位 | None | None |
| UnitId | 单元ID | 1 | 1 |
| Timeout | 超时时间(ms) | 5000 | 3000 |

#### 配置示例
```csharp
var rtuConfig = new IndustrialProtocolConfig
{
    Name = "ModbusRTU_Device",
    Type = "ModbusRTU",
    Parameters = new Dictionary<string, string>
    {
        { "Port", "COM1" },
        { "BaudRate", "9600" },
        { "DataBits", "8" },
        { "StopBits", "1" },
        { "Parity", "None" },
        { "UnitId", "1" },
        { "Timeout", "5000" }
    }
};
```

---

## 5. 地址格式

### 5.1 Modbus地址格式

#### D格式地址（推荐）
```csharp
"D4500"  // 数据寄存器D4500
"D100"   // 数据寄存器D100
"D1"     // 数据寄存器D1
```

#### 标准Modbus地址
```csharp
"40001"  // Holding Register 40001
"30001"  // Input Register 30001
"10001"  // Discrete Input 10001
"1"      // Coil 1
```

### 5.2 Modbus RTU地址格式

```csharp
"D4500"       // 数据寄存器D4500
"M100"        // 内部继电器M100
"X0"          // 输入X0
"Y0"          // 输出Y0
"1"           // 线圈1
"10001"       // 离散输入10001
```

---

## 6. 数据类型

### 6.1 支持的数据类型

| 类型 | 说明 | 字节数 | 示例 |
|------|------|--------|------|
| bool/coil | 布尔值 | 1 | true/false |
| uint16/unsigned | 无符号16位整数 | 2 | 0-65535 |
| int16/signed | 有符号16位整数 | 2 | -32768-32767 |
| uint32 | 无符号32位整数 | 4 | 0-4294967295 |
| int32 | 有符号32位整数 | 4 | -2147483648-2147483647 |
| float | 32位浮点数 | 4 | 3.14f |
| uint64 | 无符号64位整数 | 8 | 0-18446744073709551615 |
| int64 | 有符号64位整数 | 8 | -9223372036854775808-9223372036854775807 |
| double | 64位浮点数 | 8 | 3.141592653589793 |

### 6.2 数据类型使用示例

```csharp
// 读取不同数据类型
var boolValue = await protocol.ReadAsync("D4500", "bool");
var intValue = await protocol.ReadAsync("D4501", "signed");
var floatValue = await protocol.ReadAsync("D4502", "float");

// 写入不同数据类型
await protocol.WriteAsync("D4500", "bool", true);
await protocol.WriteAsync("D4501", "signed", 123);
await protocol.WriteAsync("D4502", "float", 3.14f);
```

---

## 7. 性能优化

### 7.1 连接池管理

#### 配置连接池大小
```csharp
var config = new IndustrialProtocolConfig
{
    // ... 其他配置
    Parameters = new Dictionary<string, string>
    {
        { "MaxConnections", "50" }  // 最大连接数
    }
};
```

#### 连接池监控
```csharp
// 获取连接池状态
var status = protocol.GetConnectionStatus();
Console.WriteLine($"活跃连接数: {status.ActiveConnections}");
Console.WriteLine($"可用连接数: {status.AvailableConnections}");
```

### 7.2 批量操作

#### 批量读取连续地址
```csharp
var dataPoints = new List<ModbusDataPoint>();
for (int i = 0; i < 100; i++)
{
    dataPoints.Add(new ModbusDataPoint($"D{i + 4500}", $"D{i + 4500}", 3, "signed"));
}

var results = await protocol.ReadBatchAsync(dataPoints);
```

#### 批量写入
```csharp
var writeData = new List<(string address, object value)>
{
    ("D4500", 100),
    ("D4501", 200),
    ("D4502", 300)
};

foreach (var (address, value) in writeData)
{
    await protocol.WriteAsync(address, "signed", value);
}
```

### 7.3 异步操作

#### 并发读取
```csharp
var tasks = new List<Task<object>>();
for (int i = 0; i < 10; i++)
{
    tasks.Add(protocol.ReadAsync($"D{i + 4500}", "signed"));
}

var results = await Task.WhenAll(tasks);
```

#### 异步批量操作
```csharp
var batchTasks = new List<Task<List<ModbusDataPoint>>>();
for (int i = 0; i < 5; i++)
{
    var batch = CreateBatch(i * 20, 20);
    batchTasks.Add(protocol.ReadBatchAsync(batch));
}

var batchResults = await Task.WhenAll(batchTasks);
```

---

## 8. 错误处理

### 8.1 异常类型

#### ModbusException
```csharp
try
{
    var value = await protocol.ReadAsync("D4500", "signed");
}
catch (ModbusException ex)
{
    Console.WriteLine($"Modbus错误: {ex.Message}");
    Console.WriteLine($"错误码: {ex.ErrorCode}");
}
```

#### TimeoutException
```csharp
try
{
    var value = await protocol.ReadAsync("D4500", "signed");
}
catch (TimeoutException ex)
{
    Console.WriteLine($"超时错误: {ex.Message}");
}
```

#### 通用异常处理
```csharp
try
{
    var value = await protocol.ReadAsync("D4500", "signed");
}
catch (Exception ex)
{
    Console.WriteLine($"其他错误: {ex.Message}");
}
```

### 8.2 重试机制

#### 简单重试
```csharp
public async Task<object> ReadWithRetry(string address, string dataType, int maxRetries = 3)
{
    for (int i = 0; i < maxRetries; i++)
    {
        try
        {
            return await protocol.ReadAsync(address, dataType);
        }
        catch (Exception ex) when (i < maxRetries - 1)
        {
            await Task.Delay(1000); // 等待1秒后重试
        }
    }
    throw new Exception($"读取失败，已重试{maxRetries}次");
}
```

#### 指数退避重试
```csharp
public async Task<object> ReadWithExponentialBackoff(string address, string dataType, int maxRetries = 3)
{
    for (int i = 0; i < maxRetries; i++)
    {
        try
        {
            return await protocol.ReadAsync(address, dataType);
        }
        catch (Exception ex) when (i < maxRetries - 1)
        {
            var delay = (int)Math.Pow(2, i) * 1000; // 指数退避
            await Task.Delay(delay);
        }
    }
    throw new Exception($"读取失败，已重试{maxRetries}次");
}
```

---

## 9. 日志配置

### 9.1 启用日志

#### 基本日志
```csharp
// 日志会自动输出到控制台
// 可以通过配置文件调整日志级别
```

#### 自定义日志
```csharp
// 配置日志级别
Log.SetLevel(LogLevel.Debug);

// 自定义日志输出
Log.Information("应用程序启动");
Log.Warning("警告信息");
Log.Error("错误信息");
```

### 9.2 日志级别

| 级别 | 说明 | 使用场景 |
|------|------|----------|
| Debug | 详细的调试信息 | 开发和调试 |
| Information | 一般信息 | 正常运行 |
| Warning | 警告信息 | 潜在问题 |
| Error | 错误信息 | 错误处理 |
| Fatal | 严重错误 | 系统故障 |

---

## 10. 最佳实践

### 10.1 资源管理

#### 使用using语句
```csharp
// 使用using语句确保资源释放
using (var protocol = new ModbusTcpProtocol(config))
{
    protocol.Start();
    // 使用协议
}
```

#### 手动资源管理
```csharp
var protocol = new ModbusTcpProtocol(config);
try
{
    protocol.Start();
    // 使用协议
}
finally
{
    protocol.Dispose();
}
```

### 10.2 连接管理

#### 连接池配置
```csharp
// 根据实际需求配置连接池大小
var config = new IndustrialProtocolConfig
{
    Parameters = new Dictionary<string, string>
    {
        { "MaxConnections", "20" }  // 适中的连接池大小
    }
};
```

#### 连接健康检查
```csharp
// 定期检查连接状态
var timer = new Timer(async _ =>
{
    if (!protocol.IsRunning)
    {
        await protocol.StartAsync();
    }
}, null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
```

### 10.3 错误处理

#### 分层错误处理
```csharp
try
{
    var value = await protocol.ReadAsync("D4500", "signed");
    // 处理成功结果
}
catch (ModbusException ex)
{
    // 处理Modbus特定错误
    Log.Error($"Modbus错误: {ex.Message}");
}
catch (TimeoutException ex)
{
    // 处理超时错误
    Log.Warning($"读取超时: {ex.Message}");
}
catch (Exception ex)
{
    // 处理其他错误
    Log.Error($"未知错误: {ex.Message}");
}
```

### 10.4 性能优化

#### 批量操作优化
```csharp
// 使用批量操作减少网络请求
var dataPoints = CreateDataPoints(100);
var results = await protocol.ReadBatchAsync(dataPoints);
```

#### 异步操作优化
```csharp
// 使用异步方法避免阻塞
public async Task ProcessDataAsync()
{
    var tasks = dataPoints.Select(dp => protocol.ReadAsync(dp.Address, dp.DataType));
    var results = await Task.WhenAll(tasks);
    // 处理结果
}
```

---

## 11. 示例代码

### 11.1 完整示例

```csharp
using BYWGLib;
using BYWGLib.Protocols;

class Program
{
    static async Task Main(string[] args)
    {
        // 创建配置
        var config = new IndustrialProtocolConfig
        {
            Name = "TestDevice",
            Type = "ModbusTCP",
            Parameters = new Dictionary<string, string>
            {
                { "Host", "192.168.6.6" },
                { "Port", "502" },
                { "Timeout", "5000" }
            }
        };

        // 创建协议实例
        using var protocol = new ModbusTcpProtocol(config);
        
        try
        {
            // 启动协议
            protocol.Start();
            
            // 订阅事件
            protocol.DataReceived += OnDataReceived;
            
            // 读取数据
            var value = await protocol.ReadAsync("D4500", "signed");
            Console.WriteLine($"D4500 = {value}");
            
            // 写入数据
            await protocol.WriteAsync("D4500", "signed", 123);
            
            // 批量读取
            var dataPoints = new List<ModbusDataPoint>
            {
                new ModbusDataPoint("D4500", "D4500", 3, "signed"),
                new ModbusDataPoint("D4501", "D4501", 3, "signed")
            };
            
            var results = await protocol.ReadBatchAsync(dataPoints);
            foreach (var result in results)
            {
                Console.WriteLine($"{result.Address} = {result.Value}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"错误: {ex.Message}");
        }
        finally
        {
            protocol.Stop();
        }
    }
    
    static void OnDataReceived(object sender, DataReceivedEventArgs e)
    {
        Console.WriteLine($"接收到数据: {e.ProtocolName}");
        foreach (var item in e.DataItems)
        {
            Console.WriteLine($"  {item.Id} = {item.Value}");
        }
    }
}
```

### 11.2 高级示例

```csharp
public class IndustrialDataCollector
{
    private readonly ModbusTcpProtocol _protocol;
    private readonly Timer _pollingTimer;
    
    public IndustrialDataCollector(IndustrialProtocolConfig config)
    {
        _protocol = new ModbusTcpProtocol(config);
        _pollingTimer = new Timer(PollData, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
    }
    
    private async void PollData(object state)
    {
        try
        {
            var dataPoints = GetDataPoints();
            var results = await _protocol.ReadBatchAsync(dataPoints);
            
            foreach (var result in results)
            {
                if (result.Quality)
                {
                    ProcessData(result);
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "数据轮询错误");
        }
    }
    
    private void ProcessData(ModbusDataPoint dataPoint)
    {
        // 处理数据
        Console.WriteLine($"{dataPoint.Address}: {dataPoint.Value}");
    }
    
    public void Dispose()
    {
        _pollingTimer?.Dispose();
        _protocol?.Dispose();
    }
}
```

---

## 12. 故障排除

### 12.1 常见问题

#### 连接失败
**问题**: 无法连接到设备
**解决方案**:
1. 检查设备IP地址和端口
2. 确认网络连通性
3. 检查防火墙设置
4. 验证设备是否支持Modbus TCP

#### 读取失败
**问题**: 读取数据失败
**解决方案**:
1. 检查地址格式是否正确
2. 确认设备支持该地址
3. 检查数据类型是否匹配
4. 验证设备配置

#### 性能问题
**问题**: 性能不理想
**解决方案**:
1. 调整连接池大小
2. 使用批量操作
3. 优化网络配置
4. 检查设备性能

### 12.2 调试技巧

#### 启用详细日志
```csharp
Log.SetLevel(LogLevel.Debug);
```

#### 使用网络抓包工具
- Wireshark
- Fiddler
- 网络监控工具

#### 检查设备端配置
1. 验证Modbus配置
2. 检查地址映射
3. 确认协议版本

---

## 13. 总结

### 13.1 主要优势

1. **高性能**: 响应时间极低，支持高并发
2. **零依赖**: 部署简单，无第三方依赖
3. **易用性**: API设计简洁，易于使用
4. **可扩展性**: 支持Modbus TCP/RTU协议
5. **稳定性**: 完善的错误处理和资源管理

### 13.2 适用场景

- Modbus TCP/RTU通信系统
- 工业自动化数据采集
- 边缘网关Modbus通信
- 实时Modbus数据监控
- 工业设备Modbus通信

### 13.3 推荐使用

BYWGLib库适合用于对性能要求较高的Modbus通信场景，建议在充分测试后部署到生产环境。

---

**文档版本**: 1.0  
**最后更新**: 2025年9月27日  
**维护者**: BYWG开发团队

---

*本文档基于BYWGLib v1.0.0.0编写，如有疑问请联系开发团队。*
