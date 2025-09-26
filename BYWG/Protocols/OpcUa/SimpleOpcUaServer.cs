using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using BYWG.Utils;

namespace BYWG.Protocols.OpcUa
{
    /// <summary>
    /// 简化的OPC UA服务器实现
    /// </summary>
    public class SimpleOpcUaServer : IDisposable
    {
        private TcpListener _tcpListener;
        private bool _isRunning;
        private readonly Dictionary<NodeId, BaseNode> _nodes;
        private readonly Dictionary<string, OpcUaSession> _sessions;
        private readonly object _lockObject = new object();
        private ushort _namespaceIndex = 1;
        private uint _sessionIdCounter = 1;

        public event EventHandler<NodeValueChangedEventArgs> NodeValueChanged;

        public SimpleOpcUaServer()
        {
            _nodes = new Dictionary<NodeId, BaseNode>();
            _sessions = new Dictionary<string, OpcUaSession>();
            InitializeDefaultNodes();
        }

        /// <summary>
        /// 启动服务器
        /// </summary>
        public async Task StartAsync(int port = 4840)
        {
            try
            {
                _tcpListener = new TcpListener(IPAddress.Any, port);
                _tcpListener.Start();
                _isRunning = true;

                Log.Information("简化OPC UA服务器已启动，端口: {0}", port);

                // 开始接受客户端连接
                _ = Task.Run(AcceptClientsAsync);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "启动OPC UA服务器失败");
                throw;
            }
        }

