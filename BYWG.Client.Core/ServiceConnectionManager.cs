using BYWG.Contracts;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using BYWG.Contracts;

namespace BYWG.Client.Core
{
    /// <summary>
    /// 服务端连接管理器
    /// 负责管理与服务端的连接，包括创建、维护和关闭连接
    /// </summary>
    public class ServiceConnectionManager : IDisposable
    {
        private readonly ILogger<ServiceConnectionManager> _logger;
        private GrpcChannel _channel;
        private ProtocolService.ProtocolServiceClient _protocolClient;
        private OpcUaService.OpcUaServiceClient _opcUaClient;
        private bool _isConnected;
        private string _serverAddress = "http://localhost:5001";

        public event EventHandler<bool> ConnectionStatusChanged;

        public bool IsConnected => _isConnected;

        public ServiceConnectionManager(ILogger<ServiceConnectionManager> logger = null)
        {
            _logger = logger;
        }

        /// <summary>
        /// 连接到服务端
        /// </summary>
        /// <param name="serverAddress">服务端地址，默认为 https://localhost:5001</param>
        /// <returns>是否连接成功</returns>
        public async Task<bool> ConnectAsync(string serverAddress = null)
        {
            try
            {
                if (!string.IsNullOrEmpty(serverAddress))
                {
                    _serverAddress = serverAddress;
                }

                // 创建HTTP处理程序与明文HTTP/2支持（仅开发环境）
                if (_serverAddress.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
                {
                    AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
                }

                var socketsHandler = new SocketsHttpHandler
                {
                    // 仅用于开发环境的证书忽略
                    SslOptions = new System.Net.Security.SslClientAuthenticationOptions
                    {
                        RemoteCertificateValidationCallback = (sender, cert, chain, errors) => true
                    },
                    EnableMultipleHttp2Connections = true,
                    AutomaticDecompression = System.Net.DecompressionMethods.All
                };

                // 创建gRPC通道
                _channel = GrpcChannel.ForAddress(_serverAddress, new GrpcChannelOptions
                {
                    HttpHandler = socketsHandler
                });

                // 创建客户端代理
                _protocolClient = new ProtocolService.ProtocolServiceClient(_channel);
                _opcUaClient = new OpcUaService.OpcUaServiceClient(_channel);

                // 测试连接
                await TestConnectionAsync();

                _isConnected = true;
                _logger?.LogInformation("已连接到服务端: {0}", _serverAddress);
                ConnectionStatusChanged?.Invoke(this, _isConnected);

                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "连接到服务端失败: {0}", ex.Message);
                _isConnected = false;
                ConnectionStatusChanged?.Invoke(this, _isConnected);
                return false;
            }
        }

        /// <summary>
        /// 断开与服务端的连接
        /// </summary>
        public void Disconnect()
        {
            try
            {
                _channel?.ShutdownAsync().Wait();
                _channel = null;
                _protocolClient = null;
                _opcUaClient = null;
                _isConnected = false;
                _logger?.LogInformation("已断开与服务端的连接");
                ConnectionStatusChanged?.Invoke(this, _isConnected);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "断开连接时出错: {0}", ex.Message);
            }
        }

        /// <summary>
        /// 测试连接是否正常
        /// </summary>
        private async Task TestConnectionAsync()
        {
            try
            {
                // 发送一个简单的请求以测试连接
                var request = new GetProtocolsRequest();
                var response = await _protocolClient.GetProtocolsAsync(request);
                if (response.BaseResponse.StatusCode != BYWG.Contracts.StatusCode.Ok)
                {
                    throw new Exception("连接测试失败: " + response.BaseResponse.Message);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "连接测试失败: {0}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// 获取协议服务客户端
        /// </summary>
        /// <returns>协议服务客户端</returns>
        /// <exception cref="InvalidOperationException">当未连接到服务端时抛出</exception>
        public ProtocolService.ProtocolServiceClient GetProtocolClient()
        {
            if (!_isConnected || _protocolClient == null)
            {
                throw new InvalidOperationException("未连接到服务端");
            }
            return _protocolClient;
        }

        /// <summary>
        /// 获取OPC UA服务客户端
        /// </summary>
        /// <returns>OPC UA服务客户端</returns>
        /// <exception cref="InvalidOperationException">当未连接到服务端时抛出</exception>
        public OpcUaService.OpcUaServiceClient GetOpcUaClient()
        {
            if (!_isConnected || _opcUaClient == null)
            {
                throw new InvalidOperationException("未连接到服务端");
            }
            return _opcUaClient;
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            Disconnect();
        }
    }
}