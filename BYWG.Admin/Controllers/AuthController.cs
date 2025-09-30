using Microsoft.AspNetCore.Mvc;

namespace BYWG.Admin.Controllers;

[ApiController]
[Route("auth")] 
public class AuthController : ControllerBase
{
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        // 开发期直通：接受任何用户名密码，返回固定token
        var user = new
        {
            id = 1,
            username = request.username,
            email = "admin@bywg.local",
            role = "Admin",
            isEnabled = true,
            createdAt = DateTime.UtcNow,
            updatedAt = DateTime.UtcNow
        };

        return Ok(new
        {
            token = "dev-token",
            user,
            expiresIn = 3600
        });
    }

    [HttpPost("logout")]
    public IActionResult Logout() => Ok();

    [HttpGet("me")]
    public IActionResult Me()
    {
        return Ok(new
        {
            id = 1,
            username = "admin",
            email = "admin@bywg.local",
            role = "Admin",
            isEnabled = true,
            createdAt = DateTime.UtcNow,
            updatedAt = DateTime.UtcNow
        });
    }
}

public class LoginRequest
{
    public string username { get; set; } = string.Empty;
    public string password { get; set; } = string.Empty;
    public bool rememberMe { get; set; }
}


