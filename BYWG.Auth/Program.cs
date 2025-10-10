using BYWG.Auth.Data;
using BYWG.Auth.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Serilog;

// 在开发环境中，如果检测到环境变量包含占位符，则临时清除它
// 这样可以确保开发环境使用配置文件，生产环境使用环境变量
if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
{
    var envVar = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
    if (!string.IsNullOrEmpty(envVar) && envVar.Contains("<"))
    {
        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", null);
        Console.WriteLine("开发环境：检测到环境变量包含占位符，已临时清除。将使用配置文件中的连接字符串。");
    }
}

var builder = WebApplication.CreateBuilder(args);

// 配置Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/auth-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// 添加服务
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "BYWG Auth API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new()
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
    });
});

// 添加数据库
builder.Services.AddDbContext<AuthDbContext>(options =>
{
    // 配置优先级：
    // 开发环境：配置文件优先，环境变量作为备选
    // 生产环境：环境变量优先，配置文件作为备选
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException("数据库连接字符串未配置。请在配置文件中配置 DefaultConnection 或设置环境变量 ConnectionStrings__DefaultConnection。");
    }
    
    // 检查连接字符串是否包含占位符
    if (connectionString.Contains("<") && connectionString.Contains(">"))
    {
        Log.Error("检测到连接字符串包含占位符: {ConnectionString}", connectionString);
        throw new InvalidOperationException($"连接字符串包含未替换的占位符。请检查环境变量或配置文件中的连接字符串配置。当前值: {connectionString}");
    }
    
    // 隐藏密码信息用于日志记录
    var safeConnectionString = connectionString;
    var passwordPart = connectionString.Split(';').FirstOrDefault(s => s.StartsWith("Password="));
    if (!string.IsNullOrEmpty(passwordPart))
    {
        safeConnectionString = connectionString.Replace(passwordPart, "Password=***");
    }
    
    // 根据环境显示不同的日志信息
    if (builder.Environment.IsDevelopment())
    {
        Log.Information("开发环境：使用PostgreSQL数据库连接: {ConnectionString}", safeConnectionString);
    }
    else
    {
        Log.Information("生产环境：使用PostgreSQL数据库连接: {ConnectionString}", safeConnectionString);
    }
    
    options.UseNpgsql(connectionString);
});

// 添加身份认证
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JWT:Issuer"],
            ValidAudience = builder.Configuration["JWT:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["JWT:SecretKey"])),
            // 容忍少量时间偏差，减少边界时间401
            ClockSkew = TimeSpan.FromMinutes(2)
        };
    });

// 添加授权
builder.Services.AddAuthorization();

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
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddSingleton<IOnlineUserService, OnlineUserService>();

var app = builder.Build();

// 配置管道
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// 运行时执行迁移（生产推荐）：自动将数据库迁移到最新
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    context.Database.Migrate();
}

Log.Information("BYWG Auth Service 启动完成");

app.Run();
