using System;
using System.Text;

namespace BYWG.Protocols.OpcUa
{
    /// <summary>
    /// OPC UA节点ID
    /// </summary>
    public class NodeId : IEquatable<NodeId>
    {
        public NodeIdType Type { get; set; }
        public object Value { get; set; }
        public ushort NamespaceIndex { get; set; }

        public NodeId()
        {
            Type = NodeIdType.Numeric;
            Value = 0;
            NamespaceIndex = 0;
        }

        public NodeId(uint value, ushort namespaceIndex = 0)
        {
            Type = NodeIdType.Numeric;
            Value = value;
            NamespaceIndex = namespaceIndex;
        }

        public NodeId(string value, ushort namespaceIndex = 0)
        {
            Type = NodeIdType.String;
            Value = value;
            NamespaceIndex = namespaceIndex;
        }

        public NodeId(byte[] value, ushort namespaceIndex = 0)
        {
            Type = NodeIdType.ByteString;
            Value = value;
            NamespaceIndex = namespaceIndex;
        }

        public NodeId(Guid value, ushort namespaceIndex = 0)
        {
            Type = NodeIdType.Guid;
            Value = value;
            NamespaceIndex = namespaceIndex;
        }

        public override string ToString()
        {
            switch (Type)
            {
                case NodeIdType.Numeric:
                    return $"ns={NamespaceIndex};i={Value}";
                case NodeIdType.String:
                    return $"ns={NamespaceIndex};s={Value}";
                case NodeIdType.ByteString:
                    return $"ns={NamespaceIndex};b={Convert.ToBase64String((byte[])Value)}";
                case NodeIdType.Guid:
                    return $"ns={NamespaceIndex};g={Value}";
                default:
                    return $"ns={NamespaceIndex};i={Value}";
            }
        }

        public bool Equals(NodeId other)
        {
            if (other == null) return false;
            return Type == other.Type && 
                   NamespaceIndex == other.NamespaceIndex && 
                   Equals(Value, other.Value);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as NodeId);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Type, NamespaceIndex, Value);
        }

        public static bool operator ==(NodeId left, NodeId right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (left is null) return false;
            return left.Equals(right);
        }

        public static bool operator !=(NodeId left, NodeId right)
        {
            return !(left == right);
        }

        public static NodeId Parse(string nodeIdString)
        {
            if (string.IsNullOrEmpty(nodeIdString))
                throw new ArgumentException("NodeId string cannot be null or empty");

            var parts = nodeIdString.Split(';');
            ushort namespaceIndex = 0;
            NodeIdType type = NodeIdType.Numeric;
            object value = null;

            foreach (var part in parts)
            {
                var keyValue = part.Split('=');
                if (keyValue.Length != 2) continue;

                switch (keyValue[0].ToLower())
                {
                    case "ns":
                        namespaceIndex = ushort.Parse(keyValue[1]);
                        break;
                    case "i":
                        type = NodeIdType.Numeric;
                        value = uint.Parse(keyValue[1]);
                        break;
                    case "s":
                        type = NodeIdType.String;
                        value = keyValue[1];
                        break;
                    case "b":
                        type = NodeIdType.ByteString;
                        value = Convert.FromBase64String(keyValue[1]);
                        break;
                    case "g":
                        type = NodeIdType.Guid;
                        value = Guid.Parse(keyValue[1]);
                        break;
                }
            }

            return new NodeId { Type = type, NamespaceIndex = namespaceIndex, Value = value };
        }
    }

    /// <summary>
    /// 节点ID类型
    /// </summary>
    public enum NodeIdType : byte
    {
        Numeric = 1,
        String = 2,
        ByteString = 3,
        Guid = 4
    }
}
