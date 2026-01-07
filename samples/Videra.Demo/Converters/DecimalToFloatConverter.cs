using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Videra.Demo.Converters;

/// <summary>
///     用于在 NumericUpDown (decimal) 和 引擎属性 (float) 之间进行双向转换
/// </summary>
public class DecimalToFloatConverter : IValueConverter
{
    // 从 Source (ViewModel: decimal) -> Target (View: float)
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is decimal d)
            return (float)d;

        // 容错处理
        if (value is double db) return (float)db;
        if (value is int i) return (float)i;

        return 0.0f;
    }

    // 从 Target (View: float) -> Source (ViewModel: decimal)
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is float f)
            return (decimal)f;

        // 容错处理：Avalonia 的依赖属性有时候可能是 double
        if (value is double d) return (decimal)d;

        return 0m;
    }
}