using BYWGLib;
using BYWGLib.Protocols;
using BYWG.Gateway.Data;
using BYWG.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Text.Json;

namespace BYWG.Gateway.Services;

/// <summary>
/// 数据采集服务
/// </summary>
public class DataCollectionService : IHostedService, IDisposable
{
    private readonly ProtocolManager _protocolManager;
    private readonly IDbContextFactory<GatewayDbContext> _dbFactory;
    private readonly ILogger<DataCollectionService> _logger;
    private readonly Timer _collectionTimer;
    private readonly Timer _connectionMonitorTimer;
    private readonly ConcurrentDictionary<string, object> _latestData = new();
    // 维护当前已注册的协议集合，避免通过 _latestData 推断导致误清理
    private readonly ConcurrentDictionary<string, byte> _activeProtocols = new();
    private readonly CommunicationService _communicationService;
    private readonly Dictionary<int, DateTime> _lastDataReceived = new();
    private static readonly HttpClient _sharedHttpClient = new HttpClient();
    private readonly string _adminApiUrl;
    private int _isCollecting = 0;
    private int _isMonitoring = 0;

    public DataCollectionService(
        ProtocolManager protocolManager,
        IDbContextFactory<GatewayDbContext> dbFactory,
        ILogger<DataCollectionService> logger,
        CommunicationService communicationService,
        IConfiguration configuration)
    {
        _protocolManager = protocolManager;
        _dbFactory = dbFactory;
        _logger = logger;
        _communicationService = communicationService;
        _adminApiUrl = configuration["ADMIN_API_URL"]
            ?? Environment.GetEnvironmentVariable("ADMIN_API_URL")
            ?? "http://localhost:5000";
        _collectionTimer = new Timer(CollectData, null, Timeout.Infinite, Timeout.Infinite);
        _connectionMonitorTimer = new Timer(MonitorConnections, null, Timeout.Infinite, Timeout.Infinite);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("数据采集服务启动");

        // 从数据库加载协议配置
        await LoadProtocolConfigurations();

        // 仅当存在启用的协议配置时才启动协议与轮询
        using (var ctx = await _dbFactory.CreateDbContextAsync())
        {
            bool hasEnabled = await ctx.ProtocolConfigs.AnyAsync(p => p.IsEnabled);
            if (hasEnabled)
            {
                _protocolManager.StartAllProtocols();
                _protocolManager.StartPolling();
            }
            else
            {
                _logger.LogInformation("未发现启用的协议配置，暂不启动轮询");
            }
        }

        // 订阅数据变化事件
        _protocolManager.DataChanged += OnDataChanged;
        _logger.LogInformation("已订阅ProtocolManager的DataChanged事件");

        // 启动数据采集定时器（每5秒采集一次）
        _collectionTimer.Change(TimeSpan.Zero, TimeSpan.FromSeconds(5));
        
        // 启动连接监控定时器（每30秒检查一次连接状态）
        _connectionMonitorTimer.Change(TimeSpan.Zero, TimeSpan.FromSeconds(30));

        _logger.LogInformation("数据采集服务启动完成");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("数据采集服务停止");

        _collectionTimer.Change(Timeout.Infinite, Timeout.Infinite);
        _connectionMonitorTimer.Change(Timeout.Infinite, Timeout.Infinite);
        _protocolManager.StopAllProtocols();
        _protocolManager.StopPolling();
        _protocolManager.DataChanged -= OnDataChanged;

        _logger.LogInformation("数据采集服务停止完成");
        return Task.CompletedTask;
    }

    private async Task LoadProtocolConfigurations()
    {
        try
        {
            using var context = await _dbFactory.CreateDbContextAsync();
            var configs = await context.ProtocolConfigs
                .Where(p => p.IsEnabled)
                .ToListAsync();

            foreach (var config in configs)
            {
                var protocolConfig = new IndustrialProtocolConfig
                {
                    Name = config.Name,
                    Type = config.ProtocolType,
                    Parameters = config.Parameters,
                    Enabled = config.IsEnabled
                };

                _protocolManager.AddProtocol(protocolConfig);
                _activeProtocols[config.Name] = 1;
                _logger.LogInformation("加载协议配置: {ProtocolName}", config.Name);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "加载协议配置失败");
        }
    }

