using System;
using System.Threading;
using BYWGLib.Logging;
using BYWGLib.Protocols;

namespace BYWGLib.Examples
{
    /// <summary>
    /// 高性能协议使用示例
    /// </summary>
    public class HighPerformanceProtocolExample
    {
        private ProtocolManager _protocolManager;
        
        public void RunExample()
        {
            try
            {
                // 初始化协议管理器
                _protocolManager = new ProtocolManager();
                _protocolManager.Initialize();
                
                // 订阅数据变化事件
                _protocolManager.DataChanged += OnDataChanged;
                
                // 示例1: 创建Modbus TCP协议
                var modbusTcpConfig = new IndustrialProtocolConfig
                {
                    Name = "ModbusTCP_Device1",
                    Type = "MODBUS_TCP",
                    Enabled = true,
                    Parameters = new System.Collections.Generic.Dictionary<string, string>
                    {
                        ["IpAddress"] = "192.168.1.100",
                        ["Port"] = "502",
                        ["SlaveId"] = "1",
                        ["Timeout"] = "3000",
                        ["DataPoints"] = "Temperature,D100,float,3;Pressure,D102,float,3;Status,M0,bool,1"
                    }
                };
                
                _protocolManager.AddProtocol(modbusTcpConfig);
                
                // 示例2: 创建Modbus RTU协议
                var modbusRtuConfig = new IndustrialProtocolConfig
                {
                    Name = "ModbusRTU_Device1",
                    Type = "MODBUS_RTU",
                    Enabled = true,
                    Parameters = new System.Collections.Generic.Dictionary<string, string>
                    {
                        ["PortName"] = "COM1",
                        ["BaudRate"] = "9600",
                        ["SlaveId"] = "1",
                        ["Timeout"] = "3000",
                        ["DataBits"] = "8",
                        ["Parity"] = "None",
                        ["StopBits"] = "One",
                        ["DataPoints"] = "Temperature,100,float,3;Pressure,102,float,3;Status,0,bool,1"
                    }
                };
                
                _protocolManager.AddProtocol(modbusRtuConfig);
                
                // 示例3: 创建S7协议
                var s7Config = new IndustrialProtocolConfig
                {
                    Name = "S7_Device1",
                    Type = "S7",
                    Enabled = true,
                    Parameters = new System.Collections.Generic.Dictionary<string, string>
                    {
                        ["IpAddress"] = "192.168.1.101",
                        ["Port"] = "102",
                        ["Rack"] = "0",
                        ["Slot"] = "2",
                        ["Timeout"] = "3000",
                        ["DataPoints"] = "Temperature,DB1:0,float;Pressure,DB1:4,float;Status,M0.0,bool"
                    }
                };
                
                _protocolManager.AddProtocol(s7Config);
                
                // 示例4: 创建三菱MC协议
                var mcConfig = new IndustrialProtocolConfig
                {
                    Name = "MC_Device1",
                    Type = "MC",
                    Enabled = true,
                    Parameters = new System.Collections.Generic.Dictionary<string, string>
                    {
                        ["IpAddress"] = "192.168.1.102",
                        ["Port"] = "5007",
                        ["NetworkNo"] = "0",
                        ["PcNo"] = "255",
                        ["Timeout"] = "3000",
                        ["DataPoints"] = "Temperature,D100,float;Pressure,D102,float;Status,M0,bool"
                    }
                };
                
                _protocolManager.AddProtocol(mcConfig);
                
                Log.Information("所有协议已启动，开始数据采集...");
                
                // 运行一段时间
                Thread.Sleep(30000); // 运行30秒
                
                // 显示统计信息
                Log.Information("总数据传输量: {0} 字节", _protocolManager.GetTotalBytesTransferred());
                Log.Information("当前数据速率: {0:F2} 字节/秒", _protocolManager.GetCurrentDataRate());
            }
            catch (Exception ex)
            {
                Log.Error(ex, "运行示例时出错");
            }
            finally
            {
                // 清理资源
                _protocolManager?.Dispose();
            }
        }
        
        /// <summary>
        /// 数据变化事件处理
        /// </summary>
        private void OnDataChanged(object sender, DataChangedEventArgs e)
        {
            foreach (var item in e.ChangedItems)
            {
                Log.Information("数据变化: {0} = {1} ({2})", 
                    item.Name, item.Value, item.DataType);
            }
        }
        
        /// <summary>
        /// 使用工厂方法创建协议的示例
        /// </summary>
        public void FactoryExample()
        {
            try
            {
                // 使用工厂方法创建协议
                var modbusTcp = HighPerformanceProtocolFactory.CreateModbusTCP(
                    "Factory_ModbusTCP", "192.168.1.100", 502, 1, 3000);
                
                var modbusRtu = HighPerformanceProtocolFactory.CreateModbusRTU(
                    "Factory_ModbusRTU", "COM1", 9600, 1, 3000);
                
                var s7 = HighPerformanceProtocolFactory.CreateS7(
                    "Factory_S7", "192.168.1.101", 0, 2, 102, 3000);
                
                var mc = HighPerformanceProtocolFactory.CreateMitsubishiMC(
                    "Factory_MC", "192.168.1.102", 5007, 0, 255, 3000);
                
                // 启动协议
                modbusTcp.Start();
                modbusRtu.Start();
                s7.Start();
                mc.Start();
                
                Log.Information("使用工厂方法创建的协议已启动");
                
                // 读取数据示例
                try
                {
                    var temperature = modbusTcp.Read("100", "float");
                    Log.Information("Modbus TCP读取温度: {0}", temperature);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "读取Modbus TCP数据时出错");
                }
                
                // 运行一段时间
                Thread.Sleep(10000);
                
                // 停止协议
                modbusTcp.Stop();
                modbusRtu.Stop();
                s7.Stop();
                mc.Stop();
                
                // 释放资源
                modbusTcp.Dispose();
                modbusRtu.Dispose();
                s7.Dispose();
                mc.Dispose();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "工厂示例运行时出错");
            }
        }
    }
}
