# BYWGLib 使用说明报告

## 概述

BYWGLib是一个专为工业协议边缘网关设计的高性能C#库，支持多种常见的工业协议，采用零依赖设计，性能媲美C++/C。

**版本**: v1.0.0.0  
**目标框架**: .NET 8.0  
**许可证**: MIT License  

## 核心特性

### 🚀 性能特性
- **零依赖**: 不依赖任何第三方库，完全自实现
- **高性能**: 使用unsafe代码、内存池、零拷贝技术
- **低延迟**: 优化的网络通信和数据处理
- **内存优化**: 对象池和内存池减少GC压力
- **批量处理**: 智能合并连续地址的读取请求

### 📋 支持的协议
- **Modbus TCP** - 高性能TCP实现
- **Modbus RTU** - 优化的串口通信
- **西门子S7** - 零拷贝S7协议实现
- **三菱MC** - 高性能MC协议实现

## 快速开始

### 1. 安装和引用

```csharp
// 在项目中引用BYWGLib
<ProjectReference Include="BYWGLib\BYWGLib.csproj" />
```

### 2. 基本使用

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

// S7协议
var s7Config = new IndustrialProtocolConfig
{
    Name = "S7_Device1",
    Type = "S7",
    Parameters = new Dictionary<string, string>
    {
        { "Host", "192.168.1.101" },
        { "Port", "102" },
        { "Rack", "0" },
        { "Slot", "2" }
    }
};
var s7 = new S7Protocol(s7Config);
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

## 详细使用指南

### 协议管理器

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

### 数据点管理

```csharp
// 创建数据点
var dataPoint = new ModbusDataPoint("Temperature", "D4500", 3, "signed");

// 批量读取
var dataPoints = new List<ModbusDataPoint>
{
    new ModbusDataPoint("Temp1", "D4500", 3, "signed"),
    new ModbusDataPoint("Temp2", "D4501", 3, "signed"),
    new ModbusDataPoint("Temp3", "D4502", 3, "signed")
};

var results = await protocol.ReadBatchAsync(dataPoints);
```

### 事件处理

```csharp
// 订阅数据接收事件
protocol.DataReceived += (sender, e) =>
{
    foreach (var item in e.DataItems)
    {
        Console.WriteLine($"地址: {item.Id}, 值: {item.Value}, 质量: {item.Quality}");
    }
};
```

## 配置参数

### Modbus TCP配置

| 参数 | 说明 | 默认值 |
|------|------|--------|
| Host | 设备IP地址 | localhost |
| Port | 端口号 | 502 |
| Timeout | 超时时间(ms) | 5000 |
| UnitId | 单元ID | 1 |
| MaxConnections | 最大连接数 | 100 |

### S7协议配置

| 参数 | 说明 | 默认值 |
|------|------|--------|
| Host | 设备IP地址 | localhost |
| Port | 端口号 | 102 |
| Rack | 机架号 | 0 |
| Slot | 插槽号 | 2 |
| Timeout | 超时时间(ms) | 5000 |

### 三菱MC协议配置

| 参数 | 说明 | 默认值 |
|------|------|--------|
| Host | 设备IP地址 | localhost |
| Port | 端口号 | 5007 |
| NetworkNo | 网络号 | 0 |
| PCNo | PC号 | 255 |
| Timeout | 超时时间(ms) | 5000 |

## 地址格式

### Modbus地址格式

```csharp
// D格式地址（推荐）
"D4500"  // 数据寄存器D4500

// 标准Modbus地址
"40001"  // Holding Register 40001
"30001"  // Input Register 30001
"10001"  // Discrete Input 10001
"1"      // Coil 1
```

### S7地址格式

```csharp
"DB1.DBD0"    // 数据块1，双字0
"DB1.DBW0"    // 数据块1，字0
"DB1.DBB0"    // 数据块1，字节0
"M0.0"        // 标志位M0.0
"I0.0"        // 输入I0.0
"Q0.0"        // 输出Q0.0
```

### 三菱MC地址格式

