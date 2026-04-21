using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MediaBrush = System.Windows.Media.Brush;
using MediaFontFamily = System.Windows.Media.FontFamily;
using WpfImage = System.Windows.Controls.Image;

namespace Ordir.Views;

public partial class InfoTab : UserControl
{
    private static readonly Regex InlineCodeRegex = new(@"`([^`]+)`", RegexOptions.Compiled);
    private static readonly Regex MarkdownLinkRegex = new(@"\[(?<text>[^\]]+)\]\((?<url>https?://[^)\s]+)\)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex ParentheticalRegex = new(@"\([^()\r\n]+\)", RegexOptions.Compiled);

    /// <summary>Image filenames to try, in order: Buy Me a Coffee, GitHub, Discord (left to right).</summary>
    private static readonly string[][] PromoImageCandidates =
    {
        new[] { "buy-me-a-coffee-button.png", "promo-buymeacoffee.png", "buymeacoffee-btn.png", "btn-buymeacoffee.png", "buymeacoffee-button.png", "buymeacoffee.png", "bmc-button.png" },
        new[] { "promo-github.png", "github-btn.png", "btn-github.png", "github-button.png", "github.png" },
        new[] { "promo-discord.png", "discord-btn.png", "btn-discord.png", "discord-button.png", "discord.png" }
    };

    private static readonly string[] PromoUrls =
    {
        "https://buymeacoffee.com/landn.thrn",
        "https://github.com/landnthrn",
        "https://discord.com/users/831735011588964392"
    };

    public InfoTab()
    {
        InitializeComponent();
        Loaded += (_, _) => BuildAboutContent();
    }

    private static string DisplayVersionLabel()
    {
        var v = typeof(InfoTab).Assembly.GetName().Version;
        return v is null ? "v2.0.0" : $"v{v.Major}.{v.Minor}.{v.Build}";
    }

    private void BuildAboutContent()
    {
        AboutRich.Document = new FlowDocument
        {
            PagePadding = new Thickness(0),
            Background = System.Windows.Media.Brushes.Transparent,
            TextAlignment = TextAlignment.Left
        };
        var doc = AboutRich.Document;

        var img = new WpfImage
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            Stretch = Stretch.Uniform,
            Height = 104,
            Margin = new Thickness(0, 0, 0, 6),
            SnapsToDevicePixels = true
        };
        try
        {
            img.Source = new BitmapImage(new Uri("pack://application:,,,/Ordir;component/Assets/ordir-title.png", UriKind.Absolute));
        }
        catch
        {
            img.Visibility = Visibility.Collapsed;
        }

        AppendAboutElement(doc, img);
        AppendAboutElement(doc, new TextBlock
        {
            Text = "by landn.thrn",
            FontFamily = new MediaFontFamily("Consolas"),
            FontSize = 15,
            Foreground = (MediaBrush)FindResource("Brush.Muted"),
            Margin = new Thickness(0, 0, 0, 2),
            TextWrapping = TextWrapping.Wrap
        });
        AppendAboutElement(doc, new TextBlock
        {
            Text = DisplayVersionLabel(),
            FontFamily = new MediaFontFamily("Consolas"),
            FontSize = 14,
            FontWeight = FontWeights.SemiBold,
            Foreground = (MediaBrush)FindResource("Brush.SectionTitle"),
            Margin = new Thickness(0, 0, 0, 10),
            TextWrapping = TextWrapping.Wrap
        });

        string raw;
        try
        {
            const string aboutUri = "pack://application:,,,/Ordir;component/Assets/About-section.txt";
            var sri = Application.GetResourceStream(new Uri(aboutUri, UriKind.Absolute))
                      ?? throw new IOException("Missing embedded About-section.txt resource.");
            using var reader = new StreamReader(sri.Stream, Encoding.UTF8);
            raw = reader.ReadToEnd();
        }
        catch
        {
            AppendAboutElement(doc, new TextBlock
            {
                Text = "Could not load embedded About content.",
                Foreground = (MediaBrush)FindResource("Brush.Muted"),
                TextWrapping = TextWrapping.Wrap
            });
            return;
        }

        var sectionTitle = (MediaBrush)FindResource("Brush.SectionTitle");
        var muted = (MediaBrush)FindResource("Brush.Muted");
        var text = (MediaBrush)FindResource("Brush.Text");
        var accent = (MediaBrush)FindResource("Brush.AccentBlueViolet");

        foreach (var el in ParseAboutBlocks(raw, sectionTitle, muted, text, accent))
            AppendAboutElement(doc, el);
    }

