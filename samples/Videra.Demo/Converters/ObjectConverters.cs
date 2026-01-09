using System;
using Avalonia.Data.Converters;

namespace Videra.Demo.Converters;

public static class ObjectConverters
{
    public static IValueConverter IsNull { get; } = new NullToBoolConverter(true);
    public static IValueConverter IsNotNull { get; } = new NullToBoolConverter(false);

    private sealed class NullToBoolConverter : IValueConverter
    {
        private readonly bool _isNullTarget;

        public NullToBoolConverter(bool isNullTarget)
        {
            _isNullTarget = isNullTarget;
        }

        public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            var isNull = value is null;
            return _isNullTarget ? isNull : !isNull;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
