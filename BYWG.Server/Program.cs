using BYWG.Server.Core;
using BYWG.Server.Core.Services;
using BYWGLib.Protocols;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using BYWG.Server.Models;
using Grpc.AspNetCore.Web;

var builder = WebApplication.CreateBuilder(args);

// 配置为Windows服务（可选）
builder.Host.UseWindowsService();

// 添加gRPC服务
builder.Services.AddGrpc();
builder.Services.AddLogging();
// 允许浏览器来源（gRPC-Web 需要 CORS）
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", p => p
        .AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod());
});

// 添加边缘网关服务
builder.Services.AddSingleton<EdgeGatewayService>();
builder.Services.AddSingleton<ProtocolManagerService>();
builder.Services.AddSingleton<OpcUaServiceImpl>();
builder.Services.AddSingleton<DataProcessingService>();
builder.Services.AddSingleton<DeviceManagementService>();
builder.Services.AddSingleton<AlarmService>();

// 配置Kestrel以同时支持HTTP/2和HTTP/1.1
builder.WebHost.ConfigureKestrel(options =>
{
    // HTTP/2 (h2c) for gRPC
    options.ListenLocalhost(5002, listenOptions =>
    {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
    });
    // HTTP/1.1 for health
    options.ListenLocalhost(5000, listenOptions =>
    {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1;
    });
});

var app = builder.Build();

// 启用 CORS 与 gRPC-Web
app.UseCors("AllowAll");
app.UseGrpcWeb();

// 配置gRPC路由
app.MapGrpcService<ProtocolManagerService>()
   .EnableGrpcWeb()
   .RequireCors("AllowAll");
app.MapGrpcService<OpcUaServiceImpl>()
   .EnableGrpcWeb()
   .RequireCors("AllowAll");

// 配置HTTP API（可选）
app.MapGet("/", () => "BYWG服务端正在运行。请使用gRPC客户端连接。");

// ====== REST 适配器，直接调用核心服务 ======
var deviceService = app.Services.GetRequiredService<DeviceManagementService>();
_ = deviceService.InitializeAsync();

app.MapGet("/devices", () =>
{
    var infos = deviceService.GetAllDevices();
    var statuses = deviceService.GetAllDeviceStatuses().ToDictionary(s => s.DeviceId, s => s);
    var result = infos.Select(i => new DeviceDto
    {
        Id = i.DeviceId,
        Name = i.DeviceName,
        Ip = i.IpAddress,
        Status = statuses.TryGetValue(i.DeviceId, out var st) && st.IsOnline ? "online" : "offline"
    }).ToList();
    return Results.Ok(result);
});

app.MapGet("/devices/{id}", (string id) =>
{
    var info = deviceService.GetDevice(id);
    if (info is null) return Results.NotFound();
    var status = deviceService.GetDeviceStatus(id);
    var dto = new DeviceDto
    {
        Id = info.DeviceId,
        Name = info.DeviceName,
        Ip = info.IpAddress,
        Status = (status != null && status.IsOnline) ? "online" : "offline"
    };
    return Results.Ok(dto);
});

app.MapPost("/devices", async (DeviceCreateRequest request) =>
{
    var newInfo = new DeviceInfo
    {
        DeviceId = Guid.NewGuid().ToString("N"),
        DeviceName = string.IsNullOrWhiteSpace(request.Name) ? "未命名设备" : request.Name!,
        DeviceType = string.IsNullOrWhiteSpace(request.Protocol) ? "Unknown" : request.Protocol!,
        IpAddress = request.Ip ?? "",
        Port = 0,
        Description = request.ConnectionString ?? string.Empty
    };
    var ok = await deviceService.RegisterDeviceAsync(newInfo);
    if (!ok) return Results.BadRequest();
    return Results.Ok(new DeviceDto
    {
        Id = newInfo.DeviceId,
        Name = newInfo.DeviceName,
        Ip = newInfo.IpAddress,
        Protocol = newInfo.DeviceType,
        ConnectionString = newInfo.Description,
        Status = "offline"
    });
});

app.MapPut("/devices/{id}", (string id, DeviceCreateRequest request) =>
{
    var info = deviceService.GetDevice(id);
    if (info is null) return Results.NotFound();
    if (!string.IsNullOrWhiteSpace(request.Name)) info.DeviceName = request.Name!;
    if (!string.IsNullOrWhiteSpace(request.Ip)) info.IpAddress = request.Ip!;
    if (!string.IsNullOrWhiteSpace(request.Protocol)) info.DeviceType = request.Protocol!;
    if (!string.IsNullOrWhiteSpace(request.ConnectionString)) info.Description = request.ConnectionString!;
    var status = deviceService.GetDeviceStatus(id);
    if (status != null && !string.IsNullOrWhiteSpace(request.Status))
    {
        status.IsOnline = string.Equals(request.Status, "online", StringComparison.OrdinalIgnoreCase);
        status.LastSeen = DateTime.Now;
    }
    var dto = new DeviceDto
    {
        Id = info.DeviceId,
        Name = info.DeviceName,
        Ip = info.IpAddress,
        Protocol = info.DeviceType,
        ConnectionString = info.Description,
        Status = (status != null && status.IsOnline) ? "online" : "offline"
    };
    return Results.Ok(dto);
});

app.MapDelete("/devices/{id}", async (string id) =>
{
    var ok = await deviceService.UnregisterDeviceAsync(id);
    return ok ? Results.NoContent() : Results.NotFound();
});
// ====== REST 适配器结束 ======

// 简单连通性测试（例如 Modbus TCP 502 端口）
app.MapPost("/devices/test-connection", async (TestConnectionRequest req) =>
{
    var host = req.Ip;
    var port = req.Port ?? (string.Equals(req.Protocol, "Modbus TCP", StringComparison.OrdinalIgnoreCase) ? 502 : 0);
    if (string.IsNullOrWhiteSpace(host) || port <= 0)
    {
        return Results.BadRequest(new TestConnectionResponse { Success = false, Message = "缺少 IP 或端口" });
    }
    var sw = System.Diagnostics.Stopwatch.StartNew();
    try
    {
        using var client = new System.Net.Sockets.TcpClient();
        var task = client.ConnectAsync(host, port);
        var completed = await Task.WhenAny(task, Task.Delay(TimeSpan.FromSeconds(3)));
        if (completed != task || !client.Connected)
        {
            return Results.Ok(new TestConnectionResponse { Success = false, Message = "连接超时", LatencyMs = sw.ElapsedMilliseconds });
        }
        sw.Stop();
        return Results.Ok(new TestConnectionResponse { Success = true, Message = "连接成功", LatencyMs = sw.ElapsedMilliseconds });
    }
    catch (Exception ex)
    {
        sw.Stop();
        return Results.Ok(new TestConnectionResponse { Success = false, Message = ex.Message, LatencyMs = sw.ElapsedMilliseconds });
    }
});

app.Run();