using System;
using BYWGLib.Logging;

namespace BYWGLib.Protocols
{
    /// <summary>
    /// 高性能协议工厂
    /// 统一创建和管理高性能工业协议实例
    /// </summary>
    public static class HighPerformanceProtocolFactory
    {
        /// <summary>
        /// 创建高性能协议实例
        /// </summary>
        /// <param name="config">协议配置</param>
        /// <returns>协议实例</returns>
        public static IIndustrialProtocol CreateProtocol(IndustrialProtocolConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));
            
            if (string.IsNullOrEmpty(config.Type))
                throw new ArgumentException("协议类型不能为空", nameof(config));
            
            switch (config.Type.ToUpper())
            {
                case "MODBUS_TCP":
                case "MODBUSTCP":
                case "MODBUS":
                    return new AsyncModbusTcpProtocol(config);
                
                case "MODBUS_RTU":
                case "MODBUSRTU":
                    return new AsyncModbusTcpProtocol(config); // 使用TCP协议替代RTU
                
                case "S7":
                case "SIEMENS_S7":
                case "SIEMENSS7":
                case "SIEMENS":
                    return new S7Protocol(config);
                
                case "MC":
                case "MITSUBISHI_MC":
                case "MITSUBISHIMC":
                case "MITSUBISHI":
                    return new AsyncMitsubishiMCProtocol(config);
                
                default:
                    throw new NotSupportedException($"不支持的协议类型: {config.Type}");
            }
        }
        
        /// <summary>
        /// 创建Modbus TCP协议
        /// </summary>
        /// <param name="name">协议名称</param>
        /// <param name="ipAddress">IP地址</param>
        /// <param name="port">端口号</param>
        /// <param name="slaveId">从站ID</param>
        /// <param name="timeout">超时时间(毫秒)</param>
        /// <returns>Modbus TCP协议实例</returns>
        public static IIndustrialProtocol CreateModbusTCP(string name, string ipAddress, int port = 502, int slaveId = 1, int timeout = 3000)
        {
            var config = new IndustrialProtocolConfig
            {
                Name = name,
                Type = "MODBUS_TCP",
                Enabled = true,
                Parameters = new System.Collections.Generic.Dictionary<string, string>
                {
                    ["IpAddress"] = ipAddress,
                    ["Port"] = port.ToString(),
                    ["SlaveId"] = slaveId.ToString(),
                    ["Timeout"] = timeout.ToString()
                }
            };
            
            return new AsyncModbusTcpProtocol(config);
        }
        
        /// <summary>
        /// 创建Modbus RTU协议
        /// </summary>
        /// <param name="name">协议名称</param>
        /// <param name="portName">串口名称</param>
        /// <param name="baudRate">波特率</param>
        /// <param name="slaveId">从站ID</param>
        /// <param name="timeout">超时时间(毫秒)</param>
        /// <returns>Modbus RTU协议实例</returns>
        public static IIndustrialProtocol CreateModbusRTU(string name, string portName, int baudRate = 9600, int slaveId = 1, int timeout = 3000)
        {
            var config = new IndustrialProtocolConfig
            {
                Name = name,
                Type = "MODBUS_RTU",
                Enabled = true,
                Parameters = new System.Collections.Generic.Dictionary<string, string>
                {
                    ["PortName"] = portName,
                    ["BaudRate"] = baudRate.ToString(),
                    ["SlaveId"] = slaveId.ToString(),
                    ["Timeout"] = timeout.ToString(),
                    ["DataBits"] = "8",
                    ["Parity"] = "None",
                    ["StopBits"] = "One"
                }
            };
            
            return new AsyncModbusTcpProtocol(config); // 使用TCP协议替代RTU
        }
        
        /// <summary>
        /// 创建S7协议
        /// </summary>
        /// <param name="name">协议名称</param>
        /// <param name="ipAddress">IP地址</param>
        /// <param name="rack">机架号</param>
        /// <param name="slot">插槽号</param>
        /// <param name="port">端口号</param>
        /// <param name="timeout">超时时间(毫秒)</param>
        /// <returns>S7协议实例</returns>
        public static IIndustrialProtocol CreateS7(string name, string ipAddress, int rack = 0, int slot = 2, int port = 102, int timeout = 3000)
        {
            var config = new IndustrialProtocolConfig
            {
                Name = name,
                Type = "S7",
                Enabled = true,
                Parameters = new System.Collections.Generic.Dictionary<string, string>
                {
                    ["IpAddress"] = ipAddress,
                    ["Port"] = port.ToString(),
                    ["Rack"] = rack.ToString(),
                    ["Slot"] = slot.ToString(),
                    ["Timeout"] = timeout.ToString(),
                    ["PduSize"] = "1024"
                }
            };
            
            return new S7Protocol(config);
        }
        
        /// <summary>
        /// 创建三菱MC协议
        /// </summary>
        /// <param name="name">协议名称</param>
        /// <param name="ipAddress">IP地址</param>
        /// <param name="port">端口号</param>
        /// <param name="networkNo">网络号</param>
        /// <param name="pcNo">PC号</param>
        /// <param name="timeout">超时时间(毫秒)</param>
        /// <returns>三菱MC协议实例</returns>
        public static IIndustrialProtocol CreateMitsubishiMC(string name, string ipAddress, int port = 5007, byte networkNo = 0, byte pcNo = 255, int timeout = 3000)
        {
            var config = new IndustrialProtocolConfig
            {
                Name = name,
                Type = "MC",
                Enabled = true,
                Parameters = new System.Collections.Generic.Dictionary<string, string>
                {
                    ["IpAddress"] = ipAddress,
                    ["Port"] = port.ToString(),
                    ["NetworkNo"] = networkNo.ToString(),
                    ["PcNo"] = pcNo.ToString(),
                    ["Timeout"] = timeout.ToString()
                }
            };
            
            return new AsyncMitsubishiMCProtocol(config);
        }
        
        /// <summary>
        /// 获取支持的协议类型列表
        /// </summary>
        /// <returns>支持的协议类型</returns>
        public static string[] GetSupportedProtocols()
        {
            return new[]
            {
                "MODBUS_TCP",
                "MODBUS_RTU", 
                "S7",
                "MC"
            };
        }
        
        /// <summary>
        /// 检查是否支持指定的协议类型
        /// </summary>
        /// <param name="protocolType">协议类型</param>
        /// <returns>是否支持</returns>
        public static bool IsProtocolSupported(string protocolType)
        {
            if (string.IsNullOrEmpty(protocolType))
                return false;
            
            var supportedProtocols = GetSupportedProtocols();
            return Array.Exists(supportedProtocols, p => 
                string.Equals(p, protocolType, StringComparison.OrdinalIgnoreCase));
        }
    }
}
