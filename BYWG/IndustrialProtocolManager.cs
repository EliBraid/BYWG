using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Threading.Tasks;
using System.Reflection;
using System.Diagnostics;
using BYWGLib;
using BYWGLib.Protocols;
using BYWG.Utils;

namespace BYWG
{
    /// <summary>
    /// 工业协议管理器 - 使用BYWGLib高性能协议库
    /// </summary>
    public class IndustrialProtocolManager : IDisposable
    {
        private ProtocolManager protocolManager;
        private object lockObject = new object();
        
        public event EventHandler<DataChangedEventArgs> DataChanged;
        
        public IndustrialProtocolManager()
        {
            // 初始化BYWGLib的协议管理器
            protocolManager = new ProtocolManager();
            protocolManager.Initialize();
            
            // 订阅数据变化事件
            protocolManager.DataChanged += OnDataChanged;
        }
        
        /// <summary>
        /// 初始化协议管理器
        /// </summary>
        public void Initialize(int pollingInterval = 1000)
        {
            try
            {
                Log.Information("工业协议管理器初始化完成");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "工业协议管理器初始化失败");
                throw;
            }
        }
        
        /// <summary>
        /// 添加协议配置
        /// </summary>
        public void AddProtocol(string name, string type, string connectionString, Dictionary<string, string> parameters)
        {
            lock (lockObject)
            {
                try
                {
                    var config = new IndustrialProtocolConfig
                    {
                        Name = name,
                        Type = type,
                        Enabled = true, // 默认启用
                        Parameters = parameters ?? new Dictionary<string, string>()
                    };
                    
                    // 根据协议类型设置连接参数
                    switch (type.ToUpper())
                    {
                        case "MODBUS_TCP":
                        case "MODBUSTCP":
                        case "MODBUS":
                            // 确保有必要的参数
                            if (!config.Parameters.ContainsKey("IpAddress"))
                                config.Parameters["IpAddress"] = connectionString;
                            if (!config.Parameters.ContainsKey("Port"))
                                config.Parameters["Port"] = "502";
                            if (!config.Parameters.ContainsKey("SlaveId"))
                                config.Parameters["SlaveId"] = "1";
                            if (!config.Parameters.ContainsKey("Timeout"))
                                config.Parameters["Timeout"] = "3000";
                            break;
                            
                        case "MODBUS_RTU":
                        case "MODBUSRTU":
                            config.Parameters["PortName"] = connectionString;
                            break;
                            
                        case "S7":
                        case "SIEMENS_S7":
                        case "SIEMENSS7":
                        case "SIEMENS":
                            // 确保有必要的参数
                            if (!config.Parameters.ContainsKey("IpAddress"))
                                config.Parameters["IpAddress"] = connectionString;
                            if (!config.Parameters.ContainsKey("Port"))
                                config.Parameters["Port"] = "102";
                            if (!config.Parameters.ContainsKey("Rack"))
                                config.Parameters["Rack"] = "0";
                            if (!config.Parameters.ContainsKey("Slot"))
                                config.Parameters["Slot"] = "1";
                            if (!config.Parameters.ContainsKey("Timeout"))
                                config.Parameters["Timeout"] = "3000";
                            break;
                            
                        case "MC":
                        case "MITSUBISHI_MC":
                        case "MITSUBISHIMC":
                        case "MITSUBISHI":
                            // 确保有必要的参数
                            if (!config.Parameters.ContainsKey("IpAddress"))
                                config.Parameters["IpAddress"] = connectionString;
                            if (!config.Parameters.ContainsKey("Port"))
                                config.Parameters["Port"] = "5007";
                            if (!config.Parameters.ContainsKey("NetworkNo"))
                                config.Parameters["NetworkNo"] = "0";
                            if (!config.Parameters.ContainsKey("PcNo"))
                                config.Parameters["PcNo"] = "255";
                            if (!config.Parameters.ContainsKey("Timeout"))
                                config.Parameters["Timeout"] = "3000";
                            break;
                    }
                    
                    protocolManager.AddProtocol(config);
                    Log.Information("已添加协议: {0} ({1})", name, type);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "添加协议失败: {0}", name);
                    throw;
                }
            }
        }
        
        /// <summary>
        /// 启动指定协议
        /// </summary>
        public void StartProtocol(string protocolName)
        {
            lock (lockObject)
            {
                try
                {
                    protocolManager.StartProtocol(protocolName);
                    Log.Information("协议已启动: {0}", protocolName);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "启动协议失败: {0}", protocolName);
                    throw;
                }
            }
        }
        
        /// <summary>
        /// 停止指定协议
        /// </summary>
        public void StopProtocol(string protocolName)
        {
            lock (lockObject)
            {
                try
                {
                    protocolManager.StopProtocol(protocolName);
                    Log.Information("协议已停止: {0}", protocolName);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "停止协议失败: {0}", protocolName);
                    throw;
                }
            }
        }
        
        /// <summary>
        /// 处理数据变化事件
        /// </summary>
        private void OnDataChanged(object sender, BYWGLib.DataChangedEventArgs e)
        {
            try
            {
                // 转换为BYWG项目的数据变化事件
                var changedItems = e.ChangedItems.Select(item => new IndustrialDataItem
                {
                    Id = item.Id,
                    Name = item.Name,
                    Value = item.Value,
                    DataType = item.DataType,
                    Timestamp = item.Timestamp,
                    Quality = item.Quality
                }).ToList();
                
                DataChanged?.Invoke(this, new DataChangedEventArgs { ChangedItems = changedItems });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "处理数据变化事件时出错");
            }
        }
        
        /// <summary>
        /// 获取活跃协议数量
        /// </summary>
        public int GetActiveProtocolCount()
        {
            lock (lockObject)
            {
                try
                {
                    // 这里需要从BYWGLib的ProtocolManager获取活跃协议数量
                    // 由于BYWGLib的ProtocolManager没有直接提供这个方法，我们返回一个估算值
                    return 0; // 暂时返回0，后续可以扩展BYWGLib来提供这个信息
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "获取活跃协议数量时出错");
                    return 0;
                }
            }
        }
        
        /// <summary>
        /// 获取当前数据传输率
        /// </summary>
        public double GetCurrentDataRate()
        {
            lock (lockObject)
            {
                try
                {
                    return protocolManager.GetCurrentDataRate();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "获取数据传输率时出错");
                    return 0.0;
                }
            }
        }

        /// <summary>
        /// 临时测试读取：根据协议名与地址、数据类型进行一次读取，用于对话框测试
        /// </summary>
        public (bool ok, object value, string message) TryReadValue(string protocolName, string protocolType, string address, string dataType)
        {
            try
            {
                // 从BYWGLib内部拿到协议实例（反射，避免强耦合）
                var pmType = typeof(ProtocolManager);
                var getMethod = pmType.GetMethod("GetProtocol", BindingFlags.Public | BindingFlags.Instance);
                if (getMethod == null)
                {
                    return (false, null, "ProtocolManager缺少GetProtocol方法");
                }
                var proto = getMethod.Invoke(protocolManager, new object[] { protocolName });
                if (proto == null)
                {
                    return (false, null, $"未找到协议: {protocolName}");
                }

                // 优先找同步 Read(address, dataType)
                var readMethod = proto.GetType().GetMethod("Read", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(string), typeof(string) }, null);
                if (readMethod != null)
                {
                    var value = readMethod.Invoke(proto, new object[] { address, dataType });
                    return (true, value, "OK");
                }

                // 备选：若无同步方法，可在此处支持异步 ReadDataPointAsync(address, dataType)
                return (false, null, "协议不支持同步读取");
            }
            catch (Exception ex)
            {
                return (false, null, ex.Message);
            }
        }
        
        /// <summary>
        /// 获取总数据传输量
        /// </summary>
        public long GetTotalBytesTransferred()
        {
            lock (lockObject)
            {
                try
                {
                    return protocolManager.GetTotalBytesTransferred();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "获取总传输量时出错");
                    return 0;
                }
            }
        }
        
        /// <summary>
        /// 获取所有数据项
        /// </summary>
        public List<IndustrialDataItem> GetAllDataItems()
        {
            lock (lockObject)
            {
                try
                {
                    // 这里需要从BYWGLib获取数据项
                    // 暂时返回空列表，后续可以扩展BYWGLib来提供这个功能
                    return new List<IndustrialDataItem>();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "获取数据项时出错");
                    return new List<IndustrialDataItem>();
                }
            }
        }
        
        /// <summary>
        /// 关闭协议管理器
        /// </summary>
        public void Shutdown()
        {
            lock (lockObject)
            {
                try
                {
                    protocolManager?.Dispose();
                    Log.Information("工业协议管理器已关闭");
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "关闭协议管理器时出错");
                }
            }
        }
        
        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            Shutdown();
        }
    }
    
    /// <summary>
    /// 数据变化事件参数
    /// </summary>
    public class DataChangedEventArgs : EventArgs
    {
        public List<IndustrialDataItem> ChangedItems { get; set; } = new List<IndustrialDataItem>();
    }
    
    /// <summary>
    /// 工业数据项
    /// </summary>
    public class IndustrialDataItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public object Value { get; set; }
        public string DataType { get; set; }
        public DateTime Timestamp { get; set; }
        public Quality Quality { get; set; }
    }
    
    /// <summary>
    /// 数据质量枚举
    /// </summary>
    public enum Quality
    {
        Good,
        Bad,
        Uncertain
    }
}