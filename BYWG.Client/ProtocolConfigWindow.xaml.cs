using BYWG.Contracts;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace BYWG.Client
{
    /// <summary>
    /// ProtocolConfigWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ProtocolConfigWindow : Window
    {
        public string ProtocolName => ProtocolNameTextBox.Text.Trim();
        public ProtocolType ProtocolType => (ProtocolType)ProtocolTypeComboBox.SelectedItem;
        public string ConnectionString => ConnectionStringTextBox.Text.Trim();
        public Dictionary<string, string> Parameters => GetParameters();

        public ProtocolConfigWindow()
        {
            InitializeComponent();
            InitializeProtocolTypes();
            InitializeParametersGrid();
        }

        private void InitializeProtocolTypes()
        {
            // 获取ProtocolType枚举的所有值并添加到下拉列表
            var protocolTypes = System.Enum.GetValues(typeof(ProtocolType)).Cast<ProtocolType>();
            ProtocolTypeComboBox.ItemsSource = protocolTypes;
            if (protocolTypes.Any())
            {
                ProtocolTypeComboBox.SelectedIndex = 0;
            }
        }

        private void InitializeParametersGrid()
        {
            // 创建一个空的字典集合作为参数网格的数据源
            var parametersList = new List<KeyValuePair<string, string>>();
            ParametersDataGrid.ItemsSource = parametersList;
        }

        private Dictionary<string, string> GetParameters()
        {
            var parameters = new Dictionary<string, string>();
            
            if (ParametersDataGrid.ItemsSource is List<KeyValuePair<string, string>> items)
            {
                foreach (var item in items)
                {
                    if (!string.IsNullOrEmpty(item.Key))
                    {
                        parameters[item.Key] = item.Value ?? string.Empty;
                    }
                }
            }

            return parameters;
        }

        private bool ValidateInput()
        {
            // 验证协议名称
            if (string.IsNullOrEmpty(ProtocolName))
            {
                MessageBox.Show("请输入协议名称", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                ProtocolNameTextBox.Focus();
                return false;
            }

            // 验证协议类型
            if (ProtocolTypeComboBox.SelectedItem == null)
            {
                MessageBox.Show("请选择协议类型", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                ProtocolTypeComboBox.Focus();
                return false;
            }

            // 验证连接字符串
            if (string.IsNullOrEmpty(ConnectionString))
            {
                MessageBox.Show("请输入连接字符串", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                ConnectionStringTextBox.Focus();
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