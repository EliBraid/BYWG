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

    public DevicePointsController(AdminDbContext context)
    {
        _context = context;
    }
    public record DevicePointDto(
        int Id,
        string Name,
        string Address,
        string DataType,
        string? Unit,
        string Access, // R/W
        int IntervalMs,
        bool Enabled
    );

    /// <summary>
    /// 获取设备点位列表（占位实现，待接入实际存储）
    /// </summary>
    [HttpGet("{deviceId}/points")]
    public async Task<IActionResult> GetDevicePoints(int deviceId)
    {
        var points = await _context.DataPoints
            .Where(p => p.DeviceId == deviceId)
            .OrderBy(p => p.Id)
            .ToListAsync();

        var result = points.Select(p =>
        {
            string? unit = null; string? access = null; int intervalMs = 1000;
            if (!string.IsNullOrWhiteSpace(p.Description))
            {
                try
                {
                    var meta = JsonSerializer.Deserialize<Dictionary<string, string>>(p.Description!);
                    if (meta != null)
                    {
                        meta.TryGetValue("unit", out unit);
                        if (meta.TryGetValue("access", out var acc)) access = acc;
                        if (meta.TryGetValue("intervalMs", out var intervalStr) && int.TryParse(intervalStr, out var iv)) intervalMs = iv;
                    }
                }
                catch { }
            }

            return new DevicePointDto(
                Id: p.Id,
                Name: p.Name,
                Address: p.Address,
                DataType: p.DataType,
                Unit: unit,
                Access: access ?? "R",
                IntervalMs: intervalMs,
                Enabled: p.IsEnabled
            );
        });

        return Ok(result);
    }

    /// <summary>
    /// 从协议模板导入点位（占位实现）
    /// </summary>
    [HttpPost("{deviceId}/points/import-template")]
    [Authorize(Roles = "Admin")]
    public IActionResult ImportFromTemplate(int deviceId, [FromBody] object request)
    {
        // TODO: 根据模板ID导入到设备点位表
        return Ok(new { message = "import scheduled" });
    }

    public record CreatePointRequest(
        string Name,
        string Address,
        string DataType,
        string? Unit,
        string Access,
        int IntervalMs,
        bool Enabled
    );

    public record UpdatePointRequest(
        string? Name,
        string? Address,
        string? DataType,
        string? Unit,
        string? Access,
        int? IntervalMs,
        bool? Enabled
    );

    /// <summary>
    /// 新增点位（占位实现）
    /// </summary>
    [HttpPost("{deviceId}/points")]
    [Authorize(Roles = "Admin,admin")]
    public async Task<IActionResult> CreatePoint(int deviceId, [FromBody] CreatePointRequest request)
    {
        var meta = new Dictionary<string, string>();
        if (!string.IsNullOrWhiteSpace(request.Unit)) meta["unit"] = request.Unit!;
        meta["access"] = request.Access;
        meta["intervalMs"] = request.IntervalMs.ToString();

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

        var created = new DevicePointDto(
            Id: entity.Id,
            Name: entity.Name,
            Address: entity.Address,
            DataType: entity.DataType,
            Unit: request.Unit,
            Access: request.Access,
            IntervalMs: request.IntervalMs,
            Enabled: entity.IsEnabled
        );
        return Ok(created);
    }

    /// <summary>
    /// 更新点位（占位实现）
    /// </summary>
    [HttpPut("{deviceId}/points/{pointId}")]
    [Authorize(Roles = "Admin,admin")]
    public async Task<IActionResult> UpdatePoint(int deviceId, int pointId, [FromBody] UpdatePointRequest request)
    {
        var entity = await _context.DataPoints.FirstOrDefaultAsync(p => p.Id == pointId && p.DeviceId == deviceId);
        if (entity == null) return NotFound(new { message = "点位不存在" });

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
        if (request.Unit != null) meta["unit"] = request.Unit;
        if (request.Access != null) meta["access"] = request.Access;
        if (request.IntervalMs.HasValue) meta["intervalMs"] = request.IntervalMs.Value.ToString();
        entity.Description = meta.Count > 0 ? JsonSerializer.Serialize(meta) : null;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var result = new DevicePointDto(
            Id: entity.Id,
            Name: entity.Name,
            Address: entity.Address,
            DataType: entity.DataType,
            Unit: meta.TryGetValue("unit", out var u) ? u : null,
            Access: meta.TryGetValue("access", out var a) ? a : "R",
            IntervalMs: meta.TryGetValue("intervalMs", out var iv) && int.TryParse(iv, out var ivv) ? ivv : 1000,
            Enabled: entity.IsEnabled
        );
        return Ok(result);
    }

    /// <summary>
    /// 删除点位（占位实现）
    /// </summary>
    [HttpDelete("{deviceId}/points/{pointId}")]
    [Authorize(Roles = "Admin,admin")]
    public async Task<IActionResult> DeletePoint(int deviceId, int pointId)
    {
        var entity = await _context.DataPoints.FirstOrDefaultAsync(p => p.Id == pointId && p.DeviceId == deviceId);
        if (entity == null) return NotFound(new { message = "点位不存在" });
        _context.DataPoints.Remove(entity);
        await _context.SaveChangesAsync();
        return Ok(new { message = "deleted", pointId });
    }
}


