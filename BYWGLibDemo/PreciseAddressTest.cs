using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BYWGLib;
using BYWGLib.Logging;
using BYWGLib.Protocols;

namespace BYWGLibDemo
{
    /// <summary>
    /// 精确地址测试 - 验证地址映射关系
    /// </summary>
    public class PreciseAddressTest
    {
        public static async Task RunPreciseAddressTest()
        {
            try
            {
                Log.Information("=== BYWGLib 精确地址映射测试 ===");
                Log.Information(VersionInfo.FullVersionInfo);

                // 创建配置
                var config = new IndustrialProtocolConfig
                {
                    Name = "Precise_Address_Test",
                    Type = "ModbusTCP",
                    Parameters = new Dictionary<string, string>
                    {
                        { "IpAddress", "192.168.6.6" },
                        { "Port", "502" },
                        { "Timeout", "5000" },
                        { "UnitId", "1" }
                    }
                };

                using var protocol = new AsyncModbusTcpProtocol(config);
                Log.Information($"正在连接设备 {config.Parameters["IpAddress"]}:{config.Parameters["Port"]}...");

                // 测试不同的地址格式来找到正确的映射
                Log.Information("\n=== 测试不同地址格式的映射关系 ===");
                
                var testCases = new[]
                {
                    // 测试4509的不同格式
                    ("4509 (直接)", "4509"),
                    ("D4509 (D前缀)", "D4509"),
                    ("@4509 (0基)", "@4509"),
                    ("44509 (标准Modbus)", "44509"),
                    ("44510 (标准Modbus+1)", "44510"),
                    ("44511 (标准Modbus+2)", "44511"),
                    
                    // 测试4510的不同格式
                    ("4510 (直接)", "4510"),
                    ("D4510 (D前缀)", "D4510"),
                    ("@4510 (0基)", "@4510"),
                    ("44510 (标准Modbus)", "44510"),
                    ("44511 (标准Modbus+1)", "44511"),
                    ("44512 (标准Modbus+2)", "44512")
                };

                foreach (var (description, address) in testCases)
                {
                    try
                    {
                        Log.Information($"\n测试: {description}");
                        var value = await protocol.ReadAsync(address, "signed");
                        Log.Information($"✅ {description} = {value}");
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"❌ {description} 失败: {ex.Message}");
                    }
                    
                    await Task.Delay(100);
                }

                // 扫描连续地址范围来找到正确的映射
                Log.Information("\n=== 扫描连续地址范围 ===");
                await ScanContinuousRange(protocol);

                Log.Information("=== 精确地址测试完成 ===");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "精确地址测试过程中发生错误: {0}", ex.Message);
            }
        }

        private static async Task ScanContinuousRange(AsyncModbusTcpProtocol protocol)
        {
            Log.Information("扫描地址范围 4500-4520，寻找正确的映射关系...");
            
            for (int addr = 4500; addr <= 4520; addr++)
            {
                try
                {
                    var value = await protocol.ReadAsync(addr.ToString(), "signed");
                    Log.Information($"地址 {addr} = {value}");
                }
                catch (Exception ex)
                {
                    Log.Debug($"地址 {addr} 不可用: {ex.Message}");
                }
                
                await Task.Delay(50);
            }
        }
    }
}
