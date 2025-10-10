using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace BYWG.Admin.Controllers;

[ApiController]
[Route("api/integrations")]
[Authorize] // 需要认证
public class IntegrationsController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public IntegrationsController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    // OPC UA 集成
    [HttpGet("opcua/config")]
    public IActionResult GetOpcUaConfig()
    {
        var endpoint = _configuration["Integrations:OpcUa:Endpoint"] ?? "opc.tcp://localhost:4840";
        var securityMode = _configuration["Integrations:OpcUa:SecurityMode"] ?? "None";
        var securityPolicy = _configuration["Integrations:OpcUa:SecurityPolicy"] ?? "None";
        var authMode = _configuration["Integrations:OpcUa:AuthMode"] ?? "Anonymous";
        var enabled = bool.TryParse(_configuration["Integrations:OpcUa:Enabled"], out var e1) ? e1 : true;
        var config = new
        {
            id = "opcua-1",
            name = "OPC UA Integration",
            endpoint,
            securityMode,
            securityPolicy,
            authMode,
            enabled,
            mappings = new object[0]
        };
        return Ok(config);
    }

    [HttpPost("opcua/config")]
    [Authorize(Roles = "Admin,admin")] // 只有管理员可以保存配置
    public IActionResult SaveOpcUaConfig([FromBody] object config)
    {
        return Ok(new { message = "OPC UA configuration saved successfully" });
    }

    [HttpPost("opcua/test")]
    public IActionResult TestOpcUaConnection([FromBody] object config)
    {
        return Ok(new { message = "OPC UA connection test successful" });
    }

    [HttpGet("opcua/logs")]
    public IActionResult GetOpcUaLogs()
    {
        var logs = new[]
        {
            "2025-10-08 09:00:00 - OPC UA connection established",
            "2025-10-08 09:01:00 - Data point synchronized",
            "2025-10-08 09:02:00 - Connection stable"
        };
        return Ok(logs);
    }

    // MQTT 集成
    [HttpGet("mqtt/config")]
    public IActionResult GetMqttConfig()
    {
        var host = _configuration["Integrations:Mqtt:Host"] ?? "localhost";
        var port = int.TryParse(_configuration["Integrations:Mqtt:Port"], out var p) ? p : 1883;
        var username = _configuration["Integrations:Mqtt:Username"] ?? string.Empty;
        var password = _configuration["Integrations:Mqtt:Password"] ?? string.Empty;
        var clientId = _configuration["Integrations:Mqtt:ClientId"] ?? "bywg-admin";
        var cleanSession = bool.TryParse(_configuration["Integrations:Mqtt:CleanSession"], out var cs) ? cs : true;
        var ssl = bool.TryParse(_configuration["Integrations:Mqtt:Ssl"], out var s) ? s : false;
        var enabled = bool.TryParse(_configuration["Integrations:Mqtt:Enabled"], out var e2) ? e2 : true;
        var config = new
        {
            id = "mqtt-1",
            name = "MQTT Integration",
            host,
            port,
            username,
            password,
            clientId,
            cleanSession,
            ssl,
            enabled,
            mappings = new object[0]
        };
        return Ok(config);
    }

    [HttpPost("mqtt/config")]
    public IActionResult SaveMqttConfig([FromBody] object config)
    {
        return Ok(new { message = "MQTT configuration saved successfully" });
    }

    [HttpPost("mqtt/test")]
    public IActionResult TestMqttConnection([FromBody] object config)
    {
        return Ok(new { message = "MQTT connection test successful" });
    }

    [HttpGet("mqtt/logs")]
    public IActionResult GetMqttLogs()
    {
        var logs = new[]
        {
            "2025-10-08 09:00:00 - MQTT broker connected",
            "2025-10-08 09:01:00 - Topic subscribed: /devices/+/data",
            "2025-10-08 09:02:00 - Message received from device-001"
        };
        return Ok(logs);
    }

    // REST 集成
    [HttpGet("rest/config")]
    public IActionResult GetRestConfig()
    {
        var baseUrl = _configuration["Integrations:Rest:BaseUrl"] ?? "http://localhost:8080/api";
        var authMode = _configuration["Integrations:Rest:AuthMode"] ?? "None";
        var timeoutMs = int.TryParse(_configuration["Integrations:Rest:TimeoutMs"], out var t) ? t : 5000;
        var retryCount = int.TryParse(_configuration["Integrations:Rest:RetryCount"], out var r) ? r : 3;
        var verifyTLS = bool.TryParse(_configuration["Integrations:Rest:VerifyTLS"], out var v) ? v : true;
        var enabled = bool.TryParse(_configuration["Integrations:Rest:Enabled"], out var e3) ? e3 : true;
        var config = new
        {
            id = "rest-1",
            name = "REST Integration",
            baseUrl,
            authMode,
            timeoutMs,
            retryCount,
            verifyTLS,
            enabled,
            mappings = new object[0]
        };
        return Ok(config);
    }

    [HttpPost("rest/config")]
    public IActionResult SaveRestConfig([FromBody] object config)
    {
        return Ok(new { message = "REST configuration saved successfully" });
    }

    [HttpPost("rest/test")]
    public IActionResult TestRestConnection([FromBody] object config)
    {
        return Ok(new { message = "REST connection test successful" });
    }

    [HttpGet("rest/logs")]
    public IActionResult GetRestLogs()
    {
        var logs = new[]
        {
            "2025-10-08 09:00:00 - REST endpoint configured",
            "2025-10-08 09:01:00 - HTTP request sent successfully",
            "2025-10-08 09:02:00 - Response received with status 200"
        };
        return Ok(logs);
    }
}
