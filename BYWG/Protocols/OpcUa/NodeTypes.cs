using System;
using System.Collections.Generic;

namespace BYWG.Protocols.OpcUa
{
    /// <summary>
    /// 数据变量节点
    /// </summary>
    public class DataVariableNode : BaseNode
    {
        public DataValue Value { get; set; }
        public NodeId DataType { get; set; }
        public int ValueRank { get; set; }
        public uint[] ArrayDimensions { get; set; }
        public byte AccessLevel { get; set; }
        public byte UserAccessLevel { get; set; }
        public double MinimumSamplingInterval { get; set; }
        public bool Historizing { get; set; }

        public DataVariableNode(NodeId nodeId)
        {
            NodeId = nodeId;
            NodeClass = NodeClass.Variable;
            Value = new DataValue();
            DataType = new NodeId(1); // Boolean
            ValueRank = -1; // Scalar
            AccessLevel = 3; // CurrentRead | CurrentWrite
            UserAccessLevel = 3;
            MinimumSamplingInterval = 0;
            Historizing = false;
        }

        public override DataValue ReadAttribute(AttributeId attributeId)
        {
            return attributeId switch
            {
                AttributeId.Value => Value,
                AttributeId.DataType => new DataValue(DataType),
                AttributeId.ValueRank => new DataValue(ValueRank),
                AttributeId.ArrayDimensions => new DataValue(ArrayDimensions),
                AttributeId.AccessLevel => new DataValue(AccessLevel),
                AttributeId.UserAccessLevel => new DataValue(UserAccessLevel),
                AttributeId.MinimumSamplingInterval => new DataValue(MinimumSamplingInterval),
                AttributeId.Historizing => new DataValue(Historizing),
                _ => base.ReadAttribute(attributeId)
            };
        }

        public override StatusCode WriteAttribute(AttributeId attributeId, DataValue value)
        {
            try
            {
                switch (attributeId)
                {
                    case AttributeId.Value:
                        Value = value;
                        return StatusCode.Good;
                    case AttributeId.AccessLevel:
                        AccessLevel = (byte)value.Value.Value;
                        return StatusCode.Good;
                    case AttributeId.UserAccessLevel:
                        UserAccessLevel = (byte)value.Value.Value;
                        return StatusCode.Good;
                    default:
                        return StatusCode.Bad;
                }
            }
            catch
            {
                return StatusCode.Bad;
            }
        }
    }

    /// <summary>
    /// 对象节点
    /// </summary>
    public class ObjectNode : BaseNode
    {
        public bool IsAbstract { get; set; }
        public byte EventNotifier { get; set; }

        public ObjectNode(NodeId nodeId)
        {
            NodeId = nodeId;
            NodeClass = NodeClass.Object;
            IsAbstract = false;
            EventNotifier = 0;
        }

        public override DataValue ReadAttribute(AttributeId attributeId)
        {
            return attributeId switch
            {
                AttributeId.IsAbstract => new DataValue(IsAbstract),
                AttributeId.EventNotifier => new DataValue(EventNotifier),
                _ => base.ReadAttribute(attributeId)
            };
        }

        public override StatusCode WriteAttribute(AttributeId attributeId, DataValue value)
        {
            try
            {
                switch (attributeId)
                {
                    case AttributeId.IsAbstract:
                        IsAbstract = (bool)value.Value.Value;
                        return StatusCode.Good;
                    case AttributeId.EventNotifier:
                        EventNotifier = (byte)value.Value.Value;
                        return StatusCode.Good;
                    default:
                        return StatusCode.Bad;
                }
            }
            catch
            {
                return StatusCode.Bad;
            }
        }
    }

    /// <summary>
    /// 文件夹节点
    /// </summary>
    public class FolderNode : ObjectNode
    {
        public FolderNode(NodeId nodeId) : base(nodeId)
        {
            BrowseName = new QualifiedName("Folder");
            DisplayName = new LocalizedText("Folder");
        }
    }
}