```csharp
"D4500"       // 数据寄存器D4500
"M100"        // 内部继电器M100
"X0"          // 输入X0
"Y0"          // 输出Y0
```

## 数据类型

### 支持的数据类型

| 类型 | 说明 | 字节数 |
|------|------|--------|
| bool/coil | 布尔值 | 1 |
| uint16/unsigned | 无符号16位整数 | 2 |
| int16/signed | 有符号16位整数 | 2 |
| uint32 | 无符号32位整数 | 4 |
| int32 | 有符号32位整数 | 4 |
| float | 32位浮点数 | 4 |
| uint64 | 无符号64位整数 | 8 |
| int64 | 有符号64位整数 | 8 |
| double | 64位浮点数 | 8 |

## 性能优化

### 连接池管理

```csharp
// 配置连接池大小
var config = new IndustrialProtocolConfig
{
    // ... 其他配置
    Parameters = new Dictionary<string, string>
    {
        { "MaxConnections", "50" }  // 最大连接数
    }
};
```

### 批量操作

```csharp
// 批量读取连续地址
var dataPoints = new List<ModbusDataPoint>();
for (int i = 0; i < 100; i++)
{
    dataPoints.Add(new ModbusDataPoint($"D{i + 4500}", $"D{i + 4500}", 3, "signed"));
}

var results = await protocol.ReadBatchAsync(dataPoints);
```

### 异步操作

```csharp
// 使用异步方法提高性能
var tasks = new List<Task<object>>();
for (int i = 0; i < 10; i++)
{
    tasks.Add(protocol.ReadAsync($"D{i + 4500}", "signed"));
}

var results = await Task.WhenAll(tasks);
```

## 错误处理

### 异常类型

```csharp
try
{
    var value = await protocol.ReadAsync("D4500", "signed");
}
catch (ModbusException ex)
{
    Console.WriteLine($"Modbus错误: {ex.Message}, 错误码: {ex.ErrorCode}");
}
catch (TimeoutException ex)
{
    Console.WriteLine($"超时错误: {ex.Message}");
}
catch (Exception ex)
{
    Console.WriteLine($"其他错误: {ex.Message}");
}
```

### 重试机制

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

## 日志配置

### 启用日志

```csharp
// 日志会自动输出到控制台
// 可以通过配置文件调整日志级别
```

### 日志级别

- **Debug**: 详细的调试信息
- **Information**: 一般信息
- **Warning**: 警告信息
- **Error**: 错误信息
- **Fatal**: 严重错误

## 最佳实践

### 1. 资源管理

```csharp
// 使用using语句确保资源释放
using (var protocol = new ModbusTcpProtocol(config))
{
    protocol.Start();
    // 使用协议
}
```

### 2. 连接管理

```csharp
// 避免频繁创建和销毁连接
// 使用连接池复用连接
```

### 3. 错误处理

```csharp
// 实现适当的错误处理和重试机制
// 监控连接状态
```

### 4. 性能优化

```csharp
// 使用批量操作减少网络请求
// 合理设置超时时间
// 使用异步方法避免阻塞
```

## 示例代码

### 完整示例

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

## 故障排除

### 常见问题

1. **连接失败**
   - 检查设备IP地址和端口
   - 确认网络连通性
   - 检查防火墙设置

2. **读取失败**
   - 检查地址格式是否正确
   - 确认设备支持该地址
   - 检查数据类型是否匹配

3. **性能问题**
   - 调整连接池大小
   - 使用批量操作
   - 优化网络配置

### 调试技巧

1. **启用详细日志**
2. **使用网络抓包工具**
3. **检查设备端配置**
4. **验证协议实现**

## 总结

BYWGLib是一个功能强大、性能优秀的工业协议通信库，支持多种主流工业协议，具有零依赖、高性能、低延迟的特点。通过合理使用其API和配置参数，可以满足各种工业通信需求。

---

**文档版本**: 1.0  
**最后更新**: 2025-09-27  
**维护者**: BYWG开发团队
