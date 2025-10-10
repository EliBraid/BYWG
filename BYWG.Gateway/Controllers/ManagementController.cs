using Microsoft.AspNetCore.Mvc;
using BYWG.Gateway.Services;

namespace BYWG.Gateway.Controllers;

[ApiController]
[Route("api/management")]
public class ManagementController : ControllerBase
{
    private readonly IServiceProvider _serviceProvider;

    public ManagementController(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    private DataCollectionService GetDataCollectionService()
    {
        return _serviceProvider.GetServices<IHostedService>()
            .OfType<DataCollectionService>()
            .FirstOrDefault() ?? throw new InvalidOperationException("DataCollectionService not found");
    }

    private CommunicationService GetCommunicationService()
    {
        return _serviceProvider.GetServices<IHostedService>()
            .OfType<CommunicationService>()
            .FirstOrDefault() ?? throw new InvalidOperationException("CommunicationService not found");
    }

    [HttpGet("health")]
    public IActionResult Health()
    {
        var dataCollectionService = GetDataCollectionService();
        return Ok(new
        {
            status = "ok",
            collectors = dataCollectionService.GetStatusSummary()
        });
    }

    [HttpPost("reload")]
    public IActionResult Reload()
    {
        var dataCollectionService = GetDataCollectionService();
        dataCollectionService.ReloadConfiguration();
        return Ok(new { message = "reload triggered" });
    }

    [HttpGet("latest")]
    public IActionResult Latest()
    {
        var dataCollectionService = GetDataCollectionService();
        var data = dataCollectionService.GetLatestData();
        return Ok(data);
    }

    [HttpPost("sync")]
    public async Task<IActionResult> Sync()
    {
        var communicationService = GetCommunicationService();
        var dataCollectionService = GetDataCollectionService();
        
        // 先同步设备数据
        communicationService.TriggerDataSync();
        
        // 然后重新加载协议配置（从Admin获取最新数据点配置）
        await dataCollectionService.ReloadProtocolConfigurations();
        
        // 重载后立刻触发一次数据采集，让新设备/新点位尽快有数据
        dataCollectionService.TriggerImmediateCollection();
        
        return Ok(new { message = "数据同步和协议配置重载已完成" });
    }

    public record WriteCommandRequest(int DeviceId, string Address, object Value, int? FunctionCode, bool? ReadBack);

    [HttpPost("write")]
    public async Task<IActionResult> Write([FromBody] WriteCommandRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Address))
        {
            return BadRequest(new { message = "参数无效" });
        }

        var dataCollectionService = GetDataCollectionService();
        try
        {
            var result = await dataCollectionService.WriteToDeviceAsync(
                request.DeviceId,
                request.Address,
                request.Value,
                request.FunctionCode,
                request.ReadBack == true
            );
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("points")]
    public IActionResult GetPoints()
    {
        var dataCollectionService = GetDataCollectionService();
        var data = dataCollectionService.GetLatestData();
        return Ok(new 
        { 
            message = "获取点位数据成功",
            count = data.Count,
            data = data
        });
    }

    [HttpPost("test-data-event")]
    public IActionResult TestDataEvent()
    {
        var dataCollectionService = GetDataCollectionService();
        dataCollectionService.TestDataChangedEvent();
        return Ok(new { message = "测试数据变化事件已触发" });
    }

    [HttpGet("debug-data")]
    public IActionResult DebugData()
    {
        var dataCollectionService = GetDataCollectionService();
        var data = dataCollectionService.GetLatestData();
        return Ok(new 
        { 
            message = "调试数据",
            count = data.Count,
            keys = data.Keys.ToList(),
            data = data
        });
    }
}


