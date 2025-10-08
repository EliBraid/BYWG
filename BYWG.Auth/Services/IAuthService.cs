using BYWG.Shared.Models;

namespace BYWG.Auth.Services;

public interface IAuthService
{
    Task<User?> AuthenticateAsync(string username, string password);
    Task<User> RegisterAsync(string username, string email, string password);
    Task<bool> ValidatePasswordAsync(User user, string password);
    Task<User?> GetUserByIdAsync(int id);
    Task<User?> GetUserByUsernameAsync(string username);
    Task<User?> GetUserByEmailAsync(string email);
}
