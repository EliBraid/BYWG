using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using BYWGLib.Logging;
using BYWGLib.Protocols;

namespace BYWGLib.PerformanceTests
{
    /// <summary>
    /// 异步协议性能测试
    /// 对比同步协议和异步协议的性能差异
    /// </summary>
    public class AsyncProtocolPerformanceTest
    {
        private readonly AsyncProtocolManager _asyncManager;
        private readonly List<IIndustrialProtocol> _asyncProtocols;
        private readonly List<IIndustrialProtocol> _syncProtocols;
        private readonly object _metrics = new object();
        
        public AsyncProtocolPerformanceTest()
        {
            _asyncManager = new AsyncProtocolManager();
            _asyncProtocols = new List<IIndustrialProtocol>();
            _syncProtocols = new List<IIndustrialProtocol>();
            _metrics = new object();
        }
        
        /// <summary>
        /// 运行完整的性能测试
        /// </summary>
        public async Task<PerformanceTestResult> RunFullPerformanceTest()
        {
            Log.Information("开始异步协议性能测试...");
            
            var result = new PerformanceTestResult
            {
                TestName = "异步协议性能测试",
                StartTime = DateTime.Now
            };
            
            try
            {
                // 1. 测试异步协议性能
                var asyncResult = await TestAsyncProtocolPerformance();
                result.AsyncProtocolResults = asyncResult;
                
                // 2. 测试同步协议性能
                var syncResult = await TestSyncProtocolPerformance();
                result.SyncProtocolResults = syncResult;
                
                // 3. 计算性能提升
                result.PerformanceImprovement = CalculatePerformanceImprovement(asyncResult, syncResult);
                
                result.EndTime = DateTime.Now;
                result.Duration = result.EndTime - result.StartTime;
                
                Log.Information("异步协议性能测试完成，总耗时: {0}ms", result.Duration.TotalMilliseconds);
                
                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "异步协议性能测试失败");
                result.Error = ex.Message;
                return result;
            }
        }
        
