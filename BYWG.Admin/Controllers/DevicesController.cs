using Microsoft.AspNetCore.Mvc;
using BYWG.Admin.Services;
using BYWG.Shared.Models;
using Microsoft.AspNetCore.Authorization;

namespace BYWG.Admin.Controllers;

/// <summary>
/// 设备管理控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class DevicesController : ControllerBase
{
    private readonly IDeviceService _deviceService;
    private readonly ILogger<DevicesController> _logger;

    public DevicesController(IDeviceService deviceService, ILogger<DevicesController> logger)
    {
        _deviceService = deviceService;
        _logger = logger;
    }

    /// <summary>
    /// 获取所有设备
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Device>>> GetDevices()
    {
        try
        {
            var devices = await _deviceService.GetAllDevicesAsync();
            return Ok(devices);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取设备列表失败");
            return StatusCode(500, "获取设备列表失败");
        }
    }

    /// <summary>
    /// 根据ID获取设备
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Device>> GetDevice(int id)
    {
        try
        {
            var device = await _deviceService.GetDeviceByIdAsync(id);
            if (device == null)
            {
                return NotFound($"设备 {id} 不存在");
            }
            return Ok(device);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取设备 {DeviceId} 失败", id);
            return StatusCode(500, "获取设备失败");
        }
    }

    /// <summary>
    /// 创建设备
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")] // 只有管理员可以创建设备
    public async Task<ActionResult<Device>> CreateDevice([FromBody] Device device)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdDevice = await _deviceService.CreateDeviceAsync(device);
            return CreatedAtAction(nameof(GetDevice), new { id = createdDevice.Id }, createdDevice);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建设备失败");
            return StatusCode(500, "创建设备失败");
        }
    }

    /// <summary>
    /// 更新设备
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")] // 只有管理员可以更新设备
    public async Task<ActionResult<Device>> UpdateDevice(int id, [FromBody] Device device)
    {
        try
        {
            if (id != device.Id)
            {
                return BadRequest("设备ID不匹配");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updatedDevice = await _deviceService.UpdateDeviceAsync(device);
            return Ok(updatedDevice);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新设备 {DeviceId} 失败", id);
            return StatusCode(500, "更新设备失败");
        }
    }

    /// <summary>
    /// 删除设备
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")] // 只有管理员可以删除设备
    public async Task<ActionResult> DeleteDevice(int id)
    {
        try
        {
            var result = await _deviceService.DeleteDeviceAsync(id);
            if (!result)
            {
                return NotFound($"设备 {id} 不存在");
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除设备 {DeviceId} 失败", id);
            return StatusCode(500, "删除设备失败");
        }
    }

    /// <summary>
    /// 获取网关的设备
    /// </summary>
    [HttpGet("gateway/{gatewayId}")]
    public async Task<ActionResult<IEnumerable<Device>>> GetDevicesByGateway(int gatewayId)
    {
        try
        {
            var devices = await _deviceService.GetDevicesByGatewayIdAsync(gatewayId);
            return Ok(devices);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取网关 {GatewayId} 的设备失败", gatewayId);
            return StatusCode(500, "获取网关设备失败");
        }
    }

    /// <summary>
    /// 更新设备状态
    /// </summary>
    [HttpPatch("{id}/status")]
    public async Task<ActionResult> UpdateDeviceStatus(int id, [FromBody] UpdateDeviceStatusRequest request)
    {
        try
        {
            var result = await _deviceService.UpdateDeviceStatusAsync(id, request.Status);
            if (!result)
            {
                return NotFound($"设备 {id} 不存在");
            }
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新设备 {DeviceId} 状态失败", id);
            return StatusCode(500, "更新设备状态失败");
        }
    }

    /// <summary>
    /// 搜索设备
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<Device>>> SearchDevices([FromQuery] string q)
    {
        try
        {
            var devices = await _deviceService.SearchDevicesAsync(q);
            return Ok(devices);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "搜索设备失败: {SearchTerm}", q);
            return StatusCode(500, "搜索设备失败");
        }
    }
}

/// <summary>
/// 更新设备状态请求
/// </summary>
public class UpdateDeviceStatusRequest
{
    public DeviceStatus Status { get; set; }
}
