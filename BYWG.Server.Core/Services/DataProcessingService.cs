using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BYWGLib;
using Microsoft.Extensions.Logging;

namespace BYWG.Server.Core.Services
{
    /// <summary>
    /// 数据处理服务 - 边缘计算数据处理
    /// </summary>
    public class DataProcessingService
    {
        private readonly ILogger<DataProcessingService> _logger;
        private readonly Dictionary<string, List<IndustrialDataItem>> _dataBuffer;
        private readonly Dictionary<string, DateTime> _lastUpdateTime;
        
        public event EventHandler<DataProcessedEventArgs> DataProcessed;
        
        public DataProcessingService(ILogger<DataProcessingService> logger)
        {
            _logger = logger;
            _dataBuffer = new Dictionary<string, List<IndustrialDataItem>>();
            _lastUpdateTime = new Dictionary<string, DateTime>();
        }
        
        /// <summary>
        /// 处理工业数据
        /// </summary>
        public async Task ProcessDataAsync(string protocolName, List<IndustrialDataItem> dataItems)
        {
            try
            {
                // 数据预处理
                var processedData = await PreprocessDataAsync(dataItems);
                
                // 数据验证
                var validatedData = await ValidateDataAsync(processedData);
                
                // 数据转换
                var convertedData = await ConvertDataAsync(validatedData);
                
                // 数据存储
                await StoreDataAsync(protocolName, convertedData);
                
                // 触发事件
                DataProcessed?.Invoke(this, new DataProcessedEventArgs
                {
                    ProtocolName = protocolName,
                    DataItems = convertedData,
                    Timestamp = DateTime.Now
                });
                
                _logger.LogDebug("协议 '{0}' 数据处理完成，共 {1} 个数据点", protocolName, convertedData.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理协议 '{0}' 数据失败", protocolName);
            }
        }
        
        /// <summary>
        /// 数据预处理
        /// </summary>
        private async Task<List<IndustrialDataItem>> PreprocessDataAsync(List<IndustrialDataItem> dataItems)
        {
            var processedItems = new List<IndustrialDataItem>();
            
            foreach (var item in dataItems)
            {
                // 数据清洗
                if (item.Quality == Quality.Good)
                {
                    // 数据平滑处理
                    var smoothedValue = await SmoothValueAsync(item);
                    
                    // 异常值检测
                    if (await IsValidValueAsync(item, smoothedValue))
                    {
                        processedItems.Add(new IndustrialDataItem
                        {
                            Id = item.Id,
                            Name = item.Name,
                            Value = smoothedValue,
                            DataType = item.DataType,
                            Timestamp = DateTime.Now,
                            Quality = Quality.Good
                        });
                    }
                }
            }
            
            return processedItems;
        }
        
        /// <summary>
        /// 数据验证
        /// </summary>
        private async Task<List<IndustrialDataItem>> ValidateDataAsync(List<IndustrialDataItem> dataItems)
        {
            var validatedItems = new List<IndustrialDataItem>();
            
            foreach (var item in dataItems)
            {
                // 范围检查
                if (await IsInValidRangeAsync(item))
                {
                    // 时间戳检查
                    if (await IsValidTimestampAsync(item))
                    {
                        validatedItems.Add(item);
                    }
                }
            }
            
            return validatedItems;
        }
        
        /// <summary>
        /// 数据转换
        /// </summary>
        private async Task<List<IndustrialDataItem>> ConvertDataAsync(List<IndustrialDataItem> dataItems)
        {
            var convertedItems = new List<IndustrialDataItem>();
            
            foreach (var item in dataItems)
            {
                // 单位转换
                var convertedValue = await ConvertUnitAsync(item);
                
                // 数据类型转换
                var convertedItem = new IndustrialDataItem
                {
                    Id = item.Id,
                    Name = item.Name,
                    Value = convertedValue,
                    DataType = item.DataType,
                    Timestamp = item.Timestamp,
                    Quality = item.Quality
                };
                
                convertedItems.Add(convertedItem);
            }
            
            return convertedItems;
        }
        
        /// <summary>
        /// 数据存储
        /// </summary>
        private async Task StoreDataAsync(string protocolName, List<IndustrialDataItem> dataItems)
        {
            // 更新数据缓冲区
            if (!_dataBuffer.ContainsKey(protocolName))
            {
                _dataBuffer[protocolName] = new List<IndustrialDataItem>();
            }
            
            _dataBuffer[protocolName].AddRange(dataItems);
            _lastUpdateTime[protocolName] = DateTime.Now;
            
            // 保持缓冲区大小
            if (_dataBuffer[protocolName].Count > 1000)
            {
                _dataBuffer[protocolName] = _dataBuffer[protocolName]
                    .OrderByDescending(x => x.Timestamp)
                    .Take(500)
                    .ToList();
            }
        }
        
        /// <summary>
        /// 获取协议数据
        /// </summary>
        public List<IndustrialDataItem> GetProtocolData(string protocolName)
        {
            if (_dataBuffer.TryGetValue(protocolName, out var data))
            {
                return new List<IndustrialDataItem>(data);
            }
            
            return new List<IndustrialDataItem>();
        }
        
        /// <summary>
        /// 获取所有数据
        /// </summary>
        public Dictionary<string, List<IndustrialDataItem>> GetAllData()
        {
            return new Dictionary<string, List<IndustrialDataItem>>(_dataBuffer);
        }
        
        /// <summary>
        /// 数据平滑处理
        /// </summary>
        private async Task<object> SmoothValueAsync(IndustrialDataItem item)
        {
            // 简单的移动平均算法
            if (item.Value is double doubleValue)
            {
                return doubleValue * 0.8 + (doubleValue * 0.2); // 简化处理
            }
            
            return item.Value;
        }
        
        /// <summary>
        /// 值有效性检查
        /// </summary>
        private async Task<bool> IsValidValueAsync(IndustrialDataItem item, object value)
        {
            // 基本有效性检查
            if (value == null) return false;
            
            // 数值范围检查
            if (value is double doubleValue)
            {
                return !double.IsNaN(doubleValue) && !double.IsInfinity(doubleValue);
            }
            
            return true;
        }
        
        /// <summary>
        /// 范围检查
        /// </summary>
        private async Task<bool> IsInValidRangeAsync(IndustrialDataItem item)
        {
            // 根据数据类型进行范围检查
            if (item.Value is double doubleValue)
            {
                return doubleValue >= -1000000 && doubleValue <= 1000000;
            }
            
            return true;
        }
        
        /// <summary>
        /// 时间戳检查
        /// </summary>
        private async Task<bool> IsValidTimestampAsync(IndustrialDataItem item)
        {
            var timeDiff = DateTime.Now - item.Timestamp;
            return timeDiff.TotalMinutes < 5; // 5分钟内的数据认为有效
        }
        
        /// <summary>
        /// 单位转换
        /// </summary>
        private async Task<object> ConvertUnitAsync(IndustrialDataItem item)
        {
            // 根据数据类型进行单位转换
            if (item.DataType == "temperature" && item.Value is double tempValue)
            {
                // 摄氏度转华氏度示例
                return tempValue * 9.0 / 5.0 + 32.0;
            }
            
            return item.Value;
        }
    }
    
    /// <summary>
    /// 数据处理事件参数
    /// </summary>
    public class DataProcessedEventArgs : EventArgs
    {
        public string ProtocolName { get; set; }
        public List<IndustrialDataItem> DataItems { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
