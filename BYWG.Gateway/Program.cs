using BYWG.Gateway.Services;
using BYWG.Gateway.Data;
using BYWG.Gateway.Hubs;
using Microsoft.EntityFrameworkCore;
using Serilog;
using BYWGLib;

var builder = WebApplication.CreateBuilder(args);

// 配置Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/gateway-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// 添加服务
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 添加数据库（仅使用工厂，避免与池化选项冲突）
builder.Services.AddPooledDbContextFactory<GatewayDbContext>(options =>
{
    var conn = builder.Configuration.GetConnectionString("DefaultConnection");
    if (string.IsNullOrWhiteSpace(conn))
    {
        // 本地默认文件，避免未配置连接串时的空值问题
        conn = "Data Source=gateway.db";
    }
    options.UseSqlite(conn);
});

// 预先创建数据库（在构建应用前，避免托管服务启动时表未创建）
using (var sp = builder.Services.BuildServiceProvider())
{
    var factory = sp.GetRequiredService<IDbContextFactory<GatewayDbContext>>();
    using var ctx = factory.CreateDbContext();
    ctx.Database.EnsureCreated();
}

// 添加SignalR
builder.Services.AddSignalR();
builder.Services.AddHttpClient();

// 添加CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// 注册服务
builder.Services.AddSingleton<ProtocolManager>();
builder.Services.AddHostedService<DataCollectionService>();
builder.Services.AddHostedService<CommunicationService>();
// 已移除不存在的后台服务

var app = builder.Build();

// 配置Swagger为默认启动页
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "BYWG Gateway API v1");
    c.RoutePrefix = string.Empty; // 根路径直接展示Swagger UI
});

app.UseCors("AllowAll");
app.UseRouting();
app.UseAuthorization();

app.MapControllers();
app.MapHub<DataHub>("/dataHub");

// （运行时再次确保一次，防御性处理）
using (var scope = app.Services.CreateScope())
{
    var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<GatewayDbContext>>();
    using var context = factory.CreateDbContext();
    context.Database.EnsureCreated();
}

Log.Information("BYWG Gateway 启动完成");

app.Run();
