using BYWG.Contracts;
using BYWGLib;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using BYWG.Contracts;
using ProtocolService = BYWG.Contracts.ProtocolService;

namespace BYWG.Server.Core
{
    /// <summary>
    /// 工业协议管理服务实现
    /// </summary>
    public class ProtocolManagerService : ProtocolService.ProtocolServiceBase
    {
        private readonly ILogger<ProtocolManagerService> _logger;
        private readonly ProtocolManager _protocolManager;
        private readonly Dictionary<string, IndustrialProtocolConfig> _protocols = new(StringComparer.OrdinalIgnoreCase);
        private readonly object _lockObject = new();

        public ProtocolManagerService(ILogger<ProtocolManagerService> logger)
        {
            _logger = logger;
            _protocolManager = new ProtocolManager();
            _protocolManager.DataChanged += OnDataChanged;
        }

        /// <summary>
        /// 添加协议
        /// </summary>
        public override Task<BYWG.Contracts.AddProtocolResponse> AddProtocol(BYWG.Contracts.AddProtocolRequest request, ServerCallContext context)
        {
            try
            {
                lock (_lockObject)
                {
                    var name = (request.Name ?? string.Empty).Trim();
                    if (_protocols.ContainsKey(name))
                    {
                        return Task.FromResult(new BYWG.Contracts.AddProtocolResponse
                        {
                            BaseResponse = new Contracts.BaseResponse
                            {
                                StatusCode = Contracts.StatusCode.AlreadyExists,
                                Message = "协议已存在"
                            }
                        });
                    }

                    // 协议类型枚举映射为工厂可识别字符串
                    var normalizedType = MapProtocolType(request.Type);
                    if (string.IsNullOrEmpty(normalizedType))
                    {
                        _logger.LogWarning("添加协议失败: 不支持的协议类型 {Type} (name={Name})", request.Type, name);
                        return Task.FromResult(new BYWG.Contracts.AddProtocolResponse
                        {
                            BaseResponse = new Contracts.BaseResponse
                            {
                                StatusCode = Contracts.StatusCode.Error,
                                Message = "不支持的协议类型"
                            }
                        });
                    }

                    // 创建协议配置
                    var config = new IndustrialProtocolConfig
                    {
                        Name = name,
                        Type = normalizedType,
                        ConnectionString = request.ConnectionString,
                        Parameters = request.Parameters.ToDictionary(kv => kv.Key, kv => kv.Value)
                    };

                    // 添加到协议管理器
                    _protocolManager.AddProtocol(config);
                    _protocols[name] = config;

                    _logger.LogInformation("已添加协议: {0} (Type={Type})", name, normalizedType);

                    return Task.FromResult(new BYWG.Contracts.AddProtocolResponse
                    {
                        BaseResponse = new Contracts.BaseResponse
                        {
                            StatusCode = Contracts.StatusCode.Ok,
                            Message = "添加成功"
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "添加协议失败: {0}", request.Name);
                return Task.FromResult(new BYWG.Contracts.AddProtocolResponse
                {
                    BaseResponse = new Contracts.BaseResponse
                    {
                        StatusCode = Contracts.StatusCode.Error,
                        Message = ex.Message
                    }
                });
            }
        }

        /// <summary>
        /// 删除协议
        /// </summary>
        public override Task<BYWG.Contracts.RemoveProtocolResponse> RemoveProtocol(BYWG.Contracts.RemoveProtocolRequest request, ServerCallContext context)
        {
            try
            {
                lock (_lockObject)
                {
                    var name = (request.Name ?? string.Empty).Trim();
                    if (!_protocols.ContainsKey(name))
                    {
                        return Task.FromResult(new BYWG.Contracts.RemoveProtocolResponse
                        {
                            BaseResponse = new Contracts.BaseResponse
                            {
                                StatusCode = Contracts.StatusCode.NotFound,
                                Message = "协议不存在"
                            }
                        });
                    }

                    // 停止协议
                    if (_protocols[name].Enabled)
                    {
                        _protocolManager.StopProtocol(name);
                    }

                    // 移除协议
                    _protocolManager.RemoveProtocol(name);
                    _protocols.Remove(name);

                    _logger.LogInformation("已删除协议: {0}", name);

                    return Task.FromResult(new BYWG.Contracts.RemoveProtocolResponse
                    {
                        BaseResponse = new Contracts.BaseResponse
                        {
                            StatusCode = Contracts.StatusCode.Ok,
                            Message = "删除成功"
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除协议失败: {0}", request.Name);
                return Task.FromResult(new BYWG.Contracts.RemoveProtocolResponse
                {
                    BaseResponse = new Contracts.BaseResponse
                    {
                        StatusCode = Contracts.StatusCode.Error,
                        Message = ex.Message
                    }
                });
            }
        }

        /// <summary>
        /// 启动协议
        /// </summary>
        public override Task<BYWG.Contracts.StartProtocolResponse> StartProtocol(BYWG.Contracts.StartProtocolRequest request, ServerCallContext context)
        {
            try
            {
                lock (_lockObject)
                {
                    var name = (request.Name ?? string.Empty).Trim();
                    if (!_protocols.ContainsKey(name))
                    {
                        return Task.FromResult(new BYWG.Contracts.StartProtocolResponse
                        {
                            BaseResponse = new Contracts.BaseResponse
                            {
                                StatusCode = Contracts.StatusCode.NotFound,
                                Message = "协议不存在"
                            }
                        });
                    }

                    if (_protocols[name].Enabled)
                    {
                        return Task.FromResult(new BYWG.Contracts.StartProtocolResponse
                        {
                            BaseResponse = new Contracts.BaseResponse
                            {
                                StatusCode = Contracts.StatusCode.Ok,
                                Message = "协议已经在运行中"
                            }
                        });
                    }

                    // 启动协议
                    _protocolManager.StartProtocol(name);
                    _protocols[name].Enabled = true;

                    _logger.LogInformation("已启动协议: {0}", name);

                    return Task.FromResult(new BYWG.Contracts.StartProtocolResponse
                    {
                        BaseResponse = new Contracts.BaseResponse
                        {
                            StatusCode = Contracts.StatusCode.Ok,
                            Message = "启动成功"
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "启动协议失败: {0}", request.Name);
                return Task.FromResult(new BYWG.Contracts.StartProtocolResponse
                {
                    BaseResponse = new Contracts.BaseResponse
                    {
                        StatusCode = Contracts.StatusCode.Error,
                        Message = ex.Message
                    }
                });
            }
        }

        /// <summary>
        /// 停止协议
        /// </summary>
        public override Task<BYWG.Contracts.StopProtocolResponse> StopProtocol(BYWG.Contracts.StopProtocolRequest request, ServerCallContext context)
        {
            try
            {
                lock (_lockObject)
                {
                    var name = (request.Name ?? string.Empty).Trim();
                    if (!_protocols.ContainsKey(name))
                    {
                        return Task.FromResult(new BYWG.Contracts.StopProtocolResponse
                        {
                            BaseResponse = new Contracts.BaseResponse
                            {
                                StatusCode = Contracts.StatusCode.NotFound,
                                Message = "协议不存在"
                            }
                        });
                    }

                    if (!_protocols[name].Enabled)
                    {
                        return Task.FromResult(new BYWG.Contracts.StopProtocolResponse
                        {
                            BaseResponse = new Contracts.BaseResponse
                            {
                                StatusCode = Contracts.StatusCode.Ok,
                                Message = "协议已经停止"
                            }
                        });
                    }

                    // 停止协议
                    _protocolManager.StopProtocol(name);
                    _protocols[name].Enabled = false;

                    _logger.LogInformation("已停止协议: {0}", name);

                    return Task.FromResult(new BYWG.Contracts.StopProtocolResponse
                    {
                        BaseResponse = new Contracts.BaseResponse
                        {
                            StatusCode = Contracts.StatusCode.Ok,
                            Message = "停止成功"
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "停止协议失败: {0}", request.Name);
                return Task.FromResult(new BYWG.Contracts.StopProtocolResponse
                {
                    BaseResponse = new Contracts.BaseResponse
                    {
                        StatusCode = Contracts.StatusCode.Error,
                        Message = ex.Message
                    }
                });
            }
        }

        /// <summary>
        /// 获取协议列表
        /// </summary>
        public override Task<BYWG.Contracts.GetProtocolsResponse> GetProtocols(BYWG.Contracts.GetProtocolsRequest request, ServerCallContext context)
        {
            try
            {
                lock (_lockObject)
                {
                    var response = new BYWG.Contracts.GetProtocolsResponse
                    {
                        BaseResponse = new Contracts.BaseResponse
                        {
                            StatusCode = Contracts.StatusCode.Ok,
                            Message = "获取成功"
                        }
                    };

                    // 添加协议信息
                    foreach (var protocol in _protocols)
                    {
                        var protocolType = MapNormalizedTypeToEnum(protocol.Value.Type);
                        response.Protocols.Add(new BYWG.Contracts.ProtocolInfo
                        {
                            Name = protocol.Key,
                            Type = protocolType,
                            IsRunning = protocol.Value.Enabled,
                            ConnectionString = protocol.Value.ConnectionString,
                            Parameters = { protocol.Value.Parameters }
                        });
                    }

                    return Task.FromResult(response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取协议列表失败");
                return Task.FromResult(new BYWG.Contracts.GetProtocolsResponse
                {
                    BaseResponse = new Contracts.BaseResponse
                    {
                        StatusCode = Contracts.StatusCode.Error,
                        Message = ex.Message
                    }
                });
            }
        }

        /// <summary>
        /// 测试读取（根据设备/协议名称 + 地址 + 数据类型读取一次）
        /// </summary>
        public override Task<BYWG.Contracts.TestReadResponse> TestRead(BYWG.Contracts.TestReadRequest request, ServerCallContext context)
        {
            try
            {
                lock (_lockObject)
                {
                    var name = (request.Name ?? string.Empty).Trim();
                    if (!_protocols.ContainsKey(name))
                    {
                        return Task.FromResult(new BYWG.Contracts.TestReadResponse
                        {
                            BaseResponse = new BYWG.Contracts.BaseResponse
                            {
                                StatusCode = BYWG.Contracts.StatusCode.NotFound,
                                Message = "协议不存在"
                            }
                        });
                    }

                    // 通过BYWGLib.ProtocolManager 反射获取具体协议，然后调用 Read(address, dataType)
                    // 优先使用 TryGetProtocol 获取实例
                    var tryGet = typeof(ProtocolManager).GetMethod("TryGetProtocol", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    if (tryGet == null)
                    {
                        return Task.FromResult(new BYWG.Contracts.TestReadResponse
                        {
                            BaseResponse = new BYWG.Contracts.BaseResponse
                            {
                                StatusCode = BYWG.Contracts.StatusCode.Error,
                                Message = "服务端协议管理器不支持获取协议实例"
                            }
                        });
                    }
                    object[] args = new object[] { name, null };
                    var ok = (bool)tryGet.Invoke(_protocolManager, args);
                    if (!ok || args[1] == null)
                    {
                        return Task.FromResult(new BYWG.Contracts.TestReadResponse
                        {
                            BaseResponse = new BYWG.Contracts.BaseResponse
                            {
                                StatusCode = BYWG.Contracts.StatusCode.NotFound,
                                Message = "未找到对应协议实例"
                            }
                        });
                    }
                    var proto = args[1];
                    object value = null;
                    var readMethod = proto.GetType().GetMethod("Read", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance, null, new System.Type[] { typeof(string), typeof(string) }, null);
                    if (readMethod != null)
                    {
                        try
                        {
                            value = readMethod.Invoke(proto, new object[] { request.Address ?? string.Empty, request.DataType ?? string.Empty });
                        }
                        catch (System.Reflection.TargetInvocationException tie) when (tie.InnerException is NotSupportedException)
                        {
                            // 同步Read不受支持，改走异步
                            readMethod = null;
                        }
                    }
                    if (readMethod == null)
                    {
                    // 尝试异步方法: ReadDataPointAsync(address, dataType) 或 ReadAsync(address, dataType)
                    var asyncMethod = proto.GetType().GetMethod("ReadDataPointAsync", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance, null, new System.Type[] { typeof(string), typeof(string) }, null)
                                     ?? proto.GetType().GetMethod("ReadAsync", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance, null, new System.Type[] { typeof(string), typeof(string) }, null);
                        if (asyncMethod == null)
                        {
                            return Task.FromResult(new BYWG.Contracts.TestReadResponse
                            {
                                BaseResponse = new BYWG.Contracts.BaseResponse
                                {
                                    StatusCode = BYWG.Contracts.StatusCode.Error,
                                    Message = "该协议不支持读取(无 Read/ReadAsync/ReadDataPointAsync)"
                                }
                            });
                        }
                        var taskObj = asyncMethod.Invoke(proto, new object[] { request.Address ?? string.Empty, request.DataType ?? string.Empty });
                        if (taskObj is System.Threading.Tasks.Task task)
                        {
                            task.GetAwaiter().GetResult();
                            var resultProp = task.GetType().GetProperty("Result");
                            value = resultProp?.GetValue(task);
                        }
                    }

                    // 映射为 Contracts.DataValue
                    var dv = new BYWG.Contracts.DataValue
                    {
                        Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                        Quality = BYWG.Contracts.DataQuality.Good
                    };
                    if (value is bool b) dv.BooleanValue = b;
                    else if (value is short s) dv.Int32Value = s;
                    else if (value is int i) dv.Int32Value = i;
                    else if (value is float f) dv.FloatValue = f;
                    else if (value is double d) dv.DoubleValue = d;
                    else if (value is string str) dv.StringValue = str;
                    else dv.StringValue = value?.ToString() ?? string.Empty;

                    return Task.FromResult(new BYWG.Contracts.TestReadResponse
                    {
                        BaseResponse = new BYWG.Contracts.BaseResponse
                        {
                            StatusCode = BYWG.Contracts.StatusCode.Ok,
                            Message = "读取成功"
                        },
                        DataValue = dv
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "测试读取失败: name={Name}, addr={Addr}", request.Name, request.Address);
                return Task.FromResult(new BYWG.Contracts.TestReadResponse
                {
                    BaseResponse = new BYWG.Contracts.BaseResponse
                    {
                        StatusCode = BYWG.Contracts.StatusCode.Error,
                        Message = ex.Message
                    }
                });
            }
        }

        /// <summary>
        /// 处理数据变化事件
        /// </summary>
        private void OnDataChanged(object sender, BYWGLib.DataChangedEventArgs e)
        {
            try
            {
                foreach (var item in e.ChangedItems)
                {
                    _logger.LogDebug("数据变化: 名称={0}, 值={1}", item.Name, item.Value);
                }
                // 这里可以实现数据变化的处理逻辑，如更新OPC UA节点值等
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理数据变化事件失败");
            }
        }
        private static string MapProtocolType(ProtocolType type)
        {
            return type switch
            {
                ProtocolType.Modbus => "MODBUS_TCP",
                ProtocolType.Siemens => "S7",
                ProtocolType.Mitsubishi => "MC",
                _ => string.Empty
            };
        }
        
        private static ProtocolType MapNormalizedTypeToEnum(string normalizedType)
        {
            switch ((normalizedType ?? string.Empty).Trim().ToUpperInvariant())
            {
                case "MODBUS_TCP":
                    return ProtocolType.Modbus;
                case "S7":
                    return ProtocolType.Siemens;
                case "MC":
                    return ProtocolType.Mitsubishi;
                case "OMRON":
                    return ProtocolType.Omron;
                case "AB":
                    return ProtocolType.Ab;
                default:
                    return ProtocolType.Unspecified;
            }
        }
    }
}