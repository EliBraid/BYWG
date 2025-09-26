using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using BYWGLib.Logging;

namespace BYWGLibDemo
{
    public class SimpleDiagnostic
    {
        public static async Task RunDiagnostic()
        {
            Log.Information("=== 简单设备诊断 ===");
            
            const string host = "192.168.6.6";
            const int port = 502;
            
            try
            {
                // 1. 测试基本TCP连接
                Log.Information($"测试TCP连接到 {host}:{port}...");
                using var client = new TcpClient();
                await client.ConnectAsync(host, port);
                Log.Information("✅ TCP连接成功");
                
                // 2. 发送简单的Modbus请求
                Log.Information("发送简单的Modbus请求...");
                var stream = client.GetStream();
                
                // 构建一个简单的Modbus TCP请求 (读取Holding Register 40001)
                var request = new byte[]
                {
                    0x00, 0x01, // Transaction ID
                    0x00, 0x00, // Protocol ID
                    0x00, 0x06, // Length
                    0x01,       // Unit ID
                    0x03,       // Function Code (Read Holding Registers)
                    0x00, 0x00, // Starting Address (0)
                    0x00, 0x01  // Quantity (1)
                };
                
                Log.Information($"发送请求: {BitConverter.ToString(request)}");
                await stream.WriteAsync(request, 0, request.Length);
                
                // 读取响应
                var responseBuffer = new byte[256];
                var bytesRead = await stream.ReadAsync(responseBuffer, 0, responseBuffer.Length);
                
                Log.Information($"接收到响应: {bytesRead} 字节");
                Log.Information($"响应数据: {BitConverter.ToString(responseBuffer, 0, bytesRead)}");
                
                if (bytesRead >= 6)
                {
                    var transactionId = (responseBuffer[0] << 8) | responseBuffer[1];
                    var protocolId = (responseBuffer[2] << 8) | responseBuffer[3];
                    var length = (responseBuffer[4] << 8) | responseBuffer[5];
                    
                    Log.Information($"事务ID: {transactionId}");
                    Log.Information($"协议ID: {protocolId}");
                    Log.Information($"长度: {length}");
                    
                    if (length > 0 && length < 255)
                    {
                        Log.Information("✅ 设备响应正常");
                        
                        if (bytesRead >= 9)
                        {
                            var functionCode = responseBuffer[7];
                            Log.Information($"功能码: {functionCode:X2}");
                            
                            if ((functionCode & 0x80) != 0)
                            {
                                var errorCode = responseBuffer[8];
                                Log.Warning($"设备返回错误: {GetModbusErrorMessage(errorCode)}");
                            }
                            else
                            {
                                Log.Information("✅ 设备响应成功");
                            }
                        }
                    }
                    else
                    {
                        Log.Warning($"❌ 响应长度异常: {length}");
                    }
                }
                else
                {
                    Log.Warning($"❌ 响应太短: {bytesRead} 字节");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "诊断过程中发生错误: {0}", ex.Message);
            }
        }
        
        private static string GetModbusErrorMessage(byte errorCode)
        {
            return errorCode switch
            {
                0x01 => "非法功能码",
                0x02 => "非法数据地址",
                0x03 => "非法数据值",
                0x04 => "从站设备故障",
                0x05 => "确认",
                0x06 => "从站设备忙",
                0x08 => "存储奇偶性差错",
                0x0A => "不可用网关路径",
                0x0B => "网关目标设备响应失败",
                _ => $"未知错误码: {errorCode:X2}"
            };
        }
    }
}
