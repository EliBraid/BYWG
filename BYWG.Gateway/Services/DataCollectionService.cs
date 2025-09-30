using BYWGLib;
using BYWG.Gateway.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace BYWG.Gateway.Services;

/// <summary>
/// 数据采集服务
/// </summary>
public class DataCollectionService : IHostedService, IDisposable
{
    private readonly ProtocolManager _protocolManager;
    private readonly GatewayDbContext _context;
    private readonly ILogger<DataCollectionService> _logger;
    private readonly Timer _collectionTimer;
    private readonly ConcurrentDictionary<string, object> _latestData = new();

    public DataCollectionService(
        ProtocolManager protocolManager,
        GatewayDbContext context,
        ILogger<DataCollectionService> logger)
    {
        _protocolManager = protocolManager;
        _context = context;
        _logger = logger;
        _collectionTimer = new Timer(CollectData, null, Timeout.Infinite, Timeout.Infinite);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("数据采集服务启动");

        // 从数据库加载协议配置
        await LoadProtocolConfigurations();

        // 启动协议管理器
        _protocolManager.StartAllProtocols();
        _protocolManager.StartPolling();

        // 订阅数据变化事件
        _protocolManager.DataChanged += OnDataChanged;

        // 启动数据采集定时器（每5秒采集一次）
        _collectionTimer.Change(TimeSpan.Zero, TimeSpan.FromSeconds(5));

        _logger.LogInformation("数据采集服务启动完成");
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("数据采集服务停止");

        _collectionTimer.Change(Timeout.Infinite, Timeout.Infinite);
        _protocolManager.StopAllProtocols();
        _protocolManager.StopPolling();
        _protocolManager.DataChanged -= OnDataChanged;

        _logger.LogInformation("数据采集服务停止完成");
    }

    private async Task LoadProtocolConfigurations()
    {
        try
        {
            var configs = await _context.ProtocolConfigs
                .Where(p => p.IsEnabled)
                .ToListAsync();

            foreach (var config in configs)
            {
                var protocolConfig = new IndustrialProtocolConfig
                {
                    Name = config.Name,
                    ProtocolType = config.ProtocolType,
                    Parameters = config.Parameters,
                    Enabled = config.IsEnabled
                };

                _protocolManager.AddProtocol(protocolConfig);
                _logger.LogInformation("加载协议配置: {ProtocolName}", config.Name);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "加载协议配置失败");
        }
    }

    private void CollectData(object? state)
    {
        try
        {
            // 获取所有启用的协议
            var protocols = _context.ProtocolConfigs
                .Where(p => p.IsEnabled)
                .ToList();

            foreach (var protocol in protocols)
            {
                if (_protocolManager.TryGetProtocol(protocol.Name, out var protocolInstance))
                {
                    try
                    {
                        protocolInstance.PollData();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "协议 {ProtocolName} 数据采集失败", protocol.Name);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "数据采集定时器执行失败");
        }
    }

    private void OnDataChanged(object? sender, DataChangedEventArgs e)
    {
        try
        {
            foreach (var item in e.DataItems)
            {
                var key = $"{item.ProtocolName}.{item.Name}";
                _latestData[key] = item.Value;

                _logger.LogDebug("数据更新: {Key} = {Value}", key, item.Value);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "处理数据变化事件失败");
        }
    }

    /// <summary>
    /// 获取最新数据
    /// </summary>
    public Dictionary<string, object> GetLatestData()
    {
        return _latestData.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    /// <summary>
    /// 获取指定协议的数据
    /// </summary>
    public Dictionary<string, object> GetProtocolData(string protocolName)
    {
        return _latestData
            .Where(kvp => kvp.Key.StartsWith($"{protocolName}."))
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    public void Dispose()
    {
        _collectionTimer?.Dispose();
    }
}
