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
    /// 增强版D4500测试 - 使用高性能方法
    /// 演示ParseBatchResponseUltraOptimized、OptimizeDataPointRequestsUltra、
    /// CreateReadRequestAsync、ReadDataPointsAsync、PollDataAsync等核心方法
    /// </summary>
    public class EnhancedD4500Test
    {
        public static async Task RunEnhancedD4500Test()
        {
            try
            {
                Log.Information("=== BYWGLib 增强版D4500-D4510 高性能测试开始 ===");
                Log.Information(VersionInfo.FullVersionInfo);

                // 创建配置
                var config = new IndustrialProtocolConfig
                {
                    Name = "Enhanced_D4500_Test",
                    Type = "ModbusTCP",
                    Parameters = new Dictionary<string, string>
                    {
                        { "IpAddress", "192.168.6.6" },
                        { "Port", "502" },
                        { "Timeout", "5000" },
                        { "UnitId", "1" }
                    }
                };

                // 创建协议实例
                using var protocol = new AsyncModbusTcpProtocol(config);
                
                Log.Information("正在连接设备 192.168.6.6:502...");
                
                // 1. 测试单个地址读取（传统方式）
                await TestSingleAddressReads(protocol);
                
                // 2. 测试批量地址读取（使用OptimizeDataPointRequestsUltra）
                await TestBatchAddressReads(protocol);
                
                // 3. 测试异步轮询（使用PollDataAsync）
                await TestAsyncPolling(protocol);
                
                // 4. 性能对比测试
                await TestPerformanceComparison(protocol);
                
                Log.Information("=== 增强版测试完成 ===");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "增强版D4500测试过程中发生错误: {0}", ex.Message);
            }
        }

        /// <summary>
        /// 测试单个地址读取（传统方式）
        /// </summary>
        private static async Task TestSingleAddressReads(AsyncModbusTcpProtocol protocol)
        {
            Log.Information("\n=== 1. 单个地址读取测试 ===");
            
            var testResults = new List<EnhancedTestResult>();
            var stopwatch = Stopwatch.StartNew();
            
            for (int i = 4500; i <= 4510; i++)
            {
                var address = $"{i}";
                var result = await TestSingleAddress(protocol, address);
                testResults.Add(result);
                
                // 避免过快请求
                await Task.Delay(50);
            }
            
            stopwatch.Stop();
            
            Log.Information($"单个地址读取完成，总耗时: {stopwatch.ElapsedMilliseconds}ms");
            LogTestResults("单个地址读取", testResults);
        }

        /// <summary>
        /// 测试批量地址读取（使用OptimizeDataPointRequestsUltra优化）
        /// </summary>
        private static async Task TestBatchAddressReads(AsyncModbusTcpProtocol protocol)
        {
            Log.Information("\n=== 2. 批量地址读取测试（使用OptimizeDataPointRequestsUltra） ===");
            
            // 创建数据点列表 - 只测试有效的地址范围（4500-4508）
            var dataPoints = new List<ModbusDataPoint>();
            for (int i = 4500; i <= 4508; i++) // 只测试有效的地址范围
            {
                dataPoints.Add(new ModbusDataPoint
                {
                    Name = $"D{i}", // 使用D前缀格式，与单个读取保持一致
                    Address = i.ToString(), // 地址保持数字格式
                    FunctionCode = 3, // 读保持寄存器
                    DataType = "signed"
                });
            }
            
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                // 使用ReadDataPointsAsync方法，内部会调用OptimizeDataPointRequestsUltra
                var dataItems = await ReadDataPointsAsync(protocol, dataPoints);
                stopwatch.Stop();
                
                Log.Information($"批量地址读取完成，总耗时: {stopwatch.ElapsedMilliseconds}ms");
                Log.Information($"读取到 {dataItems.Count} 个数据项");
                
                // 显示结果
                foreach (var item in dataItems)
                {
                    Log.Information($"✅ {item.Name}: {item.Value} (质量: {item.Quality})");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "批量地址读取失败: {0}", ex.Message);
            }
        }

        /// <summary>
        /// 测试异步轮询（使用PollDataAsync）
        /// </summary>
        private static async Task TestAsyncPolling(AsyncModbusTcpProtocol protocol)
        {
            Log.Information("\n=== 3. 异步轮询测试（使用PollDataAsync） ===");
            
            try
            {
                // 启动协议
                protocol.Start();
                
                // 设置数据点（模拟配置数据点）
                var dataPoints = new List<ModbusDataPoint>();
                for (int i = 4500; i <= 4505; i++) // 测试前6个地址
                {
                    dataPoints.Add(new ModbusDataPoint
                    {
                        Name = $"D{i}",
                        Address = i.ToString(),
                        FunctionCode = 3,
                        DataType = "signed"
                    });
                }
                
                // 订阅数据接收事件
                protocol.DataReceived += (sender, e) =>
                {
                    Log.Information($"📡 接收到数据: {e.ProtocolName}, 数据项数量: {e.DataItems.Count}");
                    foreach (var item in e.DataItems)
                    {
                        Log.Information($"  - {item.Name}: {item.Value} (时间: {item.Timestamp:HH:mm:ss.fff})");
                    }
                };
                
                // 执行3次轮询
                for (int i = 1; i <= 3; i++)
                {
                    Log.Information($"执行第 {i} 次轮询...");
                    await protocol.PollDataAsync();
                    await Task.Delay(1000); // 等待1秒
                }
                
                protocol.Stop();
                Log.Information("异步轮询测试完成");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "异步轮询测试失败: {0}", ex.Message);
            }
        }

        /// <summary>
        /// 性能对比测试
        /// </summary>
        private static async Task TestPerformanceComparison(AsyncModbusTcpProtocol protocol)
        {
            Log.Information("\n=== 4. 性能对比测试 ===");
            
            var testAddresses = new[] { "4500", "4501", "4502", "4503", "4504" };
            
            // 测试1: 逐个读取
            Log.Information("测试1: 逐个读取方式");
            var stopwatch1 = Stopwatch.StartNew();
            foreach (var address in testAddresses)
            {
                try
                {
                    await protocol.ReadAsync(address, "signed");
                }
                catch (Exception ex)
                {
                    Log.Debug($"地址 {address} 读取失败: {ex.Message}");
                }
                await Task.Delay(50);
            }
            stopwatch1.Stop();
            Log.Information($"逐个读取完成，耗时: {stopwatch1.ElapsedMilliseconds}ms");
            
            // 测试2: 批量读取（使用优化方法）
            Log.Information("测试2: 批量读取方式（使用OptimizeDataPointRequestsUltra）");
            var stopwatch2 = Stopwatch.StartNew();
            
            var dataPoints = testAddresses.Select(addr => new ModbusDataPoint
            {
                Name = $"D{addr}",
                Address = addr,
                FunctionCode = 3,
                DataType = "signed"
            }).ToList();
            
            try
            {
                var dataItems = await ReadDataPointsAsync(protocol, dataPoints);
                stopwatch2.Stop();
                Log.Information($"批量读取完成，耗时: {stopwatch2.ElapsedMilliseconds}ms");
                Log.Information($"性能提升: {stopwatch1.ElapsedMilliseconds / (double)stopwatch2.ElapsedMilliseconds:F2}倍");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "批量读取失败: {0}", ex.Message);
            }
        }

        /// <summary>
        /// 使用真正的批量读取方法（通过反射调用私有方法）
        /// </summary>
        private static async Task<List<IndustrialDataItem>> ReadDataPointsAsync(AsyncModbusTcpProtocol protocol, List<ModbusDataPoint> dataPoints)
        {
            try
            {
                // 使用反射调用私有的ReadDataPointsAsync方法
                var method = typeof(AsyncModbusTcpProtocol).GetMethod("ReadDataPointsAsync", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (method != null)
                {
                    var task = (Task<List<IndustrialDataItem>>)method.Invoke(protocol, new object[] { dataPoints });
                    return await task;
                }
                else
                {
                    Log.Warning("无法找到ReadDataPointsAsync私有方法，使用单个读取模拟");
                    return await ReadDataPointsIndividually(protocol, dataPoints);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "调用批量读取方法失败，使用单个读取模拟: {0}", ex.Message);
                return await ReadDataPointsIndividually(protocol, dataPoints);
            }
        }

        /// <summary>
        /// 单个读取模拟方法
        /// </summary>
        private static async Task<List<IndustrialDataItem>> ReadDataPointsIndividually(AsyncModbusTcpProtocol protocol, List<ModbusDataPoint> dataPoints)
        {
            var dataItems = new List<IndustrialDataItem>();
            
            foreach (var dataPoint in dataPoints)
            {
                try
                {
                    var value = await protocol.ReadAsync(dataPoint.Address, dataPoint.DataType);
                    dataItems.Add(new IndustrialDataItem
                    {
                        Id = $"{protocol.Name}.{dataPoint.Name}",
                        Name = dataPoint.Name,
                        Value = value,
                        DataType = dataPoint.DataType,
                        Timestamp = DateTime.Now,
                        Quality = Quality.Good
                    });
                }
                catch (Exception ex)
                {
                    Log.Debug($"读取数据点 {dataPoint.Name} 失败: {ex.Message}");
                    dataItems.Add(new IndustrialDataItem
                    {
                        Id = $"{protocol.Name}.{dataPoint.Name}",
                        Name = dataPoint.Name,
                        Value = null,
                        DataType = dataPoint.DataType,
                        Timestamp = DateTime.Now,
                        Quality = Quality.Bad
                    });
                }
            }
            
            return dataItems;
        }

        /// <summary>
        /// 测试单个地址
        /// </summary>
        private static async Task<EnhancedTestResult> TestSingleAddress(AsyncModbusTcpProtocol protocol, string address)
        {
            var result = new EnhancedTestResult
            {
                Address = address,
                TestTime = DateTime.Now
            };

            try
            {
                Log.Information($"测试地址: {address}");
                
                var startTime = DateTime.Now;
                var value = await protocol.ReadAsync(address, "signed");
                var endTime = DateTime.Now;
                
                result.Success = true;
                result.Value = value;
                result.ResponseTime = (endTime - startTime).TotalMilliseconds;
                
                Log.Information($"✅ {address} = {value} (响应时间: {result.ResponseTime:F2}ms)");
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Error = ex.Message;
                result.ResponseTime = 0;
                
                Log.Error($"❌ {address} 失败: {ex.Message}");
            }

            return result;
        }

        /// <summary>
        /// 记录测试结果
        /// </summary>
        private static void LogTestResults(string testName, List<EnhancedTestResult> results)
        {
            Log.Information($"\n=== {testName} 结果统计 ===");
            Log.Information($"总测试数: {results.Count}");
            
            var successCount = results.Count(r => r.Success);
            var failureCount = results.Count - successCount;
            var successRate = successCount * 100.0 / results.Count;
            
            Log.Information($"成功数量: {successCount}");
            Log.Information($"失败数量: {failureCount}");
            Log.Information($"成功率: {successRate:F1}%");
            
            if (successCount > 0)
            {
                var avgResponseTime = results.Where(r => r.Success).Average(r => r.ResponseTime);
                var minResponseTime = results.Where(r => r.Success).Min(r => r.ResponseTime);
                var maxResponseTime = results.Where(r => r.Success).Max(r => r.ResponseTime);
                
                Log.Information($"平均响应时间: {avgResponseTime:F2}ms");
                Log.Information($"最小响应时间: {minResponseTime:F2}ms");
                Log.Information($"最大响应时间: {maxResponseTime:F2}ms");
            }
        }
    }

    /// <summary>
    /// 增强版测试结果
    /// </summary>
    public class EnhancedTestResult
    {
        public string Address { get; set; } = string.Empty;
        public bool Success { get; set; }
        public object Value { get; set; } = null;
        public string Error { get; set; } = string.Empty;
        public double ResponseTime { get; set; }
        public DateTime TestTime { get; set; }
        public string TestMethod { get; set; } = string.Empty;
    }
}
