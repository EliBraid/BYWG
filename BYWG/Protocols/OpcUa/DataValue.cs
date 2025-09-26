using System;

namespace BYWG.Protocols.OpcUa
{
    /// <summary>
    /// OPC UA数据值
    /// </summary>
    public class DataValue
    {
        public Variant Value { get; set; }
        public StatusCode StatusCode { get; set; }
        public DateTime SourceTimestamp { get; set; }
        public DateTime ServerTimestamp { get; set; }

        public DataValue()
        {
            Value = new Variant();
            StatusCode = StatusCode.Good;
            SourceTimestamp = DateTime.UtcNow;
            ServerTimestamp = DateTime.UtcNow;
        }

        public DataValue(object value)
        {
            Value = new Variant(value);
            StatusCode = StatusCode.Good;
            SourceTimestamp = DateTime.UtcNow;
            ServerTimestamp = DateTime.UtcNow;
        }

        public DataValue(Variant value)
        {
            Value = value;
            StatusCode = StatusCode.Good;
            SourceTimestamp = DateTime.UtcNow;
            ServerTimestamp = DateTime.UtcNow;
        }

        public DataValue(Variant value, StatusCode statusCode)
        {
            Value = value;
            StatusCode = statusCode;
            SourceTimestamp = DateTime.UtcNow;
            ServerTimestamp = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// OPC UA变体类型
    /// </summary>
    public class Variant
    {
        public object Value { get; set; }
        public VariantType Type { get; set; }

        public Variant()
        {
            Value = null;
            Type = VariantType.Null;
        }

        public Variant(object value)
        {
            Value = value;
            Type = GetVariantType(value);
        }

        private VariantType GetVariantType(object value)
        {
            if (value == null) return VariantType.Null;
            
            return value switch
            {
                bool => VariantType.Boolean,
                sbyte => VariantType.SByte,
                byte => VariantType.Byte,
                short => VariantType.Int16,
                ushort => VariantType.UInt16,
                int => VariantType.Int32,
                uint => VariantType.UInt32,
                long => VariantType.Int64,
                ulong => VariantType.UInt64,
                float => VariantType.Float,
                double => VariantType.Double,
                string => VariantType.String,
                DateTime => VariantType.DateTime,
                Guid => VariantType.Guid,
                byte[] => VariantType.ByteString,
                _ => VariantType.String
            };
        }
    }

    /// <summary>
    /// 变体类型枚举
    /// </summary>
    public enum VariantType : byte
    {
        Null = 0,
        Boolean = 1,
        SByte = 2,
        Byte = 3,
        Int16 = 4,
        UInt16 = 5,
        Int32 = 6,
        UInt32 = 7,
        Int64 = 8,
        UInt64 = 9,
        Float = 10,
        Double = 11,
        String = 12,
        DateTime = 13,
        Guid = 14,
        ByteString = 15
    }

    /// <summary>
    /// OPC UA状态码
    /// </summary>
    public class StatusCode
    {
        public uint Code { get; set; }

        public StatusCode(uint code)
        {
            Code = code;
        }

        public static StatusCode Good => new StatusCode(0x00000000);
        public static StatusCode Bad => new StatusCode(0x80000000);
        public static StatusCode Uncertain => new StatusCode(0x40000000);

        public bool IsGood => Code == 0x00000000;
        public bool IsBad => (Code & 0x80000000) != 0;
        public bool IsUncertain => (Code & 0x40000000) != 0;

        public static bool operator ==(StatusCode left, StatusCode right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (left is null) return false;
            return left.Code == right.Code;
        }

        public static bool operator !=(StatusCode left, StatusCode right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            return obj is StatusCode other && Code == other.Code;
        }

        public override int GetHashCode()
        {
            return Code.GetHashCode();
        }

        public override string ToString()
        {
            return $"0x{Code:X8}";
        }
    }
}
