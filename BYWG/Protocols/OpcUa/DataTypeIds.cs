using System;

namespace BYWG.Protocols.OpcUa
{
    /// <summary>
    /// OPC UA数据类型标识符
    /// </summary>
    public static class DataTypeIds
    {
        /// <summary>
        /// Boolean数据类型
        /// </summary>
        public static NodeId Boolean => new NodeId(1);
        
        /// <summary>
        /// Int16数据类型
        /// </summary>
        public static NodeId Int16 => new NodeId(4);
        
        /// <summary>
        /// Int32数据类型
        /// </summary>
        public static NodeId Int32 => new NodeId(6);
        
        /// <summary>
        /// Float数据类型
        /// </summary>
        public static NodeId Float => new NodeId(10);
        
        /// <summary>
        /// Double数据类型
        /// </summary>
        public static NodeId Double => new NodeId(11);
        
        /// <summary>
        /// String数据类型
        /// </summary>
        public static NodeId String => new NodeId(12);
    }
}