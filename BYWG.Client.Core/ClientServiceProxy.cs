using Microsoft.Extensions.Logging;
using BYWG.Contracts;

namespace BYWG.Client.Core
{
    /// <summary>
    /// 客户端服务代理
    /// 封装与服务端的所有通信逻辑，提供更高级别的API给客户端应用程序使用
    /// </summary>
    public class ClientServiceProxy
    {
        private readonly ServiceConnectionManager _connectionManager;
        private readonly ILogger<ClientServiceProxy> _logger;

        public bool IsConnected => _connectionManager.IsConnected;

        public event EventHandler<bool> ConnectionStatusChanged
        {
            add => _connectionManager.ConnectionStatusChanged += value;
            remove => _connectionManager.ConnectionStatusChanged -= value;
        }

        public ClientServiceProxy(ServiceConnectionManager connectionManager, ILogger<ClientServiceProxy> logger = null)
        {
            _connectionManager = connectionManager;
            _logger = logger;
        }

        /// <summary>
        /// 连接到服务端
        /// </summary>
        /// <param name="serverAddress">服务端地址</param>
        /// <returns>是否连接成功</returns>
        public async Task<bool> ConnectAsync(string serverAddress = null)
        {
            return await _connectionManager.ConnectAsync(serverAddress);
        }

        /// <summary>
        /// 断开与服务端的连接
        /// </summary>
        public void Disconnect()
        {
            _connectionManager.Disconnect();
        }

        #region 工业协议管理

        /// <summary>
        /// 添加工业协议
        /// </summary>
        /// <param name="name">协议名称</param>
        /// <param name="type">协议类型</param>
        /// <param name="connectionString">连接字符串</param>
        /// <param name="parameters">协议参数</param>
        /// <returns>操作结果</returns>
        public async Task<OperationResult> AddProtocolAsync(string name, ProtocolType type, string connectionString, Dictionary<string, string> parameters = null)
        {
            try
            {
                var client = _connectionManager.GetProtocolClient();
                var request = new AddProtocolRequest
                {
                    Name = name,
                    Type = type,
                    ConnectionString = connectionString
                };

                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        request.Parameters.Add(param.Key, param.Value);
                    }
                }

