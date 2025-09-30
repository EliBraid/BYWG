using BYWG.Admin.Data;
using BYWG.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace BYWG.Admin.Services;

/// <summary>
/// 网关服务实现
/// </summary>
public class GatewayService : IGatewayService
{
    private readonly AdminDbContext _context;
    private readonly ILogger<GatewayService> _logger;

    public GatewayService(AdminDbContext context, ILogger<GatewayService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<Gateway>> GetAllGatewaysAsync()
    {
        try
        {
            return await _context.Gateways
                .Include(g => g.Devices)
                .OrderBy(g => g.Name)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取所有网关失败");
            throw;
        }
    }

    public async Task<Gateway?> GetGatewayByIdAsync(int id)
    {
        try
        {
            return await _context.Gateways
                .Include(g => g.Devices)
                .FirstOrDefaultAsync(g => g.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取网关 {GatewayId} 失败", id);
            throw;
        }
    }

    public async Task<Gateway> CreateGatewayAsync(Gateway gateway)
    {
        try
        {
            gateway.CreatedAt = DateTime.UtcNow;
            gateway.UpdatedAt = DateTime.UtcNow;

            _context.Gateways.Add(gateway);
            await _context.SaveChangesAsync();

            _logger.LogInformation("创建网关成功: {GatewayName}", gateway.Name);
            return gateway;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建网关失败: {GatewayName}", gateway.Name);
            throw;
        }
    }

    public async Task<Gateway> UpdateGatewayAsync(Gateway gateway)
    {
        try
        {
            var existingGateway = await _context.Gateways.FindAsync(gateway.Id);
            if (existingGateway == null)
            {
                throw new ArgumentException($"网关 {gateway.Id} 不存在");
            }

            existingGateway.Name = gateway.Name;
            existingGateway.IpAddress = gateway.IpAddress;
            existingGateway.Port = gateway.Port;
            existingGateway.Status = gateway.Status;
            existingGateway.IsEnabled = gateway.IsEnabled;
            existingGateway.Description = gateway.Description;
            existingGateway.Version = gateway.Version;
            existingGateway.SystemInfo = gateway.SystemInfo;
            existingGateway.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("更新网关成功: {GatewayName}", gateway.Name);
            return existingGateway;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新网关失败: {GatewayId}", gateway.Id);
            throw;
        }
    }

    public async Task<bool> DeleteGatewayAsync(int id)
    {
        try
        {
            var gateway = await _context.Gateways.FindAsync(id);
            if (gateway == null)
            {
                return false;
            }

            _context.Gateways.Remove(gateway);
            await _context.SaveChangesAsync();

            _logger.LogInformation("删除网关成功: {GatewayName}", gateway.Name);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除网关失败: {GatewayId}", id);
            throw;
        }
    }

    public async Task<Gateway> RegisterGatewayAsync(GatewayRegistrationRequest request)
    {
        try
        {
            // 检查是否已存在
            var existingGateway = await _context.Gateways
                .FirstOrDefaultAsync(g => g.Name == request.Name || g.IpAddress == request.IpAddress);

            if (existingGateway != null)
            {
                // 更新现有网关
                existingGateway.IpAddress = request.IpAddress;
                existingGateway.Port = request.Port;
                existingGateway.Version = request.Version;
                existingGateway.SystemInfo = request.SystemInfo;
                existingGateway.Status = GatewayStatus.Online;
                existingGateway.LastHeartbeat = DateTime.UtcNow;
                existingGateway.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                _logger.LogInformation("更新网关注册成功: {GatewayName}", request.Name);
                return existingGateway;
            }
            else
            {
                // 创建新网关
                var gateway = new Gateway
                {
                    Name = request.Name,
                    IpAddress = request.IpAddress,
                    Port = request.Port,
                    Version = request.Version,
                    SystemInfo = request.SystemInfo,
                    Status = GatewayStatus.Online,
                    LastHeartbeat = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Gateways.Add(gateway);
                await _context.SaveChangesAsync();

                _logger.LogInformation("注册新网关成功: {GatewayName}", request.Name);
                return gateway;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "注册网关失败: {GatewayName}", request.Name);
            throw;
        }
    }

    public async Task<bool> UpdateGatewayHeartbeatAsync(string gatewayId, GatewayHeartbeatRequest request)
    {
        try
        {
            var gateway = await _context.Gateways
                .FirstOrDefaultAsync(g => g.Name == gatewayId || g.IpAddress == gatewayId);

            if (gateway == null)
            {
                _logger.LogWarning("网关 {GatewayId} 不存在", gatewayId);
                return false;
            }

            gateway.LastHeartbeat = DateTime.UtcNow;
            gateway.Status = Enum.Parse<GatewayStatus>(request.Status);
            gateway.SystemInfo = request.SystemInfo;
            gateway.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogDebug("更新网关心跳成功: {GatewayName}", gateway.Name);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新网关心跳失败: {GatewayId}", gatewayId);
            throw;
        }
    }

    public async Task<bool> UpdateGatewayStatusAsync(int gatewayId, GatewayStatus status)
    {
        try
        {
            var gateway = await _context.Gateways.FindAsync(gatewayId);
            if (gateway == null)
            {
                return false;
            }

            gateway.Status = status;
            gateway.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("更新网关状态成功: {GatewayId} -> {Status}", gatewayId, status);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新网关状态失败: {GatewayId}", gatewayId);
            throw;
        }
    }

    public async Task<IEnumerable<Gateway>> SearchGatewaysAsync(string searchTerm)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return await GetAllGatewaysAsync();
            }

            return await _context.Gateways
                .Include(g => g.Devices)
                .Where(g => g.Name.Contains(searchTerm) ||
                           g.IpAddress.Contains(searchTerm) ||
                           (g.Description != null && g.Description.Contains(searchTerm)) ||
                           (g.Version != null && g.Version.Contains(searchTerm)))
                .OrderBy(g => g.Name)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "搜索网关失败: {SearchTerm}", searchTerm);
            throw;
        }
    }
}