        /// <summary>
        /// 停止服务器
        /// </summary>
        public void Stop()
        {
            try
            {
                _isRunning = false;
                _tcpListener?.Stop();

                // 关闭所有会话
                lock (_lockObject)
                {
                    foreach (var session in _sessions.Values)
                    {
                        session.Close();
                    }
                    _sessions.Clear();
                }

                Console.WriteLine("OPC UA服务器已停止");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"停止OPC UA服务器时出错: {ex.Message}");
            }
        }

        /// <summary>
        /// 接受客户端连接
        /// </summary>
        private async Task AcceptClientsAsync()
        {
            while (_isRunning)
            {
                try
                {
                    var tcpClient = await _tcpListener.AcceptTcpClientAsync();
                    _ = Task.Run(() => HandleClientAsync(tcpClient));
                }
                catch (ObjectDisposedException)
                {
                    // 服务器已停止
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"接受客户端连接时出错: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 处理客户端连接
        /// </summary>
        private async Task HandleClientAsync(TcpClient tcpClient)
        {
            OpcUaSession session = null;
            try
            {
                session = new OpcUaSession(tcpClient, _sessionIdCounter++);
                session.RequestReceived += OnRequestReceived;
                session.Closed += OnSessionClosed;

                lock (_lockObject)
                {
                    _sessions[session.SessionId] = session;
                }

                Log.Information("新客户端连接: {0}", session.SessionId);
                await session.ProcessAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "处理客户端连接时出错");
            }
            finally
            {
                if (session != null)
                {
                    lock (_lockObject)
                    {
                        _sessions.Remove(session.SessionId);
                    }
                    session.Dispose();
                }
            }
        }

        /// <summary>
        /// 处理请求
        /// </summary>
        private void OnRequestReceived(object sender, OpcUaRequestEventArgs e)
        {
            try
            {
                var session = sender as OpcUaSession;
                var response = ProcessRequest(e.Request);
                session?.SendResponse(response);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "处理OPC UA请求时出错");
            }
        }

        /// <summary>
        /// 处理会话关闭
        /// </summary>
        private void OnSessionClosed(object sender, EventArgs e)
        {
            var session = sender as OpcUaSession;
            if (session != null)
            {
                lock (_lockObject)
                {
                    _sessions.Remove(session.SessionId);
                }
                Log.Information("客户端会话已关闭: {0}", session.SessionId);
            }
        }

        /// <summary>
        /// 处理OPC UA请求
        /// </summary>
        private OpcUaResponse ProcessRequest(OpcUaRequest request)
        {
            return request.Type switch
            {
                OpcUaRequestType.Read => ProcessReadRequest(request),
                OpcUaRequestType.Write => ProcessWriteRequest(request),
                OpcUaRequestType.Browse => ProcessBrowseRequest(request),
                _ => new OpcUaResponse { RequestId = request.RequestId, StatusCode = StatusCode.Bad }
            };
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
                StatusCode = StatusCode.Good
            };

            try
            {
                var nodeId = request.NodeId;
                var attributeId = request.AttributeId;

                lock (_lockObject)
                {
                    if (_nodes.TryGetValue(nodeId, out var node))
                    {
                        response.DataValue = node.ReadAttribute(attributeId);
                    }
                    else
                    {
                        response.StatusCode = StatusCode.Bad;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "处理读取请求时出错");
                response.StatusCode = StatusCode.Bad;
            }

            return response;
        }

        /// <summary>
        /// 处理写入请求
        /// </summary>
        private OpcUaResponse ProcessWriteRequest(OpcUaRequest request)
        {
            var response = new OpcUaResponse
            {
                RequestId = request.RequestId,
                Type = OpcUaResponseType.Write,
                StatusCode = StatusCode.Good
            };

            try
            {
                var nodeId = request.NodeId;
                var attributeId = request.AttributeId;
                var dataValue = request.DataValue;

                lock (_lockObject)
                {
                    if (_nodes.TryGetValue(nodeId, out var node))
                    {
                        response.StatusCode = node.WriteAttribute(attributeId, dataValue);
                        
                        // 触发值变化事件
                        if (response.StatusCode.IsGood && attributeId == AttributeId.Value)
                        {
                            NodeValueChanged?.Invoke(this, new NodeValueChangedEventArgs
                            {
                                NodeId = nodeId,
                                NewValue = dataValue
                            });
                        }
                    }
                    else
                    {
                        response.StatusCode = StatusCode.Bad;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "处理写入请求时出错");
                response.StatusCode = StatusCode.Bad;
            }

            return response;
        }

        /// <summary>
        /// 处理浏览请求
        /// </summary>
        private OpcUaResponse ProcessBrowseRequest(OpcUaRequest request)
        {
            var response = new OpcUaResponse
            {
                RequestId = request.RequestId,
                Type = OpcUaResponseType.Browse,
                StatusCode = StatusCode.Good
            };

            try
            {
                var nodeId = request.NodeId;
                var references = new List<ReferenceDescription>();

                lock (_lockObject)
                {
                    if (_nodes.TryGetValue(nodeId, out var node))
                    {
                        references.AddRange(node.References);
                    }
                }

                response.References = references;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "处理浏览请求时出错");
                response.StatusCode = StatusCode.Bad;
            }

            return response;
        }

        /// <summary>
        /// 添加节点
        /// </summary>
        public void AddNode(BaseNode node)
        {
            lock (_lockObject)
            {
                _nodes[node.NodeId] = node;
            }
            Log.Information("已添加节点: {0}", node.NodeId);
        }

        /// <summary>
        /// 删除节点
        /// </summary>
        public void RemoveNode(NodeId nodeId)
        {
            lock (_lockObject)
            {
                _nodes.Remove(nodeId);
            }
            Log.Information("已删除节点: {0}", nodeId);
        }

        /// <summary>
        /// 更新节点值
        /// </summary>
        public void UpdateNodeValue(NodeId nodeId, object value)
        {
            lock (_lockObject)
            {
                if (_nodes.TryGetValue(nodeId, out var node) && node is DataVariableNode dataNode)
                {
                    dataNode.Value = new DataValue(value);
                    
                    // 触发值变化事件
                    NodeValueChanged?.Invoke(this, new NodeValueChangedEventArgs
                    {
                        NodeId = nodeId,
                        NewValue = dataNode.Value
                    });
                }
            }
        }

        /// <summary>
        /// 初始化默认节点
        /// </summary>
        private void InitializeDefaultNodes()
        {
            // 添加根对象
            var rootObject = new ObjectNode(new NodeId("Root", _namespaceIndex))
            {
                BrowseName = new QualifiedName("Root", _namespaceIndex),
                DisplayName = new LocalizedText("Root")
            };
            AddNode(rootObject);
        }

        /// <summary>
        /// 获取所有数据变量类型的节点快照
        /// </summary>
        public IReadOnlyList<DataVariableNode> GetDataVariableNodes()
        {
            lock (_lockObject)
            {
                return _nodes.Values.OfType<DataVariableNode>().ToList();
            }
        }

        /// <summary>
        /// 获取当前会话数量
        /// </summary>
        public int GetSessionCount()
        {
            lock (_lockObject)
            {
                return _sessions.Count;
            }
        }

        /// <summary>
        /// 获取服务器状态
        /// </summary>
        public bool IsRunning => _isRunning;

        public void Dispose()
        {
            Stop();
        }
    }

    /// <summary>
    /// 节点值变化事件参数
    /// </summary>
    public class NodeValueChangedEventArgs : EventArgs
    {
        public NodeId NodeId { get; set; }
        public DataValue NewValue { get; set; }
    }
}
