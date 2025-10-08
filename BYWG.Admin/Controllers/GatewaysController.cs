using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BYWG.Admin.Services;
using BYWG.Shared.Models;

namespace BYWG.Admin.Controllers;

/// <summary>
/// 网关管理控制器
/// </summary>
[ApiController]
[Route("api/gateways")]
[Authorize] // 需要认证
public class GatewaysController : ControllerBase
{
    private readonly IGatewayService _gatewayService;
    private readonly ILogger<GatewaysController> _logger;

    public GatewaysController(IGatewayService gatewayService, ILogger<GatewaysController> logger)
    {
        _gatewayService = gatewayService;
        _logger = logger;
    }

    /// <summary>
    /// 获取所有网关
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Gateway>>> GetGateways()
    {
        try
        {
            var gateways = await _gatewayService.GetAllGatewaysAsync();
            return Ok(gateways);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取网关列表失败");
            return StatusCode(500, "获取网关列表失败");
        }
    }

    /// <summary>
    /// 根据ID获取网关
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Gateway>> GetGateway(int id)
    {
        try
        {
            var gateway = await _gatewayService.GetGatewayByIdAsync(id);
            if (gateway == null)
            {
                return NotFound($"网关 {id} 不存在");
            }
            return Ok(gateway);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取网关 {GatewayId} 失败", id);
            return StatusCode(500, "获取网关失败");
        }
    }

    /// <summary>
    /// 创建网关
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")] // 只有管理员可以创建网关
    public async Task<ActionResult<Gateway>> CreateGateway([FromBody] Gateway gateway)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdGateway = await _gatewayService.CreateGatewayAsync(gateway);
            return CreatedAtAction(nameof(GetGateway), new { id = createdGateway.Id }, createdGateway);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建网关失败");
            return StatusCode(500, "创建网关失败");
        }
    }

    /// <summary>
    /// 更新网关
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")] // 只有管理员可以更新网关
    public async Task<ActionResult<Gateway>> UpdateGateway(int id, [FromBody] Gateway gateway)
    {
        try
        {
            if (id != gateway.Id)
            {
                return BadRequest("网关ID不匹配");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updatedGateway = await _gatewayService.UpdateGatewayAsync(gateway);
            return Ok(updatedGateway);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新网关 {GatewayId} 失败", id);
            return StatusCode(500, "更新网关失败");
        }
    }

    /// <summary>
    /// 删除网关
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")] // 只有管理员可以删除网关
    public async Task<ActionResult> DeleteGateway(int id)
    {
        try
        {
            var result = await _gatewayService.DeleteGatewayAsync(id);
            if (!result)
            {
                return NotFound($"网关 {id} 不存在");
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除网关 {GatewayId} 失败", id);
            return StatusCode(500, "删除网关失败");
        }
    }

    /// <summary>
    /// 注册网关
    /// </summary>
    [HttpPost("register")]
    public async Task<ActionResult<Gateway>> RegisterGateway([FromBody] GatewayRegistrationRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var gateway = await _gatewayService.RegisterGatewayAsync(request);
            return Ok(gateway);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "注册网关失败");
            return StatusCode(500, "注册网关失败");
        }
    }

    /// <summary>
    /// 更新网关心跳
    /// </summary>
    [HttpPost("{gatewayId}/heartbeat")]
    public async Task<ActionResult> UpdateHeartbeat(string gatewayId, [FromBody] GatewayHeartbeatRequest request)
    {
        try
        {
            var result = await _gatewayService.UpdateGatewayHeartbeatAsync(gatewayId, request);
            if (!result)
            {
                return NotFound($"网关 {gatewayId} 不存在");
            }
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新网关心跳失败: {GatewayId}", gatewayId);
            return StatusCode(500, "更新网关心跳失败");
        }
    }

    /// <summary>
    /// 更新网关状态
    /// </summary>
    [HttpPatch("{id}/status")]
    public async Task<ActionResult> UpdateGatewayStatus(int id, [FromBody] UpdateGatewayStatusRequest request)
    {
        try
        {
            var result = await _gatewayService.UpdateGatewayStatusAsync(id, request.Status);
            if (!result)
            {
                return NotFound($"网关 {id} 不存在");
            }
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新网关 {GatewayId} 状态失败", id);
            return StatusCode(500, "更新网关状态失败");
        }
    }

    /// <summary>
    /// 搜索网关
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<Gateway>>> SearchGateways([FromQuery] string q)
    {
        try
        {
            var gateways = await _gatewayService.SearchGatewaysAsync(q);
            return Ok(gateways);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "搜索网关失败: {SearchTerm}", q);
            return StatusCode(500, "搜索网关失败");
        }
    }
}

/// <summary>
/// 更新网关状态请求
/// </summary>
public class UpdateGatewayStatusRequest
{
    public GatewayStatus Status { get; set; }
}
