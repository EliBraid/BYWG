using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace BYWG.Admin.Controllers;

[ApiController]
[Route("api/protocol-templates")]
[Authorize]
public class ProtocolTemplatesController : ControllerBase
{
    public record TemplateDto(int Id, string Name, string Protocol, string Description);

    [HttpGet]
    public IActionResult List()
    {
        // 占位数据：后续可从数据库加载
        var items = new List<TemplateDto>
        {
            new(1, "Modbus-泵站标准点表", "ModbusTCP", "常用压力/温度/电机状态"),
            new(2, "OPC UA-通用工艺点表", "OPCUA", "通用工艺变量节点映射")
        };
        return Ok(items);
    }
}