    private async void CollectData(object? state)
    {
        if (Interlocked.Exchange(ref _isCollecting, 1) == 1)
        {
            // 上一次执行尚未完成，跳过本次触发
            return;
        }
        try
        {
            // 从CommunicationService获取缓存的设备数据
            var devices = _communicationService.GetCachedDevices();
            
            if (devices.Count == 0)
            {
                _logger.LogDebug("没有启用的设备，尝试立即同步设备数据");
                // 如果没有设备，尝试立即同步设备数据
                await _communicationService.SyncDevicesImmediately();
                devices = _communicationService.GetCachedDevices();
                
                if (devices.Count == 0)
                {
                    _logger.LogDebug("同步后仍然没有启用的设备，跳过数据采集");
                    return;
                }
            }

            _logger.LogDebug("开始数据采集，设备数量: {Count}", devices.Count);

            foreach (var device in devices)
            {
                try
                {
                    var protocolName = GetProtocolName(device);
                    _logger.LogDebug("处理设备: {DeviceName}, 协议: {ProtocolName}", device.Name, protocolName);
                    
                    // 确保协议存在且配置正确
                    if (!_protocolManager.TryGetProtocol(protocolName, out var protocol))
                    {
                        _logger.LogDebug("协议 {ProtocolName} 不存在，尝试创建", protocolName);
                        await CreateProtocolForDevice(device);
                        
                        // 重新尝试获取协议
                        if (!_protocolManager.TryGetProtocol(protocolName, out protocol))
                        {
                            _logger.LogWarning("无法创建协议 {ProtocolName}，跳过设备 {DeviceName}", protocolName, device.Name);
                            await UpdateSingleDeviceStatusInAdmin(device.Id, false);
                            continue;
                        }
                    }

                    // 触发协议轮询
                    try
                    {
                        protocol.PollData();
                        _logger.LogDebug("触发协议轮询: {ProtocolName}", protocolName);
                        
                        // 更新设备状态为在线
                        await UpdateSingleDeviceStatusInAdmin(device.Id, true);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "协议轮询失败: {ProtocolName}", protocolName);
                        await UpdateSingleDeviceStatusInAdmin(device.Id, false);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "处理设备 {DeviceName} 时出错", device.Name);
                    await UpdateSingleDeviceStatusInAdmin(device.Id, false);
                }
            }
            
            _logger.LogDebug("数据采集完成，当前数据项数量: {Count}", _latestData.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "数据采集定时器执行失败");
        }
        finally
        {
            Interlocked.Exchange(ref _isCollecting, 0);
        }
    }

    /// <summary>
    /// 触发一次立即的数据采集（非阻塞对外API，不抛出异常）
    /// </summary>
    public void TriggerImmediateCollection()
    {
        try
        {
            CollectData(null);
            _logger.LogDebug("已触发一次立即数据采集");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "触发立即数据采集失败");
        }
    }

    private string GetProtocolName(Device device)
    {
        return $"{device.Protocol}_{device.IpAddress}_{device.Port}";
    }

    private async Task AddDataPointsToProtocol(Device device, string protocolName, bool stopProtocol = true)
    {
        try
        {
            // 从Admin获取设备的数据点
            var dataPoints = await GetDeviceDataPointsFromAdmin(device.Id);
            if (dataPoints == null || dataPoints.Count == 0)
            {
                _logger.LogWarning("设备 {DeviceName} 没有数据点", device.Name);
                return;
            }

            // 构建数据点配置字符串
            var dataPointConfigs = new List<string>();
            _logger.LogInformation("开始处理 {Count} 个数据点", dataPoints.Count);
            
            foreach (var dataPoint in dataPoints)
            {
                try
                {
                    // 解析Modbus特定配置
                    var functionCode = 3; // 默认功能码
                    if (!string.IsNullOrEmpty(dataPoint.Description))
                    {
                        var config = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(dataPoint.Description);
                        if (config != null && config.TryGetValue("functionCode", out var fc) && int.TryParse(fc, out var fcValue))
                        {
                            functionCode = fcValue;
                        }
                    }

                    // 格式: "Name,Address,DataType,FunctionCode"
                    var configStr = $"{dataPoint.Name},{dataPoint.Address},{dataPoint.DataType},{functionCode}";
                    dataPointConfigs.Add(configStr);
                    
                    _logger.LogInformation("添加数据点配置: {ConfigStr}", configStr);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "处理数据点配置失败: {DataPointName}", dataPoint.Name);
                }
            }
            
            _logger.LogInformation("总共构建了 {Count} 个数据点配置", dataPointConfigs.Count);

            // 将数据点配置添加到协议参数中
            if (dataPointConfigs.Count > 0)
            {
                var dataPointsStr = string.Join(";", dataPointConfigs);
                
                // 重新创建协议配置，包含数据点
                var protocolConfig = new IndustrialProtocolConfig
                {
                    Name = protocolName,
                    Type = "ModbusTcp",
                    Enabled = true,
                    Parameters = new Dictionary<string, string>
                    {
                        ["IpAddress"] = device.IpAddress,
                        ["Port"] = device.Port.ToString(),
                        ["UnitId"] = "1",
                        ["Timeout"] = "3000",
                        ["PollingInterval"] = "1000",
                        ["DataPoints"] = dataPointsStr
                    }
                };

                if (stopProtocol)
                {
                    // 停止并移除旧协议
                    // 清理该协议相关的缓存数据
                    RemoveProtocolCache(protocolName);
                    
                    // 添加新协议配置
                    _protocolManager.AddProtocol(protocolConfig);
                    _protocolManager.StartProtocol(protocolName);
                    _activeProtocols[protocolName] = 1;
                }
                else
                {
                    // 热更新：尽量不清空缓存，避免所有点位瞬时为无数据
                    _logger.LogInformation("热更新协议配置: {ProtocolName}", protocolName);
                    
                    // 轻量重启协议：不调用 RemoveProtocolCache（避免清空 _latestData 对该协议的所有键）
                    // 只重启协议实例，让新的点位配置生效
                    await Task.Delay(100);
                    _protocolManager.StopProtocol(protocolName);
                    _protocolManager.RemoveProtocol(protocolName);
                    _activeProtocols.TryRemove(protocolName, out _);
                    _protocolManager.AddProtocol(protocolConfig);
                    _protocolManager.StartProtocol(protocolName);
                    _activeProtocols[protocolName] = 1;
                    
                    _logger.LogInformation("协议 {ProtocolName} 热更新完成", protocolName);
                }
                
                _logger.LogInformation("为协议 {ProtocolName} 重新配置了 {Count} 个数据点", protocolName, dataPointConfigs.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "为协议添加数据点失败: {ProtocolName}", protocolName);
        }
    }

    private async Task<List<AdminDataPoint>?> GetDeviceDataPointsFromAdmin(int deviceId)
    {
        try
        {
            // 从CommunicationService获取HttpClient
            var response = await _sharedHttpClient.GetAsync($"{_adminApiUrl}/api/devices/{deviceId}/points");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                
                var dataPoints = System.Text.Json.JsonSerializer.Deserialize<List<AdminDataPoint>>(content, options);
                _logger.LogInformation("从Admin获取设备 {DeviceId} 的 {Count} 个数据点", deviceId, dataPoints?.Count ?? 0);
                
                // 详细记录每个数据点
                if (dataPoints != null)
                {
                    foreach (var dp in dataPoints)
                    {
                        _logger.LogInformation("数据点: ID={Id}, Name={Name}, Address={Address}, DataType={DataType}", 
                            dp.Id, dp.Name, dp.Address, dp.DataType);
                    }
                }
                
                return dataPoints;
            }
            else
            {
                _logger.LogWarning("从Admin获取设备数据点失败: {StatusCode}", response.StatusCode);
                return new List<AdminDataPoint>();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "从Admin获取设备数据点失败: {DeviceId}", deviceId);
            return new List<AdminDataPoint>();
        }
    }

    private async Task CreateProtocolForDevice(Device device)
    {
        try
        {
            _logger.LogInformation("开始为设备创建协议实例: {DeviceName}, Protocol='{Protocol}', IpAddress={IpAddress}, Port={Port}", 
                device.Name, device.Protocol, device.IpAddress, device.Port);
            
            var protocolName = GetProtocolName(device);
            
            // 检查协议类型是否为空
            if (string.IsNullOrWhiteSpace(device.Protocol))
            {
                _logger.LogError("设备 {DeviceName} 的协议类型为空，无法创建协议实例", device.Name);
                return;
            }
            
            // 根据设备协议类型创建协议配置
            IndustrialProtocolConfig protocolConfig;
            
            switch (device.Protocol.ToLower())
            {
                case "modbus":
                case "modbustcp":
                    protocolConfig = new IndustrialProtocolConfig
                    {
                        Name = protocolName,
                        Type = "ModbusTcp",
                        Enabled = true,
                        Parameters = new Dictionary<string, string>
                        {
                            ["IpAddress"] = device.IpAddress,
                            ["Port"] = device.Port.ToString(),
                            ["UnitId"] = "1",
                            ["Timeout"] = "3000",
                            ["PollingInterval"] = "1000"
                        }
                    };
                    _logger.LogInformation("创建Modbus协议配置: {ProtocolName}", protocolName);
                    break;
                case "opcua":
                case "opc ua":
                    protocolConfig = new IndustrialProtocolConfig
                    {
                        Name = protocolName,
                        Type = "OpcUa",
                        Enabled = true,
                        Parameters = new Dictionary<string, string>
                        {
                            ["EndpointUrl"] = device.IpAddress,
                            ["Timeout"] = "3000",
                            ["PollingInterval"] = "1000"
                        }
                    };
                    _logger.LogInformation("创建OPC UA协议配置: {ProtocolName}", protocolName);
                    break;
                default:
                    _logger.LogWarning("不支持的协议类型: '{ProtocolType}' (设备: {DeviceName})", device.Protocol, device.Name);
                    return;
            }

            // 检查协议是否已存在
            if (!_protocolManager.TryGetProtocol(protocolName, out _))
            {
                _protocolManager.AddProtocol(protocolConfig);
                _protocolManager.StartProtocol(protocolName);
                _logger.LogInformation("创建并启动新协议: {ProtocolName}", protocolName);
            }
            else
            {
                _logger.LogInformation("协议实例已存在，跳过创建: {ProtocolName}", protocolName);
            }
            
            // 为协议实例添加数据点
            await AddDataPointsToProtocol(device, protocolName);
            
            _logger.LogInformation("成功为设备创建协议实例: {DeviceName} -> {ProtocolName}", device.Name, protocolName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "为设备创建协议实例失败: {DeviceName}", device.Name);
        }
    }

    private object? CollectPointData(Device device, DataPoint dataPoint)
    {
        // 根据设备协议类型和点位配置采集数据
        switch (device.Protocol.ToLower())
        {
            case "modbus":
                return CollectModbusData(device, dataPoint);
            case "opcua":
                return CollectOpcUaData(device, dataPoint);
            default:
                // 返回模拟数据用于测试
                return GenerateMockData(dataPoint);
        }
    }

    private object? CollectModbusData(Device device, DataPoint dataPoint)
    {
        try
        {
            // 解析点位配置
            var description = dataPoint.Description;
            if (string.IsNullOrEmpty(description))
            {
                return GenerateMockData(dataPoint);
            }

            var config = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(description);
            if (config == null)
            {
                return GenerateMockData(dataPoint);
            }

            // 获取Modbus配置参数
            var functionCode = config.TryGetValue("functionCode", out var fc) ? int.Parse(fc) : 3;
            var slaveId = config.TryGetValue("slaveId", out var si) ? int.Parse(si) : 1;
            var quantity = config.TryGetValue("quantity", out var q) ? int.Parse(q) : 1;

            // 使用BYWGLib进行实际的Modbus数据采集
            return ReadModbusDataUsingBYWGLib(device, dataPoint, functionCode, slaveId, quantity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Modbus数据采集失败");
            return GenerateMockData(dataPoint);
        }
    }

    private object? ReadModbusDataUsingBYWGLib(Device device, DataPoint dataPoint, int functionCode, int slaveId, int quantity)
    {
        try
        {
            // 检查是否已有对应的协议实例
            var protocolName = GetProtocolName(device); // 使用统一的协议名生成方法
            if (!_protocolManager.TryGetProtocol(protocolName, out var protocol))
            {
                // 创建新的Modbus协议实例
                var protocolConfig = new IndustrialProtocolConfig
                {
                    Name = protocolName,
                    Type = "ModbusTcp",
                    Enabled = true,
                    Parameters = new Dictionary<string, string>
                    {
                        ["IpAddress"] = device.IpAddress,
                        ["Port"] = device.Port.ToString(),
                        ["UnitId"] = slaveId.ToString(),
                        ["Timeout"] = "3000",
                        ["PollingInterval"] = "1000"
                    }
                };

                _protocolManager.AddProtocol(protocolConfig);
                _protocolManager.StartProtocol(protocolName);
                
                if (!_protocolManager.TryGetProtocol(protocolName, out protocol))
                {
                    _logger.LogError("无法创建Modbus协议实例: {ProtocolName}", protocolName);
                    return GenerateMockData(dataPoint);
                }
            }

            // 使用协议实例读取数据
            if (protocol is AsyncModbusTcpProtocol modbusProtocol)
            {
                // 直接调用BYWGLib的读取方法
                var value = modbusProtocol.Read(dataPoint.Address, dataPoint.DataType);
                _logger.LogDebug("BYWGLib读取成功: {Address} = {Value}", dataPoint.Address, value);
                return value;
            }
            else
            {
                _logger.LogWarning("协议实例不是ModbusTcp类型: {ProtocolType}", protocol.GetType().Name);
                return GenerateMockData(dataPoint);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "使用BYWGLib读取Modbus数据失败: {Address}", dataPoint.Address);
            return GenerateMockData(dataPoint);
        }
    }

    private object? CollectOpcUaData(Device device, DataPoint dataPoint)
    {
        // OPC UA数据采集实现
        return GenerateMockData(dataPoint);
    }

    private object GenerateMockData(DataPoint dataPoint)
    {
        var random = new Random();
        
        switch (dataPoint.DataType.ToUpper())
        {
            case "BOOL":
                return random.Next(0, 2) == 1;
            case "INT16":
                return random.Next(-32768, 32767);
            case "INT32":
                return random.Next(-2147483648, 2147483647);
            case "FLOAT32":
                return Math.Round((float)(random.NextDouble() * 100), 2);
            case "STRING":
                return $"Test_{dataPoint.Address}_{random.Next(100)}";
            default:
                return random.Next(0, 100);
        }
    }

    private async void OnDataChanged(object? sender, DataChangedEventArgs e)
    {
        try
        {
            _logger.LogInformation("收到数据变化事件: 数据项数量={Count}", e.ChangedItems.Count);
            
            foreach (var item in e.ChangedItems)
            {
                // 仅使用规范键（Id）存储，避免字典无界增长
                var key = item.Id;
                if (!string.IsNullOrEmpty(key))
                {
                    _latestData[key] = item.Value;
                    _logger.LogDebug("存储数据: Key={Key}, Value={Value}", key, item.Value);
                }

                _logger.LogDebug("BYWGLib数据更新: {Name} = {Value} (质量: {Quality})", 
                    item.Name, item.Value, item.Quality);
            }
            
            _logger.LogDebug("当前_latestData包含 {Count} 个数据项", _latestData.Count);
            
            // 记录数据接收时间
            foreach (var item in e.ChangedItems)
            {
                var protocolName = item.Id.Split('.').FirstOrDefault();
                if (!string.IsNullOrEmpty(protocolName))
                {
                    var devices = _communicationService.GetCachedDevices();
                    foreach (var device in devices)
                    {
                        var expectedProtocolName = GetProtocolName(device);
                        if (protocolName == expectedProtocolName)
                        {
                            _lastDataReceived[device.Id] = DateTime.Now;
                            break;
                        }
                    }
                }
            }
            
            // 更新设备状态为在线
            await UpdateDeviceStatusInAdmin(e.ChangedItems, true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "处理数据变化事件失败");
        }
    }

    private void RemoveProtocolCache(string protocolName)
    {
        try
        {
            _logger.LogInformation("开始清理协议缓存: {ProtocolName}", protocolName);
            
            // 1. 停止并移除协议实例
            if (_protocolManager.TryGetProtocol(protocolName, out var protocol))
            {
                _protocolManager.StopProtocol(protocolName);
                _protocolManager.RemoveProtocol(protocolName);
                _logger.LogInformation("已停止并移除协议实例: {ProtocolName}", protocolName);
                _activeProtocols.TryRemove(protocolName, out _);
            }

            // 2. 清理数据缓存
            var prefix = protocolName + ".";
            var keysToRemove = _latestData.Keys.Where(k => k.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)).ToList();
            foreach (var k in keysToRemove)
            {
                _latestData.TryRemove(k, out _);
            }
            if (keysToRemove.Count > 0)
            {
                _logger.LogInformation("已清理协议 {ProtocolName} 相关缓存键 {Count} 个", protocolName, keysToRemove.Count);
            }

            // 3. 清理设备数据接收记录
            var devices = _communicationService.GetCachedDevices();
            foreach (var device in devices)
            {
                var expectedProtocolName = GetProtocolName(device);
                if (expectedProtocolName == protocolName)
                {
                    _lastDataReceived.Remove(device.Id);
                    _logger.LogInformation("已清理设备 {DeviceId} 的数据接收记录", device.Id);
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "清理协议 {ProtocolName} 缓存失败", protocolName);
        }
    }


    /// <summary>
    /// 监控设备连接状态
    /// </summary>
    private async void MonitorConnections(object? state)
    {
        if (Interlocked.Exchange(ref _isMonitoring, 1) == 1)
        {
            // 上一次执行尚未完成，跳过本次触发
            return;
        }
        try
        {
            var devices = _communicationService.GetCachedDevices();
            var currentTime = DateTime.Now;
            
            foreach (var device in devices)
            {
                try
                {
                    // 检查设备是否在最近30秒内收到过数据
                    if (_lastDataReceived.TryGetValue(device.Id, out var lastReceived))
                    {
                        var timeSinceLastData = currentTime - lastReceived;
                        if (timeSinceLastData.TotalSeconds > 30)
                        {
                            // 超过30秒没有收到数据，标记为离线
                            _logger.LogWarning("设备 {DeviceName} 超过30秒未收到数据，标记为离线", device.Name);
                            await UpdateSingleDeviceStatusInAdmin(device.Id, false);
                        }
                    }
                    else
                    {
                        // 从未收到过数据，标记为离线
                        _logger.LogWarning("设备 {DeviceName} 从未收到过数据，标记为离线", device.Name);
                        await UpdateSingleDeviceStatusInAdmin(device.Id, false);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "监控设备 {DeviceName} 连接状态失败", device.Name);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "连接监控失败");
        }
        finally
        {
            Interlocked.Exchange(ref _isMonitoring, 0);
        }
    }

    /// <summary>
    /// 更新Admin数据库中的设备状态
    /// </summary>
    private async Task UpdateDeviceStatusInAdmin(List<IndustrialDataItem> dataItems, bool isOnline)
    {
        try
        {
            if (dataItems == null || dataItems.Count == 0)
                return;

            // 从数据项中提取设备信息
            var deviceIds = new HashSet<int>();
            foreach (var item in dataItems)
            {
                // 从协议名称中提取设备信息，格式: "ModbusTCP_192.168.6.6_502"
                var protocolName = item.Id.Split('.').FirstOrDefault();
                if (!string.IsNullOrEmpty(protocolName))
                {
                    // 通过协议名称找到对应的设备
                    var devices = _communicationService.GetCachedDevices();
                    foreach (var device in devices)
                    {
                        var expectedProtocolName = GetProtocolName(device);
                        if (protocolName == expectedProtocolName)
                        {
                            deviceIds.Add(device.Id);
                            break;
                        }
                    }
                }
            }

            // 更新每个设备的状态
            foreach (var deviceId in deviceIds)
            {
                await UpdateSingleDeviceStatusInAdmin(deviceId, isOnline);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新设备状态到Admin失败");
        }
    }

    /// <summary>
    /// 更新单个设备状态到Admin数据库
    /// </summary>
    private async Task UpdateSingleDeviceStatusInAdmin(int deviceId, bool isOnline)
    {
        try
        {
            var statusValue = isOnline ? 1 : 0; // 1=在线, 0=离线
            var updateData = new { status = statusValue };
            
            var json = System.Text.Json.JsonSerializer.Serialize(updateData);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var response = await _sharedHttpClient.PatchAsync($"{_adminApiUrl}/api/devices/{deviceId}/status", content);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("设备 {DeviceId} 状态已更新为: {Status}", deviceId, isOnline ? "在线" : "离线");
            }
            else
            {
                _logger.LogWarning("更新设备 {DeviceId} 状态失败: {StatusCode}", deviceId, response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新设备 {DeviceId} 状态到Admin失败", deviceId);
        }
    }

    /// <summary>
    /// 测试数据变化事件处理
    /// </summary>
    public void TestDataChangedEvent()
    {
        try
        {
            _logger.LogInformation("开始测试数据变化事件处理");
            
            // 创建测试数据项
            var testItem = new IndustrialDataItem
            {
                Id = "test_1",
                Name = "TestPoint",
                Value = 123,
                DataType = "INT16",
                Quality = Quality.Good
            };
            
            // 手动触发数据变化事件
            var testEventArgs = new DataChangedEventArgs(new List<IndustrialDataItem> { testItem });
            OnDataChanged(this, testEventArgs);
            
            _logger.LogInformation("测试数据变化事件处理完成");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "测试数据变化事件处理失败");
        }
    }

    /// <summary>
    /// 获取最新数据
    /// </summary>
    public async Task<Dictionary<string, object>> GetLatestDataAsync()
    {
        // 先确保设备数据已同步
        try
        {
            await _communicationService.SyncDevicesImmediately();
            _logger.LogDebug("设备数据同步完成");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "设备数据同步失败");
        }
        
        // 主动触发一次数据采集
        try
        {
            CollectData(null);
            _logger.LogDebug("主动触发数据采集完成");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "主动触发数据采集失败");
        }
        
        return _latestData.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    /// <summary>
    /// 获取最新数据（同步版本，保持向后兼容）
    /// </summary>
    public Dictionary<string, object> GetLatestData()
    {
        // 不阻塞等待同步，直接返回当前数据
        // 数据同步由定时器自动处理，避免阻塞API响应
        
        var result = _latestData.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        _logger.LogDebug("GetLatestData返回 {Count} 个数据项", result.Count);
        
        // 如果数据为空，记录警告但不阻塞
        if (result.Count == 0)
        {
            _logger.LogWarning("GetLatestData返回空数据，可能设备未配置或数据采集未开始");
        }
        
        return result;
    }

    /// <summary>
    /// 获取指定协议的数据
    /// </summary>
    public Dictionary<string, object> GetProtocolData(string protocolName)
    {
        return _latestData
            .Where(kvp => kvp.Key.StartsWith($"{protocolName}."))
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    // --- 管理接口需要的简单状态/控制 ---
    public object GetStatusSummary()
    {
        using var context = _dbFactory.CreateDbContext();
        return new
        {
            protocols = context.ProtocolConfigs.Where(p => p.IsEnabled).Select(p => p.Name).ToList(),
            items = _latestData.Count
        };
    }

    public void ReloadConfiguration()
    {
        // 这里可以扩展为从 Admin 拉取配置；当前简单实现为重启协议轮询
        _protocolManager.StopAllProtocols();
        _protocolManager.StopPolling();
        _protocolManager.StartAllProtocols();
        _protocolManager.StartPolling();
    }

    /// <summary>
    /// 重新加载协议配置（从Admin获取最新数据点配置）
    /// </summary>
    public async Task ReloadProtocolConfigurations()
    {
        try
        {
            _logger.LogInformation("开始重新加载协议配置");
            
            // 先同步设备数据，确保获取最新的设备列表
            await _communicationService.SyncDevicesImmediately();
            
            // 获取所有启用的设备
            var devices = _communicationService.GetCachedDevices();
            if (devices == null || devices.Count == 0)
            {
                _logger.LogWarning("同步后仍然没有启用的设备，清理所有协议");
                // 如果没有设备，清理所有协议
                CleanupAllProtocols();
                return;
            }

            _logger.LogInformation("找到 {Count} 个设备，开始重新配置协议", devices.Count);

            // 1. 清理已删除设备的协议和数据
            CleanupDeletedDeviceProtocols(devices);

            // 2. 为每个设备重新配置协议（增量更新，不停止所有协议）
            foreach (var device in devices)
            {
                try
                {
                    var protocolName = GetProtocolName(device);
                    _logger.LogInformation("处理设备: {DeviceName}, 协议名: {ProtocolName}", device.Name, protocolName);
                    
                    // 检查协议是否已存在
                    if (!_protocolManager.TryGetProtocol(protocolName, out _))
                    {
                        // 如果协议不存在，创建新协议并添加数据点
                        await CreateProtocolForDevice(device);
                        await AddDataPointsToProtocol(device, protocolName, stopProtocol: true);
                        _logger.LogInformation("为新设备创建协议并添加数据点: {ProtocolName}", protocolName);
                    }
                    else
                    {
                        // 如果协议已存在，重新配置数据点（尝试热更新）
                        await AddDataPointsToProtocol(device, protocolName, stopProtocol: false);
                        _logger.LogInformation("为现有协议重新配置数据点: {ProtocolName}", protocolName);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "为设备 {DeviceName} 重新加载协议配置失败", device.Name);
                }
            }
            
            _logger.LogInformation("协议配置重新加载完成，处理了 {Count} 个设备", devices.Count);
            
            // 重载完成后立刻触发一次采集，确保新增点位/配置即时生效
            TriggerImmediateCollection();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "重新加载协议配置失败");
        }
    }

    /// <summary>
    /// 清理已删除设备的协议和数据
    /// </summary>
    private void CleanupDeletedDeviceProtocols(List<Device> currentDevices)
    {
        try
        {
            _logger.LogInformation("开始清理已删除设备的协议和数据");
            
            // 获取当前设备应该存在的协议名
            var currentProtocolNames = currentDevices.Select(GetProtocolName).ToHashSet();
            
            // 使用已登记的协议集合，避免因为缓存暂时为空导致误清理
            var existingProtocolNames = _activeProtocols.Keys.ToList();
            
            // 找出需要清理的协议（存在于数据缓存但不在当前设备列表中）
            var protocolsToCleanup = existingProtocolNames.Where(name => !currentProtocolNames.Contains(name)).ToList();
            
            foreach (var protocolName in protocolsToCleanup)
            {
                _logger.LogInformation("清理已删除设备的协议: {ProtocolName}", protocolName);
                RemoveProtocolCache(protocolName);
            }
            
            if (protocolsToCleanup.Count > 0)
            {
                _logger.LogInformation("已清理 {Count} 个已删除设备的协议", protocolsToCleanup.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "清理已删除设备的协议失败");
        }
    }

    /// <summary>
    /// 清理所有协议（当没有设备时）
    /// </summary>
    private void CleanupAllProtocols()
    {
        try
        {
            _logger.LogInformation("开始清理所有协议");
            
            // 使用已登记的协议集合，避免因为缓存暂时为空导致误清理
            var existingProtocolNames = _activeProtocols.Keys.ToList();
            
            foreach (var protocolName in existingProtocolNames)
            {
                _logger.LogInformation("清理协议: {ProtocolName}", protocolName);
                RemoveProtocolCache(protocolName);
            }
            
            if (existingProtocolNames.Count > 0)
            {
                _logger.LogInformation("已清理所有 {Count} 个协议", existingProtocolNames.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "清理所有协议失败");
        }
    }

    public void Dispose()
    {
        _collectionTimer?.Dispose();
        _connectionMonitorTimer?.Dispose();
    }
}

// Admin数据点模型
public class AdminDataPoint
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public string? Access { get; set; }
    public int IntervalMs { get; set; }
    public bool Enabled { get; set; }
    public string? Description { get; set; }
}

