namespace BYWG.Admin.Services;

/// <summary>
/// 监控服务接口
/// </summary>
public interface IMonitoringService
{
    /// <summary>
    /// 获取系统概览
    /// </summary>
    Task<SystemOverview> GetSystemOverviewAsync();

    /// <summary>
    /// 获取网关统计
    /// </summary>
    Task<GatewayStatistics> GetGatewayStatisticsAsync();

    /// <summary>
    /// 获取设备统计
    /// </summary>
    Task<DeviceStatistics> GetDeviceStatisticsAsync();

    /// <summary>
    /// 获取实时数据
    /// </summary>
    Task<IEnumerable<RealtimeData>> GetRealtimeDataAsync();

    /// <summary>
    /// 获取历史数据
    /// </summary>
    Task<IEnumerable<HistoricalData>> GetHistoricalDataAsync(DateTime startTime, DateTime endTime);

    /// <summary>
    /// 获取性能指标
    /// </summary>
    Task<PerformanceMetrics> GetPerformanceMetricsAsync();
}

/// <summary>
/// 系统概览
/// </summary>
public class SystemOverview
{
    public int TotalGateways { get; set; }
    public int OnlineGateways { get; set; }
    public int TotalDevices { get; set; }
    public int OnlineDevices { get; set; }
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int TotalAlerts { get; set; }
    public int UnresolvedAlerts { get; set; }
    public DateTime LastUpdated { get; set; }
}

/// <summary>
/// 网关统计
/// </summary>
public class GatewayStatistics
{
    public int Total { get; set; }
    public int Online { get; set; }
    public int Offline { get; set; }
    public int Maintenance { get; set; }
    public double AverageCpuUsage { get; set; }
    public double AverageMemoryUsage { get; set; }
    public long TotalNetworkTraffic { get; set; }
}

/// <summary>
/// 设备统计
/// </summary>
public class DeviceStatistics
{
    public int Total { get; set; }
    public int Online { get; set; }
    public int Offline { get; set; }
    public int Error { get; set; }
    public int Maintenance { get; set; }
    public Dictionary<string, int> ByProtocol { get; set; } = new();
    public Dictionary<string, int> ByType { get; set; } = new();
}

/// <summary>
/// 实时数据
/// </summary>
public class RealtimeData
{
    public int DeviceId { get; set; }
    public string DeviceName { get; set; } = string.Empty;
    public string DataPoint { get; set; } = string.Empty;
    public object Value { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Quality { get; set; } = string.Empty;
}

/// <summary>
/// 历史数据
/// </summary>
public class HistoricalData
{
    public int DeviceId { get; set; }
    public string DeviceName { get; set; } = string.Empty;
    public string DataPoint { get; set; } = string.Empty;
    public object Value { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Quality { get; set; } = string.Empty;
}

/// <summary>
/// 性能指标
/// </summary>
public class PerformanceMetrics
{
    public double CpuUsage { get; set; }
    public double MemoryUsage { get; set; }
    public double DiskUsage { get; set; }
    public long NetworkTraffic { get; set; }
    public int ActiveConnections { get; set; }
    public double ResponseTime { get; set; }
    public int RequestsPerSecond { get; set; }
    public DateTime Timestamp { get; set; }
}
