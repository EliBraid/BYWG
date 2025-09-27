using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BYWGLib;
using BYWGLib.Logging;
using BYWGLib.Protocols;

namespace BYWGLibDemo
{
    /// <summary>
    /// BYWGLib 基本使用示例
    /// </summary>
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                Log.Information("BYWGLibDemo 启动");
                Log.Information(VersionInfo.FullVersionInfo);

                // 基本使用示例
                await BasicUsageExample();

                Log.Information("BYWGLibDemo 已停止");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "程序执行过程中发生错误: {0}", ex.Message);
            }
        }

        /// <summary>
        /// 基本使用示例
        /// </summary>
        private static async Task BasicUsageExample()
        {
            Log.Information("=== BYWGLib 基本使用示例 ===");

            // 创建配置
            var config = new IndustrialProtocolConfig
            {
                Name = "BasicExample",
                Type = "ModbusTCP",
                Parameters = new Dictionary<string, string>
                {
                    { "IpAddress", "192.168.1.100" },
                    { "Port", "502" },
                    { "Timeout", "5000" },
                    { "UnitId", "1" }
                }
            };

            using var protocol = new AsyncModbusTcpProtocol(config);
            
            Log.Information($"正在连接设备 {config.Parameters["IpAddress"]}:{config.Parameters["Port"]}...");

            try
            {
                // 读取单个地址
                var value = await protocol.ReadAsync("40001", "signed");
                Log.Information($"读取地址 40001: {value}");
            }
            catch (Exception ex)
            {
                Log.Error($"连接失败: {ex.Message}");
                Log.Information("请检查设备连接和配置");
            }
        }
    }
}