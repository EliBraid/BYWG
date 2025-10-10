using System.ComponentModel.DataAnnotations;

namespace BYWG.Shared.Models;

/// <summary>
/// 更新设备请求模型
/// </summary>
public class UpdateDeviceRequest
{
    /// <summary>
    /// 设备名称
    /// </summary>
    [StringLength(100)]
    public string? Name { get; set; }

    /// <summary>
    /// 设备类型
    /// </summary>
    [StringLength(50)]
    public string? Type { get; set; }

    /// <summary>
    /// 设备IP地址
    /// </summary>
    [StringLength(45)]
    public string? IpAddress { get; set; }

    /// <summary>
    /// 设备端口
    /// </summary>
    public int? Port { get; set; }

    /// <summary>
    /// 协议类型
    /// </summary>
    [StringLength(50)]
    public string? Protocol { get; set; }

    /// <summary>
    /// 设备状态
    /// </summary>
    public DeviceStatus? Status { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool? IsEnabled { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    [StringLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// 网关ID
    /// </summary>
    public int? GatewayId { get; set; }

    /// <summary>
    /// 设备配置参数
    /// </summary>
    public Dictionary<string, string>? Parameters { get; set; }
}
