using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace NadMatcher.UI.Converters;

public class PercentageToColorConverter : IValueConverter
{
    public static readonly PercentageToColorConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double percentage)
        {
            return percentage switch
            {
                >= 90 => new SolidColorBrush(Color.Parse("#2e7d32")), // Green
                >= 70 => new SolidColorBrush(Color.Parse("#558b2f")), // Light Green
                >= 50 => new SolidColorBrush(Color.Parse("#f57c00")), // Orange
                _ => new SolidColorBrush(Color.Parse("#d32f2f"))      // Red
            };
        }
        return new SolidColorBrush(Colors.Gray);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class PercentageToBackgroundConverter : IValueConverter
{
    public static readonly PercentageToBackgroundConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double percentage)
        {
            return percentage switch
            {
                >= 90 => new SolidColorBrush(Color.Parse("#e8f5e9")), // Light Green
                >= 70 => new SolidColorBrush(Color.Parse("#f1f8e9")), // Very Light Green
                >= 50 => new SolidColorBrush(Color.Parse("#fff3e0")), // Light Orange
                _ => new SolidColorBrush(Color.Parse("#ffebee"))      // Light Red
            };
        }
        return new SolidColorBrush(Colors.White);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class BoolToBackgroundConverter : IValueConverter
{
    public static readonly BoolToBackgroundConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isSelected && isSelected)
        {
            return new SolidColorBrush(Color.Parse("#e3f2fd")); // Light Blue
        }
        return new SolidColorBrush(Colors.Transparent);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class PerfectMatchBackgroundConverter : IValueConverter
{
    public static readonly PerfectMatchBackgroundConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool hasPerfectMatch && hasPerfectMatch)
        {
            return new SolidColorBrush(Color.Parse("#c8e6c9")); // Light Green
        }
        return new SolidColorBrush(Color.Parse("#fff3e0")); // Light Orange
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class ListToStringConverter : IValueConverter
{
    public static readonly ListToStringConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is System.Collections.IEnumerable enumerable)
        {
            var items = new System.Collections.Generic.List<string>();
            foreach (var item in enumerable)
            {
                items.Add(item?.ToString() ?? string.Empty);
            }
            return string.Join(", ", items);
        }
        return string.Empty;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class IsGreaterThanZeroConverter : IValueConverter
{
    public static readonly IsGreaterThanZeroConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            int i => i > 0,
            double d => d > 0,
            _ => false
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class BoolToExpandConverter : IValueConverter
{
    public static readonly BoolToExpandConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isExpanded)
        {
            return isExpanded ? "▲" : "▼";
        }
        return "▼";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class BoolToThemeIconConverter : IValueConverter
{
    public static readonly BoolToThemeIconConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isDarkMode)
        {
            return isDarkMode ? "☀️" : "🌙";
        }
        return "🌙";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class BoolToThemeTextConverter : IValueConverter
{
    public static readonly BoolToThemeTextConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isDarkMode)
        {
            return isDarkMode ? "Light Mode" : "Dark Mode";
        }
        return "Dark Mode";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
