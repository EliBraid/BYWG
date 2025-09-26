using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using BYWGLib;

namespace BYWG.Client.Views
{
    /// <summary>
    /// ProtocolConfigurationWizard.xaml 的交互逻辑
    /// </summary>
    public partial class ProtocolConfigurationWizard : Window
    {
        private int _currentStep = 1;
        private string _selectedProtocolType = "";
        private IndustrialProtocolConfig _protocolConfig;
        
        public IndustrialProtocolConfig ProtocolConfig => _protocolConfig;
        
        public ProtocolConfigurationWizard()
        {
            InitializeComponent();
            _protocolConfig = new IndustrialProtocolConfig();
            UpdateStepDisplay();
        }
        
        /// <summary>
        /// 协议类型选择事件
        /// </summary>
        private void ProtocolType_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton radioButton)
            {
                _selectedProtocolType = radioButton.Tag?.ToString() ?? "";
                UpdateStep2Content();
            }
        }
        
        /// <summary>
        /// 模板选择事件
        /// </summary>
        private void TemplateComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TemplateComboBox.SelectedItem is ComboBoxItem selectedItem)
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
                case "modbus_tcp_standard":
                    ModbusRadioButton.IsChecked = true;
                    _selectedProtocolType = "MODBUS_TCP";
                    ModbusNameTextBox.Text = "Modbus_TCP_Device";
                    ModbusIpTextBox.Text = "192.168.1.100";
                    ModbusPortTextBox.Text = "502";
                    ModbusSlaveTextBox.Text = "1";
                    break;
                    
                case "s7_1200":
                    S7RadioButton.IsChecked = true;
                    _selectedProtocolType = "S7";
                    S7NameTextBox.Text = "S7_1200_Device";
                    S7IpTextBox.Text = "192.168.1.100";
                    S7PortTextBox.Text = "102";
                    S7RackTextBox.Text = "0";
                    break;
                    
                case "s7_1500":
                    S7RadioButton.IsChecked = true;
                    _selectedProtocolType = "S7";
                    S7NameTextBox.Text = "S7_1500_Device";
                    S7IpTextBox.Text = "192.168.1.100";
                    S7PortTextBox.Text = "102";
                    S7RackTextBox.Text = "0";
                    break;
                    
                case "mitsubishi_qna3e":
                    MCRadioButton.IsChecked = true;
                    _selectedProtocolType = "MC";
                    MCNameTextBox.Text = "Mitsubishi_Qna3E_Device";
                    MCIpTextBox.Text = "192.168.1.100";
                    MCPortTextBox.Text = "5007";
                    MCProtocolTypeComboBox.SelectedIndex = 0; // Qna3E
                    MCNetworkTextBox.Text = "0";
                    break;
                    
                case "mitsubishi_fx3c":
                    MCRadioButton.IsChecked = true;
                    _selectedProtocolType = "MC";
                    MCNameTextBox.Text = "Mitsubishi_FX3C_Device";
                    MCIpTextBox.Text = "192.168.1.100";
                    MCPortTextBox.Text = "5007";
                    MCProtocolTypeComboBox.SelectedIndex = 1; // MC3C
                    MCNetworkTextBox.Text = "0";
                    break;
            }
            
            UpdateStep2Content();
        }
        
        /// <summary>
        /// 更新步骤2内容
        /// </summary>
        private void UpdateStep2Content()
        {
            // 隐藏所有配置面板
            ModbusConfigPanel.Visibility = Visibility.Collapsed;
            S7ConfigPanel.Visibility = Visibility.Collapsed;
            MCConfigPanel.Visibility = Visibility.Collapsed;
            
            // 显示对应的配置面板
            switch (_selectedProtocolType)
            {
                case "MODBUS_TCP":
                    ModbusConfigPanel.Visibility = Visibility.Visible;
                    break;
                case "S7":
                    S7ConfigPanel.Visibility = Visibility.Visible;
                    break;
                case "MC":
                    MCConfigPanel.Visibility = Visibility.Visible;
                    break;
            }
        }
        
        /// <summary>
        /// 添加数据点
        /// </summary>
        // 数据点添加已取消，统一到节点管理
        private void AddDataPoint_Click(object sender, RoutedEventArgs e) {}
        
        /// <summary>
        /// 清空数据点
        /// </summary>
        private void ClearDataPoints_Click(object sender, RoutedEventArgs e) {}
        
        /// <summary>
        /// 从模板加载数据点
        /// </summary>
        private void LoadDataPointTemplate_Click(object sender, RoutedEventArgs e)
        {
            // 数据点模板加载已取消
        }
        
        /// <summary>
        /// 上一步按钮
        /// </summary>
        private void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentStep > 1)
            {
                _currentStep--;
                UpdateStepDisplay();
            }
        }
        
        /// <summary>
        /// 下一步按钮
        /// </summary>
        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateCurrentStep())
            {
                if (_currentStep < 4)
                {
                    _currentStep++;
                    UpdateStepDisplay();
                }
            }
        }
        
        /// <summary>
        /// 完成按钮
        /// </summary>
        private void FinishButton_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateCurrentStep())
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
        /// 验证当前步骤
        /// </summary>
        private bool ValidateCurrentStep()
        {
            switch (_currentStep)
            {
                case 1:
                    if (string.IsNullOrEmpty(_selectedProtocolType))
                    {
                        MessageBox.Show("请选择协议类型", "验证失败", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return false;
                    }
                    return true;
                    
                case 2:
                    return ValidateConnectionParameters();
                    
                case 3:
                    // 不再强制要求在向导中配置数据点
                    return true;
                    
                case 4:
                    return true;
                    
                default:
                    return true;
            }
        }
        
        /// <summary>
        /// 验证连接参数
        /// </summary>
        private bool ValidateConnectionParameters()
        {
            switch (_selectedProtocolType)
            {
                case "MODBUS_TCP":
                    if (string.IsNullOrEmpty(ModbusNameTextBox.Text) || 
                        string.IsNullOrEmpty(ModbusIpTextBox.Text) || 
                        string.IsNullOrEmpty(ModbusPortTextBox.Text))
                    {
                        MessageBox.Show("请填写完整的Modbus连接参数", "验证失败", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return false;
                    }
                    break;
                    
                case "S7":
                    if (string.IsNullOrEmpty(S7NameTextBox.Text) || 
                        string.IsNullOrEmpty(S7IpTextBox.Text) || 
                        string.IsNullOrEmpty(S7PortTextBox.Text))
                    {
                        MessageBox.Show("请填写完整的S7连接参数", "验证失败", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return false;
                    }
                    break;
                    
                case "MC":
                    if (string.IsNullOrEmpty(MCNameTextBox.Text) || 
                        string.IsNullOrEmpty(MCIpTextBox.Text) || 
                        string.IsNullOrEmpty(MCPortTextBox.Text))
                    {
                        MessageBox.Show("请填写完整的MC连接参数", "验证失败", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return false;
                    }
                    break;
            }
            
            return true;
        }
        
        /// <summary>
        /// 更新步骤显示
        /// </summary>
        private void UpdateStepDisplay()
        {
            // 更新步骤导航
            UpdateStepNavigation();
            
            // 更新内容区域
            UpdateContentArea();
            
            // 更新按钮状态
            UpdateButtonStates();
        }
        
        /// <summary>
        /// 更新步骤导航
        /// </summary>
        private void UpdateStepNavigation()
        {
            // 重置所有步骤指示器
            Step1Indicator.Fill = new SolidColorBrush(Color.FromRgb(229, 231, 235));
            Step2Indicator.Fill = new SolidColorBrush(Color.FromRgb(229, 231, 235));
            Step3Indicator.Fill = new SolidColorBrush(Color.FromRgb(229, 231, 235));
            Step4Indicator.Fill = new SolidColorBrush(Color.FromRgb(229, 231, 235));
            
            // 设置当前步骤指示器
            switch (_currentStep)
            {
                case 1:
                    Step1Indicator.Fill = new SolidColorBrush(Color.FromRgb(59, 130, 246));
                    StepTitleText.Text = "步骤 1: 选择协议类型";
                    break;
                case 2:
                    Step2Indicator.Fill = new SolidColorBrush(Color.FromRgb(59, 130, 246));
                    StepTitleText.Text = "步骤 2: 配置连接参数";
                    break;
                case 3:
                    Step3Indicator.Fill = new SolidColorBrush(Color.FromRgb(59, 130, 246));
                    StepTitleText.Text = "步骤 3: 配置数据点";
                    break;
                case 4:
                    Step4Indicator.Fill = new SolidColorBrush(Color.FromRgb(59, 130, 246));
                    StepTitleText.Text = "步骤 4: 确认配置";
                    break;
            }
        }
        
        /// <summary>
        /// 更新内容区域
        /// </summary>
        private void UpdateContentArea()
        {
            // 隐藏所有步骤内容
            Step1Content.Visibility = Visibility.Collapsed;
            Step2Content.Visibility = Visibility.Collapsed;
            Step3Content.Visibility = Visibility.Collapsed;
            Step4Content.Visibility = Visibility.Collapsed;
            
            // 显示当前步骤内容
            switch (_currentStep)
            {
                case 1:
                    Step1Content.Visibility = Visibility.Visible;
                    break;
                case 2:
                    Step2Content.Visibility = Visibility.Visible;
                    UpdateStep2Content();
                    break;
                case 3:
                    Step3Content.Visibility = Visibility.Visible;
                    break;
                case 4:
                    Step4Content.Visibility = Visibility.Visible;
                    UpdateSummary();
                    break;
            }
        }
        
        /// <summary>
        /// 更新按钮状态
        /// </summary>
        private void UpdateButtonStates()
        {
            PreviousButton.IsEnabled = _currentStep > 1;
            NextButton.Visibility = _currentStep < 4 ? Visibility.Visible : Visibility.Collapsed;
            FinishButton.Visibility = _currentStep == 4 ? Visibility.Visible : Visibility.Collapsed;
        }
        
        /// <summary>
        /// 更新摘要信息
        /// </summary>
        private void UpdateSummary()
        {
            SummaryProtocolType.Text = GetProtocolTypeDisplayName(_selectedProtocolType);
            SummaryProtocolName.Text = GetProtocolName();
            SummaryConnection.Text = GetConnectionString();
            SummaryDataPoints.Text = "0";
            SummaryStatus.Text = "配置完成";
        }
        
        /// <summary>
        /// 获取协议类型显示名称
        /// </summary>
        private string GetProtocolTypeDisplayName(string protocolType)
        {
            return protocolType switch
            {
                "MODBUS_TCP" => "Modbus TCP/RTU",
                "S7" => "西门子 S7",
                "MC" => "三菱 MC",
                _ => protocolType
            };
        }
        
        /// <summary>
        /// 获取协议名称
        /// </summary>
        private string GetProtocolName()
        {
            return _selectedProtocolType switch
            {
                "MODBUS_TCP" => ModbusNameTextBox.Text,
                "S7" => S7NameTextBox.Text,
                "MC" => MCNameTextBox.Text,
                _ => ""
            };
        }
        
        /// <summary>
        /// 获取连接字符串
        /// </summary>
        private string GetConnectionString()
        {
            return _selectedProtocolType switch
            {
                "MODBUS_TCP" => $"{ModbusIpTextBox.Text}:{ModbusPortTextBox.Text} (从站:{ModbusSlaveTextBox.Text})",
                "S7" => $"{S7IpTextBox.Text}:{S7PortTextBox.Text} (机架:{S7RackTextBox.Text})",
                "MC" => $"{MCIpTextBox.Text}:{MCPortTextBox.Text} (网络:{MCNetworkTextBox.Text})",
                _ => ""
            };
        }
        
        /// <summary>
        /// 获取数据点数量
        /// </summary>
        private int GetDataPointCount()
        {
            // 数据点在节点管理中配置，这里返回0
            return 0;
        }
        
        /// <summary>
        /// 构建协议配置
        /// </summary>
        private void BuildProtocolConfig()
        {
            _protocolConfig.Name = GetProtocolName();
            _protocolConfig.Type = _selectedProtocolType;
            _protocolConfig.Parameters = new Dictionary<string, string>();
            
            switch (_selectedProtocolType)
            {
                case "MODBUS_TCP":
                    _protocolConfig.Parameters["IpAddress"] = ModbusIpTextBox.Text;
                    _protocolConfig.Parameters["Port"] = ModbusPortTextBox.Text;
                    _protocolConfig.Parameters["SlaveId"] = ModbusSlaveTextBox.Text;
                    break;
                    
                case "S7":
                    _protocolConfig.Parameters["IpAddress"] = S7IpTextBox.Text;
                    _protocolConfig.Parameters["Port"] = S7PortTextBox.Text;
                    _protocolConfig.Parameters["Rack"] = S7RackTextBox.Text;
                    break;
                    
                case "MC":
                    _protocolConfig.Parameters["IpAddress"] = MCIpTextBox.Text;
                    _protocolConfig.Parameters["Port"] = MCPortTextBox.Text;
                    _protocolConfig.Parameters["Network"] = MCNetworkTextBox.Text;
                    if (MCProtocolTypeComboBox.SelectedItem is ComboBoxItem selectedItem)
                    {
                        _protocolConfig.Parameters["ProtocolType"] = selectedItem.Tag?.ToString() ?? "Qna3E";
                    }
                    break;
            }
        }
    }
    
    /// <summary>
    /// 输入对话框
    /// </summary>
    public class InputDialog : Window
    {
        public string Answer { get; private set; }
        
        public InputDialog(string title, string prompt)
        {
            Title = title;
            Width = 400;
            Height = 150;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ResizeMode = ResizeMode.NoResize;
            
            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            
            var promptLabel = new TextBlock
            {
                Text = prompt,
                Margin = new Thickness(10),
                TextWrapping = TextWrapping.Wrap
            };
            Grid.SetRow(promptLabel, 0);
            
            var textBox = new TextBox
            {
                Name = "InputTextBox",
                Margin = new Thickness(10),
                Height = 25
            };
            Grid.SetRow(textBox, 1);
            
            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(10)
            };
            
            var okButton = new Button
            {
                Content = "确定",
                Width = 75,
                Height = 25,
                Margin = new Thickness(5, 0, 0, 0),
                IsDefault = true
            };
            okButton.Click += (s, e) =>
            {
                Answer = textBox.Text;
                DialogResult = true;
                Close();
            };
            
            var cancelButton = new Button
            {
                Content = "取消",
                Width = 75,
                Height = 25,
                Margin = new Thickness(5, 0, 0, 0),
                IsCancel = true
            };
            cancelButton.Click += (s, e) =>
            {
                DialogResult = false;
                Close();
            };
            
            buttonPanel.Children.Add(okButton);
            buttonPanel.Children.Add(cancelButton);
            Grid.SetRow(buttonPanel, 2);
            
            grid.Children.Add(promptLabel);
            grid.Children.Add(textBox);
            grid.Children.Add(buttonPanel);
            
            Content = grid;
            
            textBox.Focus();
        }
    }
}
