using System.ComponentModel.DataAnnotations;

namespace BYWG.Shared.Models;

/// <summary>
/// 设备模型
/// </summary>
public class Device
{
    /// <summary>
    /// 设备ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 设备名称
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 设备类型
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// 设备IP地址
    /// </summary>
    [Required]
    [StringLength(45)]
    public string IpAddress { get; set; } = string.Empty;

    /// <summary>
    /// 设备端口
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// 协议类型
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Protocol { get; set; } = string.Empty;

    /// <summary>
    /// 设备状态
    /// </summary>
    public DeviceStatus Status { get; set; } = DeviceStatus.Offline;

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
    /// 最后连接时间
    /// </summary>
    public DateTime? LastConnectedAt { get; set; }

    /// <summary>
    /// 网关ID
    /// </summary>
    public int? GatewayId { get; set; }

    /// <summary>
    /// 关联的网关
    /// </summary>
    public Gateway? Gateway { get; set; }

    /// <summary>
    /// 设备配置参数
    /// </summary>
    public Dictionary<string, string> Parameters { get; set; } = new();
}

/// <summary>
/// 设备状态枚举
/// </summary>
public enum DeviceStatus
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
