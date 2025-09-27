using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using BYWGLib;
using BYWGLib.Logging;
using BYWGLib.Protocols;

namespace BYWGLibDemo
{
    /// <summary>
    /// 修正版D4500测试 - 使用正确的地址格式访问4510
    /// </summary>
    public class CorrectedD4500Test
    {
        public static async Task RunCorrectedD4500Test()
        {
            try
            {
                Log.Information("=== BYWGLib 修正版D4500-D4510 测试开始 ===");
                Log.Information(VersionInfo.FullVersionInfo);

                // 创建配置
                var config = new IndustrialProtocolConfig
                {
                    Name = "Corrected_D4500_Test",
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

                var testResults = new List<(string address, object value, string error, long responseTimeMs)>();
                var stopwatch = new Stopwatch();

                // 测试4500-4508（0基地址）
                Log.Information("\n=== 1. 测试4500-4508（0基地址） ===");
                for (int i = 4500; i <= 4508; i++)
                {
                    string address = i.ToString();
                    Log.Information($"测试地址: {address}");
                    object result = null;
                    string error = null;
                    long responseTime = 0;

                    try
                    {
                        stopwatch.Restart();
                        result = await protocol.ReadAsync(address, "signed");
                        stopwatch.Stop();
                        responseTime = stopwatch.ElapsedMilliseconds;

                        if (result != null)
                        {
                            Log.Information($"✅ {address} = {result} (响应时间: {responseTime}ms)");
                        }
                        else
                        {
                            error = "返回null";
                            Log.Error($"❌ {address} 失败: {error}");
                        }
                    }
                    catch (Exception ex)
                    {
                        stopwatch.Stop();
                        responseTime = stopwatch.ElapsedMilliseconds;
                        error = ex.Message;
                        Log.Error($"❌ {address} 失败: {ex.Message}");
                    }
                    testResults.Add((address, result, error, responseTime));
                    await Task.Delay(100);
                }

                // 测试4509-4510（使用标准Modbus格式）
                Log.Information("\n=== 2. 测试4509-4510（标准Modbus格式） ===");
                for (int i = 4509; i <= 4510; i++)
                {
                    // 使用标准Modbus格式：4509 -> 44509, 4510 -> 44510
                    string address = (i + 40000).ToString();
                    string displayAddress = i.ToString();
                    Log.Information($"测试地址: {displayAddress} (使用标准Modbus格式: {address})");
                    object result = null;
                    string error = null;
                    long responseTime = 0;

                    try
                    {
                        stopwatch.Restart();
                        result = await protocol.ReadAsync(address, "signed");
                        stopwatch.Stop();
                        responseTime = stopwatch.ElapsedMilliseconds;

                        if (result != null)
                        {
                            Log.Information($"✅ {displayAddress} = {result} (响应时间: {responseTime}ms)");
                        }
                        else
                        {
                            error = "返回null";
                            Log.Error($"❌ {displayAddress} 失败: {error}");
                        }
                    }
                    catch (Exception ex)
                    {
                        stopwatch.Stop();
                        responseTime = stopwatch.ElapsedMilliseconds;
                        error = ex.Message;
                        Log.Error($"❌ {displayAddress} 失败: {ex.Message}");
                    }
                    testResults.Add((displayAddress, result, error, responseTime));
                    await Task.Delay(100);
                }

                // 生成报告
                GenerateReport(testResults, config);
                Log.Information("=== 测试完成 ===");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "修正版测试过程中发生错误: {0}", ex.Message);
            }
        }

        private static void GenerateReport(List<(string address, object value, string error, long responseTimeMs)> results, IndustrialProtocolConfig config)
        {
            Log.Information("\n=== 修正版测试报告 ===");
            Log.Information($"测试时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            Log.Information($"设备地址: {config.Parameters["IpAddress"]}:{config.Parameters["Port"]}");
            Log.Information($"测试范围: 4500-4510");
            Log.Information($"总测试数: {results.Count}");

            var successfulTests = results.Where(r => r.value != null).ToList();
            var failedTests = results.Where(r => r.value == null).ToList();

            Log.Information($"成功数量: {successfulTests.Count}");
            Log.Information($"失败数量: {failedTests.Count}");
            Log.Information($"成功率: {successfulTests.Count * 100.0 / results.Count:F1}%");

            if (successfulTests.Any())
            {
                var avgResponseTime = successfulTests.Average(r => r.responseTimeMs);
                var minResponseTime = successfulTests.Min(r => r.responseTimeMs);
                var maxResponseTime = successfulTests.Max(r => r.responseTimeMs);
                Log.Information($"平均响应时间: {avgResponseTime:F2}ms");
                Log.Information($"最小响应时间: {minResponseTime}ms");
                Log.Information($"最大响应时间: {maxResponseTime}ms");
            }
            else
            {
                Log.Information("无成功读取，无法计算响应时间统计。");
            }

            Log.Information("\n=== 详细结果 ===");
            foreach (var r in results)
            {
                if (r.value != null)
                {
                    Log.Information($"✅ {r.address}: {r.value} ({r.responseTimeMs}ms)");
                }
                else
                {
                    Log.Information($"❌ {r.address}: {r.error}");
                }
            }

            // 地址格式说明
            Log.Information("\n=== 地址格式说明 ===");
            Log.Information("4500-4508: 使用0基地址格式（直接使用数字）");
            Log.Information("4509-4510: 使用标准Modbus格式（4509 -> 44509, 4510 -> 44510）");
            Log.Information("这是因为您的Modbus Slave配置的是1基地址，需要标准Modbus格式来访问4510");
        }
    }
}
