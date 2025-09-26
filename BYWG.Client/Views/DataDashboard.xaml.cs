using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using BYWGLib;

namespace BYWG.Client.Views
{
    /// <summary>
    /// DataDashboard.xaml 的交互逻辑
    /// </summary>
    public partial class DataDashboard : Window
    {
        private readonly ObservableCollection<IndustrialDataItem> _dataItems;
        private readonly ObservableCollection<ProtocolInfo> _protocols;
        private readonly ObservableCollection<AlarmInfo> _alarms;
        private readonly DispatcherTimer _updateTimer;
        private readonly Random _random;
        
        public DataDashboard()
        {
            InitializeComponent();
            
            _dataItems = new ObservableCollection<IndustrialDataItem>();
            _protocols = new ObservableCollection<ProtocolInfo>();
            _alarms = new ObservableCollection<AlarmInfo>();
            
            _random = new Random();
            
            // 设置数据绑定
            DataGrid.ItemsSource = _dataItems;
            ProtocolListBox.ItemsSource = _protocols;
            AlarmListBox.ItemsSource = _alarms;
            
            // 启动更新定时器
            _updateTimer = new DispatcherTimer();
            _updateTimer.Interval = TimeSpan.FromSeconds(1);
            _updateTimer.Tick += UpdateTimer_Tick;
            _updateTimer.Start();
            
            // 初始化数据
            InitializeData();
        }
        
        /// <summary>
        /// 初始化数据
        /// </summary>
        private void InitializeData()
        {
            // 初始化协议数据
            _protocols.Add(new ProtocolInfo
            {
                Name = "MC_Protocol",
                Type = "Mitsubishi MC",
                IsRunning = true
            });
            _protocols.Add(new ProtocolInfo
            {
                Name = "Modbus_Protocol",
                Type = "Modbus TCP",
                IsRunning = true
            });
            _protocols.Add(new ProtocolInfo
            {
                Name = "S7_Protocol",
                Type = "Siemens S7",
                IsRunning = false
            });
            
            // 初始化设备状态
            UpdateDeviceStatus();
            
            // 初始化系统状态
            SystemStatusText.Text = "运行中";
            UpdateFrequencyText.Text = "1秒";
        }
        
        /// <summary>
        /// 更新定时器事件
        /// </summary>
        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                // 更新实时数据
                UpdateRealTimeData();
                
                // 更新设备状态
                UpdateDeviceStatus();
                
                // 更新图表
                UpdateTrendChart();
                
                // 更新状态信息
                UpdateStatusInfo();
                
