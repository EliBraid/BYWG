using BYWG.Server.Core;
using BYWG.Server.Core.Services;
using BYWGLib.Protocols;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// 配置为Windows服务（可选）
builder.Host.UseWindowsService();

// 添加gRPC服务
builder.Services.AddGrpc();
builder.Services.AddLogging();

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
    options.ListenLocalhost(5001, listenOptions =>
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

// 配置gRPC路由
app.MapGrpcService<ProtocolManagerService>();
app.MapGrpcService<OpcUaServiceImpl>();

// 配置HTTP API（可选）
app.MapGet("/", () => "BYWG服务端正在运行。请使用gRPC客户端连接。");

app.Run();