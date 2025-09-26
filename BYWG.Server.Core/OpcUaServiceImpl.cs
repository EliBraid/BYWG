using System;
using System.Reflection;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using BYWG.Server.Core.OpcUa;
using OpcUaService = BYWG.Contracts.OpcUaService;
using Contracts = BYWG.Contracts;
using OpcUaStatusCode = BYWG.Server.Core.OpcUa.StatusCode;

namespace BYWG.Server.Core
{
    /// <summary>
    /// OPC UA服务实现
    /// </summary>
    public class OpcUaServiceImpl : OpcUaService.OpcUaServiceBase
    {
        private readonly ILogger<OpcUaServiceImpl> _logger;
        private SimpleOpcUaServer _opcServer;
        private readonly object _lockObject = new();

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="logger"></param>
        public OpcUaServiceImpl(ILogger<OpcUaServiceImpl> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 启动OPC UA服务器
        /// </summary>
        public override async Task<Contracts.StartOpcUaServerResponse> StartOpcUaServer(Contracts.StartOpcUaServerRequest request, ServerCallContext context)
        {
            try
            {
                lock (_lockObject)
                {
                    if (_opcServer != null && _opcServer.IsRunning)
                    {
                        _logger.LogInformation("OPC UA服务器已经在运行中");
                        return new Contracts.StartOpcUaServerResponse
                        {
                            BaseResponse = new Contracts.BaseResponse
                            {
                                StatusCode = Contracts.StatusCode.Ok,
                                Message = "OPC UA服务器已经在运行中"
                            }
                        };
                    }

                    // 创建并启动OPC UA服务器
                    _opcServer = new SimpleOpcUaServer();
                    _opcServer.NodeValueChanged += OnNodeValueChanged;
                }

                await _opcServer.StartAsync();
                _logger.LogInformation("OPC UA服务器启动成功");

                return new Contracts.StartOpcUaServerResponse
                {
                    BaseResponse = new Contracts.BaseResponse
                    {
                        StatusCode = Contracts.StatusCode.Ok,
                        Message = "服务器已启动"
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "启动OPC UA服务器失败");
                return new Contracts.StartOpcUaServerResponse
                {
                    BaseResponse = new Contracts.BaseResponse
                    {
                        StatusCode = Contracts.StatusCode.Error,
                        Message = ex.Message
                    }
                };
            }
        }

        /// <summary>
        /// 停止OPC UA服务器
        /// </summary>
        public override Task<Contracts.StopOpcUaServerResponse> StopOpcUaServer(Contracts.StopOpcUaServerRequest request, ServerCallContext context)
        {
            try
            {
                lock (_lockObject)
                {
                    if (_opcServer == null || !_opcServer.IsRunning)
                    {
                        return Task.FromResult(new Contracts.StopOpcUaServerResponse
                        {
                            BaseResponse = new Contracts.BaseResponse
                            {
                                StatusCode = Contracts.StatusCode.Ok,
                                Message = "OPC UA服务器已经停止"
                            }
                        });
                    }

                    // 停止OPC UA服务器
                    _opcServer.Stop();
                    _opcServer.NodeValueChanged -= OnNodeValueChanged;
                    _opcServer = null;

                    _logger.LogInformation("OPC UA服务器已停止");

                    return Task.FromResult(new Contracts.StopOpcUaServerResponse
                    {
                        BaseResponse = new Contracts.BaseResponse
                        {
                            StatusCode = Contracts.StatusCode.Ok,
                            Message = "OPC UA服务器停止成功"
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "停止OPC UA服务器失败");
                return Task.FromResult(new Contracts.StopOpcUaServerResponse
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
        /// 获取服务器状态
        /// </summary>
        public override Task<Contracts.GetServerStatusResponse> GetServerStatus(Contracts.GetServerStatusRequest request, ServerCallContext context)
        {
            try
            {
                lock (_lockObject)
                {
                    var response = new Contracts.GetServerStatusResponse
                    {
                        BaseResponse = new Contracts.BaseResponse
                        {
                            StatusCode = Contracts.StatusCode.Ok,
                            Message = "获取成功"
                        },
                        IsRunning = _opcServer != null && _opcServer.IsRunning
                    };

                    if (_opcServer != null && _opcServer.IsRunning)
                    {
                        response.SessionCount = _opcServer.GetSessionCount();
                        // 注意：这里假设SimpleOpcUaServer有一个方法获取节点数量
                        // 如果没有，可能需要修改这个值或添加这个方法
                        // response.NodeCount = _opcServer.GetNodeCount();
                    }

                    return Task.FromResult(response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取服务器状态失败");
                return Task.FromResult(new Contracts.GetServerStatusResponse
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
        /// 添加节点
        /// </summary>
        public override Task<Contracts.AddNodeResponse> AddNode(Contracts.AddNodeRequest request, ServerCallContext context)
        {
            try
            {
                lock (_lockObject)
                {
                    if (_opcServer == null || !_opcServer.IsRunning)
                    {
                        return Task.FromResult(new Contracts.AddNodeResponse
                        {
                            BaseResponse = new Contracts.BaseResponse
                            {
                                StatusCode = Contracts.StatusCode.Error,
                                Message = "OPC UA服务器未运行"
                            }
                        });
                    }

                    // 创建节点ID
                    var nodeId = new BYWG.Server.Core.OpcUa.NodeId(request.NodeId, (ushort)1);

                    // 创建数据变量节点
                    var dataNode = new DataVariableNode(nodeId)
                    {
                        BrowseName = new QualifiedName(request.NodeId, 1),
                        DisplayName = new LocalizedText(request.DisplayName)
                    };

                    // 设置初始值
                    if (request.InitialValue != null)
                    {
                        dataNode.Value = ConvertToDataValue(request.InitialValue);
                    }

                    // 添加到服务器
                    _opcServer.AddNode(dataNode);

                    _logger.LogInformation("已添加节点: {0}", request.NodeId);

                    return Task.FromResult(new Contracts.AddNodeResponse
                    {
                        BaseResponse = new Contracts.BaseResponse
                        {
                            StatusCode = Contracts.StatusCode.Ok,
                            Message = "添加节点成功"
                        },
                        NodeId = new Contracts.NodeId
                        {
                            Identifier = request.NodeId,
                            NamespaceIndex = 1u
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "添加节点失败: {0}", request.NodeId);
                return Task.FromResult(new Contracts.AddNodeResponse
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
        /// 删除节点
        /// </summary>
        public override Task<Contracts.RemoveNodeResponse> RemoveNode(Contracts.RemoveNodeRequest request, ServerCallContext context)
        {
            try
            {
                lock (_lockObject)
                {
                    if (_opcServer == null || !_opcServer.IsRunning)
                    {
                        return Task.FromResult(new Contracts.RemoveNodeResponse
                        {
                            BaseResponse = new Contracts.BaseResponse
                            {
                                StatusCode = Contracts.StatusCode.Error,
                                Message = "OPC UA服务器未运行"
                            }
                        });
                    }

                    // 创建节点ID
                    var nodeId = new BYWG.Server.Core.OpcUa.NodeId(request.NodeId.Identifier, (ushort)request.NodeId.NamespaceIndex);

                    // 从服务器删除节点
                    _opcServer.RemoveNode(nodeId);

                    _logger.LogInformation("已删除节点: {0}", request.NodeId.Identifier);

                    return Task.FromResult(new Contracts.RemoveNodeResponse
                    {
                        BaseResponse = new Contracts.BaseResponse
                        {
                            StatusCode = Contracts.StatusCode.Ok,
                            Message = "删除节点成功"
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除节点失败: {0}", request.NodeId.Identifier);
                return Task.FromResult(new Contracts.RemoveNodeResponse
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
        /// 读取节点值
        /// </summary>
        public override Task<Contracts.ReadNodeResponse> ReadNode(Contracts.ReadNodeRequest request, ServerCallContext context)
        {
            try
            {
                lock (_lockObject)
                {
                    if (_opcServer == null || !_opcServer.IsRunning)
                    {
                        return Task.FromResult(new Contracts.ReadNodeResponse
                        {
                            BaseResponse = new Contracts.BaseResponse
                            {
                                StatusCode = Contracts.StatusCode.Error,
                                Message = "OPC UA服务器未运行"
                            }
                        });
                    }

                    // 创建节点ID
                    var nodeId = new BYWG.Server.Core.OpcUa.NodeId(request.NodeId.Identifier, (ushort)request.NodeId.NamespaceIndex);

                    // 创建读取请求
                    var readRequest = new OpcUaRequest
                    {
                        Type = OpcUaRequestType.Read,
                        NodeId = nodeId,
                        AttributeId = AttributeId.Value
                    };

                    // 处理读取请求
                    var response = ProcessReadRequest(readRequest);

                    if (response.StatusCode.IsGood)
                    {
                        _logger.LogDebug("读取节点成功: {0}", request.NodeId.Identifier);
                        return Task.FromResult(new Contracts.ReadNodeResponse
                        {
                            BaseResponse = new Contracts.BaseResponse
                            {
                                StatusCode = Contracts.StatusCode.Ok,
                                Message = "读取成功"
                            },
                            DataValue = ConvertFromDataValue(response.DataValue)
                        });
                    }
                    else
                    {
                        _logger.LogWarning("读取节点失败: {0}, 状态码: {1}", request.NodeId.Identifier, response.StatusCode);
                        return Task.FromResult(new Contracts.ReadNodeResponse
                        {
                            BaseResponse = new Contracts.BaseResponse
                            {
                                StatusCode = Contracts.StatusCode.NotFound,
                                Message = "节点不存在或读取失败"
                            }
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "读取节点失败: {0}", request.NodeId.Identifier);
                return Task.FromResult(new Contracts.ReadNodeResponse
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
        /// 写入节点值
        /// </summary>
        public override Task<Contracts.WriteNodeResponse> WriteNode(Contracts.WriteNodeRequest request, ServerCallContext context)
        {
            try
            {
                lock (_lockObject)
                {
                    if (_opcServer == null || !_opcServer.IsRunning)
                    {
                        return Task.FromResult(new Contracts.WriteNodeResponse
                        {
                            BaseResponse = new Contracts.BaseResponse
                            {
                                StatusCode = Contracts.StatusCode.Error,
                                Message = "OPC UA服务器未运行"
                            }
                        });
                    }

                    // 创建节点ID
                    var nodeId = new BYWG.Server.Core.OpcUa.NodeId(request.NodeId.Identifier, (ushort)request.NodeId.NamespaceIndex);

                    // 转换数据值
                    var dataValue = ConvertToDataValue(request.DataValue);

                    // 更新节点值
                    _opcServer.UpdateNodeValue(nodeId, dataValue.Value);

                    _logger.LogInformation("写入节点成功: {0}", request.NodeId.Identifier);

                    return Task.FromResult(new Contracts.WriteNodeResponse
                    {
                        BaseResponse = new Contracts.BaseResponse
                        {
                            StatusCode = Contracts.StatusCode.Ok,
                            Message = "写入成功"
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "写入节点失败: {0}", request.NodeId.Identifier);
                return Task.FromResult(new Contracts.WriteNodeResponse
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
        /// 获取节点列表
        /// </summary>
        public override Task<Contracts.GetNodesResponse> GetNodes(Contracts.GetNodesRequest request, ServerCallContext context)
        {
            try
            {
                lock (_lockObject)
                {
                    if (_opcServer == null || !_opcServer.IsRunning)
                    {
                        return Task.FromResult(new Contracts.GetNodesResponse
                        {
                            BaseResponse = new Contracts.BaseResponse
                            {
                                StatusCode = Contracts.StatusCode.Error,
                                Message = "OPC UA服务器未运行"
                            }
                        });
                    }
                    
                    var response = new Contracts.GetNodesResponse
                    {
                        BaseResponse = new Contracts.BaseResponse
                        {
                            StatusCode = Contracts.StatusCode.Ok,
                            Message = "获取成功"
                        }
                    };

                // 从服务器读取真实的数据变量节点
                var dataNodes = _opcServer.GetDataVariableNodes();
                foreach (var dn in dataNodes)
                {
                    var nodeInfo = new Contracts.NodeInfo
                    {
                        NodeId = new Contracts.NodeId
                        {
                            Identifier = dn.NodeId.Value?.ToString() ?? string.Empty,
                            NamespaceIndex = dn.NodeId.NamespaceIndex
                        },
                        DisplayName = dn.DisplayName?.Text ?? dn.BrowseName?.Name ?? string.Empty,
                        DataType = MapToContractsDataType(dn.Value?.Value?.Value),
                        CurrentValue = ConvertFromDataValue(dn.Value)
                    };
                    response.Nodes.Add(nodeInfo);
                }

                return Task.FromResult(response);
            }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取节点列表失败");
                return Task.FromResult(new Contracts.GetNodesResponse
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
        /// 处理读取请求
        /// </summary>
        private OpcUaResponse ProcessReadRequest(OpcUaRequest request)
        {
            var response = new OpcUaResponse
            {
                RequestId = request.RequestId,
                Type = OpcUaResponseType.Read,
                StatusCode = OpcUaStatusCode.Good
            };

            try
            {
                // 这里直接调用SimpleOpcUaServer的内部方法
                // 在实际实现中，可能需要修改SimpleOpcUaServer类以支持直接调用这些方法
                var method = typeof(SimpleOpcUaServer).GetMethod("ProcessReadRequest", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (method != null)
                {
                    response = method.Invoke(_opcServer, new object[] { request }) as OpcUaResponse;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理读取请求时出错");
                response.StatusCode = OpcUaStatusCode.Bad;
            }

            return response;
        }

        /// <summary>
        /// 处理节点值变化事件
        /// </summary>
        private void OnNodeValueChanged(object sender, NodeValueChangedEventArgs e)
        {
            try
            {
                _logger.LogDebug("节点值变化: 节点ID={0}, 新值={1}", e.NodeId, e.NewValue.Value);
                // 这里可以实现节点值变化的处理逻辑，如通知客户端等
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理节点值变化事件失败");
            }
        }

        /// <summary>
        /// 转换Contracts.DataValue到服务器内部DataValue
        /// </summary>
        private static DataValue ConvertToDataValue(Contracts.DataValue dataValue)
        {
            if (dataValue == null)
                return null;

            object value = null;
            switch (dataValue.ValueCase)
            {
                case Contracts.DataValue.ValueOneofCase.BooleanValue:
                    value = dataValue.BooleanValue;
                    break;
                case Contracts.DataValue.ValueOneofCase.Int32Value:
                    value = dataValue.Int32Value;
                    break;
                case Contracts.DataValue.ValueOneofCase.FloatValue:
                    value = dataValue.FloatValue;
                    break;
                case Contracts.DataValue.ValueOneofCase.DoubleValue:
                    value = dataValue.DoubleValue;
                    break;
                case Contracts.DataValue.ValueOneofCase.StringValue:
                    value = dataValue.StringValue;
                    break;
            }

            return new DataValue(value);
        }

        /// <summary>
        /// 转换服务器内部DataValue到Contracts.DataValue
        /// </summary>
        private static Contracts.DataValue ConvertFromDataValue(DataValue dataValue)
        {
            if (dataValue == null)
                return null;

            var result = new Contracts.DataValue
            {
                Timestamp = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow).Seconds * 1000,
                Quality = Contracts.DataQuality.Good // 默认值
            };

            if (dataValue.Value != null && dataValue.Value.Value != null)
            {
                switch (dataValue.Value.Value)
                {
                    case bool boolValue:
                        result.BooleanValue = boolValue;
                        break;
                    case int intValue:
                        result.Int32Value = intValue;
                        break;
                    case float floatValue:
                        result.FloatValue = floatValue;
                        break;
                    case double doubleValue:
                        result.DoubleValue = doubleValue;
                        break;
                    case string stringValue:
                        result.StringValue = stringValue;
                        break;
                }
            }

            return result;
        }

        /// <summary>
        /// 将内部值映射为 Contracts.DataType
        /// </summary>
        private static Contracts.DataType MapToContractsDataType(object value)
        {
            if (value == null) return Contracts.DataType.String;
            return value switch
            {
                bool => Contracts.DataType.Boolean,
                sbyte => Contracts.DataType.Int32,
                byte => Contracts.DataType.Int32,
                short => Contracts.DataType.Int32,
                ushort => Contracts.DataType.Int32,
                int => Contracts.DataType.Int32,
                uint => Contracts.DataType.Int32,
                long => Contracts.DataType.Int32,
                ulong => Contracts.DataType.Int32,
                float => Contracts.DataType.Float,
                double => Contracts.DataType.Double,
                string => Contracts.DataType.String,
                _ => Contracts.DataType.String
            };
        }

        /// <summary>
        /// 转换DataQuality到StatusCode
        /// </summary>
        private static OpcUaStatusCode ConvertToStatusCode(Contracts.DataQuality quality)
        {
            return quality switch
            {
                Contracts.DataQuality.Good => OpcUaStatusCode.Good,
                Contracts.DataQuality.Bad => OpcUaStatusCode.Bad,
                _ => OpcUaStatusCode.Bad
            };
        }
    }
}