    private static void AppendAboutElement(FlowDocument doc, UIElement el)
    {
        switch (el)
        {
            case TextBlock tb:
                doc.Blocks.Add(ParagraphFromTextBlock(tb));
                break;
            case Separator sep:
                doc.Blocks.Add(new BlockUIContainer(sep) { Margin = sep.Margin });
                break;
            case StackPanel sp:
                doc.Blocks.Add(new BlockUIContainer(sp) { Margin = sp.Margin });
                break;
            case Border b:
                doc.Blocks.Add(new BlockUIContainer(b));
                break;
            case WpfImage image:
                doc.Blocks.Add(new BlockUIContainer(new Border
                {
                    Margin = image.Margin,
                    HorizontalAlignment = image.HorizontalAlignment,
                    Child = image
                }));
                break;
            default:
                doc.Blocks.Add(new BlockUIContainer(el));
                break;
        }
    }

    private static Paragraph ParagraphFromTextBlock(TextBlock tb)
    {
        // FlowDocument Paragraph.Margin does not allow negative components (throws ArgumentException).
        var m = tb.Margin;
        var p = new Paragraph
        {
            Margin = new Thickness(
                Math.Max(0, m.Left),
                Math.Max(0, m.Top),
                Math.Max(0, m.Right),
                Math.Max(0, m.Bottom)),
            TextAlignment = tb.TextAlignment
        };
        if (tb.LineHeight > 0 && !double.IsNaN(tb.LineHeight))
            p.LineHeight = tb.LineHeight;

        if (tb.Inlines.Count == 0)
        {
            if (!string.IsNullOrEmpty(tb.Text))
            {
                p.Inlines.Add(new Run(tb.Text)
                {
                    Foreground = tb.Foreground,
                    FontSize = tb.FontSize,
                    FontWeight = tb.FontWeight,
                    FontFamily = tb.FontFamily
                });
            }

            return p;
        }

        foreach (var inline in tb.Inlines.ToList())
        {
            switch (inline)
            {
                case Run r:
                    p.Inlines.Add(new Run(r.Text)
                    {
                        Foreground = r.Foreground,
                        FontSize = r.FontSize,
                        FontWeight = r.FontWeight,
                        FontFamily = r.FontFamily
                    });
                    break;
                case LineBreak:
                    p.Inlines.Add(new LineBreak());
                    break;
                case Hyperlink h:
                {
                    var label = string.Concat(h.Inlines.OfType<Run>().Select(r => r.Text));
                    if (string.IsNullOrEmpty(label))
                        label = h.NavigateUri?.ToString() ?? string.Empty;
                    var nh = new Hyperlink
                    {
                        Foreground = h.Foreground,
                        NavigateUri = h.NavigateUri
                    };
                    AddBracketAwareRuns(
                        nh.Inlines,
                        label,
                        h.Foreground ?? ((MediaBrush?)Application.Current?.TryFindResource("Brush.AccentBlueViolet")
                            ?? new SolidColorBrush(System.Windows.Media.Color.FromRgb(0x7B, 0x7F, 0xD8))));
                    nh.RequestNavigate += (_, e) =>
                    {
                        try
                        {
                            if (e.Uri != null)
                                Process.Start(new ProcessStartInfo { FileName = e.Uri.AbsoluteUri, UseShellExecute = true });
                        }
                        catch
                        {
                            // ignore
                        }

                        e.Handled = true;
                    };
                    p.Inlines.Add(nh);
                    break;
                }
            }
        }

        return p;
    }

