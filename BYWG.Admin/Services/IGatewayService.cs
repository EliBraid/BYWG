using BYWG.Shared.Models;

namespace BYWG.Admin.Services;

/// <summary>
/// 网关服务接口
/// </summary>
public interface IGatewayService
{
    /// <summary>
    /// 获取所有网关
    /// </summary>
    Task<IEnumerable<Gateway>> GetAllGatewaysAsync();

    /// <summary>
    /// 根据ID获取网关
    /// </summary>
    Task<Gateway?> GetGatewayByIdAsync(int id);

    /// <summary>
    /// 创建网关
    /// </summary>
    Task<Gateway> CreateGatewayAsync(Gateway gateway);

    /// <summary>
    /// 更新网关
    /// </summary>
    Task<Gateway> UpdateGatewayAsync(Gateway gateway);

    /// <summary>
    /// 删除网关
    /// </summary>
    Task<bool> DeleteGatewayAsync(int id);

    /// <summary>
    /// 注册网关
    /// </summary>
    Task<Gateway> RegisterGatewayAsync(GatewayRegistrationRequest request);

    /// <summary>
    /// 更新网关心跳
    /// </summary>
    Task<bool> UpdateGatewayHeartbeatAsync(string gatewayId, GatewayHeartbeatRequest request);

    /// <summary>
    /// 更新网关状态
    /// </summary>
    Task<bool> UpdateGatewayStatusAsync(int gatewayId, GatewayStatus status);

    /// <summary>
    /// 搜索网关
    /// </summary>
    Task<IEnumerable<Gateway>> SearchGatewaysAsync(string searchTerm);
}

/// <summary>
/// 网关注册请求
/// </summary>
public class GatewayRegistrationRequest
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public int Port { get; set; } = 8080;
    public string? Version { get; set; }
    public GatewaySystemInfo? SystemInfo { get; set; }
}

/// <summary>
/// 网关心跳请求
/// </summary>
public class GatewayHeartbeatRequest
{
    public string GatewayId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Status { get; set; } = string.Empty;
    public GatewaySystemInfo? SystemInfo { get; set; }
}