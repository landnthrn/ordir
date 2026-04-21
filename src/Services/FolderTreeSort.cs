using System.Collections.ObjectModel;
using Ordir.Models;

namespace Ordir.Services;

public static class FolderTreeSort
{
    /// <param name="sortIndex">0 name, 1 date modified, 2 size, 3 infotip # (when present)</param>
    public static void SortRecursive(ObservableCollection<FolderTreeNode> roots, int sortIndex)
    {
        SortChildren(roots, sortIndex);
        foreach (var r in roots)
            SortRecursive(r.Children, sortIndex);
    }

    private static void SortChildren(ObservableCollection<FolderTreeNode> nodes, int sortIndex)
    {
        if (nodes.Count <= 1) return;

        List<FolderTreeNode> ordered = sortIndex switch
        {
            1 => nodes.OrderByDescending(n => SafeLastWriteUtc(n.Row.FullPath)).ToList(),
            2 => nodes.OrderByDescending(n => FolderFileBytesOnly(n.Row.FullPath)).ToList(),
            3 => nodes
                .OrderBy(n => DesktopIniService.InfoTipNumericSortKey(n.Row.CurrentInfoTip))
                .ThenBy(n => n.Row.Name, StringComparer.OrdinalIgnoreCase)
                .ToList(),
            _ => nodes.OrderBy(n => n.Row.Name, StringComparer.OrdinalIgnoreCase).ToList()
        };

        nodes.Clear();
        foreach (var n in ordered)
            nodes.Add(n);
    }

    private static DateTime SafeLastWriteUtc(string path)
    {
        try
        {
            return new DirectoryInfo(path).LastWriteTimeUtc;
        }
        catch
        {
            return DateTime.MinValue;
        }
    }

    private static long FolderFileBytesOnly(string dir)
    {
        try
        {
            return Directory.EnumerateFiles(dir).Sum(f => new FileInfo(f).Length);
        }
        catch
        {
            return 0;
        }
    }
}
