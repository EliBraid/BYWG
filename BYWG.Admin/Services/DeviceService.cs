using BYWG.Admin.Data;
using BYWG.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace BYWG.Admin.Services;

/// <summary>
/// 设备服务实现
/// </summary>
public class DeviceService : IDeviceService
{
    private readonly AdminDbContext _context;
    private readonly ILogger<DeviceService> _logger;

    public DeviceService(AdminDbContext context, ILogger<DeviceService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<Device>> GetAllDevicesAsync()
    {
        try
        {
            return await _context.Devices
                .Include(d => d.Gateway)
                .OrderBy(d => d.Name)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取所有设备失败");
            throw;
        }
    }

    public async Task<Device?> GetDeviceByIdAsync(int id)
    {
        try
        {
            return await _context.Devices
                .Include(d => d.Gateway)
                .FirstOrDefaultAsync(d => d.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取设备 {DeviceId} 失败", id);
            throw;
        }
    }

    public async Task<Device> CreateDeviceAsync(Device device)
    {
        try
        {
            device.CreatedAt = DateTime.UtcNow;
            device.UpdatedAt = DateTime.UtcNow;

            _context.Devices.Add(device);
            await _context.SaveChangesAsync();

            _logger.LogInformation("创建设备成功: {DeviceName}", device.Name);
            return device;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建设备失败: {DeviceName}", device.Name);
            throw;
        }
    }

    public async Task<Device> UpdateDeviceAsync(Device device)
    {
        try
        {
            var existingDevice = await _context.Devices.FindAsync(device.Id);
            if (existingDevice == null)
            {
                throw new ArgumentException($"设备 {device.Id} 不存在");
            }

            existingDevice.Name = device.Name;
            existingDevice.Type = device.Type;
            existingDevice.IpAddress = device.IpAddress;
            existingDevice.Port = device.Port;
            existingDevice.Protocol = device.Protocol;
            existingDevice.Status = device.Status;
            existingDevice.IsEnabled = device.IsEnabled;
            existingDevice.Description = device.Description;
            existingDevice.GatewayId = device.GatewayId;
            existingDevice.Parameters = device.Parameters;
            existingDevice.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("更新设备成功: {DeviceName}", device.Name);
            return existingDevice;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新设备失败: {DeviceId}", device.Id);
            throw;
        }
    }

    public async Task<bool> DeleteDeviceAsync(int id)
    {
        try
        {
            var device = await _context.Devices.FindAsync(id);
            if (device == null)
            {
                return false;
            }

            _context.Devices.Remove(device);
            await _context.SaveChangesAsync();

            _logger.LogInformation("删除设备成功: {DeviceName}", device.Name);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除设备失败: {DeviceId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<Device>> GetDevicesByGatewayIdAsync(int gatewayId)
    {
        try
        {
            return await _context.Devices
                .Where(d => d.GatewayId == gatewayId)
                .Include(d => d.Gateway)
                .OrderBy(d => d.Name)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取网关 {GatewayId} 的设备失败", gatewayId);
            throw;
        }
    }

    public async Task<bool> UpdateDeviceStatusAsync(int deviceId, DeviceStatus status)
    {
        try
        {
            var device = await _context.Devices.FindAsync(deviceId);
            if (device == null)
            {
                return false;
            }

            device.Status = status;
            device.UpdatedAt = DateTime.UtcNow;
            if (status == DeviceStatus.Online)
            {
                device.LastConnectedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("更新设备状态成功: {DeviceId} -> {Status}", deviceId, status);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新设备状态失败: {DeviceId}", deviceId);
            throw;
        }
    }

    public async Task<IEnumerable<Device>> SearchDevicesAsync(string searchTerm)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return await GetAllDevicesAsync();
            }

            return await _context.Devices
                .Include(d => d.Gateway)
                .Where(d => d.Name.Contains(searchTerm) ||
                           d.Type.Contains(searchTerm) ||
                           d.IpAddress.Contains(searchTerm) ||
                           d.Protocol.Contains(searchTerm) ||
                           (d.Description != null && d.Description.Contains(searchTerm)))
                .OrderBy(d => d.Name)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "搜索设备失败: {SearchTerm}", searchTerm);
            throw;
        }
    }
}
