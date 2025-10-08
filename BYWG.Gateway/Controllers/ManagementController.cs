using Microsoft.AspNetCore.Mvc;
using BYWG.Gateway.Services;

namespace BYWG.Gateway.Controllers;

[ApiController]
[Route("api/management")]
public class ManagementController : ControllerBase
{
    private readonly DataCollectionService _dataCollectionService;
    private readonly CommunicationService _communicationService;

    public ManagementController(DataCollectionService dataCollectionService, CommunicationService communicationService)
    {
        _dataCollectionService = dataCollectionService;
        _communicationService = communicationService;
    }

    [HttpGet("health")]
    public IActionResult Health()
    {
            return Ok(new
            {
                status = "ok",
                collectors = _dataCollectionService.GetStatusSummary()
            });
    }

    [HttpPost("reload")]
    public IActionResult Reload()
    {
        _dataCollectionService.ReloadConfiguration();
        return Ok(new { message = "reload triggered" });
    }

    [HttpGet("latest")]
    public IActionResult Latest()
    {
        var data = _dataCollectionService.GetLatestData();
        return Ok(data);
    }
}


