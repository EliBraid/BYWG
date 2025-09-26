using BYWG.Contracts;
using System.Linq;
using System.Windows;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Collections.ObjectModel;

namespace BYWG.Client
{
    /// <summary>
    /// NodeConfigWindow.xaml 的交互逻辑
    /// </summary>
    public partial class NodeConfigWindow : Window
    {
        public string NodeId => NodeIdTextBox.Text.Trim();
        public string DisplayName => DisplayNameTextBox.Text.Trim();
        public DataType DataType => (DataType)DataTypeComboBox.SelectedItem;
        public string InitialValue => InitialValueTextBox.Text.Trim();
        public string AddressText => AddressTextBox.Text?.Trim() ?? string.Empty;
        public string SelectedDeviceName => (DeviceComboBox.SelectedItem as ProtocolInfo)?.Name ?? string.Empty;
        public string DataTypeDisplay => DataTypeComboBox.SelectedItem?.ToString() ?? string.Empty;

        private List<ProtocolInfo> _devices = new();
        private readonly ObservableCollection<KeyValuePair<string, string>> _deviceParams = new();

        public NodeConfigWindow()
        {
            InitializeComponent();
            InitializeDataTypes();
        }

        private void InitializeDataTypes()
        {
            // 获取DataType枚举的所有值并添加到下拉列表
            var dataTypes = System.Enum.GetValues(typeof(DataType)).Cast<DataType>();
            DataTypeComboBox.ItemsSource = dataTypes;
            if (dataTypes.Any())
            {
                DataTypeComboBox.SelectedIndex = 0;
            }
        }

        public void SetAvailableDevices(IEnumerable<ProtocolInfo> devices)
        {
            _devices = devices?.ToList() ?? new List<ProtocolInfo>();
            DeviceComboBox.ItemsSource = _devices;
            DeviceComboBox.DisplayMemberPath = nameof(ProtocolInfo.Name);
            if (_devices.Count > 0)
            {
                DeviceComboBox.SelectedIndex = 0;
            }
            DeviceParamsGrid.ItemsSource = _deviceParams;
        }

        public void SetDefaultDevice(ProtocolInfo device)
        {
            if (device == null) return;
            var idx = _devices.FindIndex(d => d.Name == device.Name);
            if (idx >= 0)
            {
                DeviceComboBox.SelectedIndex = idx;
            }
        }

        private void DeviceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var device = DeviceComboBox.SelectedItem as ProtocolInfo;
            if (device == null)
            {
                ProtocolTextBox.Text = string.Empty;
                AddressTypeComboBox.ItemsSource = null;
                _deviceParams.Clear();
                return;
            }
            ProtocolTextBox.Text = device.Type.ToString();
            PopulateAddressTypes(device.Type.ToString());

            // 展示设备参数（从服务端协议信息中的 parameters）
            _deviceParams.Clear();
            if (device.Parameters != null)
            {
                foreach (var kv in device.Parameters)
                {
                    _deviceParams.Add(new KeyValuePair<string, string>(kv.Key, kv.Value));
                }
            }
        }

        private void PopulateAddressTypes(string protocolType)
        {
            var items = new List<string>();
            var p = (protocolType ?? string.Empty).ToUpperInvariant();
            if (p.Contains("MODBUS"))
            {
                items.AddRange(new[] { "Holding Register (4x)", "Input Register (3x)", "Coil (0x)", "Discrete Input (1x)" });
            }
            else if (p.Contains("SIEMENS") || p == "S7" || p.Contains("SIEMENS_S7"))
            {
                items.AddRange(new[] { "DB (DBx.DBW/DBD/DBX)", "M (Merker)", "I (Input)", "Q (Output)" });
            }
            else if (p.Contains("MITSUBISHI") || p == "MC" || p.Contains("MITSUBISHI_MC"))
            {
                items.AddRange(new[] { "D (Data Register)", "M (Internal Relay)", "X (Input)", "Y (Output)" });
            }
            else
            {
                items.Add("通用地址");
            }
            AddressTypeComboBox.ItemsSource = items;
            if (items.Count > 0) AddressTypeComboBox.SelectedIndex = 0;

            UpdateAddressToolTip();
        }

