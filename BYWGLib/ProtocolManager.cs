using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Threading.Tasks;
using BYWGLib.Logging;
using BYWGLib.Protocols;

namespace BYWGLib
{
    /// <summary>
    /// 协议管理器
    /// </summary>
    public class ProtocolManager : IDisposable
    {
        private Dictionary<string, IIndustrialProtocol> protocols = new Dictionary<string, IIndustrialProtocol>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<string, IndustrialDataItem> dataItems = new Dictionary<string, IndustrialDataItem>();
        private Dictionary<string, System.Timers.Timer> pollingTimers = new Dictionary<string, System.Timers.Timer>();
        private readonly object lockObject = new object();
        private long totalBytesTransferred = 0;
        private double currentDataRate = 0;
        private long lastBytesTransferred = 0;
        private DateTime lastRateUpdateTime = DateTime.Now;
        
        public event EventHandler<DataChangedEventArgs> DataChanged;
        
        public ProtocolManager()
        {
            // 初始化数据速率监控
            var rateTimer = new System.Timers.Timer(1000); // 每秒更新一次数据速率
            rateTimer.Elapsed += (s, e) => UpdateDataRate();
            rateTimer.Start();
        }
        
        /// <summary>
        /// 初始化协议管理器
        /// </summary>
        public void Initialize()
        {
            // 在这里可以从配置文件加载协议配置
            Log.Information("协议管理器已初始化");
        }
        
        /// <summary>
        /// 添加一个工业协议
        /// </summary>
        public void AddProtocol(IndustrialProtocolConfig config)
        {
            lock (lockObject)
            {
                var name = (config?.Name ?? string.Empty).Trim();
                if (string.IsNullOrEmpty(name))
                {
                    throw new ArgumentException("协议名称不能为空");
                }
                config.Name = name;

                if (protocols.ContainsKey(config.Name))
                {
                    throw new ArgumentException($"协议 '{config.Name}' 已经存在");
                }
                
                // 根据协议类型创建实例
                IIndustrialProtocol protocol = CreateProtocolInstance(config);
                protocols[config.Name] = protocol;
                
                // 如果协议已启用，则启动它
                if (config.Enabled)
                {
                    StartProtocol(config.Name);
                }
                
                Log.Information("已添加协议: {0}", config.Name);
            }
        }

        public bool RemoveProtocol(string protocolName)
        {
            lock (lockObject)
            {
                var key = (protocolName ?? string.Empty).Trim();
                if (!protocols.TryGetValue(key, out var protocol))
                {
                    return false;
                }
                StopPolling(key);
                if (protocol.IsRunning)
                {
                    protocol.Stop();
                }
                protocol.Dispose();
                protocols.Remove(key);
                Log.Information("已移除协议: {0}", key);
                return true;
            }
        }

        public bool TryGetProtocol(string protocolName, out IIndustrialProtocol protocol)
        {
            lock (lockObject)
            {
                var key = (protocolName ?? string.Empty).Trim();
                return protocols.TryGetValue(key, out protocol);
            }
        }

        public void StartAllProtocols()
        {
            lock (lockObject)
            {
                foreach (var name in protocols.Keys.ToList())
                {
                    StartProtocol(name);
                }
            }
        }

        public void StopAllProtocols()
        {
            lock (lockObject)
            {
                foreach (var name in protocols.Keys.ToList())
                {
                    StopProtocol(name);
                }
            }
        }

        public void StartPolling()
        {
            lock (lockObject)
            {
                foreach (var kv in protocols)
                {
                    if (kv.Value.Config.Parameters.ContainsKey("PollingInterval") &&
                        int.TryParse(kv.Value.Config.Parameters["PollingInterval"], out int interval) &&
                        interval > 0)
                    {
                        StartPolling(kv.Key, interval);
                    }
                }
            }
        }

        public void StopPolling()
        {
            lock (lockObject)
            {
                foreach (var name in pollingTimers.Keys.ToList())
                {
                    StopPolling(name);
                }
            }
        }
        
        /// <summary>
        /// 根据协议配置创建协议实例
        /// </summary>
        /// <param name="config">协议配置</param>
        /// <returns>协议实例</returns>
        protected virtual IIndustrialProtocol CreateProtocolInstance(IndustrialProtocolConfig config)
        {
            return HighPerformanceProtocolFactory.CreateProtocol(config);
        }
        
        /// <summary>
        /// 启动指定的协议
        /// </summary>
        /// <param name="protocolName">协议名称</param>
        public void StartProtocol(string protocolName)
        {
            lock (lockObject)
            {
                if (protocols.TryGetValue(protocolName, out var protocol))
                {
                    if (!protocol.IsRunning)
                    {
                        protocol.Start();
                        protocol.DataReceived += Protocol_DataReceived;
                        
                        // 启动数据轮询（如果配置了轮询间隔）
                        if (protocol.Config.Parameters.ContainsKey("PollingInterval") &&
                            int.TryParse(protocol.Config.Parameters["PollingInterval"], out int interval) &&
                            interval > 0)
                        {
                            StartPolling(protocolName, interval);
                        }
                        
                        Log.Information("已启动协议: {0}", protocolName);
                    }
                }
                else
                {
                    throw new ArgumentException($"找不到协议: {protocolName}");
                }
            }
        }
        
