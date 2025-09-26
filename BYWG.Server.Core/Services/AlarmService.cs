using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BYWGLib;
using Microsoft.Extensions.Logging;

namespace BYWG.Server.Core.Services
{
    /// <summary>
    /// 报警服务 - 边缘网关报警管理
    /// </summary>
    public class AlarmService
    {
        private readonly ILogger<AlarmService> _logger;
        private readonly List<AlarmRule> _alarmRules;
        private readonly List<AlarmEvent> _alarmEvents;
        
        public event EventHandler<AlarmTriggeredEventArgs> AlarmTriggered;
        
        public AlarmService(ILogger<AlarmService> logger)
        {
            _logger = logger;
            _alarmRules = new List<AlarmRule>();
            _alarmEvents = new List<AlarmEvent>();
            
            // 初始化默认报警规则
            InitializeDefaultAlarmRules();
        }
        
        /// <summary>
        /// 添加报警规则
        /// </summary>
        public void AddAlarmRule(AlarmRule rule)
        {
            _alarmRules.Add(rule);
            _logger.LogInformation("添加报警规则: {RuleName}", rule.Name);
        }
        
        /// <summary>
        /// 移除报警规则
        /// </summary>
        public void RemoveAlarmRule(string ruleName)
        {
            var rule = _alarmRules.FirstOrDefault(r => r.Name == ruleName);
            if (rule != null)
            {
                _alarmRules.Remove(rule);
                _logger.LogInformation("移除报警规则: {RuleName}", ruleName);
            }
        }
        
        /// <summary>
        /// 检查数据并触发报警
        /// </summary>
        public async Task CheckAlarmsAsync(string deviceId, List<IndustrialDataItem> dataItems)
        {
            try
            {
                foreach (var rule in _alarmRules.Where(r => r.IsEnabled))
                {
                    if (await EvaluateAlarmRuleAsync(rule, deviceId, dataItems))
                    {
                        await TriggerAlarmAsync(rule, deviceId, dataItems);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "检查报警规则失败: {DeviceId}", deviceId);
            }
        }
        
        /// <summary>
        /// 评估报警规则
        /// </summary>
        private async Task<bool> EvaluateAlarmRuleAsync(AlarmRule rule, string deviceId, List<IndustrialDataItem> dataItems)
        {
            try
            {
                // 检查设备匹配
                if (!string.IsNullOrEmpty(rule.DeviceId) && rule.DeviceId != deviceId)
                {
                    return false;
                }
                
                // 检查数据点匹配
                var matchingData = dataItems.Where(d => 
                    string.IsNullOrEmpty(rule.DataPointName) || d.Name == rule.DataPointName).ToList();
                
                if (!matchingData.Any())
                {
                    return false;
                }
                
                // 根据规则类型评估
                return rule.ConditionType switch
                {
                    AlarmConditionType.ValueThreshold => EvaluateValueThreshold(rule, matchingData),
                    AlarmConditionType.QualityCheck => EvaluateQualityCheck(rule, matchingData),
                    AlarmConditionType.DeviceOffline => EvaluateDeviceOffline(rule, deviceId),
                    AlarmConditionType.ProtocolError => EvaluateProtocolError(rule, deviceId),
                    _ => false
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "评估报警规则失败: {RuleName}", rule.Name);
                return false;
            }
        }
        
        /// <summary>
        /// 评估数值阈值
        /// </summary>
        private bool EvaluateValueThreshold(AlarmRule rule, List<IndustrialDataItem> dataItems)
        {
            foreach (var item in dataItems)
            {
                if (item.Value is double doubleValue)
                {
                    return rule.Operator switch
                    {
                        AlarmOperator.GreaterThan => doubleValue > rule.ThresholdValue,
                        AlarmOperator.LessThan => doubleValue < rule.ThresholdValue,
                        AlarmOperator.Equal => Math.Abs(doubleValue - rule.ThresholdValue) < 0.001,
                        AlarmOperator.NotEqual => Math.Abs(doubleValue - rule.ThresholdValue) >= 0.001,
                        _ => false
                    };
                }
            }
            return false;
        }
        
        /// <summary>
        /// 评估数据质量
        /// </summary>
        private bool EvaluateQualityCheck(AlarmRule rule, List<IndustrialDataItem> dataItems)
        {
            var badQualityCount = dataItems.Count(d => d.Quality == Quality.Bad);
            return badQualityCount >= rule.ThresholdValue;
        }
        
        /// <summary>
        /// 评估设备离线
        /// </summary>
        private bool EvaluateDeviceOffline(AlarmRule rule, string deviceId)
        {
            // 这里应该检查设备状态
            // 简化实现，实际应该从设备管理服务获取状态
            return false;
        }
        
        /// <summary>
        /// 评估协议错误
        /// </summary>
        private bool EvaluateProtocolError(AlarmRule rule, string deviceId)
        {
            // 这里应该检查协议状态
            // 简化实现，实际应该从协议管理服务获取状态
            return false;
        }
        
        /// <summary>
        /// 触发报警
        /// </summary>
        private async Task TriggerAlarmAsync(AlarmRule rule, string deviceId, List<IndustrialDataItem> dataItems)
        {
            try
            {
                var alarmEvent = new AlarmEvent
                {
                    Id = Guid.NewGuid().ToString(),
                    RuleName = rule.Name,
                    DeviceId = deviceId,
                    Message = rule.Message,
                    Level = rule.Level,
                    Timestamp = DateTime.Now,
                    DataItems = dataItems
                };
                
                _alarmEvents.Add(alarmEvent);
                
                // 触发事件
                AlarmTriggered?.Invoke(this, new AlarmTriggeredEventArgs
                {
                    AlarmEvent = alarmEvent
                });
                
                _logger.LogWarning("报警触发: {RuleName} - {Message}", rule.Name, rule.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "触发报警失败: {RuleName}", rule.Name);
            }
        }
        
        /// <summary>
        /// 获取报警事件
        /// </summary>
        public List<AlarmEvent> GetAlarmEvents(DateTime? startTime = null, DateTime? endTime = null)
        {
            var query = _alarmEvents.AsQueryable();
            
            if (startTime.HasValue)
            {
                query = query.Where(e => e.Timestamp >= startTime.Value);
            }
            
            if (endTime.HasValue)
            {
                query = query.Where(e => e.Timestamp <= endTime.Value);
            }
            
            return query.OrderByDescending(e => e.Timestamp).ToList();
        }
        
        /// <summary>
        /// 初始化默认报警规则
        /// </summary>
        private void InitializeDefaultAlarmRules()
        {
            // 数据质量报警
            AddAlarmRule(new AlarmRule
            {
                Name = "数据质量报警",
                Description = "数据质量异常时触发",
                ConditionType = AlarmConditionType.QualityCheck,
                ThresholdValue = 5,
                Level = AlarmLevel.Warning,
                Message = "数据质量异常，请检查设备连接",
                IsEnabled = true
            });
            
            // 设备离线报警
            AddAlarmRule(new AlarmRule
            {
                Name = "设备离线报警",
                Description = "设备离线时触发",
                ConditionType = AlarmConditionType.DeviceOffline,
                Level = AlarmLevel.Error,
                Message = "设备离线，请检查网络连接",
                IsEnabled = true
            });
            
            // 协议错误报警
            AddAlarmRule(new AlarmRule
            {
                Name = "协议错误报警",
                Description = "协议通信错误时触发",
                ConditionType = AlarmConditionType.ProtocolError,
                Level = AlarmLevel.Error,
                Message = "协议通信错误，请检查协议配置",
                IsEnabled = true
            });
        }
    }
    
