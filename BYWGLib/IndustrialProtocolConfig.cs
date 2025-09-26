using System.Collections.Generic;

namespace BYWGLib
{
    /// <summary>
    /// 工业协议配置
    /// </summary>
    public class IndustrialProtocolConfig
    {
        /// <summary>
        /// 协议名称
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// 协议类型
        /// </summary>
        public string Type { get; set; }
        
        /// <summary>
        /// 协议是否启用
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// 连接串
        /// </summary>
        public string ConnectionString { get; set; } = string.Empty;

        /// <summary>
        /// 协议参数
        /// </summary>
        public Dictionary<string, string> Parameters { get; set; }
        
        public IndustrialProtocolConfig()
        {
            Parameters = new Dictionary<string, string>();
        }
    }
}