    private static IEnumerable<UIElement> ParseAboutBlocks(
        string raw,
        MediaBrush sectionTitle,
        MediaBrush muted,
        MediaBrush text,
        MediaBrush accent)
    {
        var lines = raw.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
        var i = 0;
        string? lastEmittedSubheading = null;
        var lastYieldedParagraph = false;

        while (i < lines.Length)
        {
            var rawLine = lines[i];
            var t = rawLine.Trim();

            if (IsEditorNoteLine(t))
            {
                i++;
                continue;
            }

            if (string.IsNullOrEmpty(t))
            {
                i++;
                continue;
            }

            if (t == "---")
            {
                lastEmittedSubheading = null;
                lastYieldedParagraph = false;
                yield return new Separator
                {
                    Margin = new Thickness(0, 12, 0, 12),
                    Background = (MediaBrush)new BrushConverter().ConvertFromString("#45475A")!
                };
                i++;
                continue;
            }

            var trimmedForHeading = t.TrimStart();
            if (trimmedForHeading.StartsWith("# Find this useful?", StringComparison.OrdinalIgnoreCase) ||
                trimmedForHeading.StartsWith("# Found this useful?", StringComparison.OrdinalIgnoreCase))
            {
                lastEmittedSubheading = null;
                lastYieldedParagraph = false;
                i++;
                foreach (var el in ConsumeFoundSectionList(lines, ref i, text, muted))
                {
                    yield return el;
                    lastYieldedParagraph = el is TextBlock;
                }

                lastYieldedParagraph = false;
                continue;
            }

            if (trimmedForHeading.StartsWith("# ", StringComparison.Ordinal) && !trimmedForHeading.StartsWith("## ", StringComparison.Ordinal))
            {
                lastEmittedSubheading = null;
                lastYieldedParagraph = false;
                var title = trimmedForHeading[2..].Trim();
                yield return new TextBlock
                {
                    Text = title,
                    FontSize = 20,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = sectionTitle,
                    Margin = new Thickness(0, 10, 0, 6),
                    TextWrapping = TextWrapping.Wrap
                };
                i++;
                continue;
            }

            if (trimmedForHeading.StartsWith("## ", StringComparison.Ordinal))
            {
                lastYieldedParagraph = false;
                var subTitle = trimmedForHeading[3..].Trim();
                if (string.Equals(lastEmittedSubheading, subTitle, StringComparison.OrdinalIgnoreCase))
                {
                    i++;
                    continue;
                }

                lastEmittedSubheading = subTitle;
                yield return new TextBlock
                {
                    Text = subTitle,
                    FontSize = 15,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = sectionTitle,
                    Margin = new Thickness(0, 8, 0, 4),
                    TextWrapping = TextWrapping.Wrap
                };
                i++;
                continue;
            }

            if (trimmedForHeading.StartsWith("### ", StringComparison.Ordinal))
            {
                lastYieldedParagraph = false;
                var title3 = trimmedForHeading[4..].Trim();
                yield return new TextBlock
                {
                    Text = title3,
                    FontSize = 14,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = sectionTitle,
                    Margin = new Thickness(0, 8, 0, 4),
                    TextWrapping = TextWrapping.Wrap
                };
                i++;
                continue;
            }

            if (IsTreeLine(rawLine))
            {
                lastYieldedParagraph = false;
                var mono = new StringBuilder();
                while (i < lines.Length)
                {
                    var l = lines[i];
                    var tr = l.Trim();
                    if (string.IsNullOrEmpty(tr))
                    {
                        i++;
                        continue;
                    }

                    if (tr == "---" || IsEditorNoteLine(tr))
                        break;
                    if (tr.StartsWith("# ", StringComparison.Ordinal) || tr.StartsWith("## ", StringComparison.Ordinal))
                        break;
                    if (!IsTreeLine(l))
                        break;

                    mono.AppendLine(l.TrimEnd());
                    i++;
                }

                var block = mono.ToString().TrimEnd();
                if (block.Length > 0)
                {
                    var tb = new TextBlock
                    {
                        FontFamily = new MediaFontFamily("Cascadia Mono, Consolas, Courier New"),
                        FontSize = 11.5,
                        Foreground = muted,
                        TextWrapping = TextWrapping.NoWrap,
                        Margin = new Thickness(0, 0, 0, 10),
                        LineHeight = 17
                    };
                    AppendMarkdownInlines(tb.Inlines, block.Replace("\r", ""), muted, treatNewlines: true);
                    yield return tb;
                }

                continue;
            }

            if (t.StartsWith('-'))
            {
                lastYieldedParagraph = false;
                var sp = new StackPanel { Margin = new Thickness(4, 0, 0, 6) };
                while (i < lines.Length)
                {
                    var l = lines[i].Trim();
                    if (string.IsNullOrEmpty(l) || l == "---" || IsEditorNoteLine(l))
                        break;
                    if (l.StartsWith("# ", StringComparison.Ordinal))
                        break;
                    if (!l.StartsWith('-'))
                        break;

                    var row = new TextBlock
                    {
                        FontSize = 13,
                        Foreground = text,
                        TextWrapping = TextWrapping.Wrap,
                        Margin = new Thickness(0, 0, 0, 4),
                        LineHeight = 20
                    };
                    row.Inlines.Add(new Run("•  ") { Foreground = accent, FontWeight = FontWeights.SemiBold });
                    AppendMarkdownInlines(row.Inlines, l.TrimStart('-', ' ', '\t').Trim(), text, treatNewlines: false);
                    sp.Children.Add(row);
                    i++;
                }

                yield return sp;
                continue;
            }

            if (IsSectionLabelLine(t))
            {
                lastYieldedParagraph = false;
                yield return new TextBlock
                {
                    Text = t,
                    FontSize = 16,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = sectionTitle,
                    Margin = new Thickness(0, 8, 0, 4),
                    TextWrapping = TextWrapping.Wrap
                };
                i++;
                continue;
            }

            if (TryBuildUrlLine(t, muted, out var linkBlock))
            {
                lastYieldedParagraph = false;
                yield return linkBlock;
                i++;
                continue;
            }

            foreach (var el in ConsumeMarkdownParagraphGroupList(lines, ref i, text))
            {
                lastYieldedParagraph = true;
                yield return el;
            }
        }
    }