        /// <summary>
        /// 停止指定的协议
        /// </summary>
        /// <param name="protocolName">协议名称</param>
        public void StopProtocol(string protocolName)
        {
            lock (lockObject)
            {
                if (protocols.TryGetValue(protocolName, out var protocol))
                {
                    if (protocol.IsRunning)
                    {
                        // 停止轮询
                        StopPolling(protocolName);
                        
                        protocol.Stop();
                        protocol.DataReceived -= Protocol_DataReceived;
                        
                        Log.Information("已停止协议: {0}", protocolName);
                    }
                }
                else
                {
                    throw new ArgumentException($"找不到协议: {protocolName}");
                }
            }
        }
        
        /// <summary>
        /// 启动数据轮询
        /// </summary>
        /// <param name="protocolName">协议名称</param>
        /// <param name="intervalMs">轮询间隔(毫秒)</param>
        private void StartPolling(string protocolName, int intervalMs)
        {
            if (pollingTimers.ContainsKey(protocolName))
            {
                pollingTimers[protocolName].Stop();
                pollingTimers[protocolName].Dispose();
            }
            
            var timer = new System.Timers.Timer(intervalMs);
            timer.Elapsed += (s, e) =>
            {
                try
                {
                    if (protocols.TryGetValue(protocolName, out var protocol))
                    {
                        protocol.PollData();
                    }
                }
                catch (Exception ex)
                {
                    // 记录异常日志
                    Log.Error(ex, "协议 {0} 轮询异常", protocolName);
                }
            };
            
            pollingTimers[protocolName] = timer;
            timer.Start();
        }
        
        /// <summary>
        /// 停止数据轮询
        /// </summary>
        /// <param name="protocolName">协议名称</param>
        private void StopPolling(string protocolName)
        {
            if (pollingTimers.TryGetValue(protocolName, out var timer))
            {
                timer.Stop();
                timer.Dispose();
                pollingTimers.Remove(protocolName);
            }
        }
        
        /// <summary>
        /// 处理协议接收到的数据
        /// </summary>
        private void Protocol_DataReceived(object sender, DataReceivedEventArgs e)
        {
            lock (lockObject)
            {
                Log.Debug("ProtocolManager收到DataReceived事件: 协议={0}, 数据项数量={1}", e.ProtocolName, e.DataItems.Count);
                
                // 更新数据项
                var changedItems = new List<IndustrialDataItem>();
                
                foreach (var item in e.DataItems)
                {
                    string key = $"{e.ProtocolName}.{item.Name}";
                    
                    // 第一次读取或者数据值发生变化时，都触发事件
                    if (!dataItems.TryGetValue(key, out var existingItem) ||
                        !Equals(existingItem.Value, item.Value))
                    {
                        dataItems[key] = item;
                        changedItems.Add(item);
                    }
                }
                
                // 触发数据变化事件
                if (changedItems.Count > 0 && DataChanged != null)
                {
                    Log.Debug("触发DataChanged事件: 变化项数量={0}", changedItems.Count);
                    foreach (var item in changedItems)
                    {
                        Log.Debug("变化项: Id={0}, Name={1}, Value={2}", item.Id, item.Name, item.Value);
                    }
                    DataChanged(this, new DataChangedEventArgs(changedItems));
                }
                else
                {
                    Log.Debug("没有变化项或DataChanged事件为空，跳过事件触发");
                }
                
                // 更新数据传输统计
                totalBytesTransferred += e.DataItems.Sum(item => GetValueSize(item.Value));
            }
        }
        
        /// <summary>
        /// 获取值的大小(字节)
        /// </summary>
        private int GetValueSize(object value)
        {
            if (value == null)
                return 0;
                
            if (value is bool)
                return 1;
            else if (value is byte || value is sbyte)
                return 1;
            else if (value is short || value is ushort)
                return 2;
            else if (value is int || value is uint || value is float)
                return 4;
            else if (value is long || value is ulong || value is double)
                return 8;
            else if (value is string str)
                return str.Length * 2;
            else
                return 16; // 其他类型的默认大小
        }
        
        /// <summary>
        /// 更新数据速率
        /// </summary>
        private void UpdateDataRate()
        {
            lock (lockObject)
            {
                DateTime now = DateTime.Now;
                double elapsedSeconds = (now - lastRateUpdateTime).TotalSeconds;
                
                if (elapsedSeconds > 0)
                {
                    long bytesSinceLastUpdate = totalBytesTransferred - lastBytesTransferred;
                    currentDataRate = bytesSinceLastUpdate / elapsedSeconds;
                    
                    lastBytesTransferred = totalBytesTransferred;
                    lastRateUpdateTime = now;
                }
            }
        }
        
        /// <summary>
        /// 获取当前数据速率
        /// </summary>
        /// <returns>数据速率(字节/秒)</returns>
        public double GetCurrentDataRate()
        {
            lock (lockObject)
            {
                return currentDataRate;
            }
        }
        
        /// <summary>
        /// 获取总数据传输量
        /// </summary>
        /// <returns>总数据传输量(字节)</returns>
        public long GetTotalBytesTransferred()
        {
            lock (lockObject)
            {
                return totalBytesTransferred;
            }
        }
        
        /// <summary>
        /// 清理资源
        /// </summary>
        public void Dispose()
        {
            // 停止所有协议
            foreach (var protocolName in protocols.Keys.ToList())
            {
                try
                {
                    StopProtocol(protocolName);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "停止协议 {0} 时出错", protocolName);
                }
            }
            
            // 清理轮询定时器
            foreach (var timer in pollingTimers.Values)
            {
                timer.Stop();
                timer.Dispose();
            }
            
            pollingTimers.Clear();
            protocols.Clear();
            dataItems.Clear();
        }
    }
}