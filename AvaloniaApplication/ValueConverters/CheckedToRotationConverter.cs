using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace AvaloniaApplication.ValueConverters;

public class CheckedToRotationConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isExpanded) 
            return new RotateTransform(isExpanded ? 0 : 90);
        return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
    }
 
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}