    private static bool IsParagraphLineStart(string rawLine)
    {
        var t = rawLine.Trim();
        if (string.IsNullOrEmpty(t) || t == "---")
            return false;
        if (IsEditorNoteLine(t))
            return false;
        var tf = t.TrimStart();
        if (tf.StartsWith("# ", StringComparison.Ordinal) || tf.StartsWith("## ", StringComparison.Ordinal) || tf.StartsWith("### ", StringComparison.Ordinal))
            return false;
        if (t.StartsWith('-'))
            return false;
        if (IsTreeLine(rawLine))
            return false;
        if (t.StartsWith("my buymeacoffee ", StringComparison.OrdinalIgnoreCase) ||
            t.StartsWith("my github ", StringComparison.OrdinalIgnoreCase) ||
            t.StartsWith("my discord ", StringComparison.OrdinalIgnoreCase))
            return false;
        return true;
    }

    private static bool IsTreeLine(string rawLine)
    {
        if (string.IsNullOrEmpty(rawLine)) return false;
        return rawLine.Contains('|') || rawLine.Contains('│') || rawLine.Contains('└');
    }

    private static List<UIElement> ConsumeMarkdownParagraphGroupList(string[] lines, ref int i, MediaBrush text)
    {
        var block = new List<string>();
        while (i < lines.Length)
        {
            var raw = lines[i];
            var t = raw.Trim();
            if (string.IsNullOrEmpty(t) || t == "---" || IsEditorNoteLine(t))
                break;
            if (t.StartsWith("# ", StringComparison.Ordinal) || t.StartsWith("## ", StringComparison.Ordinal))
                break;
            if (IsTreeLine(raw))
                break;
            if (t.StartsWith('-'))
                break;
            block.Add(raw);
            i++;
        }

        if (block.Count == 0)
            return new List<UIElement>();

        var tb = new TextBlock
        {
            TextWrapping = TextWrapping.Wrap,
            FontSize = 13,
            Foreground = text,
            Margin = new Thickness(0, 0, 0, 8),
            LineHeight = 20
        };

        var first = true;
        foreach (var ln in block)
        {
            if (!first)
                tb.Inlines.Add(new LineBreak());
            first = false;
            AppendMarkdownInlines(tb.Inlines, ln.TrimEnd(), text, treatNewlines: false);
        }

        return new List<UIElement> { tb };
    }

