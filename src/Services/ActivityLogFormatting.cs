using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Ordir.Services;

/// <summary>Default (pastel blue-violet), skip, and error colors for activity log lines.</summary>
internal static class ActivityLogFormatting
{
    internal static readonly System.Windows.Media.Brush LineBrush =
        Freeze(new SolidColorBrush(System.Windows.Media.Color.FromRgb(0xC9, 0xCB, 0xFF)));
    /// <summary>Path and <c>&gt; </c> prefix on request-style lines (darker than <see cref="LineBrush"/>).</summary>
    internal static readonly System.Windows.Media.Brush PromptBrush =
        Freeze(new SolidColorBrush(System.Windows.Media.Color.FromRgb(0x7B, 0x6B, 0xB8)));
    internal static readonly System.Windows.Media.Brush SkipBrush =
        Freeze(new SolidColorBrush(System.Windows.Media.Color.FromRgb(0x5A, 0x4D, 0x8A)));
    internal static readonly System.Windows.Media.Brush ErrorBrush =
        Freeze(new SolidColorBrush(System.Windows.Media.Color.FromRgb(0xF3, 0x8B, 0xA8)));

    private static T Freeze<T>(T freezable) where T : Freezable
    {
        freezable.Freeze();
        return freezable;
    }

    internal static System.Windows.Media.Brush BrushForLine(string line)
    {
        var t = line.TrimStart();
        if (t.StartsWith('!'))
            return ErrorBrush;
        var gt = t.IndexOf('>', StringComparison.Ordinal);
        if (gt >= 0 && gt < t.Length - 1)
        {
            var after = t[(gt + 1)..].TrimStart();
            if (after.StartsWith('!'))
                return ErrorBrush;
        }

        if (t.StartsWith("skip ", StringComparison.OrdinalIgnoreCase))
            return SkipBrush;
        if (t.StartsWith("error", StringComparison.OrdinalIgnoreCase))
            return ErrorBrush;
        return LineBrush;
    }

    /// <summary>
    /// Adds a line to <paramref name="inlines"/>; paints the segment through the first <c>&gt; </c> (inclusive) with
    /// <see cref="PromptBrush"/>, remainder with <see cref="BrushForLine"/> unless overridden.
    /// </summary>
    internal static void AppendLogLineInlines(InlineCollection inlines, string line, System.Windows.Media.Brush? restBrush = null)
    {
        var nl = Environment.NewLine;
        var rest = restBrush ?? BrushForLine(line);
        var idx = line.IndexOf("> ", StringComparison.Ordinal);
        if (idx < 0)
        {
            inlines.Add(new Run(line + nl) { Foreground = rest });
            return;
        }

        var prefixLen = idx + 2;
        inlines.Add(new Run(line[..prefixLen]) { Foreground = PromptBrush });
        inlines.Add(new Run(line[prefixLen..] + nl) { Foreground = rest });
    }
}
