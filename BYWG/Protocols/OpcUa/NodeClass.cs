using System;
using System.Collections.Generic;

namespace BYWG.Protocols.OpcUa
{
    /// <summary>
    /// OPC UA节点类
    /// </summary>
    public enum NodeClass : uint
    {
        Unspecified = 0,
        Object = 1,
        Variable = 2,
        Method = 4,
        ObjectType = 8,
        VariableType = 16,
        ReferenceType = 32,
        DataType = 64,
        View = 128
    }

    /// <summary>
    /// OPC UA节点属性
    /// </summary>
    public enum AttributeId : uint
    {
        NodeId = 1,
        NodeClass = 2,
        BrowseName = 3,
        DisplayName = 4,
        Description = 5,
        WriteMask = 6,
        UserWriteMask = 7,
        IsAbstract = 8,
        Symmetric = 9,
        InverseName = 10,
        ContainsNoLoops = 11,
        EventNotifier = 12,
        Value = 13,
        DataType = 14,
        ValueRank = 15,
        ArrayDimensions = 16,
        AccessLevel = 17,
        UserAccessLevel = 18,
        MinimumSamplingInterval = 19,
        Historizing = 20,
        Executable = 21,
        UserExecutable = 22
    }

    /// <summary>
    /// OPC UA节点基类
    /// </summary>
    public abstract class BaseNode
    {
        public NodeId NodeId { get; set; }
        public NodeClass NodeClass { get; set; }
        public QualifiedName BrowseName { get; set; }
        public LocalizedText DisplayName { get; set; }
        public LocalizedText Description { get; set; }
        public uint WriteMask { get; set; }
        public uint UserWriteMask { get; set; }
        public List<ReferenceDescription> References { get; set; }

        protected BaseNode()
        {
            References = new List<ReferenceDescription>();
            WriteMask = 0xFFFFFFFF;
            UserWriteMask = 0xFFFFFFFF;
        }

        public abstract DataValue ReadAttribute(AttributeId attributeId);
        public abstract StatusCode WriteAttribute(AttributeId attributeId, DataValue value);
    }

    /// <summary>
    /// 限定名称
    /// </summary>
    public class QualifiedName
    {
        public string Name { get; set; }
        public ushort NamespaceIndex { get; set; }

        public QualifiedName(string name, ushort namespaceIndex = 0)
        {
            Name = name;
            NamespaceIndex = namespaceIndex;
        }

        public override string ToString()
        {
            return NamespaceIndex == 0 ? Name : $"{NamespaceIndex}:{Name}";
        }
    }

    /// <summary>
    /// 本地化文本
    /// </summary>
    public class LocalizedText
    {
        public string Text { get; set; }
        public string Locale { get; set; }

        public LocalizedText(string text, string locale = "en")
        {
            Text = text;
            Locale = locale;
        }

        public override string ToString()
        {
            return Text;
        }
    }

    /// <summary>
    /// 引用描述
    /// </summary>
    public class ReferenceDescription
    {
        public NodeId ReferenceTypeId { get; set; }
        public bool IsForward { get; set; }
        public NodeId TargetId { get; set; }
        public QualifiedName BrowseName { get; set; }
        public LocalizedText DisplayName { get; set; }
        public NodeClass NodeClass { get; set; }
        public NodeId TypeDefinition { get; set; }
    }
}
