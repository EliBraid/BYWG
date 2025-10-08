using BYWG.Shared.Models;

namespace BYWG.Auth.Services;

public interface IUserService
{
    Task<IEnumerable<User>> GetAllUsersAsync();
    Task<User?> GetUserByIdAsync(int id);
    Task<User> CreateUserAsync(User user);
    Task<User> UpdateUserAsync(User user);
    Task<bool> DeleteUserAsync(int id);
    Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
}
