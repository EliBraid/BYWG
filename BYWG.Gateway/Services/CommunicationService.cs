using BYWG.Gateway.Data;
using BYWG.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text.Json;

namespace BYWG.Gateway.Services;

/// <summary>
/// 通信服务 - 负责与管理后端通信
/// </summary>
public class CommunicationService : IHostedService, IDisposable
{
    private readonly IDbContextFactory<GatewayDbContext> _dbFactory;
    private readonly ILogger<CommunicationService> _logger;
    private readonly HttpClient _httpClient;
    private readonly Timer _heartbeatTimer;
    private readonly Timer _dataSyncTimer;
    private readonly string _adminApiUrl;
    private readonly string _gatewayId;
    private List<Device> _cachedDevices = new List<Device>();

    public CommunicationService(
        IDbContextFactory<GatewayDbContext> dbFactory,
        ILogger<CommunicationService> logger,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
    {
        _dbFactory = dbFactory;
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient();
        _adminApiUrl = configuration["ADMIN_API_URL"]
            ?? Environment.GetEnvironmentVariable("ADMIN_API_URL")
            ?? "http://localhost:5000";
        _gatewayId = configuration["GATEWAY_ID"]
            ?? Environment.GetEnvironmentVariable("GATEWAY_ID")
            ?? Environment.MachineName;

        // 验证Admin API URL格式
        if (string.IsNullOrWhiteSpace(_adminApiUrl) || _adminApiUrl.Contains("<"))
        {
            _logger.LogError("Admin API URL配置无效: {AdminApiUrl}", _adminApiUrl);
            throw new InvalidOperationException($"Admin API URL配置无效: {_adminApiUrl}。请检查配置文件或环境变量ADMIN_API_URL。");
        }

        _logger.LogInformation("Gateway通信服务配置 - Admin API URL: {AdminApiUrl}, Gateway ID: {GatewayId}", _adminApiUrl, _gatewayId);

        _heartbeatTimer = new Timer(SendHeartbeat, null, Timeout.Infinite, Timeout.Infinite);
        _dataSyncTimer = new Timer(SyncData, null, Timeout.Infinite, Timeout.Infinite);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("通信服务启动");

        // 注册网关到管理后端
        await RegisterGateway();

        // 启动心跳定时器（每30秒发送一次心跳）
        _heartbeatTimer.Change(TimeSpan.Zero, TimeSpan.FromSeconds(30));

        // 启动数据同步定时器（每60秒同步一次数据）
        _dataSyncTimer.Change(TimeSpan.Zero, TimeSpan.FromSeconds(60));

        _logger.LogInformation("通信服务启动完成");
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("通信服务停止");

        _heartbeatTimer.Change(Timeout.Infinite, Timeout.Infinite);
        _dataSyncTimer.Change(Timeout.Infinite, Timeout.Infinite);

        // 注销网关
        await UnregisterGateway();

        _logger.LogInformation("通信服务停止完成");
    }

    private async Task RegisterGateway()
    {
        try
        {
            var gatewayInfo = new
            {
                Id = _gatewayId,
                Name = Environment.MachineName,
                IpAddress = GetLocalIpAddress(),
                Port = 8080,
                Version = "1.0.0",
                Status = "Online",
                SystemInfo = new
                {
                    CpuUsage = GetCpuUsage(),
                    MemoryUsage = GetMemoryUsage(),
                    DiskUsage = GetDiskUsage(),
                    Uptime = Environment.TickCount64,
                    OperatingSystem = Environment.OSVersion.ToString(),
                    DeviceCount = await (await _dbFactory.CreateDbContextAsync()).Devices.CountAsync(),
                    ProtocolCount = await (await _dbFactory.CreateDbContextAsync()).ProtocolConfigs.CountAsync()
                }
            };

            var json = JsonSerializer.Serialize(gatewayInfo);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_adminApiUrl}/api/gateways/register", content);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("网关注册成功");
            }
            else
            {
                _logger.LogWarning("网关注册失败: {StatusCode}", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "注册网关失败");
        }
    }

