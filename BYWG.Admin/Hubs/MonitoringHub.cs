using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

namespace BYWG.Admin.Hubs;

/// <summary>
/// 监控数据Hub
/// </summary>
[Authorize]
public class MonitoringHub : Hub
{
    private readonly ILogger<MonitoringHub> _logger;

    public MonitoringHub(ILogger<MonitoringHub> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 客户端连接时
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("客户端连接: {ConnectionId}", Context.ConnectionId);
        
        // 加入监控组
        await Groups.AddToGroupAsync(Context.ConnectionId, "Monitoring");
        
        await base.OnConnectedAsync();
    }

    /// <summary>
    /// 客户端断开连接时
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("客户端断开连接: {ConnectionId}", Context.ConnectionId);
        
        // 离开监控组
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Monitoring");
        
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// 加入设备监控组
    /// </summary>
    public async Task JoinDeviceMonitoring(int deviceId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Device_{deviceId}");
        _logger.LogInformation("客户端 {ConnectionId} 加入设备 {DeviceId} 监控组", Context.ConnectionId, deviceId);
    }

    /// <summary>
    /// 离开设备监控组
    /// </summary>
    public async Task LeaveDeviceMonitoring(int deviceId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Device_{deviceId}");
        _logger.LogInformation("客户端 {ConnectionId} 离开设备 {DeviceId} 监控组", Context.ConnectionId, deviceId);
    }

    /// <summary>
    /// 加入网关监控组
    /// </summary>
    public async Task JoinGatewayMonitoring(int gatewayId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Gateway_{gatewayId}");
        _logger.LogInformation("客户端 {ConnectionId} 加入网关 {GatewayId} 监控组", Context.ConnectionId, gatewayId);
    }

    /// <summary>
    /// 离开网关监控组
    /// </summary>
    public async Task LeaveGatewayMonitoring(int gatewayId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Gateway_{gatewayId}");
        _logger.LogInformation("客户端 {ConnectionId} 离开网关 {GatewayId} 监控组", Context.ConnectionId, gatewayId);
    }

    /// <summary>
    /// 加入点位监控组
    /// </summary>
    public async Task JoinPointMonitoring(int deviceId, int pointId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Point_{deviceId}_{pointId}");
        _logger.LogInformation("客户端 {ConnectionId} 加入点位 {DeviceId}_{PointId} 监控组", Context.ConnectionId, deviceId, pointId);
    }

    /// <summary>
    /// 离开点位监控组
    /// </summary>
    public async Task LeavePointMonitoring(int deviceId, int pointId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Point_{deviceId}_{pointId}");
        _logger.LogInformation("客户端 {ConnectionId} 离开点位 {DeviceId}_{PointId} 监控组", Context.ConnectionId, deviceId, pointId);
    }
}
