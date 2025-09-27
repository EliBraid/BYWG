using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BYWGLib;
using BYWGLib.Logging;
using BYWGLib.Protocols;

namespace BYWGLibDemo
{
    /// <summary>
    /// 地址诊断工具 - 帮助诊断Modbus地址问题
    /// </summary>
    public class AddressDiagnostic
    {
        public static async Task RunAddressDiagnostic()
        {
            try
            {
                Log.Information("=== Modbus地址诊断工具 ===");
                Log.Information(VersionInfo.FullVersionInfo);

                // 创建配置
                var config = new IndustrialProtocolConfig
                {
                    Name = "Address_Diagnostic",
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
                
                Log.Information("正在连接设备 192.168.6.6:502...");
                
                // 1. 测试不同地址格式
                await TestDifferentAddressFormats(protocol);
                
                // 2. 扫描可用地址范围
                await ScanAddressRange(protocol);
                
                // 3. 测试特定地址
                await TestSpecificAddresses(protocol);
                
                Log.Information("=== 诊断完成 ===");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "地址诊断过程中发生错误: {0}", ex.Message);
            }
        }

        /// <summary>
        /// 测试不同地址格式
        /// </summary>
        private static async Task TestDifferentAddressFormats(AsyncModbusTcpProtocol protocol)
        {
            Log.Information("\n=== 1. 测试不同地址格式 ===");
            
            var testAddresses = new[]
            {
                "4500",    // 纯数字格式
                "D4500",   // D前缀格式
                "@4500",   // 0基地址格式
                "40001",   // 标准Modbus格式
                "4510",    // 目标地址
                "D4510",   // D前缀目标地址
                "@4510"    // 0基目标地址
            };

            foreach (var address in testAddresses)
            {
                try
                {
                    Log.Information($"测试地址格式: {address}");
                    var value = await protocol.ReadAsync(address, "signed");
                    Log.Information($"✅ {address} = {value}");
                }
                catch (Exception ex)
                {
                    Log.Error($"❌ {address} 失败: {ex.Message}");
                }
                
                await Task.Delay(100);
            }
        }

        /// <summary>
        /// 扫描地址范围
        /// </summary>
        private static async Task ScanAddressRange(AsyncModbusTcpProtocol protocol)
        {
            Log.Information("\n=== 2. 扫描地址范围 4490-4520 ===");
            
            var availableAddresses = new List<int>();
            
            for (int addr = 4490; addr <= 4520; addr++)
            {
                try
                {
                    var value = await protocol.ReadAsync(addr.ToString(), "signed");
                    availableAddresses.Add(addr);
                    Log.Information($"✅ 地址 {addr}: {value}");
                }
                catch (Exception ex)
                {
                    Log.Debug($"❌ 地址 {addr}: {ex.Message}");
                }
                
                await Task.Delay(50);
            }
            
            Log.Information($"\n发现 {availableAddresses.Count} 个可用地址:");
            foreach (var addr in availableAddresses)
            {
                Log.Information($"  - {addr}");
            }
        }

        /// <summary>
        /// 测试特定地址
        /// </summary>
        private static async Task TestSpecificAddresses(AsyncModbusTcpProtocol protocol)
        {
            Log.Information("\n=== 3. 测试特定地址 ===");
            
            var specificAddresses = new[] { 4508, 4509, 4510, 4511 };
            
            foreach (var addr in specificAddresses)
            {
                Log.Information($"\n--- 测试地址 {addr} ---");
                
                // 测试不同格式
                var formats = new[]
                {
                    addr.ToString(),      // 纯数字
                    $"D{addr}",          // D前缀
                    $"@{addr}",          // 0基地址
                    $"{addr + 40000}"    // 标准Modbus格式
                };
                
                foreach (var format in formats)
                {
                    try
                    {
                        Log.Information($"  格式 {format}:");
                        var value = await protocol.ReadAsync(format, "signed");
                        Log.Information($"    ✅ 成功: {value}");
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"    ❌ 失败: {ex.Message}");
                    }
                    
                    await Task.Delay(50);
                }
            }
        }
    }
}
