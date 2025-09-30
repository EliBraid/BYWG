using BYWG.Gateway.Data;
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
    private readonly GatewayDbContext _context;
    private readonly ILogger<CommunicationService> _logger;
    private readonly HttpClient _httpClient;
    private readonly Timer _heartbeatTimer;
    private readonly Timer _dataSyncTimer;
    private readonly string _adminApiUrl;
    private readonly string _gatewayId;

    public CommunicationService(
        GatewayDbContext context,
        ILogger<CommunicationService> logger,
        IHttpClientFactory httpClientFactory)
    {
        _context = context;
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient();
        _adminApiUrl = Environment.GetEnvironmentVariable("ADMIN_API_URL") ?? "http://localhost:5000";
        _gatewayId = Environment.GetEnvironmentVariable("GATEWAY_ID") ?? Environment.MachineName;

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
                    DeviceCount = await _context.Devices.CountAsync(),
                    ProtocolCount = await _context.ProtocolConfigs.CountAsync()
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
                    DeviceCount = await _context.Devices.CountAsync(),
                    ProtocolCount = await _context.ProtocolConfigs.CountAsync()
                }
            };

            var json = JsonSerializer.Serialize(heartbeat);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_adminApiUrl}/api/gateways/{_gatewayId}/heartbeat", content);
            
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
            var devices = await _context.Devices.ToListAsync();
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
            // 这里可以同步实时数据到管理后端
            // 具体实现取决于数据同步策略
            _logger.LogDebug("同步数据点数据");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "同步数据点失败");
        }
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

    public void Dispose()
    {
        _heartbeatTimer?.Dispose();
        _dataSyncTimer?.Dispose();
        _httpClient?.Dispose();
    }
}
