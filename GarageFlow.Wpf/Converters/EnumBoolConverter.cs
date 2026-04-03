using System.Globalization;
using System.Windows.Data;

namespace GarageFlow.Wpf.Converters;

public class EnumBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value?.Equals(parameter) ?? false;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => value is true ? parameter : Binding.DoNothing;
}
