using Microsoft.EntityFrameworkCore;
using BYWG.Shared.Models;

namespace BYWG.Gateway.Data;

/// <summary>
/// 网关数据库上下文
/// </summary>
public class GatewayDbContext : DbContext
{
    public GatewayDbContext(DbContextOptions<GatewayDbContext> options) : base(options)
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
