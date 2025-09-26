using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using BYWGLib;
using BYWGLib.Protocols;

namespace BYWG.IntegrationTest.Benchmarks
{
    /// <summary>
    /// 协议性能基准测试
    /// </summary>
    [MemoryDiagnoser]
    [SimpleJob(BenchmarkDotNet.Jobs.RuntimeMoniker.Net80)]
    public class ProtocolBenchmarks
    {
        private AsyncModbusTcpProtocol _modbusProtocol;
        private S7Protocol _s7Protocol;
        private AsyncMitsubishiMCProtocol _mcProtocol;
        private List<IndustrialDataItem> _testData;

        [GlobalSetup]
        public void Setup()
        {
            // 创建配置
            var modbusConfig = new IndustrialProtocolConfig
            {
                Name = "ModbusTest",
                Type = "Modbus",
                ConnectionString = "Host=127.0.0.1;Port=502;UnitId=1",
                Parameters = new Dictionary<string, string>
                {
                    { "PollingInterval", "1000" },
                    { "DataPoints", "40001:int16,40002:int32" }
                }
            };
            
            var s7Config = new IndustrialProtocolConfig
            {
                Name = "S7Test",
                Type = "S7",
                ConnectionString = "Host=127.0.0.1;Port=102;Rack=0;Slot=1",
                Parameters = new Dictionary<string, string>
                {
                    { "PollingInterval", "1000" },
                    { "DataPoints", "DB1.DBW0:int16,DB1.DBD2:int32" }
                }
            };
            
            var mcConfig = new IndustrialProtocolConfig
            {
                Name = "MCTest",
                Type = "MC",
                ConnectionString = "Host=127.0.0.1;Port=5000;ProtocolType=Qna3E",
                Parameters = new Dictionary<string, string>
                {
                    { "PollingInterval", "1000" },
                    { "DataPoints", "D100:int16,D102:int32" }
                }
            };
            
            // 初始化协议
            _modbusProtocol = new AsyncModbusTcpProtocol(modbusConfig);
            _s7Protocol = new S7Protocol(s7Config);
            _mcProtocol = new AsyncMitsubishiMCProtocol(mcConfig);

            // 创建测试数据
            _testData = new List<IndustrialDataItem>();
            for (int i = 0; i < 1000; i++)
            {
                _testData.Add(new IndustrialDataItem
                {
                    Id = $"test_{i}",
                    Name = $"Test_{i}",
                    Value = i,
                    DataType = "int",
                    Quality = Quality.Good,
                    Timestamp = DateTime.Now
                });
            }
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            _modbusProtocol?.Dispose();
            _s7Protocol?.Dispose();
            _mcProtocol?.Dispose();
        }

        /// <summary>
        /// 测试Modbus协议读取性能
        /// </summary>
        [Benchmark]
        [Arguments("40001", "int16")]
        [Arguments("40002", "int32")]
        [Arguments("40003", "float")]
        public async Task ModbusReadPerformance(string address, string dataType)
        {
            await _modbusProtocol.PollDataAsync();
        }

        /// <summary>
        /// 测试S7协议读取性能
        /// </summary>
        [Benchmark]
        [Arguments("DB1.DBW0", "int16")]
        [Arguments("DB1.DBD0", "int32")]
        [Arguments("DB1.DBD4", "float")]
        public async Task S7ReadPerformance(string address, string dataType)
        {
            await _s7Protocol.ReadDataPointAsync(address, dataType);
        }

        /// <summary>
        /// 测试MC协议读取性能
        /// </summary>
        [Benchmark]
        [Arguments("D100", "int16")]
        [Arguments("D101", "int32")]
        [Arguments("D102", "float")]
        public async Task MCReadPerformance(string address, string dataType)
        {
            await _mcProtocol.PollDataAsync();
        }