    private static List<UIElement> ConsumeFoundSectionList(
        string[] lines,
        ref int i,
        MediaBrush text,
        MediaBrush muted)
    {
        var list = new List<UIElement>();
        while (i < lines.Length && string.IsNullOrWhiteSpace(lines[i]))
            i++;

        // Preserve the section text exactly as written until the next separator.
        while (i < lines.Length)
        {
            var raw = lines[i];
            var t = raw.Trim();
            if (t == "---")
                break;

            if (string.IsNullOrWhiteSpace(t))
            {
                i++;
                continue;
            }

            if (t.StartsWith("my buymeacoffee ", StringComparison.OrdinalIgnoreCase) ||
                t.StartsWith("my github ", StringComparison.OrdinalIgnoreCase) ||
                t.StartsWith("my discord ", StringComparison.OrdinalIgnoreCase))
            {
                i++;
                continue;
            }

            if (TryBuildUrlLine(t, muted, out var rawUrlLine))
            {
                list.Add(rawUrlLine);
                i++;
                continue;
            }

            var paragraphLine = new TextBlock
            {
                FontSize = 13,
                Foreground = text,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 8),
                LineHeight = 20
            };
            AppendMarkdownInlines(paragraphLine.Inlines, t, text, treatNewlines: false);
            list.Add(paragraphLine);
            i++;
        }

