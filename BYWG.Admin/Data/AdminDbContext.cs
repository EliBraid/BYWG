using Microsoft.EntityFrameworkCore;
using BYWG.Shared.Models;

namespace BYWG.Admin.Data;

/// <summary>
/// 管理后端数据库上下文
/// </summary>
public class AdminDbContext : DbContext
{
    public AdminDbContext(DbContextOptions<AdminDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// 设备表
    /// </summary>
    public DbSet<Device> Devices { get; set; }

    /// <summary>
    /// 网关表
    /// </summary>
    public DbSet<Gateway> Gateways { get; set; }

    /// <summary>
    /// 用户表
    /// </summary>
    public DbSet<User> Users { get; set; }

    /// <summary>
    /// 报警表
    /// </summary>
    public DbSet<Alert> Alerts { get; set; }

    /// <summary>
    /// 数据点表
    /// </summary>
    public DbSet<DataPoint> DataPoints { get; set; }

    /// <summary>
    /// 协议配置表
    /// </summary>
    public DbSet<ProtocolConfig> ProtocolConfigs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 配置设备实体
        modelBuilder.Entity<Device>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
            entity.Property(e => e.IpAddress).IsRequired().HasMaxLength(45);
            entity.Property(e => e.Protocol).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Parameters).HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions)null!),
                v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(v, (System.Text.Json.JsonSerializerOptions)null!) ?? new Dictionary<string, string>()
            );

            entity.HasOne(d => d.Gateway)
                  .WithMany(g => g.Devices)
                  .HasForeignKey(d => d.GatewayId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // 配置网关实体
        modelBuilder.Entity<Gateway>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.IpAddress).IsRequired().HasMaxLength(45);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Version).HasMaxLength(50);

            // 将系统信息配置为 Owned 类型，避免需要主键
            entity.OwnsOne(e => e.SystemInfo, owned =>
            {
                owned.Property(p => p.CpuUsage);
                owned.Property(p => p.MemoryUsage);
                owned.Property(p => p.DiskUsage);
                owned.Property(p => p.NetworkTraffic);
                owned.Property(p => p.Uptime);
                owned.Property(p => p.OperatingSystem);
                owned.Property(p => p.DeviceCount);
                owned.Property(p => p.ProtocolCount);
            });
        });

        // 配置用户实体
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Department).HasMaxLength(100);
            entity.Property(e => e.Role).IsRequired().HasMaxLength(50);

            // 将用户偏好设置配置为 Owned 类型
            entity.OwnsOne(e => e.Preferences, owned =>
            {
                owned.Property(p => p.Language);
                owned.Property(p => p.Timezone);
                owned.Property(p => p.Theme);
                owned.Property(p => p.TwoFactorEnabled);
                owned.Property(p => p.LoginNotifications);
            });
        });

        // 配置报警实体
        modelBuilder.Entity<Alert>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Message).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.Source).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Level).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
        });

        // 配置数据点实体
        modelBuilder.Entity<DataPoint>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Address).IsRequired().HasMaxLength(100);
            entity.Property(e => e.DataType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(500);

            entity.HasOne(d => d.Device)
                  .WithMany()
                  .HasForeignKey(d => d.DeviceId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // 配置协议配置实体
        modelBuilder.Entity<ProtocolConfig>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ProtocolType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Parameters).HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions)null!),
                v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(v, (System.Text.Json.JsonSerializerOptions)null!) ?? new Dictionary<string, string>()
            );
        });
    }
}

/// <summary>
/// 报警模型
/// </summary>
public class Alert
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? AcknowledgedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public int? DeviceId { get; set; }
    public int? GatewayId { get; set; }
}

/// <summary>
/// 数据点模型
/// </summary>
public class DataPoint
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsEnabled { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public int DeviceId { get; set; }
    public Device? Device { get; set; }
}

/// <summary>
/// 协议配置模型
/// </summary>
public class ProtocolConfig
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ProtocolType { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsEnabled { get; set; } = true;
    public Dictionary<string, string> Parameters { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
