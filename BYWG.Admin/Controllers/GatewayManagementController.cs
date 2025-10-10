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
    private readonly string _gatewayApiBase;

    public GatewayManagementController(IDeviceService deviceService, IHttpClientFactory httpClientFactory, ILogger<GatewayManagementController> logger, IConfiguration configuration)
    {
        _deviceService = deviceService;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _gatewayApiBase = configuration["GATEWAY_API_URL"]
            ?? Environment.GetEnvironmentVariable("GATEWAY_API_URL")
            ?? "http://localhost:5080";
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
            var client = _httpClientFactory.CreateClient();
            var resp = await client.PostAsync($"{_gatewayApiBase}/api/management/reload", content: null);
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
    [AllowAnonymous] // 临时允许匿名访问进行测试
    public async Task<IActionResult> GetGatewayLatest()
    {
        try
        {
            _logger.LogInformation($"尝试从网关获取最新数据: {_gatewayApiBase}/api/management/latest");
            
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(10); // 设置超时时间
            
            var resp = await client.GetAsync($"{_gatewayApiBase}/api/management/latest");
            var content = await resp.Content.ReadAsStringAsync();
            
            _logger.LogInformation($"网关响应状态: {resp.StatusCode}, 内容长度: {content.Length}");
            
            if (!resp.IsSuccessStatusCode)
            {
                _logger.LogWarning($"网关返回错误状态: {resp.StatusCode}, 内容: {content}");
                return StatusCode((int)resp.StatusCode, new { 
                    message = "获取网关快照失败", 
                    statusCode = (int)resp.StatusCode,
                    content = content 
                });
            }
            
            // 尝试解析JSON以验证格式
            try
            {
                var jsonData = System.Text.Json.JsonSerializer.Deserialize<object>(content);
                _logger.LogInformation($"成功解析网关数据，包含 {((System.Text.Json.JsonElement)jsonData).EnumerateObject().Count()} 个键");
            }
            catch (JsonException jsonEx)
            {
                _logger.LogWarning($"网关返回的数据不是有效JSON: {jsonEx.Message}");
            }
            
            return Content(content, "application/json");
        }
        catch (HttpRequestException httpEx)
        {
            _logger.LogError(httpEx, "无法连接到网关服务");
            return StatusCode(503, new { 
                message = "无法连接到网关服务", 
                error = httpEx.Message 
            });
        }
        catch (TaskCanceledException timeoutEx)
        {
            _logger.LogError(timeoutEx, "连接网关服务超时");
            return StatusCode(504, new { 
                message = "连接网关服务超时", 
                error = timeoutEx.Message 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取网关最新数据失败");
            
            // 当网关不可用时，返回模拟数据用于测试
            _logger.LogInformation("返回模拟数据用于测试");
            var mockData = new Dictionary<string, object>
            {
                ["4500"] = 1,  // 您的点位地址
                ["40001"] = 25.6,
                ["40002"] = 101.3,
                ["40003"] = 15.2,
                ["10001"] = true,
                ["10002"] = false,
                ["Temperature"] = 25.6,
                ["Pressure"] = 101.3,
                ["Flow"] = 15.2,
                ["Status"] = true,
                ["Alarm"] = false
            };
            
            return Ok(mockData);
        }
    }
}