                var response = await client.AddProtocolAsync(request);
                return new OperationResult(response.BaseResponse.StatusCode == BYWG.Contracts.StatusCode.Ok, response.BaseResponse.Message);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "添加协议失败: {0}", ex.Message);
                return new OperationResult(false, ex.Message);
            }
        }

        /// <summary>
        /// 删除工业协议
        /// </summary>
        /// <param name="name">协议名称</param>
        /// <returns>操作结果</returns>
        public async Task<OperationResult> RemoveProtocolAsync(string name)
        {
            try
            {
                var client = _connectionManager.GetProtocolClient();
                var request = new RemoveProtocolRequest { Name = name };
                var response = await client.RemoveProtocolAsync(request);
                return new OperationResult(response.BaseResponse.StatusCode == BYWG.Contracts.StatusCode.Ok, response.BaseResponse.Message);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "删除协议失败: {0}", ex.Message);
                return new OperationResult(false, ex.Message);
            }
        }

        /// <summary>
        /// 启动工业协议
        /// </summary>
        /// <param name="name">协议名称</param>
        /// <returns>操作结果</returns>
        public async Task<OperationResult> StartProtocolAsync(string name)
        {
            try
            {
                var client = _connectionManager.GetProtocolClient();
                var request = new StartProtocolRequest { Name = (name ?? string.Empty).Trim() };
                var response = await client.StartProtocolAsync(request);
                return new OperationResult(response.BaseResponse.StatusCode == BYWG.Contracts.StatusCode.Ok, response.BaseResponse.Message);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "启动协议失败: {0}", ex.Message);
                return new OperationResult(false, ex.Message);
            }
        }

        /// <summary>
        /// 停止工业协议
        /// </summary>
        /// <param name="name">协议名称</param>
        /// <returns>操作结果</returns>
        public async Task<OperationResult> StopProtocolAsync(string name)
        {
            try
            {
                var client = _connectionManager.GetProtocolClient();
                var request = new StopProtocolRequest { Name = name };
                var response = await client.StopProtocolAsync(request);
                return new OperationResult(response.BaseResponse.StatusCode == BYWG.Contracts.StatusCode.Ok, response.BaseResponse.Message);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "停止协议失败: {0}", ex.Message);
                return new OperationResult(false, ex.Message);
            }
        }

        /// <summary>
        /// 获取协议列表
        /// </summary>
        /// <returns>协议列表</returns>
        public async Task<List<ProtocolInfo>> GetProtocolsAsync()
        {
            try
            {
                var client = _connectionManager.GetProtocolClient();
                var request = new GetProtocolsRequest();
                var response = await client.GetProtocolsAsync(request);

                if (response.BaseResponse.StatusCode == BYWG.Contracts.StatusCode.Ok)
                {
                    return response.Protocols.ToList();
                }
                else
                {
                    _logger?.LogWarning("获取协议列表失败: {0}", response.BaseResponse.Message);
                    return new List<ProtocolInfo>();
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "获取协议列表失败: {0}", ex.Message);
                return new List<ProtocolInfo>();
            }
        }

        /// <summary>
        /// 测试读取（从指定设备/协议读取一个地址值）
        /// </summary>
        public async Task<OperationResult<DataValue>> TestReadAsync(string deviceName, string address, string dataType)
        {
            try
            {
                var client = _connectionManager.GetProtocolClient();
                var request = new TestReadRequest
                {
                    Name = deviceName ?? string.Empty,
                    Address = address ?? string.Empty,
                    DataType = dataType ?? string.Empty
                };
                var response = await client.TestReadAsync(request);
                if (response.BaseResponse.StatusCode == BYWG.Contracts.StatusCode.Ok)
                {
                    return new OperationResult<DataValue>(true, response.BaseResponse.Message, response.DataValue);
                }
                return new OperationResult<DataValue>(false, response.BaseResponse.Message);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "测试读取失败: {0}", ex.Message);
                return new OperationResult<DataValue>(false, ex.Message);
            }
        }

        #endregion

        #region OPC UA服务器管理

        /// <summary>
        /// 启动OPC UA服务器
        /// </summary>
        /// <param name="port">服务器端口</param>
        /// <returns>操作结果</returns>
        public async Task<OperationResult> StartOpcUaServerAsync(int port = 4840)
        {
            try
            {
                var client = _connectionManager.GetOpcUaClient();
                var request = new StartOpcUaServerRequest { Port = port };
                var response = await client.StartOpcUaServerAsync(request);
                return new OperationResult(response.BaseResponse.StatusCode == BYWG.Contracts.StatusCode.Ok, response.BaseResponse.Message);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "启动OPC UA服务器失败: {0}", ex.Message);
                return new OperationResult(false, ex.Message);
            }
        }

        /// <summary>
        /// 停止OPC UA服务器
        /// </summary>
        /// <returns>操作结果</returns>
        public async Task<OperationResult> StopOpcUaServerAsync()
        {
            try
            {
                var client = _connectionManager.GetOpcUaClient();
                var request = new StopOpcUaServerRequest();
                var response = await client.StopOpcUaServerAsync(request);
                return new OperationResult(response.BaseResponse.StatusCode == BYWG.Contracts.StatusCode.Ok, response.BaseResponse.Message);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "停止OPC UA服务器失败: {0}", ex.Message);
                return new OperationResult(false, ex.Message);
            }
        }

        /// <summary>
        /// 获取服务器状态
        /// </summary>
        /// <returns>服务器状态</returns>
        public async Task<ServerStatusInfo> GetServerStatusAsync()
        {
            try
            {
                var client = _connectionManager.GetOpcUaClient();
                var request = new GetServerStatusRequest();
                var response = await client.GetServerStatusAsync(request);

                if (response.BaseResponse.StatusCode == BYWG.Contracts.StatusCode.Ok)
                {
                    return new ServerStatusInfo
                    {
                        IsRunning = response.IsRunning,
                        SessionCount = response.SessionCount,
                        NodeCount = response.NodeCount
                    };
                }
                else
                {
                    _logger?.LogWarning("获取服务器状态失败: {0}", response.BaseResponse.Message);
                    return new ServerStatusInfo { IsRunning = false };
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "获取服务器状态失败: {0}", ex.Message);
                return new ServerStatusInfo { IsRunning = false };
            }
        }

        #endregion

        #region OPC UA节点管理

        /// <summary>
        /// 添加OPC UA节点
        /// </summary>
        /// <param name="nodeId">节点ID</param>
        /// <param name="displayName">显示名称</param>
        /// <param name="dataType">数据类型</param>
        /// <param name="initialValue">初始值</param>
        /// <returns>操作结果</returns>
        public async Task<OperationResult<NodeId>> AddNodeAsync(string nodeId, string displayName, DataType dataType, DataValue initialValue = null)
        {
            try
            {
                var client = _connectionManager.GetOpcUaClient();
                var request = new AddNodeRequest
                {
                    NodeId = nodeId,
                    DisplayName = displayName,
                    DataType = dataType,
                    InitialValue = initialValue
                };

                var response = await client.AddNodeAsync(request);
                if (response.BaseResponse.StatusCode == BYWG.Contracts.StatusCode.Ok)
                {
                    return new OperationResult<Contracts.NodeId>(true, response.BaseResponse.Message, response.NodeId);
                }
                else
                {
                    return new OperationResult<Contracts.NodeId>(false, response.BaseResponse.Message);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "添加节点失败: {0}", ex.Message);
                return new OperationResult<Contracts.NodeId>(false, ex.Message);
            }
        }

        /// <summary>
        /// 删除OPC UA节点
        /// </summary>
        /// <param name="nodeId">节点ID</param>
        /// <returns>操作结果</returns>
        public async Task<OperationResult> RemoveNodeAsync(NodeId nodeId)
        {
            try
            {
                var client = _connectionManager.GetOpcUaClient();
                var request = new RemoveNodeRequest { NodeId = nodeId };
                var response = await client.RemoveNodeAsync(request);
                return new OperationResult(response.BaseResponse.StatusCode == BYWG.Contracts.StatusCode.Ok, response.BaseResponse.Message);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "删除节点失败: {0}", ex.Message);
                return new OperationResult(false, ex.Message);
            }
        }

        /// <summary>
        /// 读取OPC UA节点值
        /// </summary>
        /// <param name="nodeId">节点ID</param>
        /// <returns>节点值</returns>
        public async Task<OperationResult<DataValue>> ReadNodeAsync(NodeId nodeId)
        {
            try
            {
                var client = _connectionManager.GetOpcUaClient();
                var request = new ReadNodeRequest { NodeId = nodeId };
                var response = await client.ReadNodeAsync(request);

                if (response.BaseResponse.StatusCode == BYWG.Contracts.StatusCode.Ok)
                {
                    return new OperationResult<DataValue>(true, response.BaseResponse.Message, response.DataValue);
                }
                else
                {
                    return new OperationResult<DataValue>(false, response.BaseResponse.Message);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "读取节点失败: {0}", ex.Message);
                return new OperationResult<DataValue>(false, ex.Message);
            }
        }

        /// <summary>
        /// 写入OPC UA节点值
        /// </summary>
        /// <param name="nodeId">节点ID</param>
        /// <param name="dataValue">数据值</param>
        /// <returns>操作结果</returns>
        public async Task<OperationResult> WriteNodeAsync(Contracts.NodeId nodeId, Contracts.DataValue dataValue)
        {
            try
            {
                var client = _connectionManager.GetOpcUaClient();
                var request = new WriteNodeRequest { NodeId = nodeId, DataValue = dataValue };
                var response = await client.WriteNodeAsync(request);
                return new OperationResult(response.BaseResponse.StatusCode == BYWG.Contracts.StatusCode.Ok, response.BaseResponse.Message);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "写入节点失败: {0}", ex.Message);
                return new OperationResult(false, ex.Message);
            }
        }

        /// <summary>
        /// 获取节点列表
        /// </summary>
        /// <returns>节点列表</returns>
        public async Task<List<Contracts.NodeInfo>> GetNodesAsync()
        {
            try
            {
                var client = _connectionManager.GetOpcUaClient();
                var request = new GetNodesRequest();
                var response = await client.GetNodesAsync(request);

                if (response.BaseResponse.StatusCode == BYWG.Contracts.StatusCode.Ok)
                {
                    return response.Nodes.ToList();
                }
                else
                {
                    _logger?.LogWarning("获取节点列表失败: {0}", response.BaseResponse.Message);
                    return new List<NodeInfo>();
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "获取节点列表失败: {0}", ex.Message);
                return new List<NodeInfo>();
            }
        }

        #endregion

        /// <summary>
        /// 操作结果类
        /// </summary>
        public class OperationResult
        {
            public bool Success { get; }
            public string Message { get; }

            public OperationResult(bool success, string message)
            {
                Success = success;
                Message = message;
            }
        }

        /// <summary>
        /// 带返回数据的操作结果类
        /// </summary>
        public class OperationResult<T> : OperationResult
        {
            public T Data { get; }
            public bool HasData => Data != null;

            public OperationResult(bool success, string message, T data = default)
                : base(success, message)
            {
                Data = data;
            }
        }

        /// <summary>
        /// 服务器状态信息类
        /// </summary>
        public class ServerStatusInfo
        {
            public bool IsRunning { get; set; }
            public int SessionCount { get; set; }
            public int NodeCount { get; set; }
        }
    }
}