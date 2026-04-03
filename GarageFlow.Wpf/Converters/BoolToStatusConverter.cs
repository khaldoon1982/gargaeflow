using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace GarageFlow.Wpf.Converters;

public class BoolToStatusConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is true ? "Verbonden" : "Niet geconfigureerd";

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public class BoolToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is true
            ? new SolidColorBrush(Color.FromRgb(0xE8, 0xF5, 0xE9))  // green light
            : new SolidColorBrush(Color.FromRgb(0xFF, 0xEB, 0xEE));  // red light

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