    /// <summary>
    /// 报警规则
    /// </summary>
    public class AlarmRule
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public AlarmConditionType ConditionType { get; set; }
        public string DeviceId { get; set; }
        public string DataPointName { get; set; }
        public AlarmOperator Operator { get; set; }
        public double ThresholdValue { get; set; }
        public AlarmLevel Level { get; set; }
        public string Message { get; set; }
        public bool IsEnabled { get; set; }
    }
    
    /// <summary>
    /// 报警事件
    /// </summary>
    public class AlarmEvent
    {
        public string Id { get; set; }
        public string RuleName { get; set; }
        public string DeviceId { get; set; }
        public string Message { get; set; }
        public AlarmLevel Level { get; set; }
        public DateTime Timestamp { get; set; }
        public List<IndustrialDataItem> DataItems { get; set; }
    }
    
    /// <summary>
    /// 报警条件类型
    /// </summary>
    public enum AlarmConditionType
    {
        ValueThreshold,
        QualityCheck,
        DeviceOffline,
        ProtocolError
    }
    
    /// <summary>
    /// 报警操作符
    /// </summary>
    public enum AlarmOperator
    {
        GreaterThan,
        LessThan,
        Equal,
        NotEqual
    }
    
    /// <summary>
    /// 报警级别
    /// </summary>
    public enum AlarmLevel
    {
        Info,
        Warning,
        Error,
        Critical
    }
    
    /// <summary>
    /// 报警触发事件参数
    /// </summary>
    public class AlarmTriggeredEventArgs : EventArgs
    {
        public AlarmEvent AlarmEvent { get; set; }
    }
}
