using BYWG.Admin.Data;
using BYWG.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace BYWG.Admin.Services;

public class UserService : IUserService
{
    private readonly AdminDbContext _context;
    private readonly ILogger<UserService> _logger;

    public UserService(AdminDbContext context, ILogger<UserService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        return await _context.Users.OrderBy(u => u.Username).ToListAsync();
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<User> CreateUserAsync(User user)
    {
        user.CreatedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<User> UpdateUserAsync(User user)
    {
        var exists = await _context.Users.FindAsync(user.Id) ?? throw new ArgumentException("用户不存在");
        exists.FullName = user.FullName;
        exists.Email = user.Email;
        exists.Phone = user.Phone;
        exists.Department = user.Department;
        exists.Role = user.Role;
        exists.IsEnabled = user.IsEnabled;
        exists.Preferences = user.Preferences;
        exists.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return exists;
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return false;
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<User?> ValidateUserAsync(string username, string password)
    {
        // 开发期：仅按用户名存在返回；后续接入真实密码校验
        return await GetUserByUsernameAsync(username);
    }

    public async Task<bool> UpdateLastLoginAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return false;
        user.LastLoginAt = DateTime.UtcNow;
        user.LoginCount++;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<User>> SearchUsersAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm)) return await GetAllUsersAsync();
        return await _context.Users
            .Where(u => u.Username.Contains(searchTerm) || (u.FullName != null && u.FullName.Contains(searchTerm)) || u.Email.Contains(searchTerm))
            .OrderBy(u => u.Username)
            .ToListAsync();
    }
}


