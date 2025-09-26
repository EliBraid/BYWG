using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BYWGLib.Logging;

namespace BYWGLib.Protocols
{
    /// <summary>
    /// 异步协议管理器
    /// 统一管理所有异步协议，支持并发轮询和性能监控
    /// </summary>
    public sealed class AsyncProtocolManager : IDisposable
    {
        private readonly List<IAsyncIndustrialProtocol> _protocols;
        private readonly Timer _pollingTimer;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly object _lockObject = new object();
        private volatile bool _isRunning;
        private int _pollingInterval = 1000; // 默认1秒轮询间隔
        
        public AsyncProtocolManager()
        {
            _protocols = new List<IAsyncIndustrialProtocol>();
            _cancellationTokenSource = new CancellationTokenSource();
            
            // 启动轮询定时器
            _pollingTimer = new Timer(PollingCallback, null, Timeout.Infinite, Timeout.Infinite);
        }
        
        /// <summary>
        /// 轮询间隔 (毫秒)
        /// </summary>
        public int PollingInterval
        {
            get => _pollingInterval;
            set
            {
                if (value < 100) throw new ArgumentException("轮询间隔不能小于100毫秒");
                _pollingInterval = value;
                
                if (_isRunning)
                {
                    _pollingTimer.Change(0, _pollingInterval);
                }
            }
        }
        
        /// <summary>
        /// 是否正在运行
        /// </summary>
        public bool IsRunning => _isRunning;
        
        /// <summary>
        /// 协议数量
        /// </summary>
        public int ProtocolCount => _protocols.Count;
        
        /// <summary>
        /// 添加协议
        /// </summary>
        public void AddProtocol(IAsyncIndustrialProtocol protocol)
        {
            if (protocol == null)
                throw new ArgumentNullException(nameof(protocol));
            
            lock (_lockObject)
            {
                if (_protocols.Contains(protocol))
                    return;
                
                _protocols.Add(protocol);
                Log.Information("已添加异步协议: {0}", protocol.Name);
            }
        }
        
        /// <summary>
        /// 移除协议
        /// </summary>
        public void RemoveProtocol(IAsyncIndustrialProtocol protocol)
        {
            if (protocol == null)
                return;
            
            lock (_lockObject)
            {
                if (_protocols.Remove(protocol))
                {
                    Log.Information("已移除异步协议: {0}", protocol.Name);
                }
            }
        }
        
        /// <summary>
        /// 移除协议 (按名称)
        /// </summary>
        public void RemoveProtocol(string name)
        {
            if (string.IsNullOrEmpty(name))
                return;
            
            lock (_lockObject)
            {
                var protocol = _protocols.FirstOrDefault(p => p.Name == name);
                if (protocol != null)
                {
                    _protocols.Remove(protocol);
                    Log.Information("已移除异步协议: {0}", name);
                }
            }
        }
        
        /// <summary>
        /// 获取协议
        /// </summary>
        public IAsyncIndustrialProtocol GetProtocol(string name)
        {
            lock (_lockObject)
            {
                return _protocols.FirstOrDefault(p => p.Name == name);
            }
        }
        
        /// <summary>
        /// 获取所有协议
        /// </summary>
        public IAsyncIndustrialProtocol[] GetAllProtocols()
        {
            lock (_lockObject)
            {
                return _protocols.ToArray();
            }
        }
        
        /// <summary>
        /// 启动协议管理器
        /// </summary>
        public void Start()
        {
            lock (_lockObject)
            {
                if (_isRunning)
                    return;
                
                _isRunning = true;
                
                // 启动所有协议
                foreach (var protocol in _protocols)
                {
                    try
                    {
                        protocol.Start();
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "启动协议 '{0}' 时出错", protocol.Name);
                    }
                }
                
                // 启动轮询定时器
                _pollingTimer.Change(0, _pollingInterval);
                
                Log.Information("异步协议管理器已启动，轮询间隔: {0}ms", _pollingInterval);
            }
        }
        
        /// <summary>
        /// 停止协议管理器
        /// </summary>
        public void Stop()
        {
            lock (_lockObject)
            {
                if (!_isRunning)
                    return;
                
                _isRunning = false;
                
                // 停止轮询定时器
                _pollingTimer.Change(Timeout.Infinite, Timeout.Infinite);
                
                // 停止所有协议
                foreach (var protocol in _protocols)
                {
                    try
                    {
                        protocol.Stop();
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "停止协议 '{0}' 时出错", protocol.Name);
                    }
                }
                
                Log.Information("异步协议管理器已停止");
            }
        }
        
        /// <summary>
        /// 手动触发轮询
        /// </summary>
        public async Task PollAllAsync()
        {
            if (!_isRunning)
                return;
            
            var protocols = GetAllProtocols();
            if (protocols.Length == 0)
                return;
            
            try
            {
                // 并发轮询所有协议
                var tasks = protocols.Select(async protocol =>
                {
                    try
                    {
                        if (protocol.IsRunning)
                        {
                            await protocol.PollDataAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "轮询协议 '{0}' 时出错", protocol.Name);
                    }
                });
                
                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "批量轮询协议时出错");
            }
        }
        
        /// <summary>
        /// 轮询回调
        /// </summary>
        private async void PollingCallback(object state)
        {
            if (!_isRunning)
                return;
            
            try
            {
                await PollAllAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "轮询回调执行时出错");
            }
        }
        
        /// <summary>
        /// 获取协议状态统计
        /// </summary>
        public ProtocolStatusStatistics GetStatusStatistics()
        {
            lock (_lockObject)
            {
                var statistics = new ProtocolStatusStatistics
                {
                    TotalProtocols = _protocols.Count,
                    RunningProtocols = _protocols.Count(p => p.IsRunning),
                    StoppedProtocols = _protocols.Count(p => !p.IsRunning),
                    PollingInterval = _pollingInterval,
                    IsManagerRunning = _isRunning
                };
                
                return statistics;
            }
        }
        
        /// <summary>
        /// 清理资源
        /// </summary>
        public void Dispose()
        {
            Stop();
            
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _pollingTimer?.Dispose();
            
            // 释放所有协议
            foreach (var protocol in _protocols)
            {
                try
                {
                    protocol.Dispose();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "释放协议 '{0}' 时出错", protocol.Name);
                }
            }
            
            _protocols.Clear();
        }
    }
    
    /// <summary>
    /// 异步工业协议接口
    /// </summary>
    public interface IAsyncIndustrialProtocol : IIndustrialProtocol
    {
        /// <summary>
        /// 异步轮询数据
        /// </summary>
        Task PollDataAsync();
    }
    
    /// <summary>
    /// 协议状态统计
    /// </summary>
    public class ProtocolStatusStatistics
    {
        public int TotalProtocols { get; set; }
        public int RunningProtocols { get; set; }
        public int StoppedProtocols { get; set; }
        public int PollingInterval { get; set; }
        public bool IsManagerRunning { get; set; }
        
        public override string ToString()
        {
            return $"总协议数: {TotalProtocols}, 运行中: {RunningProtocols}, 已停止: {StoppedProtocols}, " +
                   $"轮询间隔: {PollingInterval}ms, 管理器状态: {(IsManagerRunning ? "运行中" : "已停止")}";
        }
    }
}
