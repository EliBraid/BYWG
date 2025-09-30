using System.ComponentModel.DataAnnotations;

namespace BYWG.Shared.Models;

/// <summary>
/// 网关模型
/// </summary>
public class Gateway
{
    /// <summary>
    /// 网关ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 网关名称
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 网关IP地址
    /// </summary>
    [Required]
    [StringLength(45)]
    public string IpAddress { get; set; } = string.Empty;

    /// <summary>
    /// 网关端口
    /// </summary>
    public int Port { get; set; } = 8080;

    /// <summary>
    /// 网关状态
    /// </summary>
    public GatewayStatus Status { get; set; } = GatewayStatus.Offline;

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// 描述
    /// </summary>
    [StringLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 最后心跳时间
    /// </summary>
    public DateTime? LastHeartbeat { get; set; }

    /// <summary>
    /// 版本信息
    /// </summary>
    [StringLength(50)]
    public string? Version { get; set; }

    /// <summary>
    /// 系统信息
    /// </summary>
    public GatewaySystemInfo? SystemInfo { get; set; }

    /// <summary>
    /// 关联的设备
    /// </summary>
    public ICollection<Device> Devices { get; set; } = new List<Device>();
}

/// <summary>
/// 网关状态枚举
/// </summary>
public enum GatewayStatus
{
    /// <summary>
    /// 离线
    /// </summary>
    Offline = 0,

    /// <summary>
    /// 在线
    /// </summary>
    Online = 1,

    /// <summary>
    /// 连接中
    /// </summary>
    Connecting = 2,

    /// <summary>
    /// 错误
    /// </summary>
    Error = 3,

    /// <summary>
    /// 维护中
    /// </summary>
    Maintenance = 4
}

/// <summary>
/// 网关系统信息
/// </summary>
public class GatewaySystemInfo
{
    /// <summary>
    /// CPU使用率
    /// </summary>
    public double CpuUsage { get; set; }

    /// <summary>
    /// 内存使用率
    /// </summary>
    public double MemoryUsage { get; set; }

    /// <summary>
    /// 磁盘使用率
    /// </summary>
    public double DiskUsage { get; set; }

    /// <summary>
    /// 网络流量
    /// </summary>
    public long NetworkTraffic { get; set; }

    /// <summary>
    /// 运行时间
    /// </summary>
    public TimeSpan Uptime { get; set; }

    /// <summary>
    /// 操作系统
    /// </summary>
    public string? OperatingSystem { get; set; }

    /// <summary>
    /// 设备数量
    /// </summary>
    public int DeviceCount { get; set; }

    /// <summary>
    /// 协议数量
    /// </summary>
    public int ProtocolCount { get; set; }
}
