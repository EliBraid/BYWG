using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace BYWG
{
    /// <summary>
    /// 添加节点对话框（简化版本）
    /// </summary>
    public partial class AddNodeDialog : Window
    {
        private readonly List<ProtocolInfo> _devices;
        public Func<string, string, string, string, (bool ok, object value, string message)> TestReadHandler { get; set; }

        public AddNodeDialog(IEnumerable<ProtocolInfo> availableDevices = null)
        {
            InitializeComponent();
            _devices = availableDevices?.ToList() ?? new List<ProtocolInfo>();
            InitializeDevices();
        }
        
        public NodeInfo GetNodeInfo()
        {
            var selectedDataType = ((ComboBoxItem)dataTypeComboBox.SelectedItem).Content.ToString();
            var selectedDevice = deviceComboBox.SelectedItem as ProtocolInfo;
            var selectedAddressType = addressTypeComboBox.SelectedItem as ComboBoxItem;

            return new NodeInfo
            {
                NodeId = nodeIdTextBox.Text,
                DisplayName = displayNameTextBox.Text,
                DataType = selectedDataType,
                Value = initialValueTextBox.Text,
                Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };
        }

        private void InitializeDevices()
        {
            deviceComboBox.ItemsSource = _devices;
            deviceComboBox.DisplayMemberPath = nameof(ProtocolInfo.Name);
            if (_devices.Count > 0)
            {
                deviceComboBox.SelectedIndex = 0;
            }
        }

        private void PopulateAddressTypesForProtocol(string protocolType)
        {
            addressTypeComboBox.Items.Clear();
            switch (protocolType)
            {
                case "ModbusTCP":
                case "ModbusRTU":
                    addressTypeComboBox.Items.Add(new ComboBoxItem { Content = "Holding Register (4x)" });
                    addressTypeComboBox.Items.Add(new ComboBoxItem { Content = "Input Register (3x)" });
                    addressTypeComboBox.Items.Add(new ComboBoxItem { Content = "Coil (0x)" });
                    addressTypeComboBox.Items.Add(new ComboBoxItem { Content = "Discrete Input (1x)" });
                    break;
                case "SiemensS7":
                    addressTypeComboBox.Items.Add(new ComboBoxItem { Content = "DB (DBx.DBW/DBD/DBX)" });
                    addressTypeComboBox.Items.Add(new ComboBoxItem { Content = "M (Merker)" });
                    addressTypeComboBox.Items.Add(new ComboBoxItem { Content = "I (Input)" });
                    addressTypeComboBox.Items.Add(new ComboBoxItem { Content = "Q (Output)" });
                    break;
                case "Mitsubishi":
                    addressTypeComboBox.Items.Add(new ComboBoxItem { Content = "D (Data Register)" });
                    addressTypeComboBox.Items.Add(new ComboBoxItem { Content = "M (Internal Relay)" });
                    addressTypeComboBox.Items.Add(new ComboBoxItem { Content = "X (Input)" });
                    addressTypeComboBox.Items.Add(new ComboBoxItem { Content = "Y (Output)" });
                    break;
                default:
                    addressTypeComboBox.Items.Add(new ComboBoxItem { Content = "通用地址" });
                    break;
            }
            addressTypeComboBox.SelectedIndex = 0;
        }

        private void DeviceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedDevice = deviceComboBox.SelectedItem as ProtocolInfo;
            if (selectedDevice == null)
            {
                protocolTextBox.Text = string.Empty;
                addressTypeComboBox.Items.Clear();
                return;
            }

            protocolTextBox.Text = selectedDevice.Type;
            PopulateAddressTypesForProtocol(selectedDevice.Type);
        }

        private void TestReadButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedDevice = deviceComboBox.SelectedItem as ProtocolInfo;
            var selectedAddressType = (addressTypeComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? string.Empty;
            var address = addressTextBox.Text?.Trim() ?? string.Empty;
            var dataType = ((ComboBoxItem)dataTypeComboBox.SelectedItem).Content.ToString();

            if (selectedDevice == null)
            {
                testReadResultText.Text = "请先选择设备";
                testReadResultText.Foreground = System.Windows.Media.Brushes.OrangeRed;
                return;
            }
            if (string.IsNullOrWhiteSpace(address))
            {
                testReadResultText.Text = "请输入设备地址";
                testReadResultText.Foreground = System.Windows.Media.Brushes.OrangeRed;
                return;
            }

            if (TestReadHandler == null)
            {
                testReadResultText.Text = "未配置测试读取处理器";
                testReadResultText.Foreground = System.Windows.Media.Brushes.OrangeRed;
                return;
            }

            try
            {
                var (ok, value, message) = TestReadHandler.Invoke(selectedDevice.Name, selectedDevice.Type, address, dataType);
                if (ok)
                {
                    testReadResultText.Text = $"读取成功: {value}";
                    testReadResultText.Foreground = System.Windows.Media.Brushes.Green;
                }
                else
                {
                    testReadResultText.Text = $"读取失败: {message}";
                    testReadResultText.Foreground = System.Windows.Media.Brushes.OrangeRed;
                }
            }
            catch (Exception ex)
            {
                testReadResultText.Text = $"读取异常: {ex.Message}";
                testReadResultText.Foreground = System.Windows.Media.Brushes.OrangeRed;
            }
        }
        
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
        
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
