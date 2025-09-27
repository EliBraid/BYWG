using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BYWGLib;
using BYWGLib.Logging;
using BYWGLib.Protocols;

namespace BYWGLibDemo
{
    public class D4500Test
    {
        public static async Task RunD4500Test()
        {
            try
            {
                Log.Information("=== BYWGLib D4500-D4510 测试开始 ===");
                Log.Information(VersionInfo.FullVersionInfo);

                // 创建配置
                var config = new IndustrialProtocolConfig
                {
                    Name = "D4500_Test",
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
                var protocol = new AsyncModbusTcpProtocol(config);
                
                Log.Information("正在连接设备 192.168.6.6:502...");
                
                // 测试D4500-D4510地址范围
                var testResults = new List<TestResult>();
                
                for (int i = 4500; i <= 4510; i++)
                {
                    var address = $"{i}";
                    var result = await TestSingleAddress(protocol, address);
                    testResults.Add(result);
                    
                    // 避免过快请求
                    await Task.Delay(100);
                }
                
                // 生成测试报告
                GenerateTestReport(testResults);
                
                protocol.Dispose();
                Log.Information("=== 测试完成 ===");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "D4500测试过程中发生错误: {0}", ex.Message);
            }
        }

        private static async Task<TestResult> TestSingleAddress(AsyncModbusTcpProtocol protocol, string address)
        {
            var result = new TestResult
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

        private static void GenerateTestReport(List<TestResult> results)
        {
            Log.Information("\n=== 测试报告 ===");
            Log.Information($"测试时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            Log.Information($"设备地址: 192.168.6.6:502");
            Log.Information($"测试范围: D4500-D4510");
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
            
            Log.Information("\n=== 详细结果 ===");
            foreach (var result in results)
            {
                if (result.Success)
                {
                    Log.Information($"✅ {result.Address}: {result.Value} ({result.ResponseTime:F2}ms)");
                }
                else
                {
                    Log.Information($"❌ {result.Address}: {result.Error}");
                }
            }
        }
    }

    public class TestResult
    {
        public string Address { get; set; } = string.Empty;
        public bool Success { get; set; }
        public object Value { get; set; } = null;
        public string Error { get; set; } = string.Empty;
        public double ResponseTime { get; set; }
        public DateTime TestTime { get; set; }
    }
}
