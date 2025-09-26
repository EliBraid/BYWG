using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using BYWGLib;

namespace BYWG.Client.Views
{
    /// <summary>
    /// QuickConnectionDialog.xaml 的交互逻辑
    /// </summary>
    public partial class QuickConnectionDialog : Window
    {
        public IndustrialProtocolConfig ProtocolConfig { get; private set; }
        
        public QuickConnectionDialog()
        {
            InitializeComponent();
            ProtocolConfig = new IndustrialProtocolConfig();
        }
        
        /// <summary>
        /// 模板选择事件
        /// </summary>
        private void TemplateListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TemplateListBox.SelectedItem is ListBoxItem selectedItem)
            {
                var template = selectedItem.Tag?.ToString();
                ApplyTemplate(template);
            }
        }
        
        /// <summary>
        /// 应用模板配置
        /// </summary>
        private void ApplyTemplate(string template)
        {
            switch (template)
            {
                case "modbus_tcp":
                    ProtocolTypeText.Text = "Modbus TCP";
                    DeviceNameTextBox.Text = "Modbus_Device";
                    IpAddressTextBox.Text = "192.168.6.6";
                    PortTextBox.Text = "502";
                    SlaveAddressLabel.Visibility = Visibility.Visible;
                    SlaveAddressTextBox.Visibility = Visibility.Visible;
                    SlaveAddressTextBox.Text = "1";
                    // 数据点不在快速连接中配置
                    break;
                    
                case "s7_1200":
                    ProtocolTypeText.Text = "西门子 S7-1200";
                    DeviceNameTextBox.Text = "S7_1200_Device";
                    IpAddressTextBox.Text = "192.168.1.100";
                    PortTextBox.Text = "102";
                    SlaveAddressLabel.Visibility = Visibility.Collapsed;
                    SlaveAddressTextBox.Visibility = Visibility.Collapsed;
                    // 数据点不在快速连接中配置
                    break;
                    
                case "s7_1500":
                    ProtocolTypeText.Text = "西门子 S7-1500";
                    DeviceNameTextBox.Text = "S7_1500_Device";
                    IpAddressTextBox.Text = "192.168.1.100";
                    PortTextBox.Text = "102";
                    SlaveAddressLabel.Visibility = Visibility.Collapsed;
                    SlaveAddressTextBox.Visibility = Visibility.Collapsed;
                    // 数据点不在快速连接中配置
                    break;
                    
                case "mitsubishi_qna3e":
                    ProtocolTypeText.Text = "三菱 Q系列 (Qna-3E)";
                    DeviceNameTextBox.Text = "Mitsubishi_Qna3E_Device";
                    IpAddressTextBox.Text = "192.168.1.100";
                    PortTextBox.Text = "5007";
                    SlaveAddressLabel.Visibility = Visibility.Collapsed;
                    SlaveAddressTextBox.Visibility = Visibility.Collapsed;
                    // 数据点不在快速连接中配置
                    break;
                    
                case "mitsubishi_fx3c":
                    ProtocolTypeText.Text = "三菱 FX系列 (3C)";
                    DeviceNameTextBox.Text = "Mitsubishi_FX3C_Device";
                    IpAddressTextBox.Text = "192.168.1.100";
                    PortTextBox.Text = "5007";
                    SlaveAddressLabel.Visibility = Visibility.Collapsed;
                    SlaveAddressTextBox.Visibility = Visibility.Collapsed;
                    // 数据点不在快速连接中配置
                    break;
            }
        }
        
        /// <summary>
        /// 测试连接
        /// </summary>
        private void TestConnection_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 这里应该实现实际的连接测试
                MessageBox.Show("连接测试功能待实现", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"连接测试失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        /// <summary>
        /// 高级配置
        /// </summary>
        private void AdvancedConfig_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var wizard = new ProtocolConfigurationWizard();
                if (wizard.ShowDialog() == true)
                {
                    ProtocolConfig = wizard.ProtocolConfig;
                    DialogResult = true;
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开高级配置失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        /// <summary>
        /// 确定按钮
        /// </summary>
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateInput())
            {
                BuildProtocolConfig();
                DialogResult = true;
                Close();
            }
        }
        
        /// <summary>
        /// 取消按钮
        /// </summary>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
        
        /// <summary>
        /// 验证输入
        /// </summary>
        private bool ValidateInput()
        {
            if (string.IsNullOrEmpty(DeviceNameTextBox.Text))
            {
                MessageBox.Show("请输入设备名称", "验证失败", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            
            if (string.IsNullOrEmpty(IpAddressTextBox.Text))
            {
                MessageBox.Show("请输入IP地址", "验证失败", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            
            if (string.IsNullOrEmpty(PortTextBox.Text))
            {
                MessageBox.Show("请输入端口号", "验证失败", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            
            // 不再要求在快速连接中配置数据点
            
            return true;
        }
        
        /// <summary>
        /// 构建协议配置
        /// </summary>
        private void BuildProtocolConfig()
        {
            var name = (DeviceNameTextBox.Text ?? string.Empty).Trim();
            var ip = (IpAddressTextBox.Text ?? string.Empty).Trim();
            var port = (PortTextBox.Text ?? string.Empty).Trim();
            // 数据点在节点管理中配置

            ProtocolConfig.Name = name;
            ProtocolConfig.ConnectionString = string.IsNullOrEmpty(ip) ? string.Empty : ($"{ip}:{port}");
            ProtocolConfig.Parameters = new Dictionary<string, string>
            {
                ["IpAddress"] = ip,
                ["Port"] = port
            };
            
            // 根据协议类型设置参数
            if (ProtocolTypeText.Text.Contains("Modbus"))
            {
                ProtocolConfig.Type = "MODBUS_TCP";
                var slave = SlaveAddressTextBox.Visibility == Visibility.Visible ? (SlaveAddressTextBox.Text ?? "1").Trim() : "1";
                ProtocolConfig.Parameters["SlaveId"] = string.IsNullOrEmpty(slave) ? "1" : slave;
                if (!ProtocolConfig.Parameters.ContainsKey("Timeout")) ProtocolConfig.Parameters["Timeout"] = "3000";
            }
            else if (ProtocolTypeText.Text.Contains("西门子"))
            {
                ProtocolConfig.Type = "S7";
                ProtocolConfig.Parameters["Rack"] = "0";
                ProtocolConfig.Parameters["Slot"] = "1";
                if (!ProtocolConfig.Parameters.ContainsKey("Timeout")) ProtocolConfig.Parameters["Timeout"] = "3000";
            }
            else if (ProtocolTypeText.Text.Contains("三菱"))
            {
                ProtocolConfig.Type = "MC";
                ProtocolConfig.Parameters["NetworkNo"] = "0";
                ProtocolConfig.Parameters["PcNo"] = "255";
                if (!ProtocolConfig.Parameters.ContainsKey("Timeout")) ProtocolConfig.Parameters["Timeout"] = "3000";
                if (ProtocolTypeText.Text.Contains("Qna-3E"))
                {
                    ProtocolConfig.Parameters["ProtocolType"] = "Qna3E";
                }
                else if (ProtocolTypeText.Text.Contains("3C"))
                {
                    ProtocolConfig.Parameters["ProtocolType"] = "MC3C";
                }
            }
        }
    }
}
