using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BYWG.Admin.Services;

namespace BYWG.Admin.Controllers;

/// <summary>
/// 实时数据控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RealtimeDataController : ControllerBase
{
    private readonly RealtimeDataService _realtimeDataService;
    private readonly ILogger<RealtimeDataController> _logger;

    public RealtimeDataController(RealtimeDataService realtimeDataService, ILogger<RealtimeDataController> logger)
    {
        _realtimeDataService = realtimeDataService;
        _logger = logger;
    }

    /// <summary>
    /// 获取最新实时数据
    /// </summary>
    [HttpGet("latest")]
    public IActionResult GetLatestData()
    {
        try
        {
            var data = _realtimeDataService.GetLatestData();
            return Ok(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取最新实时数据失败");
            return StatusCode(500, new { message = "获取实时数据失败" });
        }
    }

    /// <summary>
    /// 推送设备数据更新
    /// </summary>
    [HttpPost("device/{deviceId}/update")]
    public async Task<IActionResult> PushDeviceDataUpdate(int deviceId, [FromBody] Dictionary<string, object> data)
    {
        try
        {
            await _realtimeDataService.PushDeviceDataUpdate(deviceId, data);
            return Ok(new { message = "数据推送成功" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "推送设备数据更新失败");
            return StatusCode(500, new { message = "推送数据失败" });
        }
    }

    /// <summary>
    /// 推送点位数值更新
    /// </summary>
    [HttpPost("device/{deviceId}/point/{pointId}/update")]
    public async Task<IActionResult> PushPointValueUpdate(int deviceId, int pointId, [FromBody] PointValueUpdateRequest request)
    {
        try
        {
            await _realtimeDataService.PushPointValueUpdate(deviceId, pointId, request.Address, request.Value);
            return Ok(new { message = "点位数值推送成功" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "推送点位数值更新失败");
            return StatusCode(500, new { message = "推送点位数值失败" });
        }
    }

    public record PointValueUpdateRequest(string Address, object Value);
}
