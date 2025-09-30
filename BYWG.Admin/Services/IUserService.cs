using BYWG.Shared.Models;

namespace BYWG.Admin.Services;

/// <summary>
/// 用户服务接口
/// </summary>
public interface IUserService
{
    /// <summary>
    /// 获取所有用户
    /// </summary>
    Task<IEnumerable<User>> GetAllUsersAsync();

    /// <summary>
    /// 根据ID获取用户
    /// </summary>
    Task<User?> GetUserByIdAsync(int id);

    /// <summary>
    /// 根据用户名获取用户
    /// </summary>
    Task<User?> GetUserByUsernameAsync(string username);

    /// <summary>
    /// 创建用户
    /// </summary>
    Task<User> CreateUserAsync(User user);

    /// <summary>
    /// 更新用户
    /// </summary>
    Task<User> UpdateUserAsync(User user);

    /// <summary>
    /// 删除用户
    /// </summary>
    Task<bool> DeleteUserAsync(int id);

    /// <summary>
    /// 验证用户凭据
    /// </summary>
    Task<User?> ValidateUserAsync(string username, string password);

    /// <summary>
    /// 更新用户最后登录时间
    /// </summary>
    Task<bool> UpdateLastLoginAsync(int userId);

    /// <summary>
    /// 搜索用户
    /// </summary>
    Task<IEnumerable<User>> SearchUsersAsync(string searchTerm);
}
