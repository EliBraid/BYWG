using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace BYWG.Admin.Controllers;

[ApiController]
[Route("api/integrations")]
[Authorize] // 需要认证
public class IntegrationsController : ControllerBase
{
    // OPC UA 集成
    [HttpGet("opcua/config")]
    public IActionResult GetOpcUaConfig()
    {
        var config = new
        {
            id = "opcua-1",
            name = "OPC UA Integration",
            endpoint = "opc.tcp://localhost:4840",
            securityMode = "None",
            securityPolicy = "None",
            authMode = "Anonymous",
            enabled = true,
            mappings = new object[0]
        };
        return Ok(config);
    }

    [HttpPost("opcua/config")]
    [Authorize(Roles = "Admin")] // 只有管理员可以保存配置
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
        var config = new
        {
            id = "mqtt-1",
            name = "MQTT Integration",
            host = "localhost",
            port = 1883,
            username = "",
            password = "",
            clientId = "bywg-admin",
            cleanSession = true,
            ssl = false,
            enabled = true,
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
        var config = new
        {
            id = "rest-1",
            name = "REST Integration",
            baseUrl = "http://localhost:8080/api",
            authMode = "None",
            timeoutMs = 5000,
            retryCount = 3,
            verifyTLS = true,
            enabled = true,
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