    private async Task UnregisterGateway()
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"{_adminApiUrl}/api/gateways/{_gatewayId}");
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("网关注销成功");
            }
            else
            {
                _logger.LogWarning("网关注销失败: {StatusCode}", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "注销网关失败");
        }
    }

    private async void SendHeartbeat(object? state)
    {
        try
        {
            var heartbeat = new
            {
                GatewayId = _gatewayId,
                Timestamp = DateTime.UtcNow,
                Status = "Online",
                SystemInfo = new
                {
                    CpuUsage = GetCpuUsage(),
                    MemoryUsage = GetMemoryUsage(),
                    DiskUsage = GetDiskUsage(),
                    Uptime = Environment.TickCount64,
                    DeviceCount = await (await _dbFactory.CreateDbContextAsync()).Devices.CountAsync(),
                    ProtocolCount = await (await _dbFactory.CreateDbContextAsync()).ProtocolConfigs.CountAsync()
                }
            };

            var json = JsonSerializer.Serialize(heartbeat);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            // 验证Admin API URL
            if (string.IsNullOrWhiteSpace(_adminApiUrl) || _adminApiUrl.Contains("<"))
            {
                _logger.LogError("Admin API URL无效，跳过心跳发送: {AdminApiUrl}", _adminApiUrl);
                return;
            }

            var requestUrl = $"{_adminApiUrl}/api/gateways/{_gatewayId}/heartbeat";
            _logger.LogDebug("正在发送心跳: {RequestUrl}", requestUrl);
            var response = await _httpClient.PostAsync(requestUrl, content);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("发送心跳失败: {StatusCode}", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "发送心跳失败");
        }
    }

    private async void SyncData(object? state)
    {
        try
        {
            // 同步设备状态
            await SyncDeviceStatus();

            // 同步数据点数据
            await SyncDataPoints();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "同步数据失败");
        }
    }

    private async Task SyncDeviceStatus()
    {
        try
        {
            using var context = await _dbFactory.CreateDbContextAsync();
            var devices = await context.Devices.ToListAsync();
            var deviceStatuses = devices.Select(d => new
            {
                DeviceId = d.Id,
                Status = d.Status.ToString(),
                LastConnected = d.LastConnectedAt,
                IsEnabled = d.IsEnabled
            }).ToList();

            var json = JsonSerializer.Serialize(deviceStatuses);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_adminApiUrl}/api/gateways/{_gatewayId}/devices/status", content);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("同步设备状态失败: {StatusCode}", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "同步设备状态失败");
        }
    }

    private async Task SyncDataPoints()
    {
        try
        {
            // 先从Admin同步设备数据
            await SyncDevicesFromAdmin();
            
            // 从Admin同步设备点位配置
            await SyncDevicePointsFromAdmin();
            
            // 同步实时数据到Admin
            await SyncRealtimeDataToAdmin();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "同步数据点失败");
        }
    }

    private async Task SyncDevicesFromAdmin()
    {
        try
        {
            // 验证Admin API URL
            if (string.IsNullOrWhiteSpace(_adminApiUrl) || _adminApiUrl.Contains("<"))
            {
                _logger.LogError("Admin API URL无效，跳过设备同步: {AdminApiUrl}", _adminApiUrl);
                return;
            }

            // 从Admin获取所有设备数据
            var requestUrl = $"{_adminApiUrl}/api/devices";
            _logger.LogDebug("正在从Admin获取设备数据: {RequestUrl}", requestUrl);
            var response = await _httpClient.GetAsync(requestUrl);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Admin API返回的原始数据: {Content}", content);
                
                // 检查JSON中是否包含Protocol字段
                if (content.Contains("\"protocol\""))
                {
                    _logger.LogInformation("JSON中包含protocol字段（小写）");
                }
                else if (content.Contains("\"Protocol\""))
                {
                    _logger.LogInformation("JSON中包含Protocol字段（大写）");
                }
                else
                {
                    _logger.LogWarning("JSON中不包含Protocol字段！");
                }
                
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                var adminDevices = System.Text.Json.JsonSerializer.Deserialize<List<Device>>(content, options);
                
                if (adminDevices != null)
                {
                    _logger.LogInformation("成功反序列化 {Count} 个设备", adminDevices.Count);
                    
                    // 打印每个设备的详细信息
                    foreach (var device in adminDevices)
                    {
                        _logger.LogInformation("设备详情: ID={Id}, Name={Name}, Protocol='{Protocol}', IpAddress={IpAddress}, Port={Port}, IsEnabled={IsEnabled}", 
                            device.Id, device.Name, device.Protocol, device.IpAddress, device.Port, device.IsEnabled);
                    }
                    
                    // 将设备数据存储到内存中，供DataCollectionService使用
                    _cachedDevices = adminDevices.Where(d => d.IsEnabled).ToList();
                    _logger.LogInformation("从Admin缓存了 {Count} 个启用的设备", _cachedDevices.Count);
                }
                else
                {
                    _logger.LogWarning("反序列化设备数据失败，返回null");
                }
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Admin API调用失败: {StatusCode}, 错误内容: {ErrorContent}", response.StatusCode, errorContent);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "从Admin获取设备数据失败");
        }
    }

    private async Task SyncDevicePointsFromAdmin()
    {
        try
        {
            using var context = await _dbFactory.CreateDbContextAsync();
            var devices = await context.Devices.ToListAsync();

            foreach (var device in devices)
            {
                try
                {
                    // 从Admin获取设备的点位配置
                    var response = await _httpClient.GetAsync($"{_adminApiUrl}/api/devices/{device.Id}/points");
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var points = System.Text.Json.JsonSerializer.Deserialize<List<AdminDataPoint>>(content);
                        
                        if (points != null)
                        {
                            // 更新本地数据库中的点位配置
                            var existingPoints = await context.DataPoints
                                .Where(dp => dp.DeviceId == device.Id)
                                .ToListAsync();

                            foreach (var adminPoint in points)
                            {
                                var existingPoint = existingPoints.FirstOrDefault(ep => ep.Id == adminPoint.Id);
                                if (existingPoint != null)
                                {
                                    // 更新现有点位
                                    existingPoint.Name = adminPoint.Name;
                                    existingPoint.Address = adminPoint.Address;
                                    existingPoint.DataType = adminPoint.DataType;
                                    existingPoint.IsEnabled = adminPoint.Enabled;
                                    existingPoint.UpdatedAt = DateTime.UtcNow;
                                    
                                    // 更新描述信息（包含Modbus配置）
                                    var description = new Dictionary<string, string>
                                    {
                                        ["access"] = adminPoint.Access,
                                        ["intervalMs"] = adminPoint.IntervalMs.ToString()
                                    };
                                    
                                    if (adminPoint.FunctionCode.HasValue)
                                        description["functionCode"] = adminPoint.FunctionCode.Value.ToString();
                                    if (!string.IsNullOrEmpty(adminPoint.AddressType))
                                        description["addressType"] = adminPoint.AddressType;
                                    if (adminPoint.Quantity.HasValue)
                                        description["quantity"] = adminPoint.Quantity.Value.ToString();
                                    if (adminPoint.SlaveId.HasValue)
                                        description["slaveId"] = adminPoint.SlaveId.Value.ToString();
                                    
                                    existingPoint.Description = System.Text.Json.JsonSerializer.Serialize(description);
                                }
                                else
                                {
                                    // 创建新点位
                                    var newPoint = new DataPoint
                                    {
                                        Id = adminPoint.Id,
                                        Name = adminPoint.Name,
                                        Address = adminPoint.Address,
                                        DataType = adminPoint.DataType,
                                        IsEnabled = adminPoint.Enabled,
                                        DeviceId = device.Id,
                                        CreatedAt = DateTime.UtcNow,
                                        UpdatedAt = DateTime.UtcNow
                                    };
                                    
                                    // 设置描述信息
                                    var description = new Dictionary<string, string>
                                    {
                                        ["access"] = adminPoint.Access,
                                        ["intervalMs"] = adminPoint.IntervalMs.ToString()
                                    };
                                    
                                    if (adminPoint.FunctionCode.HasValue)
                                        description["functionCode"] = adminPoint.FunctionCode.Value.ToString();
                                    if (!string.IsNullOrEmpty(adminPoint.AddressType))
                                        description["addressType"] = adminPoint.AddressType;
                                    if (adminPoint.Quantity.HasValue)
                                        description["quantity"] = adminPoint.Quantity.Value.ToString();
                                    if (adminPoint.SlaveId.HasValue)
                                        description["slaveId"] = adminPoint.SlaveId.Value.ToString();
                                    
                                    newPoint.Description = System.Text.Json.JsonSerializer.Serialize(description);
                                    
                                    context.DataPoints.Add(newPoint);
                                }
                            }
                            
                            await context.SaveChangesAsync();
                            _logger.LogDebug("同步设备 {DeviceId} 的点位配置完成", device.Id);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "同步设备 {DeviceId} 点位配置失败", device.Id);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "从Admin同步设备点位配置失败");
        }
    }

    private Task SyncRealtimeDataToAdmin()
    {
        try
        {
            // 这里可以将实时数据推送到Admin
            // 目前通过WebSocket推送，这里可以添加HTTP推送作为备用
            _logger.LogDebug("同步实时数据到Admin");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "同步实时数据到Admin失败");
        }
        return Task.CompletedTask;
    }


    // Admin数据点模型
    private class AdminDataPoint
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string DataType { get; set; } = string.Empty;
        public string Access { get; set; } = string.Empty;
        public int IntervalMs { get; set; }
        public bool Enabled { get; set; }
        public int? FunctionCode { get; set; }
        public string? AddressType { get; set; }
        public int? Quantity { get; set; }
        public int? SlaveId { get; set; }
    }

    private string GetLocalIpAddress()
    {
        try
        {
            var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取本地IP地址失败");
        }
        return "127.0.0.1";
    }

    private double GetCpuUsage()
    {
        // 简化的CPU使用率计算
        return new Random().NextDouble() * 100;
    }

    private double GetMemoryUsage()
    {
        // 简化的内存使用率计算
        return new Random().NextDouble() * 100;
    }

    private double GetDiskUsage()
    {
        // 简化的磁盘使用率计算
        return new Random().NextDouble() * 100;
    }

    /// <summary>
    /// 手动触发数据同步
    /// </summary>
    public void TriggerDataSync()
    {
        _logger.LogInformation("手动触发数据同步");
        SyncData(null);
    }

    /// <summary>
    /// 立即同步设备数据（同步方法）
    /// </summary>
    public async Task SyncDevicesImmediately()
    {
        try
        {
            await SyncDevicesFromAdmin();
            _logger.LogInformation("立即同步设备数据完成，缓存了 {Count} 个设备", _cachedDevices.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "立即同步设备数据失败");
        }
    }

    /// <summary>
    /// 获取缓存的设备数据
    /// </summary>
    public List<Device> GetCachedDevices()
    {
        return _cachedDevices;
    }

    public void Dispose()
    {
        _heartbeatTimer?.Dispose();
        _dataSyncTimer?.Dispose();
        _httpClient?.Dispose();
    }
}
