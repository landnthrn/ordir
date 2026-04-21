using System.Globalization;
using System.Windows.Data;

namespace Ordir.Converters;

/// <summary>Maps <see cref="Models.FolderTreeNode.TreeDepth"/> to a left gutter width in device-independent pixels.</summary>
public sealed class TreeIndentWidthConverter : IValueConverter
{
    public double IndentPerLevel { get; set; } = 18.0;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var d = value is int i ? i : 0;
        if (d < 0) d = 0;
        return d * IndentPerLevel;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
