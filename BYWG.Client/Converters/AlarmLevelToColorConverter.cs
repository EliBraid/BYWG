using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using BYWG.Client.Views;

namespace BYWG.Client.Converters
{
    /// <summary>
    /// 报警级别到颜色的转换器
    /// </summary>
    public class AlarmLevelToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is AlarmLevel level)
            {
                return level switch
                {
                    AlarmLevel.Info => Brushes.Blue,
                    AlarmLevel.Warning => Brushes.Orange,
                    AlarmLevel.Error => Brushes.Red,
                    AlarmLevel.Critical => Brushes.DarkRed,
                    _ => Brushes.Gray
                };
            }
            return Brushes.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
