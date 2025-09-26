using System;

namespace BYWGLib
{
    /// <summary>
    /// Modbus数据点
    /// </summary>
    public class ModbusDataPoint
    {
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public int FunctionCode { get; set; }
        public string DataType { get; set; } = string.Empty;
        public object Value { get; set; } = 0;
        public DateTime LastUpdateTime { get; set; }
        public bool Quality { get; set; } = true;

        public ModbusDataPoint()
        {
        }

        public ModbusDataPoint(string name, string address, int functionCode, string dataType)
        {
            Name = name;
            Address = address;
            FunctionCode = functionCode;
            DataType = dataType;
            LastUpdateTime = DateTime.Now;
        }
    }

    /// <summary>
    /// Modbus功能码
    /// </summary>
    // 保留历史枚举定义注释，现使用整型FunctionCode以便与现有实现匹配
    // public enum ModbusFunctionCode : byte { }

    /// <summary>
    /// Modbus数据类型
    /// </summary>
    // 同理，数据类型使用字符串以匹配解析逻辑
    // public enum ModbusDataType { }
}
