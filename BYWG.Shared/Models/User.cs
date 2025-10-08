using System.ComponentModel.DataAnnotations;

namespace BYWG.Shared.Models;

/// <summary>
/// 用户模型
/// </summary>
public class User
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// 邮箱
    /// </summary>
    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// 真实姓名
    /// </summary>
    [StringLength(100)]
    public string? FullName { get; set; }

    /// <summary>
    /// 手机号码
    /// </summary>
    [StringLength(20)]
    public string? Phone { get; set; }

    /// <summary>
    /// 部门
    /// </summary>
    [StringLength(100)]
    public string? Department { get; set; }

    /// <summary>
    /// 密码哈希
    /// </summary>
    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// 角色
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Role { get; set; } = "User";

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 最后登录时间
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// 登录次数
    /// </summary>
    public int LoginCount { get; set; } = 0;

    /// <summary>
    /// 用户偏好设置
    /// </summary>
    public UserPreferences? Preferences { get; set; }
}

/// <summary>
/// 用户偏好设置
/// </summary>
public class UserPreferences
{
    /// <summary>
    /// 偏好设置ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 用户ID
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// 语言设置
    /// </summary>
    public string Language { get; set; } = "zh-CN";

    /// <summary>
    /// 时区设置
    /// </summary>
    public string Timezone { get; set; } = "Asia/Shanghai";

    /// <summary>
    /// 主题设置
    /// </summary>
    public string Theme { get; set; } = "light";

    /// <summary>
    /// 是否启用两步验证
    /// </summary>
    public bool TwoFactorEnabled { get; set; } = false;

    /// <summary>
    /// 是否启用登录通知
    /// </summary>
    public bool LoginNotifications { get; set; } = true;
}

/// <summary>
/// 用户角色枚举
/// </summary>
public enum UserRole
{
    /// <summary>
    /// 管理员
    /// </summary>
    Admin = 0,

    /// <summary>
    /// 操作员
    /// </summary>
    Operator = 1,

    /// <summary>
    /// 观察者
    /// </summary>
    Viewer = 2
}
