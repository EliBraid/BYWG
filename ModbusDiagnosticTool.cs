using System;
using System.Threading.Tasks;
using BYWGLib.Protocols;
using BYWGLib.Logging;
using BYWGLib;

namespace BYWG
{
    class ModbusDiagnosticTool
    {
        static async Task Main(string[] args)
        {
            try
            {
                Log.Information("=== Modbus TCP 诊断工具 ===");
                
                // 配置参数
                string ipAddress = "192.168.6.6"; // 替换为实际设备IP
                int port = 502; // 默认Modbus TCP端口
                int slaveId = 1; // 替换为实际从站ID
                
                Log.Information($"连接到设备: {ipAddress}:{port}, SlaveId: {slaveId}");
                
                // 创建Modbus TCP协议实例
                var protocol = new AsyncUltraHighPerformanceModbusTCPProtocol(
                    new IndustrialProtocolConfig
                    {
                        Name = "ModbusDiagnostic",
                        Type = "MODBUS_TCP",
                        Parameters = new System.Collections.Generic.Dictionary<string, string>
                        {
                            ["IpAddress"] = ipAddress,
                            ["Port"] = port.ToString(),
                            ["UnitId"] = slaveId.ToString(),
                            ["Timeout"] = "3000"
                        }
                    }
                );
                
                // 启动协议
                protocol.Start();
                
                Log.Information("协议已启动，开始诊断测试...");
                
                // 测试1：检查网络连接
                Log.Information("\n测试1: 检查网络连接");
                bool isConnected = await protocol.TestDeviceConnectionAsync();
                Log.Information("网络连接测试结果: {0}", isConnected ? "成功" : "失败");
                
                // 测试2：尝试使用不同格式的地址读取数据
                Log.Information("\n测试2: 使用不同格式的地址读取数据");
                
                // 测试各种地址格式
                string[] testAddresses = {
                    "40001", // 标准Modbus保持寄存器地址
                    "40002",
                    "100",   // 纯数字地址（默认保持寄存器）
                    "D100",  // D前缀格式
                    "44500"  // 大地址测试
                };
                
                foreach (string address in testAddresses)
                {
                    try
                    {
                        Log.Information("\n尝试读取地址 {0} 格式: {1}", address, GetAddressFormat(address));
                        
                        // 解析地址，显示解析结果
                        var (functionCode, startAddress, quantity) = protocol.ParseModbusAddress(address, "uint16");
                        Log.Information("解析结果: 功能码={0}, 起始地址={1}, 数量={2}", 
                            functionCode, startAddress, quantity);
                        
                        // 尝试读取数据
                        object value = protocol.Read(address, "uint16");
                        Log.Information("读取结果: {0}", value ?? "(null)");
                    }
                    catch (Exception ex)
                    {
                        Log.Error("读取地址 {0} 时出错: {1}", address, ex.Message);
                    }
                }
                
                // 测试3：运行D4500专门诊断
                Log.Information("\n测试3: 运行D4500专门诊断");
                string diagnoseResult = await protocol.DiagnoseD4500IssueAsync();
                Log.Information(diagnoseResult);
                
                // 测试4：扫描可用地址范围
                Log.Information("\n测试4: 扫描可用地址范围 (40001-40010)");
                try
                {
                    var availableAddresses = await protocol.ScanAvailableAddressesAsync(
                        startAddress: 0, // 对应40001
                        endAddress: 9,   // 对应40010
                        functionCode: 3);
                    
                    Log.Information("发现 {0} 个可用地址:", availableAddresses.Count);
                    foreach (int addr in availableAddresses)
                    {
                        try
                        {
                            object value = protocol.Read((addr + 40001).ToString(), "uint16");
                            Log.Information("地址 {0} (Modbus地址: {1}) 的值: {2}", 
                                addr + 40001, addr, value ?? "(null)");
                        }
                        catch (Exception ex)
                        {
                            Log.Error("读取地址 {0} 时出错: {1}", addr + 40001, ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("扫描地址时出错: {0}", ex.Message);
                }
                
                // 停止协议
                protocol.Stop();
                protocol.Dispose();
                
                Log.Information("\n=== 诊断完成 ===");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "诊断过程中发生异常");
            }
            
            Console.WriteLine("\n按任意键退出...");
            Console.ReadKey();
        }
        
        private static string GetAddressFormat(string address)
        {
            if (address.StartsWith("D", StringComparison.OrdinalIgnoreCase))
                return "D前缀格式";
            else if (address.StartsWith("4"))
                return "标准保持寄存器格式";
            else if (int.TryParse(address, out _))
                return "纯数字格式";
            else
                return "其他格式";
        }
    }
}