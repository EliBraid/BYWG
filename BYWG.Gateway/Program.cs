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

// 添加数据库
builder.Services.AddDbContext<GatewayDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// 添加SignalR
builder.Services.AddSignalR();

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
builder.Services.AddSingleton<DataCollectionService>();
builder.Services.AddSingleton<CommunicationService>();
builder.Services.AddHostedService<GatewayBackgroundService>();

var app = builder.Build();

// 配置管道
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseRouting();
app.UseAuthorization();

app.MapControllers();
app.MapHub<DataHub>("/dataHub");

// 确保数据库创建
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<GatewayDbContext>();
    context.Database.EnsureCreated();
}

Log.Information("BYWG Gateway 启动完成");

app.Run();
