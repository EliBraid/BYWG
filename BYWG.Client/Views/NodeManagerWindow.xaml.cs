using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using BYWG.Client.Core;
using BYWG.Contracts;

namespace BYWG.Client.Views
{
    public partial class NodeManagerWindow : Window
    {
        private readonly ClientServiceProxy _client;
        private readonly DispatcherTimer _timer;
        private readonly ObservableCollection<NodeRow> _rows = new();

        public NodeManagerWindow(ClientServiceProxy client)
        {
            InitializeComponent();
            _client = client;
            _timer = new DispatcherTimer();
            _timer.Tick += Timer_Tick;
            NodesGrid.ItemsSource = _rows;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await RefreshAsync();
            ApplyInterval();
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await RefreshAsync();
        }

        private async System.Threading.Tasks.Task RefreshAsync()
        {
            var list = await _client.GetNodesAsync();
            _rows.Clear();
            foreach (var n in list)
            {
                _rows.Add(new NodeRow
                {
                    NodeId = n.NodeId,
                    NodeIdString = n.NodeId?.Identifier ?? string.Empty,
                    DisplayName = n.DisplayName,
                    DataType = n.DataType.ToString(),
                    LatestValue = FormatDataValue(n.CurrentValue),
                    Timestamp = n.CurrentValue != null && n.CurrentValue.Timestamp > 0 ?
                        DateTimeOffset.FromUnixTimeMilliseconds(n.CurrentValue.Timestamp).LocalDateTime.ToString("yyyy-MM-dd HH:mm:ss") : string.Empty
                });
            }
        }

        private async void ReadSelectedButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var row in NodesGrid.SelectedItems.OfType<NodeRow>())
            {
                var res = await _client.ReadNodeAsync(row.NodeId);
                if (res.Success)
                {
                    row.LatestValue = FormatDataValue(res.Data);
                    row.Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                }
            }
            NodesGrid.Items.Refresh();
        }

        private void ApplyIntervalButton_Click(object sender, RoutedEventArgs e)
        {
            ApplyInterval();
        }

        private void ApplyInterval()
        {
            if (int.TryParse(IntervalText.Text, out var ms) && ms >= 100)
            {
                _timer.Interval = TimeSpan.FromMilliseconds(ms);
            }
            if (AutoRefreshCheck.IsChecked == true)
            {
                _timer.Start();
            }
            else
            {
                _timer.Stop();
            }
        }

        private async void Timer_Tick(object? sender, EventArgs e)
        {
            foreach (var row in _rows)
            {
                var res = await _client.ReadNodeAsync(row.NodeId);
                if (res.Success)
                {
                    row.LatestValue = FormatDataValue(res.Data);
                    row.Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                }
            }
            NodesGrid.Items.Refresh();
        }

        private static string FormatDataValue(DataValue dv)
        {
            if (dv == null) return string.Empty;
            var v = dv.ValueCase switch
            {
                DataValue.ValueOneofCase.BooleanValue => dv.BooleanValue.ToString(),
                DataValue.ValueOneofCase.Int32Value => dv.Int32Value.ToString(),
                DataValue.ValueOneofCase.FloatValue => dv.FloatValue.ToString(),
                DataValue.ValueOneofCase.DoubleValue => dv.DoubleValue.ToString(),
                DataValue.ValueOneofCase.StringValue => dv.StringValue,
                _ => string.Empty
            };
            return v;
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new NodeConfigWindow { Owner = this };
            if (dlg.ShowDialog() == true)
            {
                var value = new DataValue();
                if (!string.IsNullOrEmpty(dlg.InitialValue))
                {
                    // 仅演示: 按类型尝试解析初始值
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

                var add = await _client.AddNodeAsync(dlg.NodeId, dlg.DisplayName, dlg.DataType, value);
                if (add.Success)
                {
                    await RefreshAsync();
                }
                else
                {
                    MessageBox.Show($"添加节点失败: {add.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (NodesGrid.SelectedItem is not NodeRow row) return;
            var dlg = new NodeConfigWindow { Owner = this };
            // 预填
            dlg.NodeIdTextBox.Text = row.NodeIdString;
            dlg.DisplayNameTextBox.Text = row.DisplayName;
            // 数据类型下拉选择
            var dt = Enum.TryParse<DataType>(row.DataType, out var parsed) ? parsed : DataType.String;
            dlg.DataTypeComboBox.SelectedItem = dt;
            if (dlg.ShowDialog() == true)
            {
                // 简化：删除旧节点，新建新节点（若服务端无编辑接口）
                await _client.RemoveNodeAsync(row.NodeId);
                var value = new DataValue();
                var add = await _client.AddNodeAsync(dlg.NodeId, dlg.DisplayName, dlg.DataType, value);
                if (add.Success)
                {
                    await RefreshAsync();
                }
                else
                {
                    MessageBox.Show($"编辑节点失败: {add.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            var rows = NodesGrid.SelectedItems.OfType<NodeRow>().ToList();
            if (rows.Count == 0) return;
            if (MessageBox.Show($"确认删除选中 {rows.Count} 个节点?", "确认", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                foreach (var r in rows)
                {
                    await _client.RemoveNodeAsync(r.NodeId);
                }
                await RefreshAsync();
            }
        }

        public class NodeRow
        {
            public NodeId NodeId { get; set; }
            public string NodeIdString { get; set; }
            public string DisplayName { get; set; }
            public string DataType { get; set; }
            public string LatestValue { get; set; }
            public string Timestamp { get; set; }
        }
    }
}


