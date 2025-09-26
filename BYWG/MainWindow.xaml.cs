using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading.Tasks;
using System.Timers;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Reflection;
using System.Net;
using BYWG.Utils;
using BYWG.Protocols.OpcUa;

namespace BYWG
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // OPC UA 服务器相关对象
        private SimpleOpcUaServer? opcServer;
        private System.Timers.Timer? statusTimer;
        
        // 工业协议配置
        private Dictionary<string, IndustrialProtocolConfig> protocolConfigs = new Dictionary<string, IndustrialProtocolConfig>();
        
        // 工业协议管理器
        private IndustrialProtocolManager? protocolManager;
        
        // 配置对象
        private IConfiguration? configuration;
        
        public MainWindow()
        {
            InitializeComponent();
            InitializeConfiguration();
            InitializeProtocolManager();
            InitializeEvents();
            InitializeDashboard();
        }
        
        private void InitializeConfiguration()
        {
            try
            {
                // 创建配置构建器
                var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                
                // 构建配置
                configuration = builder.Build();
                
                // 读取并初始化协议配置
                LoadProtocolConfigurations();
                
                statusText.Text = "配置加载成功";
            }
            catch (Exception ex)
            {
                statusText.Text = "配置加载失败: " + ex.Message;
                Log.Error(ex, "配置加载异常");
            }
        }
        
        private void InitializeProtocolManager()
        {
            try
            {
                // 创建工业协议管理器
                protocolManager = new IndustrialProtocolManager();
                
                // 订阅数据变化事件
                protocolManager.DataChanged += ProtocolManager_DataChanged;
                
                statusText.Text = "工业协议管理器初始化成功";
            }
            catch (Exception ex)
            {
                statusText.Text = "工业协议管理器初始化失败: " + ex.Message;
                Log.Error(ex, "协议管理器初始化异常");
            }
        }
        
        private void LoadProtocolConfigurations()
        {
            try
            {
                // 在这里可以从配置文件或数据库加载协议配置
                // 目前我们只添加一些示例配置
                
                // Modbus RTU 示例配置
                protocolConfigs["ModbusRTU-1"] = new IndustrialProtocolConfig
                {
                    Name = "ModbusRTU-1",
                    Type = "ModbusRTU",
                    ConnectionString = "COM3",
                    Enabled = false,
                    Parameters = new Dictionary<string, string>
                    {
                        { "BaudRate", configuration["ModbusRTU:BaudRate"] ?? "9600" },
                        { "DataBits", configuration["ModbusRTU:DataBits"] ?? "8" },
                        { "Parity", configuration["ModbusRTU:Parity"] ?? "None" },
                        { "StopBits", configuration["ModbusRTU:StopBits"] ?? "1" },
                        { "Timeout", configuration["ModbusRTU:Timeout"] ?? "5000" }
                    }
                };
                
                // Modbus TCP 示例配置
                protocolConfigs["ModbusTCP-1"] = new IndustrialProtocolConfig
                {
                    Name = "ModbusTCP-1",
                    Type = "ModbusTCP",
                    ConnectionString = "192.168.1.100",
                    Enabled = false,
                    Parameters = new Dictionary<string, string>
                    {
                        { "Port", configuration["ModbusTCP:Port"] ?? "502" },
                        { "Timeout", configuration["ModbusTCP:Timeout"] ?? "5000" },
                        { "ConnectionTimeout", configuration["ModbusTCP:ConnectionTimeout"] ?? "5000" }
                    }
                };
                
                // Siemens S7 示例配置
                protocolConfigs["SiemensS7-1"] = new IndustrialProtocolConfig
                {
                    Name = "SiemensS7-1",
                    Type = "SiemensS7",
                    ConnectionString = "192.168.1.101",
                    Enabled = false,
                    Parameters = new Dictionary<string, string>
                    {
                        { "Port", configuration["SiemensS7:Port"] ?? "102" },
                        { "Timeout", configuration["SiemensS7:Timeout"] ?? "5000" },
                        { "ConnectionTimeout", configuration["SiemensS7:ConnectionTimeout"] ?? "5000" },
                        { "PLCType", "S71200" }
                    }
                };
                
                // Mitsubishi 示例配置
                protocolConfigs["Mitsubishi-1"] = new IndustrialProtocolConfig
                {
                    Name = "Mitsubishi-1",
                    Type = "Mitsubishi",
                    ConnectionString = "192.168.1.102",
                    Enabled = false,
                    Parameters = new Dictionary<string, string>
                    {
                        { "Port", configuration["Mitsubishi:Port"] ?? "5000" },
                        { "Timeout", configuration["Mitsubishi:Timeout"] ?? "5000" },
                        { "ConnectionTimeout", configuration["Mitsubishi:ConnectionTimeout"] ?? "5000" },
                        { "ProtocolType", "MC" }
                    }
                };
            }
            catch (Exception ex)
            {
                statusText.Text = "协议配置加载失败: " + ex.Message;
                Log.Error(ex, "协议配置加载异常");
            }
        }
        
        private void ProtocolManager_DataChanged(object sender, DataChangedEventArgs e)
        {
            // 当工业协议管理器检测到数据变化时，更新OPC UA服务器中的节点值
            try
            {
                if (opcServer != null)
                {
                    // 更新OPC UA服务器中的节点值
                    foreach (var item in e.ChangedItems)
                    {
                        var nodeId = new NodeId(item.Name, 1);
                        opcServer.UpdateNodeValue(nodeId, item.Value);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "数据更新异常");
            }
        }
        
        private void OnNodeValueChanged(object sender, NodeValueChangedEventArgs e)
        {
            // 当OPC UA节点值变化时的处理
            try
            {
                Log.Debug("OPC UA节点值变化: {0} = {1}", e.NodeId, e.NewValue?.Value?.Value);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "处理节点值变化事件时出错");
            }
        }
        
        private void InitializeEvents()
        {
            // 绑定按钮事件
            startServerBtn.Click += StartServerBtn_Click;
            stopServerBtn.Click += StopServerBtn_Click;
            
            // 初始化状态定时器
            statusTimer = new System.Timers.Timer(1000);
            statusTimer.Elapsed += StatusTimer_Elapsed;
            statusTimer.Start();
        }
        
        private void InitializeDashboard()
        {
            // 初始化数据网格
            dataGrid.ItemsSource = new List<DataGridItem>();
            
            // 初始化协议列表
            protocolListBox.ItemsSource = new List<ProtocolInfo>();
            
            // 初始化节点数据网格
            nodesDataGrid.ItemsSource = new List<NodeInfo>();

            // 初始化设备筛选下拉
            RefreshDeviceFilter();
        }
        
        private async void StartServerBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                statusText.Text = "正在启动OPC UA服务器...";
                await Task.Run(() => StartOpcServer());
                
                startServerBtn.IsEnabled = false;
                stopServerBtn.IsEnabled = true;
                connectionStatus.Text = "OPC UA服务：运行中";
                statusText.Text = "OPC UA服务器已启动";
            }
            catch (Exception ex)
            {
                statusText.Text = "启动失败: " + ex.Message;
            }
        }
        
        private void StopServerBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                statusText.Text = "正在停止OPC UA服务器...";
                StopOpcServer();
                
                startServerBtn.IsEnabled = true;
                stopServerBtn.IsEnabled = false;
                connectionStatus.Text = "OPC UA服务：未运行";
                statusText.Text = "OPC UA服务器已停止";
            }
            catch (Exception ex)
            {
                statusText.Text = "停止失败: " + ex.Message;
            }
        }
        
        private async void StartOpcServer()
        {
            try
            {
                // 从配置文件读取OPC UA服务器设置
                string endpointUrl = configuration["OpcUaServer:EndpointUrl"] ?? "opc.tcp://localhost:4840/";
                int port = 4840;
                
                // 解析端口号
                if (endpointUrl.Contains(":"))
                {
                    var parts = endpointUrl.Split(':');
                    if (parts.Length >= 3 && int.TryParse(parts[2].Replace("/", ""), out int parsedPort))
                    {
                        port = parsedPort;
                    }
                }
                
                // 创建简化的OPC UA服务器
                opcServer = new SimpleOpcUaServer();
                
                // 订阅节点值变化事件
                opcServer.NodeValueChanged += OnNodeValueChanged;
                
                // 启动服务器
                await opcServer.StartAsync(port);
                
                // 启动工业协议管理器
                StartIndustrialProtocols();
                
                Log.Information("简化OPC UA服务器启动成功，端口: {0}", port);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "OPC UA服务器启动异常");
                throw;
            }
        }
        
        private void StartIndustrialProtocols()
        {
            try
            {
                if (protocolManager == null)
                {
                    protocolManager = new IndustrialProtocolManager();
                    protocolManager.DataChanged += ProtocolManager_DataChanged;
                }
                
                // 从配置中读取轮询间隔
                int pollingInterval = int.Parse(configuration["IndustrialProtocols:PollingInterval"] ?? "1000");
                
                // 初始化协议管理器
                protocolManager.Initialize(pollingInterval);
                
                // 启动所有启用的协议
                foreach (var config in protocolConfigs.Values.Where(c => c.Enabled))
                {
                    protocolManager.AddProtocol(config.Name, config.Type, config.ConnectionString, config.Parameters);
                    protocolManager.StartProtocol(config.Name);
                }
                
                Log.Information("工业协议管理器初始化成功");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "工业协议管理器启动异常");
                throw;
            }
        }
        
        private void StopOpcServer()
        {
            try
            {
                // 停止工业协议管理器
                if (protocolManager != null)
                {
                    protocolManager.Shutdown();
                    protocolManager.DataChanged -= ProtocolManager_DataChanged;
                }
                
                // 停止OPC UA服务器
                if (opcServer != null)
                {
                    opcServer.NodeValueChanged -= OnNodeValueChanged;
                    opcServer.Stop();
                    opcServer.Dispose();
                    opcServer = null;
                }
                
                Log.Information("OPC UA服务器已停止");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "OPC UA服务器停止异常");
                throw;
            }
        }
        
        private void StatusTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            // 定期更新状态信息
            Dispatcher.Invoke(() =>
            {
                try
                {
                    // 获取仪表板中的状态文本元素
                    TextBlock connectionsText = null;
                    TextBlock activeProtocolsText = null;
                    TextBlock dataRateText = null;
                    
                    foreach (var child in dashboardGrid.Children)
                    {
                        if (child is TextBlock textBlock)
                        {
                            if (textBlock.Text.StartsWith("连接数"))
                                connectionsText = textBlock;
                            else if (textBlock.Text.StartsWith("活跃协议数"))
                                activeProtocolsText = textBlock;
                            else if (textBlock.Text.StartsWith("数据传输率"))
                                dataRateText = textBlock;
                        }
                    }
                    
                    // 更新连接数
                    if (connectionCountText != null && opcServer != null)
                    {
                        int sessionCount = opcServer.GetSessionCount();
                        connectionCountText.Text = "连接数: " + sessionCount;
                    }
                    
                    // 更新活跃协议数和数据传输率
                    if (protocolManager != null)
                    {
                        if (activeProtocolsText != null)
                        {
                            int activeProtocolCount = protocolManager.GetActiveProtocolCount();
                            activeProtocolsText.Text = "活跃协议数: " + activeProtocolCount;
                        }
                        
                        if (dataRateText != null)
                        {
                            double dataRate = protocolManager.GetCurrentDataRate();
                            dataRateText.Text = string.Format("数据传输率: {0:F2} B/s", dataRate);
                        }
                    }
                    
                    // 更新连接状态
                    if (opcServer != null && opcServer.IsRunning)
                    {
                        connectionStatus.Text = "OPC UA服务：运行中";
                    }
                    else
                    {
                        connectionStatus.Text = "OPC UA服务：未运行";
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "状态更新异常");
                }
            });
        }
        
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            
            // 确保在窗口关闭时停止服务器
            StopOpcServer();
            
            // 停止定时器
            if (statusTimer != null)
            {
                statusTimer.Stop();
                statusTimer.Dispose();
            }
        }
        
        // 新增的事件处理方法
        private void AddProtocolBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 创建添加协议对话框
                var dialog = new AddProtocolDialog();
                if (dialog.ShowDialog() == true)
                {
                    var config = dialog.GetProtocolConfig();
                    protocolConfigs[config.Name] = config;
                    
                    // 添加到协议管理器
                    protocolManager.AddProtocol(config.Name, config.Type, config.ConnectionString, config.Parameters);
                    
                    // 刷新协议列表
                    RefreshProtocolList();
                    
                    Log.Information("已添加协议: {0}", config.Name);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "添加协议时出错");
                MessageBox.Show($"添加协议失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void RefreshProtocolsBtn_Click(object sender, RoutedEventArgs e)
        {
            RefreshProtocolList();
            RefreshDeviceFilter();
        }
        
        private void ProtocolListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (protocolListBox.SelectedItem is ProtocolInfo selectedProtocol)
            {
                ShowProtocolConfig(selectedProtocol);
            }
            else
            {
                ShowNoSelection();
            }
        }
        
        private void AddNodeBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 创建添加节点对话框，传入已配置设备（协议）供选择
                var dialog = new AddNodeDialog(protocolConfigs.Values.Select(c => new ProtocolInfo
                {
                    Name = c.Name,
                    Type = c.Type,
                    Status = c.Enabled ? "运行中" : "已停止"
                }));

                // 若在节点管理界面已选择设备，则默认选中对应设备
                if (deviceFilterComboBox?.SelectedItem is ProtocolInfo selectedDevice)
                {
                    // 通过设定对话框设备下拉默认值（对话框初始化时会选第一个，无法直接赋值，这里通过重建列表将目标置顶）
                    var list = (dialog as AddNodeDialog)?.GetType().GetField("_devices", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(dialog) as System.Collections.IList;
                    if (list != null)
                    {
                        // 将选中设备移动到首位
                        var match = list.Cast<ProtocolInfo>().FirstOrDefault(d => d.Name == selectedDevice.Name);
                        if (match != null)
                        {
                            list.Remove(match);
                            list.Insert(0, match);
                        }
                    }
                }

                // 配置测试读取处理器
                dialog.TestReadHandler = (deviceName, deviceType, address, dataType) =>
                {
                    try
                    {
                        if (protocolManager == null)
                        {
                            return (false, null, "协议管理器未初始化");
                        }
                        var res = protocolManager.TryReadValue(deviceName, deviceType, address, dataType);
                        return res;
                    }
                    catch (Exception ex)
                    {
                        return (false, null, ex.Message);
                    }
                };
                if (dialog.ShowDialog() == true)
                {
                    var nodeInfo = dialog.GetNodeInfo();
                    
                    // 添加到OPC UA服务器
                    if (opcServer != null)
                    {
                        var nodeId = new NodeId(nodeInfo.NodeId, 1);
                        var dataNode = new DataVariableNode(nodeId)
                        {
                            BrowseName = new QualifiedName(nodeInfo.NodeId, 1),
                            DisplayName = new LocalizedText(nodeInfo.DisplayName),
                            Value = new DataValue(nodeInfo.Value)
                        };
                        opcServer.AddNode(dataNode);
                    }
                    
                    // 刷新节点列表
                    RefreshNodesList();
                    
                    Log.Information("已添加节点: {0}", nodeInfo.NodeId);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "添加节点时出错");
                MessageBox.Show($"添加节点失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void RemoveNodeBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (nodesDataGrid.SelectedItem is NodeInfo selectedNode)
                {
                    // 从OPC UA服务器删除节点
                    if (opcServer != null)
                    {
                        var nodeId = new NodeId(selectedNode.NodeId, 1);
                        opcServer.RemoveNode(nodeId);
                    }
                    
                    // 刷新节点列表
                    RefreshNodesList();
                    
                    Log.Information("已删除节点: {0}", selectedNode.NodeId);
                }
                else
                {
                    MessageBox.Show("请选择一个节点", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "删除节点时出错");
                MessageBox.Show($"删除节点失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void RefreshNodesBtn_Click(object sender, RoutedEventArgs e)
        {
            RefreshNodesList();
        }
        
        // 辅助方法
        private void RefreshProtocolList()
        {
            try
            {
                var protocols = protocolConfigs.Values.Select(config => new ProtocolInfo
                {
                    Name = config.Name,
                    Type = config.Type,
                    Status = config.Enabled ? "运行中" : "已停止"
                }).ToList();
                
                protocolListBox.ItemsSource = protocols;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "刷新协议列表时出错");
            }
        }
        
        private void ShowProtocolConfig(ProtocolInfo protocol)
        {
            try
            {
                protocolConfigPanel.Children.Clear();
                
                var title = new TextBlock
                {
                    Text = "协议配置",
                    FontWeight = FontWeights.Bold,
                    FontSize = 14,
                    Margin = new Thickness(0, 0, 0, 10)
                };
                protocolConfigPanel.Children.Add(title);
                
                if (protocolConfigs.TryGetValue(protocol.Name, out var config))
                {
                    // 显示协议配置信息
                    var nameText = new TextBlock { Text = $"名称: {config.Name}", Margin = new Thickness(0, 5) };
                    var typeText = new TextBlock { Text = $"类型: {config.Type}", Margin = new Thickness(0, 5) };
                    var connectionText = new TextBlock { Text = $"连接: {config.ConnectionString}", Margin = new Thickness(0, 5) };
                    var enabledText = new TextBlock { Text = $"启用: {config.Enabled}", Margin = new Thickness(0, 5) };
                    
                    protocolConfigPanel.Children.Add(nameText);
                    protocolConfigPanel.Children.Add(typeText);
                    protocolConfigPanel.Children.Add(connectionText);
                    protocolConfigPanel.Children.Add(enabledText);
                    
                    // 添加控制按钮
                    var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 10, 0, 0) };
                    
                    var startBtn = new Button { Content = "启动", Padding = new Thickness(10, 5), Margin = new Thickness(0, 0, 10, 0) };
                    startBtn.Click += (s, e) => {
                        try
                        {
                            protocolManager.StartProtocol(config.Name);
                            config.Enabled = true;
                            RefreshProtocolList();
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, "启动协议失败: {0}", config.Name);
                            MessageBox.Show($"启动协议失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    };
                    
                    var stopBtn = new Button { Content = "停止", Padding = new Thickness(10, 5) };
                    stopBtn.Click += (s, e) => {
                        try
                        {
                            protocolManager.StopProtocol(config.Name);
                            config.Enabled = false;
                            RefreshProtocolList();
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, "停止协议失败: {0}", config.Name);
                            MessageBox.Show($"停止协议失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    };
                    
                    buttonPanel.Children.Add(startBtn);
                    buttonPanel.Children.Add(stopBtn);
                    protocolConfigPanel.Children.Add(buttonPanel);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "显示协议配置时出错");
            }
        }

        private void RefreshDeviceFilter()
        {
            try
            {
                var devices = protocolConfigs.Values
                    .Select(c => new ProtocolInfo { Name = c.Name, Type = c.Type, Status = c.Enabled ? "运行中" : "已停止" })
                    .ToList();
                if (deviceFilterComboBox != null)
                {
                    deviceFilterComboBox.ItemsSource = devices;
                    if (devices.Count > 0 && deviceFilterComboBox.SelectedIndex < 0)
                    {
                        deviceFilterComboBox.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "刷新设备筛选时出错");
            }
        }

        private void RefreshDevicesBtn_Click(object sender, RoutedEventArgs e)
        {
            RefreshDeviceFilter();
        }
        
        private void ShowNoSelection()
        {
            protocolConfigPanel.Children.Clear();
            
            var title = new TextBlock
            {
                Text = "协议配置",
                FontWeight = FontWeights.Bold,
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 10)
            };
            protocolConfigPanel.Children.Add(title);
            
            var noSelectionText = new TextBlock
            {
                Text = "请选择一个协议进行配置",
                Foreground = new SolidColorBrush(Colors.Gray)
            };
            protocolConfigPanel.Children.Add(noSelectionText);
        }
        
        private void RefreshNodesList()
        {
            try
            {
                var nodes = new List<NodeInfo>();

                if (opcServer != null && opcServer.IsRunning)
                {
                    var dataNodes = opcServer.GetDataVariableNodes();
                    foreach (var dn in dataNodes)
                    {
                        nodes.Add(new NodeInfo
                        {
                            NodeId = dn.NodeId.Identifier?.ToString() ?? string.Empty,
                            DisplayName = dn.DisplayName?.Text ?? dn.BrowseName?.Name ?? string.Empty,
                            DataType = dn.Value?.Value?.GetType().Name ?? string.Empty,
                            Value = dn.Value?.Value?.ToString() ?? string.Empty,
                            Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                        });
                    }
                }

                nodesDataGrid.ItemsSource = nodes;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "刷新节点列表时出错");
            }
        }
        
        private NodeId GetDataTypeFromString(string dataType)
        {
            switch (dataType.ToLower())
            {
                case "bool":
                case "boolean":
                    return DataTypeIds.Boolean;
                case "int16":
                    return DataTypeIds.Int16;
                case "int32":
                    return DataTypeIds.Int32;
                case "float":
                case "single":
                    return DataTypeIds.Float;
                case "double":
                    return DataTypeIds.Double;
                case "string":
                    return DataTypeIds.String;
                default:
                    return DataTypeIds.String;
            }
        }
    }
    
    /// <summary>
    /// 工业协议配置类
    /// </summary>
    public class IndustrialProtocolConfig
    {
        public string Name { get; set; }
        public string Type { get; set; } // 例如: Modbus, Siemens, Mitsubishi等
        public string ConnectionString { get; set; }
        public bool Enabled { get; set; }
        public Dictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>();
    }
    
    /// <summary>
    /// 数据网格项
    /// </summary>
    public class DataGridItem
    {
        public string Protocol { get; set; }
        public string Name { get; set; }
        public object Value { get; set; }
        public string DataType { get; set; }
        public string Timestamp { get; set; }
        public string Quality { get; set; }
    }
    
    /// <summary>
    /// 协议信息
    /// </summary>
    public class ProtocolInfo
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
    }
    
    /// <summary>
    /// 节点信息
    /// </summary>
    public class NodeInfo
    {
        public string NodeId { get; set; }
        public string DisplayName { get; set; }
        public string DataType { get; set; }
        public string Value { get; set; }
        public string Timestamp { get; set; }
    }
}