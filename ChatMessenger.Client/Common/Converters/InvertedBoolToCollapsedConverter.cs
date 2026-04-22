using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ChatMessenger.Client.Common.Converters
{
    public class InvertedBoolToCollapsedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                // false일 때 Visible, true일 때 Collapsed 반환
                return !boolValue ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is Visibility visibility && visibility != Visibility.Visible;
        }
    }
}
