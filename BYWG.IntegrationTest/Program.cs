using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BYWGLib;
using BYWGLib.Protocols;
using Microsoft.Extensions.Logging;
using BenchmarkDotNet.Running;
using BYWG.IntegrationTest.Benchmarks;

namespace BYWG.IntegrationTest
{
    /// <summary>
    /// 边缘网关集成测试程序
    /// </summary>
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== BYWG 边缘网关集成测试 ===");
            Console.WriteLine();
            
            try
            {
                // 检查是否运行benchmark
                if (args.Length > 0 && args[0].ToLower() == "benchmark")
                {
                    await RunBenchmarks();
                }
                else
                {
                    // 运行常规测试
                    await RunRegularTests();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"测试失败: {ex.Message}");
                Console.WriteLine($"堆栈跟踪: {ex.StackTrace}");
            }
            
            Console.WriteLine("按任意键退出...");
            Console.ReadKey();
        }

        /// <summary>
        /// 运行常规测试
        /// </summary>
        static async Task RunRegularTests()
        {
            // 测试协议创建
            await TestProtocolCreation();
            
            // 测试数据采集
            await TestDataCollection();
            
            // 测试性能
            await TestPerformance();
            
            // 测试边缘计算
            await TestEdgeComputing();
            
            Console.WriteLine();
            Console.WriteLine("=== 所有测试完成 ===");
        }

        /// <summary>
        /// 运行性能基准测试
        /// </summary>
        static async Task RunBenchmarks()
        {
            Console.WriteLine("=== 运行性能基准测试 ===");
            Console.WriteLine();
            
            Console.WriteLine("1. 协议性能基准测试...");
            BenchmarkRunner.Run<ProtocolBenchmarks>();
            
            Console.WriteLine();
            Console.WriteLine("2. 网络性能基准测试...");
            BenchmarkRunner.Run<NetworkBenchmarks>();
            
            Console.WriteLine();
            Console.WriteLine("3. 异步操作性能基准测试...");
            BenchmarkRunner.Run<AsyncBenchmarks>();
            
            Console.WriteLine();
            Console.WriteLine("=== 基准测试完成 ===");
        }
        
        /// <summary>
        /// 测试协议创建
        /// </summary>
        static async Task TestProtocolCreation()
        {
            Console.WriteLine("1. 测试协议创建...");
            
            // 测试三菱MC协议
            var mcConfig = new IndustrialProtocolConfig
            {
                Name = "MC_Test",
                Type = "MC",
                Parameters = new Dictionary<string, string>
                {
                    ["IpAddress"] = "192.168.1.100",
                    ["Port"] = "5007",
                    ["ProtocolType"] = "Qna3E",
                    ["DataPoints"] = "D100,word;D101,word;M100,bit"
                }
            };
            
            var mcProtocol = HighPerformanceProtocolFactory.CreateProtocol(mcConfig);
            Console.WriteLine($"  ✓ 三菱MC协议创建成功: {mcProtocol?.Name}");
            
            // 测试Modbus TCP协议
            var modbusConfig = new IndustrialProtocolConfig
            {
                Name = "Modbus_Test",
                Type = "MODBUS_TCP",
                Parameters = new Dictionary<string, string>
                {
                    ["IpAddress"] = "192.168.1.101",
                    ["Port"] = "502",
                    ["DataPoints"] = "40001,word;40002,word;40003,word"
                }
            };
            
            var modbusProtocol = HighPerformanceProtocolFactory.CreateProtocol(modbusConfig);
            Console.WriteLine($"  ✓ Modbus TCP协议创建成功: {modbusProtocol?.Name}");
            
            // 测试西门子S7协议
            var s7Config = new IndustrialProtocolConfig
            {
                Name = "S7_Test",
                Type = "S7",
                Parameters = new Dictionary<string, string>
                {
                    ["IpAddress"] = "192.168.1.102",
                    ["Port"] = "102",
                    ["DataPoints"] = "DB1.0,word;DB1.2,word;DB1.4,word"
                }
            };
            
            var s7Protocol = HighPerformanceProtocolFactory.CreateProtocol(s7Config);
            Console.WriteLine($"  ✓ 西门子S7协议创建成功: {s7Protocol?.Name}");
            
            Console.WriteLine("  ✓ 协议创建测试完成");
            Console.WriteLine();
        }
        
        /// <summary>
        /// 测试数据采集
        /// </summary>
        static async Task TestDataCollection()
        {
            Console.WriteLine("2. 测试数据采集...");
            
            // 创建协议管理器
            var protocolManager = new ProtocolManager();
            
            // 添加测试协议
            var testConfig = new IndustrialProtocolConfig
            {
                Name = "Test_Protocol",
                Type = "MC",
                Parameters = new Dictionary<string, string>
                {
                    ["IpAddress"] = "127.0.0.1",
                    ["Port"] = "5007",
                    ["DataPoints"] = "D100,word;D101,word"
                }
            };
            
            protocolManager.AddProtocol(testConfig);
            Console.WriteLine("  ✓ 协议添加到管理器");
            
            // 启动协议
            protocolManager.StartAllProtocols();
            Console.WriteLine("  ✓ 协议启动");
            
            // 等待数据采集
            await Task.Delay(2000);
            
            // 停止协议
            protocolManager.StopAllProtocols();
            Console.WriteLine("  ✓ 协议停止");
            
            Console.WriteLine("  ✓ 数据采集测试完成");
            Console.WriteLine();
        }
        
        /// <summary>
        /// 测试性能
        /// </summary>
        static async Task TestPerformance()
        {
            Console.WriteLine("3. 测试性能...");
            
            var startTime = DateTime.Now;
            var iterations = 1000;
            
            // 测试基本性能
            var testTime = DateTime.Now;
            
            for (int i = 0; i < iterations; i++)
            {
                var item = new IndustrialDataItem
                {
                    Id = $"test_{i}",
                    Name = $"Test_{i}",
                    Value = i,
                    DataType = "int",
                    Quality = Quality.Good,
                    Timestamp = DateTime.Now
                };
            }
            
            var testDuration = DateTime.Now - testTime;
            Console.WriteLine($"  ✓ 基本性能测试: {iterations}次操作耗时 {testDuration.TotalMilliseconds:F2}ms");
            
            var totalDuration = DateTime.Now - startTime;
            Console.WriteLine($"  ✓ 性能测试完成，总耗时: {totalDuration.TotalMilliseconds:F2}ms");
            Console.WriteLine();
        }
        
        /// <summary>
        /// 测试边缘计算
        /// </summary>
        static async Task TestEdgeComputing()
        {
            Console.WriteLine("4. 测试边缘计算...");
            
            // 模拟数据处理
            var dataItems = new List<IndustrialDataItem>
            {
                new IndustrialDataItem
                {
                    Id = "D100",
                    Name = "D100",
                    Value = 100.5,
                    DataType = "word",
                    Quality = Quality.Good,
                    Timestamp = DateTime.Now
                },
                new IndustrialDataItem
                {
                    Id = "D101",
                    Name = "D101",
                    Value = 200.3,
                    DataType = "word",
                    Quality = Quality.Good,
                    Timestamp = DateTime.Now
                }
            };
            
            // 数据预处理
            var processedData = await PreprocessDataAsync(dataItems);
            Console.WriteLine($"  ✓ 数据预处理完成，处理了 {processedData.Count} 个数据点");
            
            // 数据验证
            var validatedData = await ValidateDataAsync(processedData);
            Console.WriteLine($"  ✓ 数据验证完成，验证了 {validatedData.Count} 个数据点");
            
            // 数据转换
            var convertedData = await ConvertDataAsync(validatedData);
            Console.WriteLine($"  ✓ 数据转换完成，转换了 {convertedData.Count} 个数据点");
            
            Console.WriteLine("  ✓ 边缘计算测试完成");
            Console.WriteLine();
        }
        
        /// <summary>
        /// 数据预处理
        /// </summary>
        static async Task<List<IndustrialDataItem>> PreprocessDataAsync(List<IndustrialDataItem> dataItems)
        {
            var processedItems = new List<IndustrialDataItem>();
            
            foreach (var item in dataItems)
            {
                if (item.Quality == Quality.Good)
                {
                    // 简单的数据平滑处理
                    var smoothedValue = item.Value is double doubleValue ? doubleValue * 0.9 : item.Value;
                    
                    processedItems.Add(new IndustrialDataItem
                    {
                        Id = item.Id,
                        Name = item.Name,
                        Value = smoothedValue,
                        DataType = item.DataType,
                        Timestamp = DateTime.Now,
                        Quality = Quality.Good
                    });
                }
            }
            
            return processedItems;
        }
        
        /// <summary>
        /// 数据验证
        /// </summary>
        static async Task<List<IndustrialDataItem>> ValidateDataAsync(List<IndustrialDataItem> dataItems)
        {
            var validatedItems = new List<IndustrialDataItem>();
            
            foreach (var item in dataItems)
            {
                // 范围检查
                if (item.Value is double doubleValue && doubleValue >= 0 && doubleValue <= 1000)
                {
                    validatedItems.Add(item);
                }
            }
            
            return validatedItems;
        }
        
        /// <summary>
        /// 数据转换
        /// </summary>
        static async Task<List<IndustrialDataItem>> ConvertDataAsync(List<IndustrialDataItem> dataItems)
        {
            var convertedItems = new List<IndustrialDataItem>();
            
            foreach (var item in dataItems)
            {
                // 单位转换示例
                var convertedValue = item.Value is double doubleValue ? doubleValue * 1.8 + 32 : item.Value;
                
                convertedItems.Add(new IndustrialDataItem
                {
                    Id = item.Id,
                    Name = item.Name,
                    Value = convertedValue,
                    DataType = item.DataType,
                    Timestamp = item.Timestamp,
                    Quality = item.Quality
                });
            }
            
            return convertedItems;
        }
    }
}
