using BYWG.Shared.Models;

namespace BYWG.Admin.Services;

/// <summary>
/// 设备服务接口
/// </summary>
public interface IDeviceService
{
    /// <summary>
    /// 获取所有设备
    /// </summary>
    Task<IEnumerable<Device>> GetAllDevicesAsync();

    /// <summary>
    /// 根据ID获取设备
    /// </summary>
    Task<Device?> GetDeviceByIdAsync(int id);

    /// <summary>
    /// 创建设备
    /// </summary>
    Task<Device> CreateDeviceAsync(Device device);

    /// <summary>
    /// 更新设备
    /// </summary>
    Task<Device> UpdateDeviceAsync(Device device);

    /// <summary>
    /// 删除设备
    /// </summary>
    Task<bool> DeleteDeviceAsync(int id);

    /// <summary>
    /// 获取网关的设备
    /// </summary>
    Task<IEnumerable<Device>> GetDevicesByGatewayIdAsync(int gatewayId);

    /// <summary>
    /// 更新设备状态
    /// </summary>
    Task<bool> UpdateDeviceStatusAsync(int deviceId, DeviceStatus status);

    /// <summary>
    /// 搜索设备
    /// </summary>
    Task<IEnumerable<Device>> SearchDevicesAsync(string searchTerm);
}