        /// <summary>
        /// 测试数据项创建性能
        /// </summary>
        [Benchmark]
        public IndustrialDataItem DataItemCreation()
        {
            return new IndustrialDataItem
            {
                Id = "test",
                Name = "Test",
                Value = 100,
                DataType = "int",
                Quality = Quality.Good,
                Timestamp = DateTime.Now
            };
        }

        /// <summary>
        /// 测试数据项列表操作性能
        /// </summary>
        [Benchmark]
        public List<IndustrialDataItem> DataItemListOperations()
        {
            var items = new List<IndustrialDataItem>();
            for (int i = 0; i < 100; i++)
            {
                items.Add(new IndustrialDataItem
                {
                    Id = $"test_{i}",
                    Name = $"Test_{i}",
                    Value = i,
                    DataType = "int",
                    Quality = Quality.Good,
                    Timestamp = DateTime.Now
                });
            }
            return items;
        }

        /// <summary>
        /// 测试内存分配性能
        /// </summary>
        [Benchmark]
        public void MemoryAllocationTest()
        {
            var data = new byte[1024];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = (byte)(i % 256);
            }
        }

        /// <summary>
        /// 测试字符串操作性能
        /// </summary>
        [Benchmark]
        [Arguments("40001", "int16")]
        [Arguments("DB1.DBW0", "int16")]
        [Arguments("D100", "int16")]
        public string StringOperations(string address, string dataType)
        {
            return $"{address}:{dataType}:{DateTime.Now.Ticks}";
        }
    }

    /// <summary>
    /// 网络性能基准测试
    /// </summary>
    [MemoryDiagnoser]
    [SimpleJob(BenchmarkDotNet.Jobs.RuntimeMoniker.Net80)]
    public class NetworkBenchmarks
    {
        private byte[] _testBuffer;

        [GlobalSetup]
        public void Setup()
        {
            _testBuffer = new byte[1024];
            for (int i = 0; i < _testBuffer.Length; i++)
            {
                _testBuffer[i] = (byte)(i % 256);
            }
        }

        /// <summary>
        /// 测试字节数组复制性能
        /// </summary>
        [Benchmark]
        public byte[] ByteArrayCopy()
        {
            var result = new byte[_testBuffer.Length];
            Array.Copy(_testBuffer, result, _testBuffer.Length);
            return result;
        }

        /// <summary>
        /// 测试字节数组处理性能
        /// </summary>
        [Benchmark]
        public int ByteArrayProcessing()
        {
            int sum = 0;
            for (int i = 0; i < _testBuffer.Length; i++)
            {
                sum += _testBuffer[i];
            }
            return sum;
        }

        /// <summary>
        /// 测试Span操作性能
        /// </summary>
        [Benchmark]
        public int SpanOperations()
        {
            var span = _testBuffer.AsSpan();
            int sum = 0;
            foreach (var b in span)
            {
                sum += b;
            }
            return sum;
        }
    }

    /// <summary>
    /// 异步操作性能基准测试
    /// </summary>
    [MemoryDiagnoser]
    [SimpleJob(BenchmarkDotNet.Jobs.RuntimeMoniker.Net80)]
    public class AsyncBenchmarks
    {
        /// <summary>
        /// 测试异步任务创建性能
        /// </summary>
        [Benchmark]
        public async Task AsyncTaskCreation()
        {
            await Task.Delay(1);
        }

        /// <summary>
        /// 测试异步数据获取性能
        /// </summary>
        [Benchmark]
        public async Task<IndustrialDataItem> AsyncDataRetrieval()
        {
            await Task.Delay(1);
            return new IndustrialDataItem
            {
                Id = "test",
                Name = "Test",
                Value = 100,
                DataType = "int",
                Quality = Quality.Good,
                Timestamp = DateTime.Now
            };
        }

        /// <summary>
        /// 测试并发异步操作性能
        /// </summary>
        [Benchmark]
        public async Task ConcurrentAsyncOperations()
        {
            var tasks = new List<Task>();
            for (int i = 0; i < 10; i++)
            {
                tasks.Add(Task.Delay(1));
            }
            await Task.WhenAll(tasks);
        }
    }
}