        private void AddressTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateAddressToolTip();
        }

        private void UpdateAddressToolTip()
        {
            var proto = (ProtocolTextBox.Text ?? string.Empty).ToUpperInvariant();
            var addrType = AddressTypeComboBox.SelectedItem as string ?? string.Empty;
            string tip = "";
            if (proto.Contains("MODBUS"))
            {
                if (addrType.StartsWith("Holding")) tip = "示例: 40001 / 100 (寄存器地址)";
                else if (addrType.StartsWith("Input")) tip = "示例: 30001 / 100";
                else if (addrType.StartsWith("Coil")) tip = "示例: 0x0001 / 1";
                else if (addrType.StartsWith("Discrete")) tip = "示例: 1x0001 / 1";
                else tip = "示例: 40001, 30001, 0x0001, 1x0001";
            }
            else if (proto.Contains("SIEMENS") || proto == "S7" || proto.Contains("SIEMENS_S7"))
            {
                tip = "示例: DB1.DBW0 / DB1.DBD0 / DB1.DBX0.0 / M10.0 / I0.0 / Q0.0";
            }
            else if (proto.Contains("MITSUBISHI") || proto == "MC" || proto.Contains("MITSUBISHI_MC"))
            {
                tip = "示例: D4500 / M100 / X10 / Y10";
            }
            else
            {
                tip = "通用地址: 按协议规范填写";
            }
            AddressTextBox.ToolTip = tip;
        }

        private async void TestReadButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var device = DeviceComboBox.SelectedItem as ProtocolInfo;
                if (device == null)
                {
                    TestReadResultText.Text = "请先选择设备";
                    TestReadResultText.Foreground = System.Windows.Media.Brushes.OrangeRed;
                    return;
                }
                var address = AddressTextBox.Text?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(address))
                {
                    TestReadResultText.Text = "请输入设备地址";
                    TestReadResultText.Foreground = System.Windows.Media.Brushes.OrangeRed;
                    return;
                }

                // 通过主窗口的服务代理执行测试读取
                var main = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                if (main == null || main.ClientServiceProxy == null)
                {
                    TestReadResultText.Text = "无法获取服务代理，请先连接服务端";
                    TestReadResultText.Foreground = System.Windows.Media.Brushes.OrangeRed;
                    return;
                }

                var dataType = DataTypeComboBox.SelectedItem?.ToString() ?? string.Empty;
                var res = await main.ClientServiceProxy.TestReadAsync(device.Name, address, dataType);
                if (res.Success && res.Data != null)
                {
                    var v = res.Data.ValueCase switch
                    {
                        BYWG.Contracts.DataValue.ValueOneofCase.BooleanValue => res.Data.BooleanValue.ToString(),
                        BYWG.Contracts.DataValue.ValueOneofCase.Int32Value => res.Data.Int32Value.ToString(),
                        BYWG.Contracts.DataValue.ValueOneofCase.FloatValue => res.Data.FloatValue.ToString(),
                        BYWG.Contracts.DataValue.ValueOneofCase.DoubleValue => res.Data.DoubleValue.ToString(),
                        BYWG.Contracts.DataValue.ValueOneofCase.StringValue => res.Data.StringValue,
                        _ => string.Empty
                    };
                    TestReadResultText.Text = $"读取成功: {v}";
                    TestReadResultText.Foreground = System.Windows.Media.Brushes.Green;
                }
                else
                {
                    TestReadResultText.Text = $"读取失败: {res.Message}";
                    TestReadResultText.Foreground = System.Windows.Media.Brushes.OrangeRed;
                }
            }
            catch (Exception ex)
            {
                TestReadResultText.Text = $"读取异常: {ex.Message}";
                TestReadResultText.Foreground = System.Windows.Media.Brushes.OrangeRed;
            }
        }

        private bool ValidateInput()
        {
            // 验证节点ID
            if (string.IsNullOrEmpty(NodeId))
            {
                MessageBox.Show("请输入节点ID", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                NodeIdTextBox.Focus();
                return false;
            }

            // 验证显示名称
            if (string.IsNullOrEmpty(DisplayName))
            {
                MessageBox.Show("请输入显示名称", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                DisplayNameTextBox.Focus();
                return false;
            }

            // 验证数据类型
            if (DataTypeComboBox.SelectedItem == null)
            {
                MessageBox.Show("请选择数据类型", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                DataTypeComboBox.Focus();
                return false;
            }

            return true;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateInput())
            {
                DialogResult = true;
                Close();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}