using BYWG.Admin.Data;
using Microsoft.EntityFrameworkCore;

namespace BYWG.Admin.Services;

public class MonitoringService : IMonitoringService
{
    private readonly AdminDbContext _context;
    private readonly ILogger<MonitoringService> _logger;

    public MonitoringService(AdminDbContext context, ILogger<MonitoringService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<SystemOverview> GetSystemOverviewAsync()
    {
        var totalGateways = await _context.Gateways.CountAsync();
        var onlineGateways = await _context.Gateways.CountAsync(g => (int)g.Status == 1);
        var totalDevices = await _context.Devices.CountAsync();
        var onlineDevices = await _context.Devices.CountAsync(d => (int)d.Status == 1);
        var totalUsers = await _context.Users.CountAsync();
        var unresolvedAlerts = await _context.Alerts.CountAsync(a => a.Status != "Resolved");

        return new SystemOverview
        {
            TotalGateways = totalGateways,
            OnlineGateways = onlineGateways,
            TotalDevices = totalDevices,
            OnlineDevices = onlineDevices,
            TotalUsers = totalUsers,
            ActiveUsers = 0,
            TotalAlerts = await _context.Alerts.CountAsync(),
            UnresolvedAlerts = unresolvedAlerts,
            LastUpdated = DateTime.UtcNow
        };
    }

    public async Task<GatewayStatistics> GetGatewayStatisticsAsync()
    {
        var gateways = await _context.Gateways.ToListAsync();
        return new GatewayStatistics
        {
            Total = gateways.Count,
            Online = gateways.Count(g => (int)g.Status == 1),
            Offline = gateways.Count(g => (int)g.Status == 0),
            Maintenance = gateways.Count(g => (int)g.Status == 4),
            AverageCpuUsage = gateways.Where(g => g.SystemInfo != null).DefaultIfEmpty().Average(g => g?.SystemInfo?.CpuUsage ?? 0),
            AverageMemoryUsage = gateways.Where(g => g.SystemInfo != null).DefaultIfEmpty().Average(g => g?.SystemInfo?.MemoryUsage ?? 0),
            TotalNetworkTraffic = (long)gateways.Where(g => g.SystemInfo != null).Sum(g => g!.SystemInfo!.NetworkTraffic)
        };
    }

    public async Task<DeviceStatistics> GetDeviceStatisticsAsync()
    {
        var devices = await _context.Devices.ToListAsync();
        return new DeviceStatistics
        {
            Total = devices.Count,
            Online = devices.Count(d => (int)d.Status == 1),
            Offline = devices.Count(d => (int)d.Status == 0),
            Error = devices.Count(d => (int)d.Status == 3),
            Maintenance = devices.Count(d => (int)d.Status == 4),
            ByProtocol = devices.GroupBy(d => d.Protocol).ToDictionary(g => g.Key, g => g.Count()),
            ByType = devices.GroupBy(d => d.Type).ToDictionary(g => g.Key, g => g.Count())
        };
    }

    public Task<IEnumerable<RealtimeData>> GetRealtimeDataAsync()
    {
        return Task.FromResult<IEnumerable<RealtimeData>>(Array.Empty<RealtimeData>());
    }

    public Task<IEnumerable<HistoricalData>> GetHistoricalDataAsync(DateTime startTime, DateTime endTime)
    {
        return Task.FromResult<IEnumerable<HistoricalData>>(Array.Empty<HistoricalData>());
    }

    public Task<PerformanceMetrics> GetPerformanceMetricsAsync()
    {
        return Task.FromResult(new PerformanceMetrics
        {
            CpuUsage = 0,
            MemoryUsage = 0,
            DiskUsage = 0,
            NetworkTraffic = 0,
            ActiveConnections = 0,
            ResponseTime = 0,
            RequestsPerSecond = 0,
            Timestamp = DateTime.UtcNow
        });
    }
}


