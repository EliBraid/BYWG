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
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _gatewayApiBase;

    public DevicesController(IDeviceService deviceService, ILogger<DevicesController> logger, IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _deviceService = deviceService;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _gatewayApiBase = configuration["GATEWAY_API_URL"]
            ?? Environment.GetEnvironmentVariable("GATEWAY_API_URL")
            ?? "http://localhost:5080";
    }

    /// <summary>
    /// 测试设备连接（基础连通性）
    /// </summary>
    public class TestConnectionRequest
    {
        public string IpAddress { get; set; } = string.Empty;
        public int Port { get; set; }
        public string Protocol { get; set; } = ""; // ModbusTCP/OPCUA/MQTT/S7
        public int TimeoutMs { get; set; } = 3000;
    }

    public class TestConnectionResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public long? LatencyMs { get; set; }
    }

    [HttpPost("test-connection")]
    [Authorize(Roles = "Admin,admin")]
    public async Task<ActionResult<TestConnectionResponse>> TestConnection([FromBody] TestConnectionRequest request)
    {
        try
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            using var cts = new CancellationTokenSource(request.TimeoutMs > 0 ? request.TimeoutMs : 3000);

            // 基础TCP连通性测试（ModbusTCP/S7/大多数基于TCP的协议）
            using var client = new System.Net.Sockets.TcpClient();
            var connectTask = client.ConnectAsync(request.IpAddress, request.Port);
            var completed = await Task.WhenAny(connectTask, Task.Delay(request.TimeoutMs, cts.Token));
            if (completed != connectTask || !client.Connected)
            {
                return Ok(new TestConnectionResponse
                {
                    Success = false,
                    Message = "连接超时或失败"
                });
            }
            sw.Stop();
            return Ok(new TestConnectionResponse
            {
                Success = true,
                Message = "连接成功",
                LatencyMs = sw.ElapsedMilliseconds
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "测试连接失败 {Ip}:{Port}", request.IpAddress, request.Port);
            return Ok(new TestConnectionResponse
            {
                Success = false,
                Message = ex.Message
            });
        }
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
    [AllowAnonymous] // 开发环境允许匿名访问，生产环境需要认证
    public async Task<ActionResult<Device>> CreateDevice([FromBody] Device device)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdDevice = await _deviceService.CreateDeviceAsync(device);
            
            // 通知Gateway重新加载配置（新设备需要创建协议）
            await NotifyGatewayReloadConfig(createdDevice.Id);
            
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
    [AllowAnonymous] // 开发环境允许匿名访问，生产环境需要认证
    public async Task<ActionResult<Device>> UpdateDevice(int id, [FromBody] UpdateDeviceRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // 获取现有设备
            var existingDevice = await _deviceService.GetDeviceByIdAsync(id);
            if (existingDevice == null)
            {
                return NotFound($"设备 {id} 不存在");
            }

            // 更新设备属性（只更新提供的字段）
            if (!string.IsNullOrEmpty(request.Name))
                existingDevice.Name = request.Name;
            
            if (!string.IsNullOrEmpty(request.Type))
                existingDevice.Type = request.Type;
            
            if (!string.IsNullOrEmpty(request.IpAddress))
                existingDevice.IpAddress = request.IpAddress;
            
            if (request.Port.HasValue)
                existingDevice.Port = request.Port.Value;
            
            if (!string.IsNullOrEmpty(request.Protocol))
                existingDevice.Protocol = request.Protocol;
            
            if (request.Status.HasValue)
                existingDevice.Status = request.Status.Value;
            
            if (request.IsEnabled.HasValue)
                existingDevice.IsEnabled = request.IsEnabled.Value;
            
            if (request.Description != null)
                existingDevice.Description = request.Description;
            
            if (request.GatewayId.HasValue)
                existingDevice.GatewayId = request.GatewayId.Value;
            
            if (request.Parameters != null)
                existingDevice.Parameters = request.Parameters;

            // 更新时间戳
            existingDevice.UpdatedAt = DateTime.UtcNow;

            var updatedDevice = await _deviceService.UpdateDeviceAsync(existingDevice);
            
            // 通知Gateway重新加载配置（设备配置变更需要更新协议）
            await NotifyGatewayReloadConfig(id);
            
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
    [Authorize(Roles = "Admin,admin")] // 只有管理员可以删除设备
    public async Task<ActionResult> DeleteDevice(int id)
    {
        try
        {
            var result = await _deviceService.DeleteDeviceAsync(id);
            if (!result)
            {
                return NotFound($"设备 {id} 不存在");
            }

            // 通知Gateway重新加载配置（清理已删除设备的协议和数据）
            // 注意：只在删除设备时通知，避免频繁重载
            await NotifyGatewayReloadConfig(id);

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


    /// <summary>
    /// 开发环境：快速更新设备IP地址（无需认证）
    /// </summary>
    [HttpPost("dev/update-ip")]
    [AllowAnonymous]
    public async Task<ActionResult> UpdateDeviceIpForDev([FromBody] UpdateDeviceIpRequest request)
    {
        try
        {
            var device = await _deviceService.GetDeviceByIdAsync(request.DeviceId);
            if (device == null)
            {
                return NotFound($"设备 {request.DeviceId} 不存在");
            }

            device.IpAddress = request.IpAddress;
            device.Port = request.Port;
            device.UpdatedAt = DateTime.UtcNow;

            await _deviceService.UpdateDeviceAsync(device);
            
            _logger.LogInformation("开发环境：设备 {DeviceId} IP已更新为 {IpAddress}:{Port}", 
                request.DeviceId, request.IpAddress, request.Port);
            
            return Ok(new { message = "设备IP更新成功", deviceId = request.DeviceId, ipAddress = request.IpAddress, port = request.Port });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新设备IP失败");
            return StatusCode(500, "更新设备IP失败");
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
            httpClient.Timeout = TimeSpan.FromSeconds(10); // 增加超时时间
            
            // 发送同步请求，包含设备ID信息
            var requestData = new { deviceId = deviceId, timestamp = DateTime.UtcNow };
            var json = System.Text.Json.JsonSerializer.Serialize(requestData);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            
            var response = await httpClient.PostAsync(url, content);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("成功通知Gateway重新加载配置，设备ID: {DeviceId}", deviceId);
            }
            else
            {
                _logger.LogWarning("通知Gateway重新加载配置失败，设备ID: {DeviceId}, 状态码: {StatusCode}", deviceId, response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "通知Gateway重新加载配置异常，设备ID: {DeviceId}", deviceId);
        }
    }
}

/// <summary>
/// 更新设备IP请求
/// </summary>
public class UpdateDeviceIpRequest
{
    public int DeviceId { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public int Port { get; set; }
}

/// <summary>
/// 更新设备状态请求
/// </summary>
public class UpdateDeviceStatusRequest
{
    public DeviceStatus Status { get; set; }
}