        /// <summary>
        /// 测试异步协议性能
        /// </summary>
        private async Task<ProtocolPerformanceResult> TestAsyncProtocolPerformance()
        {
            Log.Information("测试异步协议性能...");
            
            var result = new ProtocolPerformanceResult
            {
                ProtocolType = "异步协议",
                StartTime = DateTime.Now
            };
            
            try
            {
                // 创建测试协议
                var testProtocols = CreateTestAsyncProtocols();
                
                // 添加到管理器
                foreach (var protocol in testProtocols)
                {
                    _asyncManager.AddProtocol((IAsyncIndustrialProtocol)protocol);
                }
                
                // 启动管理器
                _asyncManager.Start();
                
                // 等待协议稳定
                await Task.Delay(2000);
                
                // 执行性能测试
                var testResults = await ExecutePerformanceTest(async () =>
                {
                    await _asyncManager.PollAllAsync();
                });
                
                result.TestResults = testResults;
                result.EndTime = DateTime.Now;
                result.Duration = result.EndTime - result.StartTime;
                
                // 停止管理器
                _asyncManager.Stop();
                
                Log.Information("异步协议性能测试完成，平均延迟: {0}ms，吞吐量: {1} req/s", 
                    testResults.AverageLatency, testResults.Throughput);
                
                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "异步协议性能测试失败");
                result.Error = ex.Message;
                return result;
            }
        }
        
        /// <summary>
        /// 测试同步协议性能
        /// </summary>
        private async Task<ProtocolPerformanceResult> TestSyncProtocolPerformance()
        {
            Log.Information("测试同步协议性能...");
            
            var result = new ProtocolPerformanceResult
            {
                ProtocolType = "同步协议",
                StartTime = DateTime.Now
            };
            
            try
            {
                // 创建测试协议
                var testProtocols = CreateTestSyncProtocols();
                
                // 启动协议
                foreach (var protocol in testProtocols)
                {
                    protocol.Start();
                }
                
                // 等待协议稳定
                await Task.Delay(2000);
                
                // 执行性能测试
                var testResults = await ExecutePerformanceTest(async () =>
                {
                    var tasks = testProtocols.Select(async protocol =>
                    {
                        if (protocol.IsRunning)
                        {
                            // 模拟同步轮询
                            await Task.Run(() => protocol.PollData());
                        }
                    });
                    
                    await Task.WhenAll(tasks);
                });
                
                result.TestResults = testResults;
                result.EndTime = DateTime.Now;
                result.Duration = result.EndTime - result.StartTime;
                
                // 停止协议
                foreach (var protocol in testProtocols)
                {
                    protocol.Stop();
                }
                
                Log.Information("同步协议性能测试完成，平均延迟: {0}ms，吞吐量: {1} req/s", 
                    testResults.AverageLatency, testResults.Throughput);
                
                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "同步协议性能测试失败");
                result.Error = ex.Message;
                return result;
            }
        }
        
        /// <summary>
        /// 执行性能测试
        /// </summary>
        private async Task<TestExecutionResult> ExecutePerformanceTest(Func<Task> testAction)
        {
            const int testIterations = 100;
            const int warmupIterations = 10;
            
            var latencies = new List<double>();
            var startTime = DateTime.Now;
            
            // 预热
            for (int i = 0; i < warmupIterations; i++)
            {
                await testAction();
            }
            
            // 正式测试
            var testStartTime = DateTime.Now;
            for (int i = 0; i < testIterations; i++)
            {
                var iterationStart = DateTime.Now;
                await testAction();
                var iterationEnd = DateTime.Now;
                
                latencies.Add((iterationEnd - iterationStart).TotalMilliseconds);
            }
            var testEndTime = DateTime.Now;
            
            var totalDuration = testEndTime - testStartTime;
            var throughput = testIterations / totalDuration.TotalSeconds;
            var averageLatency = latencies.Average();
            var minLatency = latencies.Min();
            var maxLatency = latencies.Max();
            var p95Latency = CalculatePercentile(latencies, 0.95);
            var p99Latency = CalculatePercentile(latencies, 0.99);
            
            return new TestExecutionResult
            {
                Iterations = testIterations,
                TotalDuration = totalDuration,
                Throughput = throughput,
                AverageLatency = averageLatency,
                MinLatency = minLatency,
                MaxLatency = maxLatency,
                P95Latency = p95Latency,
                P99Latency = p99Latency
            };
        }
        
        /// <summary>
        /// 计算百分位数
        /// </summary>
        private double CalculatePercentile(List<double> values, double percentile)
        {
            var sortedValues = values.OrderBy(x => x).ToList();
            int index = (int)Math.Ceiling(percentile * sortedValues.Count) - 1;
            return sortedValues[Math.Max(0, Math.Min(index, sortedValues.Count - 1))];
        }
        
        /// <summary>
        /// 计算性能提升
        /// </summary>
        private PerformanceImprovement CalculatePerformanceImprovement(
            ProtocolPerformanceResult asyncResult, 
            ProtocolPerformanceResult syncResult)
        {
            if (asyncResult.TestResults == null || syncResult.TestResults == null)
                return new PerformanceImprovement();
            
            var asyncMetrics = asyncResult.TestResults;
            var syncMetrics = syncResult.TestResults;
            
            return new PerformanceImprovement
            {
                ThroughputImprovement = (asyncMetrics.Throughput / syncMetrics.Throughput - 1) * 100,
                LatencyImprovement = (syncMetrics.AverageLatency / asyncMetrics.AverageLatency - 1) * 100,
                P95LatencyImprovement = (syncMetrics.P95Latency / asyncMetrics.P95Latency - 1) * 100,
                P99LatencyImprovement = (syncMetrics.P99Latency / asyncMetrics.P99Latency - 1) * 100
            };
        }
        
        /// <summary>
        /// 创建测试异步协议
        /// </summary>
        private List<IIndustrialProtocol> CreateTestAsyncProtocols()
        {
            var protocols = new List<IIndustrialProtocol>();
            
            // 创建测试配置
            var config1 = new IndustrialProtocolConfig
            {
                Name = "AsyncMC_Test",
                Type = "AsyncUltraHighPerformanceMitsubishiMC",
                Parameters = new Dictionary<string, string>
                {
                    ["IpAddress"] = "192.168.1.100",
                    ["Port"] = "5007",
                    ["DataPoints"] = "D100,word;D101,word;D102,word;D103,word;D104,word"
                }
            };
            
            var config2 = new IndustrialProtocolConfig
            {
                Name = "AsyncModbus_Test",
                Type = "AsyncUltraHighPerformanceModbusTCP",
                Parameters = new Dictionary<string, string>
                {
                    ["IpAddress"] = "192.168.1.101",
                    ["Port"] = "502",
                    ["DataPoints"] = "40001,word;40002,word;40003,word;40004,word;40005,word"
                }
            };
            
            var config3 = new IndustrialProtocolConfig
            {
                Name = "AsyncS7_Test",
                Type = "AsyncUltraHighPerformanceS7",
                Parameters = new Dictionary<string, string>
                {
                    ["IpAddress"] = "192.168.1.102",
                    ["Port"] = "102",
                    ["DataPoints"] = "DB1.0,word;DB1.2,word;DB1.4,word;DB1.6,word;DB1.8,word"
                }
            };
            
            // 创建协议实例
            protocols.Add(new AsyncMitsubishiMCProtocol(config1));
            protocols.Add(new AsyncModbusTcpProtocol(config2));
            protocols.Add(new S7Protocol(config3));
            
            return protocols;
        }
        
        /// <summary>
        /// 创建测试同步协议
        /// </summary>
        private List<IIndustrialProtocol> CreateTestSyncProtocols()
        {
            var protocols = new List<IIndustrialProtocol>();
            
            // 创建测试配置
            var config1 = new IndustrialProtocolConfig
            {
                Name = "SyncMC_Test",
                Type = "HighPerformanceMitsubishiMC",
                Parameters = new Dictionary<string, string>
                {
                    ["IpAddress"] = "192.168.1.100",
                    ["Port"] = "5007",
                    ["DataPoints"] = "D100,word;D101,word;D102,word;D103,word;D104,word"
                }
            };
            
            var config2 = new IndustrialProtocolConfig
            {
                Name = "SyncModbus_Test",
                Type = "HighPerformanceModbusTCP",
                Parameters = new Dictionary<string, string>
                {
                    ["IpAddress"] = "192.168.1.101",
                    ["Port"] = "502",
                    ["DataPoints"] = "40001,word;40002,word;40003,word;40004,word;40005,word"
                }
            };
            
            var config3 = new IndustrialProtocolConfig
            {
                Name = "SyncS7_Test",
                Type = "HighPerformanceS7",
                Parameters = new Dictionary<string, string>
                {
                    ["IpAddress"] = "192.168.1.102",
                    ["Port"] = "102",
                    ["DataPoints"] = "DB1.0,word;DB1.2,word;DB1.4,word;DB1.6,word;DB1.8,word"
                }
            };
            
            // 创建协议实例 - 使用异步协议作为同步协议的替代
            protocols.Add(new AsyncMitsubishiMCProtocol(config1));
            protocols.Add(new AsyncModbusTcpProtocol(config2));
            protocols.Add(new S7Protocol(config3));
            
            return protocols;
        }
        
        public void Dispose()
        {
            _asyncManager?.Dispose();
        }
    }
    
    /// <summary>
    /// 性能测试结果
    /// </summary>
    public class PerformanceTestResult
    {
        public string TestName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan Duration { get; set; }
        public ProtocolPerformanceResult AsyncProtocolResults { get; set; }
        public ProtocolPerformanceResult SyncProtocolResults { get; set; }
        public PerformanceImprovement PerformanceImprovement { get; set; }
        public string Error { get; set; }
    }
    
    /// <summary>
    /// 协议性能结果
    /// </summary>
    public class ProtocolPerformanceResult
    {
        public string ProtocolType { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan Duration { get; set; }
        public TestExecutionResult TestResults { get; set; }
        public string Error { get; set; }
    }
    
    /// <summary>
    /// 测试执行结果
    /// </summary>
    public class TestExecutionResult
    {
        public int Iterations { get; set; }
        public TimeSpan TotalDuration { get; set; }
        public double Throughput { get; set; }
        public double AverageLatency { get; set; }
        public double MinLatency { get; set; }
        public double MaxLatency { get; set; }
        public double P95Latency { get; set; }
        public double P99Latency { get; set; }
    }
    
    /// <summary>
    /// 性能提升
    /// </summary>
    public class PerformanceImprovement
    {
        public double ThroughputImprovement { get; set; }
        public double LatencyImprovement { get; set; }
        public double P95LatencyImprovement { get; set; }
        public double P99LatencyImprovement { get; set; }
    }
}
