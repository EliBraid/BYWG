using BYWG.Client.Core;
using BYWG.Contracts;
using BYWGLib;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Microsoft.VisualBasic;

namespace BYWG.Client
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ClientServiceProxy _clientServiceProxy;
        public ClientServiceProxy ClientServiceProxy => _clientServiceProxy;
        private readonly ILogger<MainWindow> _logger;

        private List<BYWG.Contracts.ProtocolInfo> _protocols = new List<BYWG.Contracts.ProtocolInfo>();
        private List<NodeInfo> _nodes = new List<NodeInfo>();
        private List<DeviceInfo> _devices = new List<DeviceInfo>();
        private List<DeviceStatus> _deviceStatuses = new List<DeviceStatus>();
        private readonly System.Windows.Threading.DispatcherTimer _nodeRefreshTimer = new System.Windows.Threading.DispatcherTimer();
        private readonly Dictionary<string, (string deviceName, string address, string dataType)> _nodeBindings = new();

        public MainWindow(ClientServiceProxy clientServiceProxy, ILogger<MainWindow> logger = null)
        {
            InitializeComponent();
            _clientServiceProxy = clientServiceProxy;
            _logger = logger;

            // 设置数据绑定
            ProtocolDataGrid.ItemsSource = _protocols;
            NodeDataGrid.ItemsSource = _nodes;
            
            // 初始化设备管理
            InitializeDeviceManagement();

            // 注册事件
            _clientServiceProxy.ConnectionStatusChanged += OnConnectionStatusChanged;
            ProtocolDataGrid.SelectionChanged += ProtocolDataGrid_SelectionChanged;
            NodeDataGrid.SelectionChanged += NodeDataGrid_SelectionChanged;

            // 初始化状态
            UpdateUIState(false);

            // 初始化节点自动刷新计时器（默认1秒）
            _nodeRefreshTimer.Interval = TimeSpan.FromMilliseconds(1000);
            _nodeRefreshTimer.Tick += async (s, e) =>
            {
                if (_clientServiceProxy.IsConnected)
                {
                    // 先根据绑定读取设备值并写回OPC UA节点
                    foreach (var kv in _nodeBindings)
                    {
                        var nodeIdStr = kv.Key;
                        var (deviceName, address, dataType) = kv.Value;
                        try
                        {
                            var read = await _clientServiceProxy.TestReadAsync(deviceName, address, dataType);
                            if (read.Success && read.Data != null)
                            {
                                var value = read.Data;
                                var nodeId = new NodeId { Identifier = nodeIdStr, NamespaceIndex = 1 };
                                await _clientServiceProxy.WriteNodeAsync(nodeId, value);
                            }
                        }
                        catch { /* 忽略单点错误，继续循环 */ }
                    }

                    // 再刷新节点列表到UI
                    await RefreshNodesAsync();
                }
            };
        }

        private void OnConnectionStatusChanged(object sender, bool isConnected)
        {
            Dispatcher.Invoke(() =>
            {
                ConnectionStatusLabel.Content = isConnected ? "已连接" : "未连接";
                ConnectionStatusLabel.Foreground = isConnected ? Brushes.Green : Brushes.Red;
                ConnectButton.IsEnabled = !isConnected;
                DisconnectButton.IsEnabled = isConnected;
                ServerAddressTextBox.IsEnabled = !isConnected;

                UpdateUIState(isConnected);

                if (isConnected)
                {
                    // 连接成功后刷新数据
                    RefreshServerStatusAsync();
                    RefreshProtocolsAsync();
                    RefreshNodesAsync();

                    if (!_nodeRefreshTimer.IsEnabled)
                        _nodeRefreshTimer.Start();
                }
                else
                {
                    if (_nodeRefreshTimer.IsEnabled)
                        _nodeRefreshTimer.Stop();
                }
            });
        }

        private void UpdateUIState(bool isConnected)
        {
            // 更新OPC UA服务器控制按钮状态
            StartOpcUaServerButton.IsEnabled = isConnected;
            StopOpcUaServerButton.IsEnabled = isConnected && ServerStatusLabel.Content.ToString() == "运行中";

            // 更新协议管理按钮状态
            RefreshProtocolsButton.IsEnabled = isConnected;
            DeleteProtocolButton.IsEnabled = isConnected && ProtocolDataGrid.SelectedItem != null;
            StartProtocolButton.IsEnabled = isConnected && ProtocolDataGrid.SelectedItem != null;
            StopProtocolButton.IsEnabled = isConnected && ProtocolDataGrid.SelectedItem != null;

            // 更新节点管理按钮状态
            AddNodeButton.IsEnabled = isConnected;
            RefreshNodesButton.IsEnabled = isConnected;
            DeleteNodeButton.IsEnabled = isConnected && NodeDataGrid.SelectedItem != null;
            ReadWriteNodeButton.IsEnabled = isConnected && NodeDataGrid.SelectedItem != null;
        }

        private void ProtocolDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool hasSelection = ProtocolDataGrid.SelectedItem != null;
            DeleteProtocolButton.IsEnabled = hasSelection && _clientServiceProxy.IsConnected;
            StartProtocolButton.IsEnabled = hasSelection && _clientServiceProxy.IsConnected;
            StopProtocolButton.IsEnabled = hasSelection && _clientServiceProxy.IsConnected;
        }

        private void NodeDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool hasSelection = NodeDataGrid.SelectedItem != null;
            DeleteNodeButton.IsEnabled = hasSelection && _clientServiceProxy.IsConnected;
            ReadWriteNodeButton.IsEnabled = hasSelection && _clientServiceProxy.IsConnected;
        }

        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            string serverAddress = ServerAddressTextBox.Text.Trim();
            if (string.IsNullOrEmpty(serverAddress))
            {
                MessageBox.Show("请输入服务地址", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                ConnectButton.IsEnabled = false;
                var result = await _clientServiceProxy.ConnectAsync(serverAddress);
                if (!result)
                {
                    MessageBox.Show("连接失败，请检查服务地址是否正确", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    ConnectButton.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "连接服务端失败");
                MessageBox.Show("连接服务端失败: " + ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                ConnectButton.IsEnabled = true;
            }
        }

        private void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _clientServiceProxy.Disconnect();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "断开连接失败");
                MessageBox.Show("断开连接失败: " + ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void StartOpcUaServerButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var result = await _clientServiceProxy.StartOpcUaServerAsync();
                if (result.Success)
                {
                    MessageBox.Show("OPC UA服务器启动成功", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                    await RefreshServerStatusAsync();
                }
                else
                {
                    MessageBox.Show("OPC UA服务器启动失败: " + result.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "启动OPC UA服务器失败");
                MessageBox.Show("启动OPC UA服务器失败: " + ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void StopOpcUaServerButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var result = await _clientServiceProxy.StopOpcUaServerAsync();
                if (result.Success)
                {
                    MessageBox.Show("OPC UA服务器停止成功", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                    await RefreshServerStatusAsync();
                }
                else
                {
                    MessageBox.Show("OPC UA服务器停止失败: " + result.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "停止OPC UA服务器失败");
                MessageBox.Show("停止OPC UA服务器失败: " + ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void AddProtocolButton_Click(object sender, RoutedEventArgs e)
        {
            var configWindow = new ProtocolConfigWindow();
            if (configWindow.ShowDialog() == true)
            {
                try
                {
                    var result = await _clientServiceProxy.AddProtocolAsync(
                        configWindow.ProtocolName,
                        configWindow.ProtocolType,
                        configWindow.ConnectionString,
                        configWindow.Parameters);

                    if (result.Success)
                    {
                        MessageBox.Show("协议添加成功", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                        await RefreshProtocolsAsync();
                    }
                    else
                    {
                        MessageBox.Show("协议添加失败: " + result.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "添加协议失败");
                    MessageBox.Show("添加协议失败: " + ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void DeleteProtocolButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProtocolDataGrid.SelectedItem is ProtocolInfo selectedProtocol)
            {
                if (MessageBox.Show($"确定要删除协议 '{selectedProtocol.Name}' 吗？", "确认", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    try
                    {
                        var result = await _clientServiceProxy.RemoveProtocolAsync(selectedProtocol.Name);
                        if (result.Success)
                        {
                            MessageBox.Show("协议删除成功", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                            await RefreshProtocolsAsync();
                        }
                        else
                        {
                            MessageBox.Show("协议删除失败: " + result.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "删除协议失败");
                        MessageBox.Show("删除协议失败: " + ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private async void StartProtocolButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProtocolDataGrid.SelectedItem is ProtocolInfo selectedProtocol)
            {
                try
                {
                    var name = (selectedProtocol.Name ?? string.Empty).Trim();
                    if (string.IsNullOrEmpty(name))
                    {
                        MessageBox.Show("协议名称为空，无法启动", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    var result = await _clientServiceProxy.StartProtocolAsync(name);
                    if (result.Success)
                    {
                        MessageBox.Show("协议启动成功", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                        await RefreshProtocolsAsync();
                    }
                    else
                    {
                        MessageBox.Show("协议启动失败: " + result.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "启动协议失败");
                    MessageBox.Show("启动协议失败: " + ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void StopProtocolButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProtocolDataGrid.SelectedItem is ProtocolInfo selectedProtocol)
            {
                try
                {
                    var result = await _clientServiceProxy.StopProtocolAsync(selectedProtocol.Name);
                    if (result.Success)
                    {
                        MessageBox.Show("协议停止成功", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                        await RefreshProtocolsAsync();
                    }
                    else
                    {
                        MessageBox.Show("协议停止失败: " + result.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "停止协议失败");
                    MessageBox.Show("停止协议失败: " + ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void AddNodeButton_Click(object sender, RoutedEventArgs e)
        {
            // 打开专用添加节点对话框，而不是节点管理窗口
            var dlg = new NodeConfigWindow();
            // 将当前协议列表传入以供选择设备（通过对话框的公有方法）
            dlg.SetAvailableDevices(_protocols);

            // 如果当前有选中的协议，作为默认设备
            if (ProtocolDataGrid.SelectedItem is ProtocolInfo selected)
            {
                dlg.SetDefaultDevice(selected);
            }

            if (dlg.ShowDialog() == true)
            {
                // 继续沿用现有添加逻辑
                var value = new DataValue();
                if (!string.IsNullOrEmpty(dlg.InitialValue))
                {
                    switch (dlg.DataType)
                    {
                        case DataType.Boolean:
                            if (bool.TryParse(dlg.InitialValue, out var b)) value.BooleanValue = b; break;
                        case DataType.Int16:
                            if (short.TryParse(dlg.InitialValue, out var s)) value.Int32Value = s; break;
                        case DataType.Int32:
                            if (int.TryParse(dlg.InitialValue, out var i)) value.Int32Value = i; break;
                        case DataType.Float:
                            if (float.TryParse(dlg.InitialValue, out var f)) value.FloatValue = f; break;
                        case DataType.Double:
                            if (double.TryParse(dlg.InitialValue, out var d)) value.DoubleValue = d; break;
                        case DataType.String:
                            value.StringValue = dlg.InitialValue; break;
                    }
                }

                var add = await _clientServiceProxy.AddNodeAsync(dlg.NodeId, dlg.DisplayName, dlg.DataType, value);
                if (add.Success)
                {
                    // 记录绑定：OPC UA节点 -> 设备实时地址（用于定时刷新写回）
                    _nodeBindings[dlg.NodeId] = (deviceName: dlg.SelectedDeviceName,
                                                  address: dlg.AddressText,
                                                  dataType: dlg.DataTypeDisplay);
                    await RefreshNodesAsync();
                }
                else
                {
                    MessageBox.Show($"添加节点失败: {add.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DeleteNodeButton_Click(object sender, RoutedEventArgs e)
        {
            // 统一在节点管理窗口中进行节点维护
            OpenNodeManagerButton_Click(sender, e);
        }

        private void ReadWriteNodeButton_Click(object sender, RoutedEventArgs e)
        {
            // 统一在节点管理窗口中进行节点维护
            OpenNodeManagerButton_Click(sender, e);
        }

        private async Task RefreshServerStatusAsync()
        {
            if (!_clientServiceProxy.IsConnected)
                return;

            try
            {
                var status = await _clientServiceProxy.GetServerStatusAsync();
                ServerStatusLabel.Content = status.IsRunning ? "运行中" : "已停止";
                ServerStatusLabel.Foreground = status.IsRunning ? Brushes.Green : Brushes.Red;
                StopOpcUaServerButton.IsEnabled = status.IsRunning;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "刷新服务器状态失败");
            }
        }

        private async void RefreshProtocolsButton_Click(object sender, RoutedEventArgs e)
        {
            await RefreshProtocolsAsync();
        }

        private async Task RefreshProtocolsAsync()
        {
            if (!_clientServiceProxy.IsConnected)
                return;

            try
            {
                var protocols = await _clientServiceProxy.GetProtocolsAsync();
                _protocols.Clear();
                _protocols.AddRange(protocols);
                ProtocolDataGrid.Items.Refresh();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "刷新协议列表失败");
                MessageBox.Show("刷新协议列表失败: " + ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void RefreshNodesButton_Click(object sender, RoutedEventArgs e)
        {
            await RefreshNodesAsync();
        }

        private async Task RefreshNodesAsync()
        {
            if (!_clientServiceProxy.IsConnected)
                return;

            try
            {
                var nodes = await _clientServiceProxy.GetNodesAsync();
                _nodes.Clear();
                // 映射当前值与时间戳字段，便于表格直接展示
                foreach (var n in nodes)
                {
                    // 将 DataValue 解读为字符串与时间戳，放入扩展字段（通过匿名类型绑定）
                }
                _nodes.AddRange(nodes);
                NodeDataGrid.Items.Refresh();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "刷新节点列表失败");
                MessageBox.Show("刷新节点列表失败: " + ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        /// <summary>
        /// 初始化设备管理
        /// </summary>
        private void InitializeDeviceManagement()
        {
            try
            {
                // 加载设备列表
                LoadDevices();
                
                // 启动设备状态监控
                StartDeviceStatusMonitoring();
                
                _logger?.LogInformation("设备管理初始化完成");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "设备管理初始化失败");
                MessageBox.Show($"设备管理初始化失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        /// <summary>
        /// 加载设备列表
        /// </summary>
        private async void LoadDevices()
        {
            try
            {
                // 这里应该从服务端获取设备列表
                // 示例数据
                _devices.Clear();
                _devices.AddRange(new[]
                {
                    new DeviceInfo
                    {
                        DeviceId = "PLC_001",
                        DeviceName = "主控PLC",
                        DeviceType = "Siemens S7",
                        IpAddress = "192.168.1.100",
                        Port = 102,
                        Description = "主控PLC设备"
                    },
                    new DeviceInfo
                    {
                        DeviceId = "PLC_002",
                        DeviceName = "从控PLC",
                        DeviceType = "Mitsubishi MC",
                        IpAddress = "192.168.1.101",
                        Port = 5007,
                        Description = "从控PLC设备"
                    },
                    new DeviceInfo
                    {
                        DeviceId = "PLC_003",
                        DeviceName = "Modbus设备",
                        DeviceType = "Modbus TCP",
                        IpAddress = "192.168.1.102",
                        Port = 502,
                        Description = "Modbus TCP设备"
                    }
                });
                
                // 更新UI
                Dispatcher.Invoke(() =>
                {
                    // 这里需要更新设备列表的UI控件
                    UpdateDeviceListUI();
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "加载设备列表失败");
            }
        }
        
        /// <summary>
        /// 启动设备状态监控
        /// </summary>
        private void StartDeviceStatusMonitoring()
        {
            // 启动定时器监控设备状态
            var timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(5);
            timer.Tick += (s, e) => UpdateDeviceStatuses();
            timer.Start();
        }
        
        /// <summary>
        /// 更新设备状态
        /// </summary>
        private async void UpdateDeviceStatuses()
        {
            try
            {
                // 这里应该从服务端获取设备状态
                // 示例：模拟设备状态更新
                _deviceStatuses.Clear();
                foreach (var device in _devices)
                {
                    _deviceStatuses.Add(new DeviceStatus
                    {
                        DeviceId = device.DeviceId,
                        IsOnline = new Random().Next(0, 2) == 1, // 模拟在线状态
                        LastSeen = DateTime.Now.AddSeconds(-new Random().Next(0, 300)),
                        DataCount = new Random().Next(0, 1000),
                        ErrorCount = new Random().Next(0, 10)
                    });
                }
                
                // 更新UI
                Dispatcher.Invoke(() =>
                {
                    UpdateDeviceStatusUI();
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "更新设备状态失败");
            }
        }
        
        /// <summary>
        /// 更新设备列表UI
        /// </summary>
        private void UpdateDeviceListUI()
        {
            // 这里需要更新设备列表的UI控件
            // 例如：DeviceDataGrid.ItemsSource = _devices;
        }
        
        /// <summary>
        /// 更新设备状态UI
        /// </summary>
        private void UpdateDeviceStatusUI()
        {
            // 这里需要更新设备状态的UI控件
            // 例如：DeviceStatusDataGrid.ItemsSource = _deviceStatuses;
        }
        
        /// <summary>
        /// 打开数据仪表板
        /// </summary>
        private void OpenDataDashboard()
        {
            try
            {
                var dashboard = new Views.DataDashboard();
                dashboard.Show();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "打开数据仪表板失败");
                MessageBox.Show($"打开数据仪表板失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenNodeManagerButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var win = new Views.NodeManagerWindow(_clientServiceProxy) { Owner = this };
                win.Show();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "打开节点管理失败");
                MessageBox.Show($"打开节点管理失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        /// <summary>
        /// 打开协议配置向导
        /// </summary>
        private void OpenProtocolConfigurationWizard()
        {
            try
            {
                var wizard = new Views.ProtocolConfigurationWizard();
                if (wizard.ShowDialog() == true)
                {
                    // 添加协议到管理器
                    AddProtocolFromWizard(wizard.ProtocolConfig);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "打开协议配置向导失败");
                MessageBox.Show($"打开协议配置向导失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        /// <summary>
        /// 从向导添加协议
        /// </summary>
        private async void AddProtocolFromWizard(IndustrialProtocolConfig config)
        {
            try
            {
                if (!_clientServiceProxy.IsConnected)
                {
                    MessageBox.Show("请先连接到服务端", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // 调用服务端添加协议（仅添加，不启动）
                var addResult = await _clientServiceProxy.AddProtocolAsync(
                    (config.Name ?? string.Empty).Trim(),
                    GetProtocolTypeFromString(config.Type),
                    config.ConnectionString ?? string.Empty,
                    config.Parameters);

                if (addResult.Success)
                {
                    // 从服务端刷新协议列表，确保与服务端一致
                    await RefreshProtocolsAsync();

                    // 启用相关按钮
                    DeleteProtocolButton.IsEnabled = true;
                    StartProtocolButton.IsEnabled = true;
                    StopProtocolButton.IsEnabled = true;

                    MessageBox.Show($"协议 '{config.Name}' 已添加到服务端。", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show($"添加协议失败: {addResult.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "添加协议失败");
                MessageBox.Show($"添加协议失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        /// <summary>
        /// 打开快速连接对话框
        /// </summary>
        private void OpenQuickConnectionDialog()
        {
            try
            {
                var dialog = new Views.QuickConnectionDialog();
                if (dialog.ShowDialog() == true)
                {
                    // 添加协议到管理器
                    AddProtocolFromWizard(dialog.ProtocolConfig);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "打开快速连接对话框失败");
                MessageBox.Show($"打开快速连接对话框失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        /// <summary>
        /// 快速连接按钮点击事件
        /// </summary>
        private void QuickConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            OpenQuickConnectionDialog();
        }
        
        /// <summary>
        /// 协议配置向导按钮点击事件
        /// </summary>
        private void ProtocolWizardButton_Click(object sender, RoutedEventArgs e)
        {
            OpenProtocolConfigurationWizard();
        }
        
        /// <summary>
        /// 数据仪表板按钮点击事件
        /// </summary>
        private void DataDashboardButton_Click(object sender, RoutedEventArgs e)
        {
            OpenDataDashboard();
        }
        
        /// <summary>
        /// 从字符串获取协议类型
        /// </summary>
        private BYWG.Contracts.ProtocolType GetProtocolTypeFromString(string typeString)
        {
            switch (typeString?.ToUpper())
            {
                case "MODBUS":
                case "MODBUS_TCP":
                case "MODBUSTCP":
                    return BYWG.Contracts.ProtocolType.Modbus;
                case "S7":
                case "SIEMENS":
                case "SIEMENS_S7":
                case "SIEMENSS7":
                    return BYWG.Contracts.ProtocolType.Siemens;
                case "MC":
                case "MITSUBISHI":
                case "MITSUBISHI_MC":
                case "MITSUBISHIMC":
                    return BYWG.Contracts.ProtocolType.Mitsubishi;
                case "OMRON":
                    return BYWG.Contracts.ProtocolType.Omron;
                case "AB":
                case "ALLEN_BRADLEY":
                    return BYWG.Contracts.ProtocolType.Ab;
                default:
                    return BYWG.Contracts.ProtocolType.Unspecified;
            }
        }
    }
    
    /// <summary>
    /// 设备信息
    /// </summary>
    public class DeviceInfo
    {
        public string DeviceId { get; set; }
        public string DeviceName { get; set; }
        public string DeviceType { get; set; }
        public string IpAddress { get; set; }
        public int Port { get; set; }
        public string Description { get; set; }
        public DateTime CreatedTime { get; set; } = DateTime.Now;
    }
    
    /// <summary>
    /// 设备状态
    /// </summary>
    public class DeviceStatus
    {
        public string DeviceId { get; set; }
        public bool IsOnline { get; set; }
        public DateTime LastSeen { get; set; }
        public int DataCount { get; set; }
        public int ErrorCount { get; set; }
        public string LastError { get; set; }
    }
    
}