using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Flow.Launcher.Converters;

/// <summary>
/// Maps <see cref="Flow.Launcher.Plugin.Result.RoundedIcon"/> to a circular opacity mask.
/// A geometry <c>Clip</c> would be cheaper, but WPF clips are not anti-aliased and leave
/// jagged edges on circular icons; an opacity mask renders smoothly.
/// </summary>
public class RoundedIconMaskConverter : IValueConverter
{
    private static readonly Brush CircleMask = CreateCircleMask();

    private static Brush CreateCircleMask()
    {
        var brush = new DrawingBrush(new GeometryDrawing(
            Brushes.Black,
            null,
            new EllipseGeometry(new Point(0.5, 0.5), 0.5, 0.5)));
        brush.Freeze();
        return brush;
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is true ? CircleMask : null;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
