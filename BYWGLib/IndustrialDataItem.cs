using System;

namespace BYWGLib
{
    /// <summary>
    /// 工业数据项
    /// </summary>
    public class IndustrialDataItem : IEquatable<IndustrialDataItem>
    {
        /// <summary>
        /// 数据项标识
        /// </summary>
        public string Id { get; set; }
        
        /// <summary>
        /// 数据名称
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// 数据值
        /// </summary>
        public object Value { get; set; }
        
        /// <summary>
        /// 数据类型
        /// </summary>
        public string DataType { get; set; }
        
        /// <summary>
        /// 时间戳
        /// </summary>
        public DateTime Timestamp { get; set; }
        
        /// <summary>
        /// 质量
        /// </summary>
        public Quality Quality { get; set; }
        
        public IndustrialDataItem()
        {
            Timestamp = DateTime.Now;
            Quality = Quality.Good;
        }
        
        public bool Equals(IndustrialDataItem other)
        {
            if (other == null)
                return false;
            
            return Id == other.Id;
        }
        
        public override bool Equals(object obj)
        {
            return Equals(obj as IndustrialDataItem);
        }
        
        public override int GetHashCode()
        {
            return Id?.GetHashCode() ?? 0;
        }
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