                // 检查报警条件
                CheckAlarmConditions();
            }
            catch (Exception ex)
            {
                // 记录错误但不中断更新
                System.Diagnostics.Debug.WriteLine($"更新数据时出错: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 更新实时数据
        /// </summary>
        private void UpdateRealTimeData()
        {
            // 模拟数据更新
            var newData = GenerateSimulatedData();
            
            // 更新数据集合
            foreach (var item in newData)
            {
                var existingItem = _dataItems.FirstOrDefault(x => x.Id == item.Id);
                if (existingItem != null)
                {
                    existingItem.Value = item.Value;
                    existingItem.Quality = item.Quality;
                    existingItem.Timestamp = item.Timestamp;
                }
                else
                {
                    _dataItems.Add(item);
                }
            }
            
            // 保持数据量在合理范围内
            if (_dataItems.Count > 100)
            {
                var itemsToRemove = _dataItems.Take(_dataItems.Count - 100).ToList();
                foreach (var item in itemsToRemove)
                {
                    _dataItems.Remove(item);
                }
            }
        }
        
        /// <summary>
        /// 生成模拟数据
        /// </summary>
        private List<IndustrialDataItem> GenerateSimulatedData()
        {
            var data = new List<IndustrialDataItem>();
            var devices = new[] { "PLC_001", "PLC_002", "PLC_003" };
            var dataPoints = new[] { "D100", "D101", "M100", "M101", "X100", "Y100" };
            
            foreach (var device in devices)
            {
                foreach (var dataPoint in dataPoints)
                {
                    data.Add(new IndustrialDataItem
                    {
                        Id = $"{device}.{dataPoint}",
                        Name = dataPoint,
                        Value = _random.Next(0, 1000),
                        DataType = "word",
                        Quality = _random.Next(0, 10) > 1 ? Quality.Good : Quality.Bad,
                        Timestamp = DateTime.Now
                    });
                }
            }
            
            return data;
        }
        
        /// <summary>
        /// 更新设备状态
        /// </summary>
        private void UpdateDeviceStatus()
        {
            var onlineCount = _random.Next(2, 4);
            var totalCount = 3;
            var offlineCount = totalCount - onlineCount;
            
            OnlineCountText.Text = onlineCount.ToString();
            OfflineCountText.Text = offlineCount.ToString();
            TotalCountText.Text = totalCount.ToString();
        }
        
        /// <summary>
        /// 更新趋势图表
        /// </summary>
        private void UpdateTrendChart()
        {
            TrendChart.Children.Clear();
            
            if (_dataItems.Count == 0) return;
            
            // 获取最近的数据点
            var recentData = _dataItems
                .Where(x => x.Quality == Quality.Good)
                .OrderBy(x => x.Timestamp)
                .Take(20)
                .ToList();
            
            if (recentData.Count < 2) return;
            
            // 绘制趋势线
            var canvas = TrendChart;
            var width = canvas.ActualWidth;
            var height = canvas.ActualHeight;
            
            if (width <= 0 || height <= 0) return;
            
            var points = new List<Point>();
            var minValue = recentData.Min(x => Convert.ToDouble(x.Value));
            var maxValue = recentData.Max(x => Convert.ToDouble(x.Value));
            var valueRange = maxValue - minValue;
            
            if (valueRange == 0) return;
            
            for (int i = 0; i < recentData.Count; i++)
            {
                var x = (i * width) / (recentData.Count - 1);
                var y = height - ((Convert.ToDouble(recentData[i].Value) - minValue) * height / valueRange);
                points.Add(new Point(x, y));
            }
            
            // 绘制折线
            var polyline = new Polyline
            {
                Stroke = new SolidColorBrush(Colors.Blue),
                StrokeThickness = 2,
                Points = new PointCollection(points)
            };
            
            canvas.Children.Add(polyline);
            
            // 绘制数据点
            foreach (var point in points)
            {
                var ellipse = new Ellipse
                {
                    Width = 4,
                    Height = 4,
                    Fill = new SolidColorBrush(Colors.Red)
                };
                
                Canvas.SetLeft(ellipse, point.X - 2);
                Canvas.SetTop(ellipse, point.Y - 2);
                canvas.Children.Add(ellipse);
            }
        }
        
        /// <summary>
        /// 更新状态信息
        /// </summary>
        private void UpdateStatusInfo()
        {
            DataPointCountText.Text = _dataItems.Count.ToString();
            LastUpdateText.Text = DateTime.Now.ToString("HH:mm:ss");
        }
        
        /// <summary>
        /// 检查报警条件
        /// </summary>
        private void CheckAlarmConditions()
        {
            // 检查数据质量
            var badQualityCount = _dataItems.Count(x => x.Quality == Quality.Bad);
            if (badQualityCount > 5)
            {
                AddAlarm("数据质量异常", AlarmLevel.Warning);
            }
            
            // 检查设备离线
            var offlineCount = int.Parse(OfflineCountText.Text);
            if (offlineCount > 0)
            {
                AddAlarm($"{offlineCount}个设备离线", AlarmLevel.Error);
            }
            
            // 检查协议状态
            var stoppedProtocols = _protocols.Count(x => !x.IsRunning);
            if (stoppedProtocols > 0)
            {
                AddAlarm($"{stoppedProtocols}个协议停止", AlarmLevel.Warning);
            }
        }
        
        /// <summary>
        /// 添加报警
        /// </summary>
        private void AddAlarm(string message, AlarmLevel level)
        {
            var alarm = new AlarmInfo
            {
                Message = message,
                Level = level,
                Timestamp = DateTime.Now
            };
            
            _alarms.Insert(0, alarm);
            
            // 保持报警数量在合理范围内
            if (_alarms.Count > 50)
            {
                var alarmsToRemove = _alarms.Skip(50).ToList();
                foreach (var alarmToRemove in alarmsToRemove)
                {
                    _alarms.Remove(alarmToRemove);
                }
            }
        }
        
        /// <summary>
        /// 窗口关闭事件
        /// </summary>
        protected override void OnClosed(EventArgs e)
        {
            _updateTimer?.Stop();
            base.OnClosed(e);
        }
    }
    
    /// <summary>
    /// 协议信息
    /// </summary>
    public class ProtocolInfo
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public bool IsRunning { get; set; }
    }
    
    /// <summary>
    /// 报警信息
    /// </summary>
    public class AlarmInfo
    {
        public string Message { get; set; }
        public AlarmLevel Level { get; set; }
        public DateTime Timestamp { get; set; }
    }
    
    /// <summary>
    /// 报警级别
    /// </summary>
    public enum AlarmLevel
    {
        Info,
        Warning,
        Error,
        Critical
    }
}
