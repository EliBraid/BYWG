using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BYWGLib.Protocols;
using BYWGLib.Logging;
using BYWGLib;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BYWG.Server.Core.Services
{
    /// <summary>
    /// 边缘网关服务 - 核心服务
    /// </summary>
    public class EdgeGatewayService : BackgroundService
    {
        private readonly ILogger<EdgeGatewayService> _logger;
        private readonly ProtocolManagerService _protocolManager;
        private readonly DataProcessingService _dataProcessing;
        private readonly DeviceManagementService _deviceManagement;
        
        private readonly Dictionary<string, IIndustrialProtocol> _activeProtocols;
        private readonly Timer _healthCheckTimer;
        private readonly Timer _dataCollectionTimer;
        
        public EdgeGatewayService(
            ILogger<EdgeGatewayService> logger,
            ProtocolManagerService protocolManager,
            DataProcessingService dataProcessing,
            DeviceManagementService deviceManagement)
        {
            _logger = logger;
            _protocolManager = protocolManager;
            _dataProcessing = dataProcessing;
            _deviceManagement = deviceManagement;
            
            _activeProtocols = new Dictionary<string, IIndustrialProtocol>();
            
            // 健康检查定时器 - 每30秒
            _healthCheckTimer = new Timer(HealthCheckCallback, null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
            
            // 数据采集定时器 - 每1秒
            _dataCollectionTimer = new Timer(DataCollectionCallback, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
        }
        
        /// <summary>
        /// 启动边缘网关服务
        /// </summary>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("边缘网关服务启动中...");
            
            try
            {
                // 初始化设备管理
                await _deviceManagement.InitializeAsync();
                
                // 启动数据采集
                await StartDataCollectionAsync();
                
                _logger.LogInformation("边缘网关服务已启动");
                
                // 保持服务运行
                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(1000, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "边缘网关服务启动失败");
                throw;
            }
        }
        
        /// <summary>
        /// 添加协议实例
        /// </summary>
        public async Task<bool> AddProtocolAsync(IndustrialProtocolConfig config)
        {
            try
            {
                var protocol = HighPerformanceProtocolFactory.CreateProtocol(config);
                
                if (protocol != null)
                {
                    _activeProtocols[config.Name] = protocol;
                    protocol.Start();
                    
                    _logger.LogInformation("协议 '{0}' 已添加到边缘网关", config.Name);
                    return true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "添加协议 '{0}' 失败", config.Name);
                return false;
            }
        }
        
        /// <summary>
        /// 移除协议实例
        /// </summary>
        public async Task<bool> RemoveProtocolAsync(string protocolName)
        {
            try
            {
                if (_activeProtocols.TryGetValue(protocolName, out var protocol))
                {
                    protocol.Stop();
                    protocol.Dispose();
                    _activeProtocols.Remove(protocolName);
                    
                    _logger.LogInformation("协议 '{0}' 已从边缘网关移除", protocolName);
                    return true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "移除协议 '{0}' 失败", protocolName);
                return false;
            }
        }
        
        /// <summary>
        /// 获取所有活跃协议
        /// </summary>
        public Dictionary<string, IIndustrialProtocol> GetActiveProtocols()
        {
            return new Dictionary<string, IIndustrialProtocol>(_activeProtocols);
        }
        
        /// <summary>
        /// 启动数据采集
        /// </summary>
        private async Task StartDataCollectionAsync()
        {
            _logger.LogInformation("启动数据采集...");
            
            foreach (var protocol in _activeProtocols.Values)
            {
                try
                {
                    protocol.Start();
                    _logger.LogInformation("协议 '{0}' 数据采集已启动", protocol.Name);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "启动协议 '{0}' 数据采集失败", protocol.Name);
                }
            }
        }
        
        /// <summary>
        /// 健康检查回调
        /// </summary>
        private void HealthCheckCallback(object state)
        {
            try
            {
                foreach (var kvp in _activeProtocols.ToList())
                {
                    var protocol = kvp.Value;
                    
                    if (!protocol.IsRunning)
                    {
                        _logger.LogWarning("协议 '{0}' 运行状态异常，尝试重启", protocol.Name);
                        
                        try
                        {
                            protocol.Stop();
                            protocol.Start();
                            _logger.LogInformation("协议 '{0}' 已重启", protocol.Name);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "重启协议 '{0}' 失败", protocol.Name);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "健康检查失败");
            }
        }
        
        /// <summary>
        /// 数据采集回调
        /// </summary>
        private void DataCollectionCallback(object state)
        {
            try
            {
                foreach (var protocol in _activeProtocols.Values)
                {
                    if (protocol.IsRunning)
                    {
                        try
                        {
                            // 触发数据轮询
                            protocol.PollData();
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "协议 '{0}' 数据采集失败", protocol.Name);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "数据采集回调失败");
            }
        }
        
        /// <summary>
        /// 停止服务
        /// </summary>
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("停止边缘网关服务...");
            
            // 停止所有协议
            foreach (var protocol in _activeProtocols.Values)
            {
                try
                {
                    protocol.Stop();
                    protocol.Dispose();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "停止协议失败");
                }
            }
            
            _activeProtocols.Clear();
            
            // 停止定时器
            _healthCheckTimer?.Dispose();
            _dataCollectionTimer?.Dispose();
            
            await base.StopAsync(cancellationToken);
            _logger.LogInformation("边缘网关服务已停止");
        }
    }
}
