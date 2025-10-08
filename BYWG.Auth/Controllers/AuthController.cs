using Microsoft.AspNetCore.Mvc;
using BYWG.Auth.Services;
using BYWG.Shared.Models;

namespace BYWG.Auth.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ITokenService _tokenService;
    private readonly IOnlineUserService _onlineUserService;

    public AuthController(IAuthService authService, ITokenService tokenService, IOnlineUserService onlineUserService)
    {
        _authService = authService;
        _tokenService = tokenService;
        _onlineUserService = onlineUserService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _authService.AuthenticateAsync(request.Username, request.Password);
        if (user == null)
        {
            return Unauthorized(new { message = "Invalid username or password" });
        }

        var token = _tokenService.GenerateJwtToken(user);
        // 记录一次心跳（视为上线）
        _onlineUserService.UpdateHeartbeat(user.Id);
        
        return Ok(new
        {
            token = token,
            user = new
            {
                id = user.Id,
                username = user.Username,
                email = user.Email,
                role = user.Role,
                isEnabled = user.IsEnabled,
                createdAt = user.CreatedAt,
                updatedAt = user.UpdatedAt
            },
            expiresIn = 8 * 3600
        });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var user = await _authService.RegisterAsync(request.Username, request.Email, request.Password);
            return Ok(new { message = "User registered successfully", userId = user.Id });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("logout")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public IActionResult Logout()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim != null)
        {
            var userId = int.Parse(userIdClaim.Value);
            // 清除该用户的在线状态
            _onlineUserService.RemoveUser(userId);
        }
        return Ok(new { message = "Logged out successfully" });
    }

    [HttpGet("me")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
        {
            return Unauthorized();
        }

        var userId = int.Parse(userIdClaim.Value);
        var user = await _authService.GetUserByIdAsync(userId);
        
        if (user == null)
        {
            return NotFound();
        }

        // 访问即刷新心跳
        _onlineUserService.UpdateHeartbeat(user.Id);

        return Ok(new
        {
            id = user.Id,
            username = user.Username,
            email = user.Email,
            role = user.Role,
            isEnabled = user.IsEnabled,
            createdAt = user.CreatedAt,
            updatedAt = user.UpdatedAt,
            online = _onlineUserService.IsUserOnline(user.Id)
        });
    }

    /// <summary>
    /// 心跳上报，刷新在线状态
    /// </summary>
    [HttpPost("heartbeat")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public IActionResult Heartbeat()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
        {
            return Unauthorized();
        }
        var userId = int.Parse(userIdClaim.Value);
        _onlineUserService.UpdateHeartbeat(userId);
        return Ok(new { message = "heartbeat ok", online = true });
    }

    /// <summary>
    /// 获取当前在线人数
    /// </summary>
    [HttpGet("online-count")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public IActionResult GetOnlineCount()
    {
        var count = _onlineUserService.GetOnlineCount();
        return Ok(new { online = count });
    }
}

public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class RegisterRequest
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
