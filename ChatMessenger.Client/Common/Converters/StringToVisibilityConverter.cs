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
            // 1. 텍스트가 존재하면 Visible
            if(!string.IsNullOrEmpty(value as string))
            {
                return Visibility.Visible;
            }
            // 2. 텍스트가 없고, 파라미터가 Hidden이면 Hidden
            if (parameter?.ToString() == "Hidden")
            {
                return Visibility.Hidden;
            }
            // 3. 그 외엔 Collapsed
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
