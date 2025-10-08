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

    public AuthController(IAuthService authService, ITokenService tokenService)
    {
        _authService = authService;
        _tokenService = tokenService;
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
            expiresIn = 3600
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
    public IActionResult Logout()
    {
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

        return Ok(new
        {
            id = user.Id,
            username = user.Username,
            email = user.Email,
            role = user.Role,
            isEnabled = user.IsEnabled,
            createdAt = user.CreatedAt,
            updatedAt = user.UpdatedAt
        });
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
