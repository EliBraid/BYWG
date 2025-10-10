using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BYWG.Admin.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace BYWG.Admin.Controllers;

[ApiController]
[Route("api/devices")] 
[Authorize]
public class DevicePointsController : ControllerBase
{
    private readonly AdminDbContext _context;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _gatewayApiBase;

    public DevicePointsController(AdminDbContext context, IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
        _gatewayApiBase = configuration["GATEWAY_API_URL"]
            ?? Environment.GetEnvironmentVariable("GATEWAY_API_URL")
            ?? "http://localhost:5080";
    }
    public record DevicePointDto(
        int Id,
        string Name,
        string Address,
        string DataType,
        string Access, // R/W
        int IntervalMs,
        bool Enabled,
        // Modbus 特有配置
        int? FunctionCode = null,
        string? AddressType = null,
        int? Quantity = null,
        int? SlaveId = null
    );

    /// <summary>
    /// 获取设备点位列表
    /// </summary>
    [HttpGet("{deviceId}/points")]
    [AllowAnonymous]
    public async Task<IActionResult> GetDevicePoints(int deviceId)
    {
        var points = await _context.DataPoints
            .Where(p => p.DeviceId == deviceId)
            .OrderBy(p => p.Id)
            .ToListAsync();

        var result = points.Select(p =>
        {
            string? access = null; int intervalMs = 1000;
            int? functionCode = null; string? addressType = null; int? quantity = null; int? slaveId = null;
            
            if (!string.IsNullOrWhiteSpace(p.Description))
            {
                try
                {
                    var meta = JsonSerializer.Deserialize<Dictionary<string, string>>(p.Description!);
                    if (meta != null)
                    {
                        if (meta.TryGetValue("access", out var acc)) access = acc;
                        if (meta.TryGetValue("intervalMs", out var intervalStr) && int.TryParse(intervalStr, out var iv)) intervalMs = iv;
                        
                        // 解析 Modbus 配置
                        if (meta.TryGetValue("functionCode", out var fc) && int.TryParse(fc, out var fcv)) functionCode = fcv;
                        if (meta.TryGetValue("addressType", out var at)) addressType = at;
                        if (meta.TryGetValue("quantity", out var q) && int.TryParse(q, out var qv)) quantity = qv;
                        if (meta.TryGetValue("slaveId", out var si) && int.TryParse(si, out var siv)) slaveId = siv;
                    }
                }
                catch { }
            }

            return new DevicePointDto(
                Id: p.Id,
                Name: p.Name,
                Address: p.Address,
                DataType: p.DataType,
                Access: access ?? "R",
                IntervalMs: intervalMs,
                Enabled: p.IsEnabled,
                FunctionCode: functionCode,
                AddressType: addressType,
                Quantity: quantity,
                SlaveId: slaveId
            );
        });

        return Ok(result);
    }

