using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BYWG.Auth.Services;
using BYWG.Shared.Models;

namespace BYWG.Auth.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IAuthService _authService;
    private readonly IOnlineUserService _onlineUserService;

    public UsersController(IUserService userService, IAuthService authService, IOnlineUserService onlineUserService)
    {
        _userService = userService;
        _authService = authService;
        _onlineUserService = onlineUserService;
    }

    /// <summary>
    /// 获取所有用户
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null, [FromQuery] string? role = null, [FromQuery] string? status = null)
    {
        try
        {
            var users = await _userService.GetAllUsersAsync();
            
            // 应用搜索过滤
            if (!string.IsNullOrEmpty(search))
            {
                users = users.Where(u => 
                    u.Username.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    u.Email.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    (u.FullName != null && u.FullName.Contains(search, StringComparison.OrdinalIgnoreCase))
                );
            }

            // 应用角色过滤
            if (!string.IsNullOrEmpty(role))
            {
                users = users.Where(u => u.Role.Equals(role, StringComparison.OrdinalIgnoreCase));
            }

            // 应用状态过滤
            if (!string.IsNullOrEmpty(status))
            {
                switch (status.ToLower())
                {
                    case "active":
                        users = users.Where(u => u.IsEnabled);
                        break;
                    case "inactive":
                        users = users.Where(u => !u.IsEnabled);
                        break;
                    case "online":
                        // 使用在线服务判断
                        users = users.Where(u => _onlineUserService.IsUserOnline(u.Id));
                        break;
                    case "offline":
                        users = users.Where(u => !_onlineUserService.IsUserOnline(u.Id));
                        break;
                }
            }

            // 分页
            var totalCount = users.Count();
            var pagedUsers = users
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    FullName = u.FullName,
                    Phone = u.Phone,
                    Department = u.Department,
                    Role = u.Role,
                    IsEnabled = u.IsEnabled,
                    CreatedAt = u.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                    UpdatedAt = u.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                    LastLoginAt = u.LastLoginAt?.ToString("yyyy-MM-dd HH:mm:ss"),
                    LoginCount = u.LoginCount,
                    Preferences = u.Preferences,
                    Online = _onlineUserService.IsUserOnline(u.Id)
                })
                .ToList();

            return Ok(new
            {
                users = pagedUsers,
                totalCount,
                page,
                pageSize,
                totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "获取用户列表失败", error = ex.Message });
        }
    }

    /// <summary>
    /// 根据ID获取用户
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(int id)
    {
        try
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "用户不存在" });
            }

            var userDto = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                Phone = user.Phone,
                Department = user.Department,
                Role = user.Role,
                IsEnabled = user.IsEnabled,
                CreatedAt = user.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                UpdatedAt = user.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                LastLoginAt = user.LastLoginAt?.ToString("yyyy-MM-dd HH:mm:ss"),
                LoginCount = user.LoginCount,
                Preferences = user.Preferences
            };

            return Ok(userDto);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "获取用户信息失败", error = ex.Message });
        }
    }

    /// <summary>
    /// 创建新用户
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        try
        {
            // 检查用户名是否已存在
            var existingUser = await _authService.GetUserByUsernameAsync(request.Username);
            if (existingUser != null)
            {
                return BadRequest(new { message = "用户名已存在" });
            }

            // 检查邮箱是否已存在
            var existingEmail = await _authService.GetUserByEmailAsync(request.Email);
            if (existingEmail != null)
            {
                return BadRequest(new { message = "邮箱已存在" });
            }

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                FullName = request.FullName,
                Phone = request.Phone,
                Department = request.Department,
                Role = request.Role ?? "User",
                IsEnabled = request.IsEnabled ?? true,
                PasswordHash = request.Password, // 将在服务中加密
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdUser = await _userService.CreateUserAsync(user);

            return Ok(new
            {
                message = "用户创建成功",
                user = new UserDto
                {
                    Id = createdUser.Id,
                    Username = createdUser.Username,
                    Email = createdUser.Email,
                    FullName = createdUser.FullName,
                    Phone = createdUser.Phone,
                    Department = createdUser.Department,
                    Role = createdUser.Role,
                    IsEnabled = createdUser.IsEnabled,
                    CreatedAt = createdUser.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                    UpdatedAt = createdUser.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                    LastLoginAt = createdUser.LastLoginAt?.ToString("yyyy-MM-dd HH:mm:ss"),
                    LoginCount = createdUser.LoginCount,
                    Preferences = createdUser.Preferences
                }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "创建用户失败", error = ex.Message });
        }
    }

    /// <summary>
    /// 更新用户信息
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserRequest request)
    {
        try
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "用户不存在" });
            }

            // 检查用户名是否被其他用户使用
            if (request.Username != user.Username)
            {
                var existingUser = await _authService.GetUserByUsernameAsync(request.Username);
                if (existingUser != null && existingUser.Id != id)
                {
                    return BadRequest(new { message = "用户名已被其他用户使用" });
                }
            }

            // 检查邮箱是否被其他用户使用
            if (request.Email != user.Email)
            {
                var existingEmail = await _authService.GetUserByEmailAsync(request.Email);
                if (existingEmail != null && existingEmail.Id != id)
                {
                    return BadRequest(new { message = "邮箱已被其他用户使用" });
                }
            }

            user.Username = request.Username;
            user.Email = request.Email;
            user.FullName = request.FullName;
            user.Phone = request.Phone;
            user.Department = request.Department;
            user.Role = request.Role;
            user.IsEnabled = request.IsEnabled;
            user.UpdatedAt = DateTime.UtcNow;

            var updatedUser = await _userService.UpdateUserAsync(user);

            return Ok(new
            {
                message = "用户更新成功",
                user = new UserDto
                {
                    Id = updatedUser.Id,
                    Username = updatedUser.Username,
                    Email = updatedUser.Email,
                    FullName = updatedUser.FullName,
                    Phone = updatedUser.Phone,
                    Department = updatedUser.Department,
                    Role = updatedUser.Role,
                    IsEnabled = updatedUser.IsEnabled,
                    CreatedAt = updatedUser.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                    UpdatedAt = updatedUser.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                    LastLoginAt = updatedUser.LastLoginAt?.ToString("yyyy-MM-dd HH:mm:ss"),
                    LoginCount = updatedUser.LoginCount,
                    Preferences = updatedUser.Preferences
                }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "更新用户失败", error = ex.Message });
        }
    }

    /// <summary>
    /// 删除用户
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        try
        {
            var success = await _userService.DeleteUserAsync(id);
            if (!success)
            {
                return NotFound(new { message = "用户不存在" });
            }

            return Ok(new { message = "用户删除成功" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "删除用户失败", error = ex.Message });
        }
    }

    /// <summary>
    /// 重置用户密码
    /// </summary>
    [HttpPost("{id}/reset-password")]
    public async Task<IActionResult> ResetPassword(int id, [FromBody] ResetPasswordRequest request)
    {
        try
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "用户不存在" });
            }

            var success = await _userService.ChangePasswordAsync(id, request.CurrentPassword, request.NewPassword);
            if (!success)
            {
                return BadRequest(new { message = "当前密码不正确" });
            }

            return Ok(new { message = "密码重置成功" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "重置密码失败", error = ex.Message });
        }
    }

    /// <summary>
    /// 批量更新用户状态
    /// </summary>
    [HttpPost("batch-update-status")]
    public async Task<IActionResult> BatchUpdateStatus([FromBody] BatchUpdateStatusRequest request)
    {
        try
        {
            var results = new List<object>();
            
            foreach (var userId in request.UserIds)
            {
                var user = await _userService.GetUserByIdAsync(userId);
                if (user != null)
                {
                    user.IsEnabled = request.IsEnabled;
                    user.UpdatedAt = DateTime.UtcNow;
                    await _userService.UpdateUserAsync(user);
                    results.Add(new { userId, success = true });
                }
                else
                {
                    results.Add(new { userId, success = false, message = "用户不存在" });
                }
            }

            return Ok(new { message = "批量更新完成", results });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "批量更新失败", error = ex.Message });
        }
    }

    /// <summary>
    /// 批量删除用户
    /// </summary>
    [HttpPost("batch-delete")]
    public async Task<IActionResult> BatchDelete([FromBody] BatchDeleteRequest request)
    {
        try
        {
            var results = new List<object>();
            
            foreach (var userId in request.UserIds)
            {
                var success = await _userService.DeleteUserAsync(userId);
                results.Add(new { userId, success });
            }

            return Ok(new { message = "批量删除完成", results });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "批量删除失败", error = ex.Message });
        }
    }

    /// <summary>
    /// 获取用户统计信息
    /// </summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetUserStats()
    {
        try
        {
            var users = await _userService.GetAllUsersAsync();
            var userList = users.ToList();

            var onlineIds = _onlineUserService.GetOnlineUserIds();
            var stats = new
            {
                total = userList.Count,
                active = userList.Count(u => u.IsEnabled),
                admins = userList.Count(u => u.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase)),
                online = userList.Count(u => u.IsEnabled && onlineIds.Contains(u.Id))
            };

            return Ok(stats);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "获取用户统计失败", error = ex.Message });
        }
    }
}

// DTO类定义
public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string? Phone { get; set; }
    public string? Department { get; set; }
    public string Role { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public string CreatedAt { get; set; } = string.Empty;
    public string UpdatedAt { get; set; } = string.Empty;
    public string? LastLoginAt { get; set; }
    public int LoginCount { get; set; }
    public UserPreferences? Preferences { get; set; }
    public bool Online { get; set; }
}

public class CreateUserRequest
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string? Phone { get; set; }
    public string? Department { get; set; }
    public string? Role { get; set; }
    public bool? IsEnabled { get; set; }
}

public class UpdateUserRequest
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string? Phone { get; set; }
    public string? Department { get; set; }
    public string Role { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
}

public class ResetPasswordRequest
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public class BatchUpdateStatusRequest
{
    public List<int> UserIds { get; set; } = new();
    public bool IsEnabled { get; set; }
}

public class BatchDeleteRequest
{
    public List<int> UserIds { get; set; } = new();
}
