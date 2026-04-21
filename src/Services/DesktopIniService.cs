using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Ordir.Services;

/// <summary>
/// Matches <c>old-version/OrganizeFolders.ps1</c>: UTF-16 LE + BOM writes, same <c>[ViewState]</c> shape (no stray <c>|</c> line), InfoTip merge.
/// </summary>
public static class DesktopIniService
{
    public static readonly Encoding Utf16LeWithBom = new UnicodeEncoding(bigEndian: false, byteOrderMark: true);

    private static readonly Regex InfoTipLine = new(@"^\s*InfoTip\s*=.*$", RegexOptions.Multiline);
    private static readonly Regex ShellClassSection = new(@"^\s*\[\.ShellClassInfo\]\s*$", RegexOptions.Multiline);
    private static readonly Regex InfoTipValue = new(@"^\s*InfoTip\s*=\s*(.+?)\s*$", RegexOptions.Multiline);
    private static readonly Regex OrderNumber = new(@"^#(\d+)$", RegexOptions.CultureInvariant);
    /// <summary>Early Ordir builds wrote a lone "|" after <c>FolderType=Generic</c>, which breaks Explorer reading <c>InfoTip</c> / Comments.</summary>
    private static readonly Regex ErroneousViewStatePipeLine = new(
        @"FolderType=Generic(\r\n|\n)\|(\r\n|\n)",
        RegexOptions.CultureInvariant);

    public const string IniFileName = "desktop.ini";

    public static string DesktopIniPath(string folderPath) =>
        Path.Combine(folderPath, IniFileName);

    /// <summary>Detects desktop.ini even when marked hidden/system (File.Exists can miss edge cases).</summary>
    public static bool DesktopIniFileExistsAtPath(string iniPath)
    {
        try
        {
            if (File.Exists(iniPath)) return true;
            var parent = Path.GetDirectoryName(iniPath);
            var name = Path.GetFileName(iniPath);
            if (string.IsNullOrEmpty(parent) || string.IsNullOrEmpty(name)) return false;
            var di = new DirectoryInfo(parent);
            if (!di.Exists) return false;
            return di.EnumerateFiles("*", SearchOption.TopDirectoryOnly)
                .Any(f => string.Equals(f.Name, name, StringComparison.OrdinalIgnoreCase));
        }
        catch
        {
            return false;
        }
    }

    public static bool DesktopIniExistsInFolder(string folderPath) =>
        DesktopIniFileExistsAtPath(DesktopIniPath(folderPath));

    public static string BuildTemplate(int orderNumber) =>
        $"""
[ViewState]
Mode=
Vid=
FolderType=Generic

[.ShellClassInfo]
ConfirmFileOp=0
InfoTip=#{orderNumber}

""";

    /// <summary>Removes mistaken <c>|</c> line after <c>FolderType=Generic</c> from legacy Ordir output.</summary>
    public static string SanitizeErroneousViewStatePipeLine(string content)
    {
        if (string.IsNullOrEmpty(content)) return content;
        return ErroneousViewStatePipeLine.Replace(content, "FolderType=Generic$1");
    }

    public static string ReadAllTextLenient(string path)
    {
        var bytes = File.ReadAllBytes(path);
        if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE)
            return Utf16LeWithBom.GetString(bytes);
        if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
            return Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3);

        var n = Math.Min(bytes.Length, 80);
        var zeroCount = 0;
        for (var i = 0; i < n; i++)
        {
            if (bytes[i] == 0) zeroCount++;
        }

        if (zeroCount >= 4)
            return Encoding.Unicode.GetString(bytes);

        return Encoding.UTF8.GetString(bytes);
    }

    public static string? ParseInfoTipRaw(string iniRaw)
    {
        var text = SanitizeErroneousViewStatePipeLine(iniRaw);
        var m = InfoTipValue.Match(text);
        return m.Success ? m.Groups[1].Value.Trim() : null;
    }

    public static bool IsWellFormedOrderTip(string? tip) =>
        tip is not null && OrderNumber.IsMatch(tip.Trim());

    /// <summary>Sort key for well-formed <c>#n</c> tips; larger values sort after numeric tips.</summary>
    public static int InfoTipNumericSortKey(string? tip)
    {
        if (string.IsNullOrWhiteSpace(tip)) return int.MaxValue;
        var m = OrderNumber.Match(tip.Trim());
        if (!m.Success) return int.MaxValue - 1;
        return int.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture);
    }

    public static string MergeInfoTipIntoIni(string rawContent, string infoTipWithHash)
    {
        var rawContentSanitized = SanitizeErroneousViewStatePipeLine(rawContent);
        if (InfoTipLine.IsMatch(rawContentSanitized))
            return InfoTipLine.Replace(rawContentSanitized, $"InfoTip={infoTipWithHash}");

        if (ShellClassSection.IsMatch(rawContentSanitized))
        {
            return ShellClassSection.Replace(rawContentSanitized, m =>
                $"{m.Value.TrimEnd()}\r\nInfoTip={infoTipWithHash}");
        }

        var trimmed = rawContentSanitized.TrimEnd();
        if (trimmed.Length == 0)
            return $"[.ShellClassInfo]\r\nInfoTip={infoTipWithHash}\r\n";

        return trimmed + "\r\n\r\n[.ShellClassInfo]\r\n" + $"InfoTip={infoTipWithHash}\r\n";
    }

    public static void WriteAllTextUnicode(string path, string content) =>
        File.WriteAllText(path, SanitizeErroneousViewStatePipeLine(content), Utf16LeWithBom);
}