    /// <summary>
    /// 从协议模板导入点位
    /// </summary>
    [HttpPost("{deviceId}/points/import-template")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ImportFromTemplate(int deviceId, [FromBody] ImportTemplateRequest request)
    {
        try
        {
            // 验证设备是否存在
            var device = await _context.Devices.FindAsync(deviceId);
            if (device == null)
            {
                return NotFound(new { message = "设备不存在" });
            }

            // 获取协议模板
            var template = await _context.ProtocolConfigs.FindAsync(request.TemplateId);
            if (template == null)
            {
                return NotFound(new { message = "模板不存在" });
            }

            // 根据模板类型创建默认点位
            var pointsToCreate = new List<DataPoint>();
            
            if (template.ProtocolType.ToLower().Contains("modbus"))
            {
                // Modbus模板：创建常用的寄存器点位
                var modbusPoints = new[]
                {
                    new { Name = "温度传感器", Address = "40001", FunctionCode = 3, DataType = "FLOAT32" },
                    new { Name = "压力传感器", Address = "40003", FunctionCode = 3, DataType = "FLOAT32" },
                    new { Name = "流量计", Address = "40005", FunctionCode = 3, DataType = "FLOAT32" },
                    new { Name = "液位计", Address = "40007", FunctionCode = 3, DataType = "FLOAT32" },
                    new { Name = "运行状态", Address = "10001", FunctionCode = 1, DataType = "BOOL" },
                    new { Name = "报警状态", Address = "10002", FunctionCode = 1, DataType = "BOOL" },
                    new { Name = "手动模式", Address = "10003", FunctionCode = 1, DataType = "BOOL" }
                };

                foreach (var point in modbusPoints)
                {
                    var meta = new Dictionary<string, string>
                    {
                        ["access"] = "R",
                        ["intervalMs"] = "1000",
                        ["functionCode"] = point.FunctionCode.ToString(),
                        ["addressType"] = "dec",
                        ["quantity"] = "1",
                        ["slaveId"] = "1"
                    };

                    pointsToCreate.Add(new DataPoint
                    {
                        Name = point.Name,
                        Address = point.Address,
                        DataType = point.DataType,
                        Description = JsonSerializer.Serialize(meta),
                        IsEnabled = true,
                        DeviceId = deviceId,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                }
            }
            else
            {
                // 其他协议模板：创建通用点位
                var genericPoints = new[]
                {
                    new { Name = "传感器1", Address = "CH1", DataType = "FLOAT32" },
                    new { Name = "传感器2", Address = "CH2", DataType = "FLOAT32" },
                    new { Name = "传感器3", Address = "CH3", DataType = "FLOAT32" },
                    new { Name = "状态位1", Address = "STATUS1", DataType = "BOOL" },
                    new { Name = "状态位2", Address = "STATUS2", DataType = "BOOL" }
                };

                foreach (var point in genericPoints)
                {
                    var meta = new Dictionary<string, string>
                    {
                        ["access"] = "R",
                        ["intervalMs"] = "1000"
                    };

                    pointsToCreate.Add(new DataPoint
                    {
                        Name = point.Name,
                        Address = point.Address,
                        DataType = point.DataType,
                        Description = JsonSerializer.Serialize(meta),
                        IsEnabled = true,
                        DeviceId = deviceId,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                }
            }

            // 批量添加点位
            _context.DataPoints.AddRange(pointsToCreate);
            await _context.SaveChangesAsync();

            return Ok(new { 
                message = $"成功导入 {pointsToCreate.Count} 个点位",
                importedCount = pointsToCreate.Count
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"导入失败：{ex.Message}" });
        }
    }

    public record ImportTemplateRequest(int TemplateId);

    public record CreatePointRequest(
        string Name,
        string Address,
        string DataType,
        string? Unit,
        string Access,
        int IntervalMs,
        bool Enabled,
        // Modbus 特有配置
        int? FunctionCode = null,
        string? AddressType = null,
        int? Quantity = null,
        int? ReadSpeed = null,
        int? SlaveId = null
    );

    public record UpdatePointRequest(
        string? Name,
        string? Address,
        string? DataType,
        string? Unit,
        string? Access,
        int? IntervalMs,
        bool? Enabled,
        // Modbus 特有配置
        int? FunctionCode = null,
        string? AddressType = null,
        int? Quantity = null,
        int? ReadSpeed = null,
        int? SlaveId = null
    );

    /// <summary>
    /// 新增点位
    /// </summary>
    [HttpPost("{deviceId}/points")]
    [Authorize(Roles = "Admin,admin")]
    public async Task<IActionResult> CreatePoint(int deviceId, [FromBody] CreatePointRequest request)
    {
        // 验证设备是否存在
        var device = await _context.Devices.FindAsync(deviceId);
        if (device == null)
        {
            return NotFound(new { message = "设备不存在" });
        }

        // 验证Modbus地址范围
        if (device.Protocol?.ToLower().Contains("modbus") == true)
        {
            if (!ValidateModbusAddress(request.Address, request.FunctionCode ?? 3))
            {
                return BadRequest(new { message = $"Modbus地址 {request.Address} 超出有效范围" });
            }
        }

        // 检查地址是否已存在
        var existingPoint = await _context.DataPoints
            .FirstOrDefaultAsync(p => p.DeviceId == deviceId && p.Address == request.Address);
        if (existingPoint != null)
        {
            return BadRequest(new { message = $"地址 {request.Address} 已存在" });
        }

        // 检查名称是否已存在
        var existingName = await _context.DataPoints
            .FirstOrDefaultAsync(p => p.DeviceId == deviceId && p.Name == request.Name);
        if (existingName != null)
        {
            return BadRequest(new { message = $"名称 {request.Name} 已存在" });
        }

        var meta = new Dictionary<string, string>();
        meta["access"] = request.Access;
        meta["intervalMs"] = request.IntervalMs.ToString();
        
        // 保存 Modbus 特有配置
        if (request.FunctionCode.HasValue) meta["functionCode"] = request.FunctionCode.Value.ToString();
        if (!string.IsNullOrWhiteSpace(request.AddressType)) meta["addressType"] = request.AddressType!;
        if (request.Quantity.HasValue) meta["quantity"] = request.Quantity.Value.ToString();
        if (request.SlaveId.HasValue) meta["slaveId"] = request.SlaveId.Value.ToString();

        var entity = new DataPoint
        {
            Name = request.Name,
            Address = request.Address,
            DataType = request.DataType,
            Description = meta.Count > 0 ? JsonSerializer.Serialize(meta) : null,
            IsEnabled = request.Enabled,
            DeviceId = deviceId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.DataPoints.Add(entity);
        await _context.SaveChangesAsync();

        // 通知Gateway重新加载配置
        await NotifyGatewayReloadConfig(deviceId);

        var created = new DevicePointDto(
            Id: entity.Id,
            Name: entity.Name,
            Address: entity.Address,
            DataType: entity.DataType,
            Access: request.Access,
            IntervalMs: request.IntervalMs,
            Enabled: entity.IsEnabled,
            FunctionCode: request.FunctionCode,
            AddressType: request.AddressType,
            Quantity: request.Quantity,
            SlaveId: request.SlaveId
        );
        return Ok(created);
    }

    /// <summary>
    /// 更新点位
    /// </summary>
    [HttpPut("{deviceId}/points/{pointId}")]
    [Authorize(Roles = "Admin,admin")]
    public async Task<IActionResult> UpdatePoint(int deviceId, int pointId, [FromBody] UpdatePointRequest request)
    {
        var entity = await _context.DataPoints.FirstOrDefaultAsync(p => p.Id == pointId && p.DeviceId == deviceId);
        if (entity == null) return NotFound(new { message = "点位不存在" });

        // 如果要更新地址，检查新地址是否已存在
        if (request.Address != null && request.Address != entity.Address)
        {
            // 验证Modbus地址范围
            var device = await _context.Devices.FindAsync(deviceId);
            if (device?.Protocol?.ToLower().Contains("modbus") == true)
            {
                if (!ValidateModbusAddress(request.Address, request.FunctionCode ?? 3))
                {
                    return BadRequest(new { message = $"Modbus地址 {request.Address} 超出有效范围" });
                }
            }

            var existingAddress = await _context.DataPoints
                .FirstOrDefaultAsync(p => p.DeviceId == deviceId && p.Address == request.Address && p.Id != pointId);
            if (existingAddress != null)
            {
                return BadRequest(new { message = $"地址 {request.Address} 已存在" });
            }
        }

        // 如果要更新名称，检查新名称是否已存在
        if (request.Name != null && request.Name != entity.Name)
        {
            var existingName = await _context.DataPoints
                .FirstOrDefaultAsync(p => p.DeviceId == deviceId && p.Name == request.Name && p.Id != pointId);
            if (existingName != null)
            {
                return BadRequest(new { message = $"名称 {request.Name} 已存在" });
            }
        }

        entity.Name = request.Name ?? entity.Name;
        entity.Address = request.Address ?? entity.Address;
        entity.DataType = request.DataType ?? entity.DataType;
        entity.IsEnabled = request.Enabled ?? entity.IsEnabled;

        // 合并/更新 meta
        var meta = new Dictionary<string, string>();
        if (!string.IsNullOrWhiteSpace(entity.Description))
        {
            try { meta = JsonSerializer.Deserialize<Dictionary<string, string>>(entity.Description!) ?? new(); } catch { meta = new(); }
        }
        if (request.Access != null) meta["access"] = request.Access;
        if (request.IntervalMs.HasValue) meta["intervalMs"] = request.IntervalMs.Value.ToString();
        
        // 更新 Modbus 特有配置
        if (request.FunctionCode.HasValue) meta["functionCode"] = request.FunctionCode.Value.ToString();
        if (request.AddressType != null) meta["addressType"] = request.AddressType;
        if (request.Quantity.HasValue) meta["quantity"] = request.Quantity.Value.ToString();
        if (request.SlaveId.HasValue) meta["slaveId"] = request.SlaveId.Value.ToString();
        entity.Description = meta.Count > 0 ? JsonSerializer.Serialize(meta) : null;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // 通知Gateway重新加载配置
        await NotifyGatewayReloadConfig(deviceId);

        var result = new DevicePointDto(
            Id: entity.Id,
            Name: entity.Name,
            Address: entity.Address,
            DataType: entity.DataType,
            Access: meta.TryGetValue("access", out var a) ? a : "R",
            IntervalMs: meta.TryGetValue("intervalMs", out var iv) && int.TryParse(iv, out var ivv) ? ivv : 1000,
            Enabled: entity.IsEnabled,
            FunctionCode: meta.TryGetValue("functionCode", out var fc) && int.TryParse(fc, out var fcv) ? fcv : null,
            AddressType: meta.TryGetValue("addressType", out var at) ? at : null,
            Quantity: meta.TryGetValue("quantity", out var q) && int.TryParse(q, out var qv) ? qv : null,
            SlaveId: meta.TryGetValue("slaveId", out var si) && int.TryParse(si, out var siv) ? siv : null
        );
        return Ok(result);
    }

    /// <summary>
    /// 删除点位
    /// </summary>
    [HttpDelete("{deviceId}/points/{pointId}")]
    [Authorize(Roles = "Admin,admin")]
    public async Task<IActionResult> DeletePoint(int deviceId, int pointId)
    {
        try
        {
            var entity = await _context.DataPoints.FirstOrDefaultAsync(p => p.Id == pointId && p.DeviceId == deviceId);
            if (entity == null) return NotFound(new { message = "点位不存在" });
            
            _context.DataPoints.Remove(entity);
            await _context.SaveChangesAsync();
            
            // 通知Gateway重新加载配置
            await NotifyGatewayReloadConfig(deviceId);
            
            return Ok(new { message = "删除成功", pointId });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"删除失败：{ex.Message}" });
        }
    }

    /// <summary>
    /// 批量删除点位
    /// </summary>
    [HttpPost("{deviceId}/points/batch-delete")]
    [Authorize(Roles = "Admin,admin")]
    public async Task<IActionResult> BatchDeletePoints(int deviceId, [FromBody] BatchDeleteRequest request)
    {
        try
        {
            var entities = await _context.DataPoints
                .Where(p => p.DeviceId == deviceId && request.PointIds.Contains(p.Id))
                .ToListAsync();

            if (!entities.Any())
            {
                return NotFound(new { message = "未找到要删除的点位" });
            }

            _context.DataPoints.RemoveRange(entities);
            await _context.SaveChangesAsync();

            // 通知Gateway重新加载配置
            await NotifyGatewayReloadConfig(deviceId);

            return Ok(new { 
                message = $"成功删除 {entities.Count} 个点位",
                deletedCount = entities.Count
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"批量删除失败：{ex.Message}" });
        }
    }

    /// <summary>
    /// 批量启用/禁用点位
    /// </summary>
    [HttpPatch("{deviceId}/points/batch")]
    [Authorize(Roles = "Admin,admin")]
    public async Task<IActionResult> BatchUpdatePointsStatus(int deviceId, [FromBody] BatchUpdateStatusRequest request)
    {
        try
        {
            var entities = await _context.DataPoints
                .Where(p => p.DeviceId == deviceId && request.PointIds.Contains(p.Id))
                .ToListAsync();

            if (!entities.Any())
            {
                return NotFound(new { message = "未找到要更新的点位" });
            }

            foreach (var entity in entities)
            {
                entity.IsEnabled = request.Enabled;
                entity.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            // 通知Gateway重新加载配置
            await NotifyGatewayReloadConfig(deviceId);

            return Ok(new { 
                message = $"成功更新 {entities.Count} 个点位状态",
                updatedCount = entities.Count,
                enabled = request.Enabled
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"批量更新失败：{ex.Message}" });
        }
    }

    /// <summary>
    /// 获取点位统计信息
    /// </summary>
    [HttpGet("{deviceId}/points/stats")]
    public async Task<IActionResult> GetPointStats(int deviceId)
    {
        try
        {
            var stats = await _context.DataPoints
                .Where(p => p.DeviceId == deviceId)
                .GroupBy(p => p.IsEnabled)
                .Select(g => new { Enabled = g.Key, Count = g.Count() })
                .ToListAsync();

            var total = stats.Sum(s => s.Count);
            var enabled = stats.FirstOrDefault(s => s.Enabled)?.Count ?? 0;
            var disabled = total - enabled;

            return Ok(new
            {
                total,
                enabled,
                disabled,
                enabledPercentage = total > 0 ? Math.Round((double)enabled / total * 100, 2) : 0
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"获取统计信息失败：{ex.Message}" });
        }
    }

    /// <summary>
    /// 通知Gateway重新加载配置
    /// </summary>
    private async Task NotifyGatewayReloadConfig(int deviceId)
    {
        try
        {
            var url = $"{_gatewayApiBase}/api/management/sync";
            
            using var httpClient = _httpClientFactory.CreateClient();
            httpClient.Timeout = TimeSpan.FromSeconds(5);
            
            var response = await httpClient.PostAsync(url, null);
            
            if (response.IsSuccessStatusCode)
            {
                // 记录成功日志
                Console.WriteLine($"成功通知Gateway重新加载配置，设备ID: {deviceId}");
            }
            else
            {
                Console.WriteLine($"通知Gateway重新加载配置失败，设备ID: {deviceId}, 状态码: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"通知Gateway重新加载配置异常，设备ID: {deviceId}, 错误: {ex.Message}");
        }
    }

    public record BatchDeleteRequest(int[] PointIds);
    public record BatchUpdateStatusRequest(int[] PointIds, bool Enabled);

    /// <summary>
    /// 验证Modbus地址范围
    /// </summary>
    private bool ValidateModbusAddress(string address, int functionCode)
    {
        if (!int.TryParse(address, out int addr))
        {
            return false;
        }

        return functionCode switch
        {
            1 => addr >= 0 && addr <= 65535,      // 线圈状态 (0x)
            2 => addr >= 0 && addr <= 65535,      // 输入状态 (1x)
            3 => addr >= 0 && addr <= 65535,      // 保持寄存器 (4x)
            4 => addr >= 0 && addr <= 65535,      // 输入寄存器 (3x)
            5 => addr >= 0 && addr <= 65535,      // 写单个线圈 (0x)
            6 => addr >= 0 && addr <= 65535,      // 写单个寄存器 (4x)
            15 => addr >= 0 && addr <= 65535,     // 写多个线圈 (0x)
            16 => addr >= 0 && addr <= 65535,     // 写多个寄存器 (4x)
            _ => false
        };
    }
}


