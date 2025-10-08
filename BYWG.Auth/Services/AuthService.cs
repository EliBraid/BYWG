using BYWG.Auth.Data;
using BYWG.Shared.Models;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace BYWG.Auth.Services;

public class AuthService : IAuthService
{
    private readonly AuthDbContext _context;

    public AuthService(AuthDbContext context)
    {
        _context = context;
    }

    public async Task<User?> AuthenticateAsync(string username, string password)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == username);

        if (user == null || !ValidatePassword(user, password))
            return null;

        return user;
    }

    public async Task<User> RegisterAsync(string username, string email, string password)
    {
        var user = new User
        {
            Username = username,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            IsEnabled = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return user;
    }

    public async Task<bool> ValidatePasswordAsync(User user, string password)
    {
        return ValidatePassword(user, password);
    }

    private bool ValidatePassword(User user, string password)
    {
        return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email);
    }
}