        var row = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 8) };
        for (var b = 0; b < 3; b++)
        {
            var btn = CreatePromoImageButton(PromoImageCandidates[b], PromoUrls[b], b switch
            {
                0 => "Buy Me a Coffee",
                1 => "GitHub",
                _ => "Discord"
            });
            row.Children.Add(btn);
        }

        list.Add(row);

        while (i < lines.Length && lines[i].Trim() != "---")
            i++;

        return list;
    }

    private static System.Windows.Controls.Button CreatePromoImageButton(
        string[] fileNameCandidates,
        string url,
        string fallbackLabel)
    {
        var src = TryLoadFirstPackImage(fileNameCandidates, decodePixelWidth: 420);
        var btn = new System.Windows.Controls.Button
        {
            Margin = new Thickness(0, 0, 12, 0),
            MinWidth = 112,
            Cursor = Cursors.Hand,
            ToolTip = url
        };

        if (Application.Current?.TryFindResource("OrdirPromoLinkButton") is Style linkStyle)
            btn.Style = linkStyle;

        if (src != null)
        {
            var im = new WpfImage
            {
                Source = src,
                Stretch = Stretch.Uniform,
                Height = 32,
                MaxWidth = 112,
                SnapsToDevicePixels = false,
                UseLayoutRounding = true
            };
            RenderOptions.SetBitmapScalingMode(im, BitmapScalingMode.HighQuality);
            btn.Content = im;
        }
        else
        {
            btn.Content = new TextBlock
            {
                Text = fallbackLabel,
                Foreground = (MediaBrush?)Application.Current?.TryFindResource("Brush.Muted")
                    ?? new SolidColorBrush(System.Windows.Media.Color.FromRgb(0xA6, 0xAD, 0xC8)),
                FontWeight = FontWeights.SemiBold,
                FontSize = 13,
                TextWrapping = TextWrapping.Wrap,
                MaxWidth = 120,
                TextAlignment = TextAlignment.Center
            };
        }

        btn.Click += (_, _) =>
        {
            try
            {
                Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
            }
            catch
            {
                // ignore
            }
        };

        return btn;
    }

    private static BitmapImage? TryLoadFirstPackImage(IEnumerable<string> relativeNames, int? decodePixelWidth = null)
    {
        foreach (var name in relativeNames)
        {
            try
            {
                var uri = new Uri($"pack://application:,,,/Ordir;component/Assets/{name}", UriKind.Absolute);
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.UriSource = uri;
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                if (decodePixelWidth is > 0 and var w)
                    bmp.DecodePixelWidth = w;
                bmp.EndInit();
                bmp.Freeze();
                return bmp;
            }
            catch
            {
                // try next filename
            }
        }

        return null;
    }

    private static bool IsEditorNoteLine(string t) =>
        t.Contains("when i use mardown", StringComparison.OrdinalIgnoreCase) ||
        t.Contains("put the order-title", StringComparison.OrdinalIgnoreCase);

    private static bool IsSectionLabelLine(string t) =>
        t.EndsWith(":", StringComparison.Ordinal) &&
        !t.StartsWith("-", StringComparison.Ordinal) &&
        !t.StartsWith("#", StringComparison.Ordinal) &&
        !IsTreeLine(t) &&
        t.Length <= 80;

    private static bool TryBuildUrlLine(string t, MediaBrush muted, out UIElement line)
    {
        line = null!;
        if (!Uri.TryCreate(t, UriKind.Absolute, out var uri))
            return false;
        if (uri.Scheme is not ("http" or "https"))
            return false;

        var tb = new TextBlock
        {
            FontSize = 12.5,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 0, 0, 10)
        };

        var linkColor = (MediaBrush?)Application.Current?.TryFindResource("Brush.AccentBlueViolet")
            ?? new SolidColorBrush(System.Windows.Media.Color.FromRgb(0x7B, 0x7F, 0xD8));
        var hl = new Hyperlink { Foreground = linkColor, NavigateUri = uri };
        AddBracketAwareRuns(hl.Inlines, uri.AbsoluteUri, linkColor);
        hl.RequestNavigate += (_, e) =>
        {
            try
            {
                Process.Start(new ProcessStartInfo { FileName = e.Uri.AbsoluteUri, UseShellExecute = true });
            }
            catch
            {
                // ignore
            }
            e.Handled = true;
        };
        tb.Inlines.Add(hl);
        AddBracketAwareRuns(tb.Inlines, " (open)", muted);
        line = tb;
        return true;
    }

    /// <summary>Renders simple markdown inline syntax: `code` and [title](url).</summary>
    private static void AppendMarkdownInlines(
        InlineCollection inlines,
        string segment,
        MediaBrush defaultFg,
        bool treatNewlines)
    {
        if (string.IsNullOrEmpty(segment))
            return;

        if (treatNewlines)
        {
            var parts = segment.Split('\n');
            var firstNl = true;
            foreach (var part in parts)
            {
                if (!firstNl)
                    inlines.Add(new LineBreak());
                firstNl = false;
                AppendMarkdownInlines(inlines, part, defaultFg, treatNewlines: false);
            }

            return;
        }

        var idx = 0;
        while (idx < segment.Length)
        {
            var linkMatch = MarkdownLinkRegex.Match(segment, idx);
            var codeMatch = InlineCodeRegex.Match(segment, idx);

            var hasLink = linkMatch.Success;
            var hasCode = codeMatch.Success;

            if (!hasLink && !hasCode)
            {
                AddBracketAwareRuns(inlines, segment[idx..], defaultFg);
                break;
            }

            Match next;
            var isLink = false;
            if (hasLink && hasCode)
            {
                if (linkMatch.Index <= codeMatch.Index)
                {
                    next = linkMatch;
                    isLink = true;
                }
                else
                {
                    next = codeMatch;
                }
            }
            else if (hasLink)
            {
                next = linkMatch;
                isLink = true;
            }
            else
            {
                next = codeMatch;
            }

            if (next.Index > idx)
                AddBracketAwareRuns(inlines, segment.Substring(idx, next.Index - idx), defaultFg);

            if (isLink)
            {
                var label = next.Groups["text"].Value;
                var url = next.Groups["url"].Value;
                if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
                {
                    var linkColor = (MediaBrush?)Application.Current?.TryFindResource("Brush.AccentBlueViolet")
                        ?? new SolidColorBrush(System.Windows.Media.Color.FromRgb(0x7B, 0x7F, 0xD8));
                    var hl = new Hyperlink { Foreground = linkColor, NavigateUri = uri };
                    AddBracketAwareRuns(hl.Inlines, label, linkColor);
                    hl.RequestNavigate += (_, e) =>
                    {
                        try
                        {
                            Process.Start(new ProcessStartInfo { FileName = e.Uri.AbsoluteUri, UseShellExecute = true });
                        }
                        catch
                        {
                            // ignore
                        }
                        e.Handled = true;
                    };
                    inlines.Add(hl);
                }
                else
                {
                    AddBracketAwareRuns(inlines, next.Value, defaultFg);
                }
            }
            else
            {
                inlines.Add(new Run(next.Groups[1].Value) { Foreground = defaultFg });
            }

            idx = next.Index + next.Length;
        }
    }

    private static void AddBracketAwareRuns(InlineCollection inlines, string text, MediaBrush brush)
    {
        if (string.IsNullOrEmpty(text))
            return;

        var start = 0;
        foreach (Match m in ParentheticalRegex.Matches(text))
        {
            if (m.Index > start)
                inlines.Add(new Run(text.Substring(start, m.Index - start)) { Foreground = brush });

            inlines.Add(new Run(m.Value)
            {
                Foreground = brush,
                FontStyle = FontStyles.Italic
            });
            start = m.Index + m.Length;
        }

        if (start < text.Length)
            inlines.Add(new Run(text[start..]) { Foreground = brush });
    }
}
