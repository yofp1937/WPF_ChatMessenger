using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ChatMessenger.Client.Common.Converters
{
    /// <summary>
    /// Bool 값에 따라 HorizontalAlignment를 반환하는 컨버터입니다.
    /// 기본값: True -> Right, False -> Left
    /// </summary>
    public class BoolToAlignmentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b)
            {
                // 파라미터가 "Inverted"라면 반대로 동작 (True -> Left, False -> Right)
                bool isInverted = parameter?.ToString() == "Inverted";

                if (isInverted)
                    return b ? HorizontalAlignment.Left : HorizontalAlignment.Right;

                return b ? HorizontalAlignment.Right : HorizontalAlignment.Left;
            }
            return HorizontalAlignment.Left;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
