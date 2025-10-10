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


