using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ChatMessenger.Client.Common.Converters
{
    /// <summary>
    /// Binding된 값이 0이면 Visiblity를 변경합니다.
    /// </summary>
    /// <remarks>
    /// 기본적으론 Hidden으로 변경하고, ConverterParameter에 'Collapsed' 문자열을 넘겨주면 Collapsed로 변경합니다.
    /// </remarks>
    public class ZeroToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 기본값 설정 (파라미터가 없으면 Hidden 기본으로 사용)
            Visibility targetVisibility = Visibility.Hidden;

            // 파라미터가 "Collapsed"로 들어오면 Collapsed로 설정
            if (parameter is string paramString && paramString.Equals("Collapsed", StringComparison.OrdinalIgnoreCase))
            {
                targetVisibility = Visibility.Collapsed;
            }

            if (value is int count)
            {
                return count > 0 ? Visibility.Visible : targetVisibility;
            }

            return targetVisibility;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
