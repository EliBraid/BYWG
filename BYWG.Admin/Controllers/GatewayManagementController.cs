using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http;
using System.Text.Json;
using BYWG.Admin.Services;

namespace BYWG.Admin.Controllers;

[ApiController]
[Route("api/gateways")]
[Authorize]
public class GatewayManagementController : ControllerBase
{
    private readonly IDeviceService _deviceService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<GatewayManagementController> _logger;

    public GatewayManagementController(IDeviceService deviceService, IHttpClientFactory httpClientFactory, ILogger<GatewayManagementController> logger)
    {
        _deviceService = deviceService;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <summary>
    /// 获取网关聚合配置（设备清单，可扩展协议与点位）
    /// </summary>
    [HttpGet("{gatewayId}/config")]
    public async Task<IActionResult> GetGatewayConfig(int gatewayId)
    {
        try
        {
            var devices = await _deviceService.GetDevicesByGatewayIdAsync(gatewayId);
            var payload = new
            {
                gatewayId,
                devices = devices.Select(d => new
                {
                    d.Id,
                    d.Name,
                    d.IsEnabled,
                    d.Status,
                    d.LastConnectedAt,
                    d.Protocol,
                    IpAddress = d.IpAddress,
                    d.Port
                })
            };
            return Ok(payload);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取网关配置失败 {GatewayId}", gatewayId);
            return StatusCode(500, new { message = "获取网关配置失败" });
        }
    }

    /// <summary>
    /// 触发指定设备在网关上的重载（通过转发到 Gateway 管理接口）
    /// </summary>
    [HttpPost("devices/{deviceId}/reload")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ReloadDevice(int deviceId)
    {
        try
        {
            // 网关管理地址（环境变量或配置）
            var gatewayApiBase = Environment.GetEnvironmentVariable("GATEWAY_API_URL") ?? "http://localhost:5080";
            var client = _httpClientFactory.CreateClient();
            var resp = await client.PostAsync($"{gatewayApiBase}/api/management/reload", content: null);
            if (!resp.IsSuccessStatusCode)
            {
                return StatusCode((int)resp.StatusCode, new { message = "转发重载请求失败" });
            }
            return Ok(new { message = "reload requested" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "设备重载失败 {DeviceId}", deviceId);
            return StatusCode(500, new { message = "设备重载失败" });
        }
    }

    /// <summary>
    /// 获取网关最新数据快照（转发到网关管理接口）
    /// </summary>
    [HttpGet("latest")]
    public async Task<IActionResult> GetGatewayLatest()
    {
        try
        {
            var gatewayApiBase = Environment.GetEnvironmentVariable("GATEWAY_API_URL") ?? "http://localhost:5080";
            var client = _httpClientFactory.CreateClient();
            var resp = await client.GetAsync($"{gatewayApiBase}/api/management/latest");
            var content = await resp.Content.ReadAsStringAsync();
            if (!resp.IsSuccessStatusCode)
            {
                return StatusCode((int)resp.StatusCode, new { message = "获取网关快照失败" });
            }
            return Content(content, "application/json");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取网关最新数据失败");
            return StatusCode(500, new { message = "获取网关最新数据失败" });
        }
    }
}


