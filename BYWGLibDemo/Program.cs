using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BYWGLib;
using BYWGLib.Logging;
using BYWGLib.Protocols;

namespace BYWGLibDemo
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            try
            {
                Log.Information("BYWGLibDemo 启动");
                Log.Information(VersionInfo.FullVersionInfo);

                // 直接诊断测试
                
                // D4500诊断测试
                await RunD4500Diagnosis();

                Log.Information("BYWGLibDemo 已停止");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "程序执行出错: {0}", ex.Message);
            }
        }

        private static async Task RunD4500Diagnosis()
        {
            try
            {
                Log.Information("=== 开始D4500诊断测试 ===");

                // 创建配置对象
                var config = new IndustrialProtocolConfig
                {
                    Name = "ModbusTCP_Diagnosis",
                    Type = "ModbusTCP",
                    Parameters = new Dictionary<string, string>
                    {
                        { "Host", "192.168.6.6" }, // 请修改为你的设备IP
                        { "Port", "502" },
                        { "Timeout", "3000" } // 减少超时时间到3秒
                    }
                };

                // 创建高性能Modbus TCP协议实例
                var protocol = new ModbusTcpProtocol(config);
                
                Log.Information("正在连接设备...");
                
                // 先测试基本连接
                try
                {
                    Log.Information("测试基本连接...");
                    var testResult = await protocol.ReadAsync("40001", "uint16");
                    if (testResult != null)
                    {
                        Log.Information($"基本连接测试成功: 40001 = {testResult}");
                    }
                    else
                    {
                        Log.Warning("基本连接测试失败: 40001 返回null");
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "基本连接测试异常: {0}", ex.Message);
                }
                
                // 扫描可用地址
                Log.Information("开始扫描可用地址...");
                await ScanAvailableAddresses(protocol);
                
                // 运行基本测试
                Log.Information("开始基本测试...");
                await RunBasicTests(protocol);
                
                // 运行性能测试
                Log.Information("开始性能测试...");
                await RunPerformanceTests(protocol);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "D4500诊断过程中发生错误: {0}", ex.Message);
            }
        }

        private static async Task ScanAvailableAddresses(ModbusTcpProtocol protocol)
        {
            try
            {
                Log.Information("=== 开始扫描可用地址 ===");
                
                // 扫描常见的Modbus地址范围
                var testAddresses = new[]
                {
                    // 常见的Holding Register地址
                    "40001", "40002", "40003", "40004", "40005",
                    "40100", "40101", "40102", "40103", "40104",
                    "40200", "40201", "40202", "40203", "40204",
                    "40300", "40301", "40302", "40303", "40304",
                    "40400", "40401", "40402", "40403", "40404",
                    "40500", "40501", "40502", "40503", "40504",
                    
                    // 常见的Input Register地址
                    "30001", "30002", "30003", "30004", "30005",
                    "30100", "30101", "30102", "30103", "30104",
                    
                    // D格式地址
                    "D1", "D2", "D3", "D4", "D5",
                    "D10", "D11", "D12", "D13", "D14",
                    "D100", "D101", "D102", "D103", "D104",
                    "D1000", "D1001", "D1002", "D1003", "D1004"
                };

                var availableAddresses = new List<string>();
                
                foreach (var address in testAddresses)
                {
                    try
                    {
                        var result = await protocol.ReadAsync(address, "uint16");
                        if (result != null)
                        {
                            availableAddresses.Add(address);
                            Log.Information($"✅ 可用地址: {address} = {result}");
                        }
                        else
                        {
                            Log.Debug($"❌ 不可用: {address}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Debug($"❌ 异常: {address} - {ex.Message}");
                    }
                    
                    // 避免过快扫描
                    await Task.Delay(100);
                }
                
                Log.Information($"=== 扫描完成，找到 {availableAddresses.Count} 个可用地址 ===");
                if (availableAddresses.Count > 0)
                {
                    Log.Information("可用地址列表:");
                    foreach (var addr in availableAddresses)
                    {
                        Log.Information($"  - {addr}");
                    }
                }
                else
                {
                    Log.Warning("未找到任何可用地址！请检查:");
                    Log.Warning("1. 设备IP地址是否正确");
                    Log.Warning("2. 设备是否支持Modbus TCP");
                    Log.Warning("3. 设备是否已启动并运行");
                    Log.Warning("4. 防火墙是否阻止了502端口");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "地址扫描过程中发生错误: {0}", ex.Message);
            }
        }

        private static async Task RunBasicTests(ModbusTcpProtocol protocol)
        {
            try
            {
                Log.Information("=== 开始基本测试 ===");
                
                // 测试D4500读取
                Log.Information("测试D4500读取...");
                try
                {
                    var result = await protocol.ReadAsync("D4500", "signed");
                    if (result != null)
                    {
                        Log.Information($"D4500 读取成功: {result}");
                    }
                    else
                    {
                        Log.Warning("D4500 读取失败: 返回null");
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"D4500 读取异常: {ex.Message}");
                }
                
                // 测试其他地址
                var testAddresses = new[] { "D100", "D1000", "D2000", "D3000", "D4000" };
                foreach (var addr in testAddresses)
                {
                    try
                    {
                        var result = await protocol.ReadAsync(addr, "signed");
                        if (result != null)
                        {
                            Log.Information($"{addr} 读取成功: {result}");
                        }
                        else
                        {
                            Log.Warning($"{addr} 读取失败: 返回null");
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"{addr} 读取异常: {ex.Message}");
                    }
                }
                
                Log.Information("=== 基本测试完成 ===");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "基本测试过程中发生错误: {0}", ex.Message);
            }
        }

        private static async Task RunPerformanceTests(ModbusTcpProtocol protocol)
        {
            try
            {
                Log.Information("=== 开始性能测试 ===");
                
                // 单次读取性能测试
                await RunSingleReadPerformanceTest(protocol);
                
                // 批量读取性能测试
                await RunBatchReadPerformanceTest(protocol);
                
                // 并发读取性能测试
                await RunConcurrentReadPerformanceTest(protocol);
                
                Log.Information("=== 性能测试完成 ===");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "性能测试过程中发生错误: {0}", ex.Message);
            }
        }

        private static async Task RunSingleReadPerformanceTest(ModbusTcpProtocol protocol)
        {
            const int iterations = 100;
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var successCount = 0;
            
            Log.Information("执行单次读取性能测试...");
            
            for (int i = 0; i < iterations; i++)
            {
                try
                {
                    var result = await protocol.ReadAsync("D4500", "signed");
                    if (result != null) successCount++;
                }
                catch (Exception ex)
                {
                    Log.Debug($"读取失败: {ex.Message}");
                }
            }
            
            stopwatch.Stop();
            var throughput = iterations / stopwatch.Elapsed.TotalSeconds;
            var avgLatency = stopwatch.Elapsed.TotalMilliseconds / iterations;
            var successRate = successCount * 100.0 / iterations;
            
            Log.Information($"单次读取性能: 吞吐量={throughput:F0} ops/sec, 平均延迟={avgLatency:F2}ms, 成功率={successRate:F1}%");
        }

        private static async Task RunBatchReadPerformanceTest(ModbusTcpProtocol protocol)
        {
            const int batchSize = 50;
            var dataPoints = new List<ModbusDataPoint>();
            
            Log.Information("执行批量读取性能测试...");
            
            for (int i = 0; i < batchSize; i++)
            {
                dataPoints.Add(new ModbusDataPoint($"D{i + 100}", $"D{i + 100}", 3, "signed"));
            }
            
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var batchResults = await protocol.ReadBatchAsync(dataPoints);
            stopwatch.Stop();
            
            var throughput = batchSize / stopwatch.Elapsed.TotalSeconds;
            var avgLatency = stopwatch.Elapsed.TotalMilliseconds / batchSize;
            var successCount = batchResults.Count(r => r.Quality == true);
            var successRate = successCount * 100.0 / batchSize;
            
            Log.Information($"批量读取性能: 吞吐量={throughput:F0} ops/sec, 平均延迟={avgLatency:F2}ms, 成功率={successRate:F1}%");
        }

        private static async Task RunConcurrentReadPerformanceTest(ModbusTcpProtocol protocol)
        {
            const int concurrentTasks = 20;
            const int iterationsPerTask = 10;
            
            Log.Information("执行并发读取性能测试...");
            
            var tasks = new List<Task<int>>();
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            for (int i = 0; i < concurrentTasks; i++)
            {
                tasks.Add(Task.Run(async () =>
                {
                    var successCount = 0;
                    for (int j = 0; j < iterationsPerTask; j++)
                    {
                        try
                        {
                            var result = await protocol.ReadAsync($"D{j + 100}", "signed");
                            if (result != null) successCount++;
                        }
                        catch
                        {
                            // 忽略错误
                        }
                    }
                    return successCount;
                }));
            }
            
            var results = await Task.WhenAll(tasks);
            stopwatch.Stop();
            
            var totalOperations = concurrentTasks * iterationsPerTask;
            var totalSuccess = results.Sum();
            var throughput = totalOperations / stopwatch.Elapsed.TotalSeconds;
            var successRate = totalSuccess * 100.0 / totalOperations;
            
            Log.Information($"并发读取性能: 吞吐量={throughput:F0} ops/sec, 成功率={successRate:F1}%");
        }
    }
}