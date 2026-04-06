/*
 * parameter로 전달받은 Text가
 * null이 아니면 Visible, null이면 Collapsed로 만들어주는 Converter
 */
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ChatMessenger.Client.Common.Converters
{
    public class StringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 텍스트가 있으면 Visible, null이면 Collapsed
            return !string.IsNullOrEmpty(value as string) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
