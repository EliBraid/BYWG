using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace BYWG
{
    /// <summary>
    /// 添加协议对话框（修复版本）
    /// </summary>
    public partial class AddProtocolDialog : Window
    {
        public AddProtocolDialog()
        {
            InitializeComponent();
            InitializeProtocolTypes();
        }
        
        private void InitializeProtocolTypes()
        {
            // 添加支持的协议类型
            typeComboBox.Items.Add(new ComboBoxItem { Content = "Modbus TCP", Tag = "MODBUS_TCP" });
            typeComboBox.Items.Add(new ComboBoxItem { Content = "西门子 S7", Tag = "S7" });
            typeComboBox.Items.Add(new ComboBoxItem { Content = "三菱 MC", Tag = "MC" });
            
            // 默认选择第一个
            typeComboBox.SelectedIndex = 0;
        }
        
        public IndustrialProtocolConfig GetProtocolConfig()
        {
            var selectedItem = (ComboBoxItem)typeComboBox.SelectedItem;
            var protocolType = selectedItem.Tag.ToString();
            
            var config = new IndustrialProtocolConfig
            {
                Name = nameTextBox.Text,
                Type = protocolType,
                Enabled = true, // 默认启用
                ConnectionString = connectionTextBox.Text,
                Parameters = new Dictionary<string, string>()
            };
            
            // 根据协议类型设置默认参数
            switch (protocolType)
            {
                case "MODBUS_TCP":
                    config.Parameters["IpAddress"] = connectionTextBox.Text;
                    config.Parameters["Port"] = "502";
                    config.Parameters["SlaveId"] = "1";
                    config.Parameters["Timeout"] = "3000";
                    config.Parameters["DataPoints"] = "Tag1,100,Int16,3;Tag2,101,Float,3;Tag3,0,Bool,1";
                    break;
                    
                case "S7":
                    config.Parameters["IpAddress"] = connectionTextBox.Text;
                    config.Parameters["Port"] = "102";
                    config.Parameters["Rack"] = "0";
                    config.Parameters["Slot"] = "1";
                    config.Parameters["Timeout"] = "3000";
                    config.Parameters["DataPoints"] = "DB1.0:int32,DB1.4:float,DB1.8:bool";
                    break;
                    
                case "MC":
                    config.Parameters["IpAddress"] = connectionTextBox.Text;
                    config.Parameters["Port"] = "5007";
                    config.Parameters["NetworkNo"] = "0";
                    config.Parameters["PcNo"] = "255";
                    config.Parameters["Timeout"] = "3000";
                    config.Parameters["DataPoints"] = "D100:int16,D101:int16,D102:int16";
                    break;
            }
            
            return config;
        }
        
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            // 验证输入
            if (string.IsNullOrEmpty(nameTextBox.Text))
            {
                MessageBox.Show("请输入协议名称", "验证失败", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            if (string.IsNullOrEmpty(connectionTextBox.Text))
            {
                MessageBox.Show("请输入连接地址", "验证失败", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
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