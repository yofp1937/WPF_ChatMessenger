/*
 * 여러 바인딩 데이터를 하나의 배열로 만들어 반환해주는 Converter
 */
using System.Globalization;
using System.Windows.Data;

namespace ChatMessenger.Client.Common.Converters
{
    public class MultiBindingConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // 전달받은 값들을 배열 그대로 반환합니다. (ViewModel에서 배열로 받을 예정)
            return values.Clone();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
