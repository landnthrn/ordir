namespace Ordir.Services;

/// <summary>Ordir list .txt format: title line, absolute root line, then "Name tip" lines.</summary>
public static class FolderListFile
{
    /// <summary>Finds the first absolute Windows path line (drive:\...).</summary>
    public static bool TryParseRootLine(IReadOnlyList<string> lines, out string? root)
    {
        foreach (var line in lines)
        {
            var t = line.Trim();
            if (t.Length >= 3
                && char.IsLetter(t[0])
                && t[1] == ':'
                && (t[2] == '\\' || t[2] == '/'))
            {
                root = t.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                return true;
            }
        }

        root = null;
        return false;
    }
}
