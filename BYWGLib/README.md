# BYWGLib - 高性能工业协议库

这是一个专为工业协议边缘网关设计的高性能C#库，支持多种常见的工业协议，采用零依赖设计，性能媲美C++/C。

## 🚀 性能特性

- **零依赖**: 不依赖任何第三方库，完全自实现
- **高性能**: 使用unsafe代码、内存池、零拷贝技术
- **低延迟**: 优化的网络通信和数据处理
- **内存优化**: 对象池和内存池减少GC压力
- **批量处理**: 智能合并连续地址的读取请求

## 📋 支持的协议

- **Modbus TCP** - 高性能TCP实现
- **Modbus RTU** - 优化的串口通信
- **西门子S7** - 零拷贝S7协议实现
- **三菱MC** - 高性能MC协议实现

## 🛠️ 核心组件

### 高性能日志系统
- 无锁队列设计
- 异步写入
- 零分配字符串格式化

### 内存管理
- 字节数组内存池
- 对象池重用
- 高性能CRC计算

### 协议优化
- 批量数据读取
- 连续地址合并
- 智能请求优化

## 📖 使用方法

### 1. 使用协议工厂创建协议

```csharp
// 创建Modbus TCP协议
var modbusTcp = HighPerformanceProtocolFactory.CreateModbusTCP(
    "Device1", "192.168.1.100", 502, 1, 3000);

// 创建Modbus RTU协议
var modbusRtu = HighPerformanceProtocolFactory.CreateModbusRTU(
    "Device2", "COM1", 9600, 1, 3000);

// 创建S7协议
var s7 = HighPerformanceProtocolFactory.CreateS7(
    "Device3", "192.168.1.101", 0, 2, 102, 3000);

// 创建三菱MC协议
var mc = HighPerformanceProtocolFactory.CreateMitsubishiMC(
    "Device4", "192.168.1.102", 5007, 0, 255, 3000);
```

### 2. 使用协议管理器

```csharp
var manager = new ProtocolManager();
manager.Initialize();

// 添加协议配置
var config = new IndustrialProtocolConfig
{
    Name = "ModbusTCP_Device1",
    Type = "MODBUS_TCP",
    Enabled = true,
    Parameters = new Dictionary<string, string>
    {
        ["IpAddress"] = "192.168.1.100",
        ["Port"] = "502",
        ["SlaveId"] = "1",
        ["Timeout"] = "3000",
        ["DataPoints"] = "Temperature,D100,float,3;Pressure,D102,float,3;Status,M0,bool,1"
    }
};

manager.AddProtocol(config);

// 订阅数据变化事件
manager.DataChanged += (sender, e) =>
{
    foreach (var item in e.ChangedItems)
    {
        Console.WriteLine($"{item.Name}: {item.Value} ({item.DataType})");
    }
};
```

### 3. 直接读取数据

```csharp
// 启动协议
modbusTcp.Start();

// 读取数据
var temperature = modbusTcp.Read("100", "float");
var pressure = modbusTcp.Read("102", "float");
var status = modbusTcp.Read("0", "bool");

// 写入数据
bool success = modbusTcp.Write("100", "float", 25.5f);

// 停止协议
modbusTcp.Stop();
```

## ⚙️ 配置说明

### Modbus TCP
```csharp
Parameters = {
    ["IpAddress"] = "192.168.1.100",    // IP地址
    ["Port"] = "502",                    // 端口号
    ["SlaveId"] = "1",                   // 从站ID
    ["Timeout"] = "3000",                // 超时时间(毫秒)
    ["DataPoints"] = "Name,Address,Type,FunctionCode;..." // 数据点配置
}
```

### Modbus RTU
```csharp
Parameters = {
    ["PortName"] = "COM1",               // 串口名称
    ["BaudRate"] = "9600",               // 波特率
    ["DataBits"] = "8",                  // 数据位
    ["Parity"] = "None",                 // 校验位
    ["StopBits"] = "One",                // 停止位
    ["SlaveId"] = "1",                   // 从站ID
    ["Timeout"] = "3000"                 // 超时时间(毫秒)
}
```

### 西门子S7
```csharp
Parameters = {
    ["IpAddress"] = "192.168.1.101",     // IP地址
    ["Port"] = "102",                    // 端口号
    ["Rack"] = "0",                      // 机架号
    ["Slot"] = "2",                      // 插槽号
    ["Timeout"] = "3000",                // 超时时间(毫秒)
    ["PduSize"] = "1024"                 // PDU大小
}
```

### 三菱MC
```csharp
Parameters = {
    ["IpAddress"] = "192.168.1.102",     // IP地址
    ["Port"] = "5007",                   // 端口号
    ["NetworkNo"] = "0",                 // 网络号
    ["PcNo"] = "255",                    // PC号
    ["Timeout"] = "3000"                 // 超时时间(毫秒)
}
```

## 🔧 数据类型支持

- **bool/bit**: 布尔值/位
- **byte**: 字节
- **int16/uint16**: 16位整数
- **int32/uint32**: 32位整数
- **float**: 单精度浮点数
- **double**: 双精度浮点数

## 📊 性能监控

```csharp
// 获取数据传输统计
long totalBytes = manager.GetTotalBytesTransferred();
double dataRate = manager.GetCurrentDataRate(); // 字节/秒

Console.WriteLine($"总传输: {totalBytes} 字节");
Console.WriteLine($"当前速率: {dataRate:F2} 字节/秒");
```

## 🏗️ 项目结构

```
BYWGLib/
├── Logging/                    # 高性能日志系统
│   ├── ILogger.cs
│   └── HighPerformanceLogger.cs
├── Utils/                      # 高性能工具类
│   ├── MemoryPool.cs
│   └── CRCUtils.cs
├── Protocols/                  # 高性能协议实现
│   ├── HighPerformanceModbusTCPProtocol.cs
│   ├── HighPerformanceModbusRTUProtocol.cs
│   ├── HighPerformanceS7Protocol.cs
│   ├── HighPerformanceMitsubishiMCProtocol.cs
│   └── HighPerformanceProtocolFactory.cs
├── Examples/                   # 使用示例
│   └── HighPerformanceProtocolExample.cs
└── ProtocolManager.cs          # 协议管理器
```

## 🎯 设计目标

- **零依赖**: 完全自实现，不依赖第三方库
- **高性能**: 性能媲美C++/C实现
- **低延迟**: 优化的网络和串口通信
- **内存效率**: 最小化GC压力
- **易用性**: 简洁的API设计

## 📄 许可证

MIT License