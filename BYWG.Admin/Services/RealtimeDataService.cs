using Microsoft.AspNetCore.SignalR;
using BYWG.Admin.Hubs;
using System.Collections.Concurrent;

namespace BYWG.Admin.Services;

/// <summary>
/// 实时数据服务
/// </summary>
public class RealtimeDataService
{
    private readonly IHubContext<MonitoringHub> _hubContext;
    private readonly ILogger<RealtimeDataService> _logger;
    private readonly ConcurrentDictionary<string, object> _latestData = new();
    private readonly Timer _dataUpdateTimer;
    private readonly string _gatewayApiBase;

    public RealtimeDataService(IHubContext<MonitoringHub> hubContext, ILogger<RealtimeDataService> logger, IConfiguration configuration)
    {
        _hubContext = hubContext;
        _logger = logger;
        // 优先读取配置，其次环境变量，最后默认值
        _gatewayApiBase = configuration["GATEWAY_API_URL"]
            ?? Environment.GetEnvironmentVariable("GATEWAY_API_URL")
            ?? "http://localhost:5080";
        
        _logger.LogInformation("RealtimeDataService 已启动，将每2秒从Gateway获取数据");
        
        // 启动定时器，每2秒更新一次数据
        _dataUpdateTimer = new Timer(UpdateRealtimeData, null, TimeSpan.Zero, TimeSpan.FromSeconds(2));
    }

    /// <summary>
    /// 更新实时数据
    /// </summary>
    private async void UpdateRealtimeData(object? state)
    {
        try
        {
            _logger.LogInformation("开始从Gateway获取最新数据");
            
            // 从网关获取最新数据
            var newData = await GetGatewayData();
            
            if (newData != null && newData.Any())
            {
                _logger.LogInformation("从Gateway获取到 {Count} 个数据点", newData.Count);
                
                // 更新本地缓存
                foreach (var item in newData)
                {
                    _latestData.AddOrUpdate(item.Key, item.Value, (key, oldValue) => item.Value);
                }

                // 推送给所有连接的客户端
                await _hubContext.Clients.Group("Monitoring").SendAsync("RealtimeDataUpdate", newData);
                
                _logger.LogDebug("推送实时数据更新，包含 {Count} 个数据点", newData.Count);
            }
            else
            {
                _logger.LogInformation("从Gateway获取的数据为空或null");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新实时数据失败: {Message}", ex.Message);
        }
    }

    /// <summary>
    /// 从网关获取数据
    /// </summary>
    private async Task<Dictionary<string, object>?> GetGatewayData()
    {
        try
        {
            var url = $"{_gatewayApiBase}/api/management/latest";
            
            _logger.LogInformation("尝试从Gateway获取数据: {Url}", url);
            
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(5);

            var response = await httpClient.GetAsync(url);
            _logger.LogInformation("Gateway响应状态: {StatusCode}", response.StatusCode);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Gateway响应内容: {Content}", content);
                
                var data = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(content);
                _logger.LogInformation("反序列化后的数据: {Data}", data);
                
                return data;
            }
            else
            {
                _logger.LogWarning("Gateway响应失败: {StatusCode} {ReasonPhrase}", response.StatusCode, response.ReasonPhrase);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "从网关获取数据失败，使用模拟数据");
        }

        // 返回模拟数据
        _logger.LogInformation("返回模拟数据");
        return GetMockData();
    }

    /// <summary>
    /// 获取模拟数据
    /// </summary>
    private Dictionary<string, object> GetMockData()
    {
        var random = new Random();
        return new Dictionary<string, object>
        {
            ["4500"] = random.Next(0, 100),  // 您的点位地址
            ["40001"] = Math.Round(20 + random.NextDouble() * 10, 2), // 温度
            ["40002"] = Math.Round(100 + random.NextDouble() * 20, 2), // 压力
            ["40003"] = Math.Round(10 + random.NextDouble() * 10, 2), // 流量
            ["10001"] = random.Next(0, 2) == 1, // 状态
            ["10002"] = random.Next(0, 2) == 1, // 报警
            ["Temperature"] = Math.Round(20 + random.NextDouble() * 10, 2),
            ["Pressure"] = Math.Round(100 + random.NextDouble() * 20, 2),
            ["Flow"] = Math.Round(10 + random.NextDouble() * 10, 2),
            ["Status"] = random.Next(0, 2) == 1,
            ["Alarm"] = random.Next(0, 2) == 1
        };
    }

    /// <summary>
    /// 获取最新数据
    /// </summary>
    public Dictionary<string, object> GetLatestData()
    {
        return _latestData.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    /// <summary>
    /// 推送特定设备的数据更新
    /// </summary>
    public async Task PushDeviceDataUpdate(int deviceId, Dictionary<string, object> data)
    {
        await _hubContext.Clients.Group($"Device_{deviceId}").SendAsync("DeviceDataUpdate", data);
    }

    /// <summary>
    /// 推送特定点位的数值更新
    /// </summary>
    public async Task PushPointValueUpdate(int deviceId, int pointId, string pointAddress, object value)
    {
        var updateData = new
        {
            DeviceId = deviceId,
            PointId = pointId,
            Address = pointAddress,
            Value = value,
            Timestamp = DateTime.UtcNow
        };

        await _hubContext.Clients.Group($"Point_{deviceId}_{pointId}").SendAsync("PointValueUpdate", updateData);
        await _hubContext.Clients.Group($"Device_{deviceId}").SendAsync("PointValueUpdate", updateData);
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        _dataUpdateTimer?.Dispose();
    }
}
