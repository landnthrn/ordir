using Ordir.Models;

namespace Ordir.Services;

public sealed class ApplyOrganizeService
{
    public sealed record ApplyResult(int Updated, int Skipped, List<string> Errors);

    /// <summary>
    /// Flat list: one global sequence #1..N in list order.
    /// </summary>
    public static ApplyResult Apply(
        string parentPath,
        IReadOnlyList<FolderRow> orderedRows,
        Action<string>? log = null)
    {
        _ = parentPath;
        var errors = new List<string>();
        var updated = 0;
        var skipped = 0;
        var index = 1;

        foreach (var row in orderedRows)
        {
            if (ApplyOneRow(row, index, ref updated, ref skipped, errors, log))
                index++;
        }

        return new ApplyResult(updated, skipped, errors);
    }

    /// <summary>
    /// Tree: for each folder, assign #1..N among its direct children (in UI order), then recurse.
    /// </summary>
    public static ApplyResult ApplyTree(IReadOnlyList<FolderTreeNode> roots, Action<string>? log = null)
    {
        var errors = new List<string>();
        var updated = 0;
        var skipped = 0;

        ApplyChildren(roots, ref updated, ref skipped, errors, log);
        foreach (var r in roots)
            ApplySubtree(r, ref updated, ref skipped, errors, log);

        return new ApplyResult(updated, skipped, errors);
    }

    private static void ApplySubtree(
        FolderTreeNode node,
        ref int updated,
        ref int skipped,
        List<string> errors,
        Action<string>? log)
    {
        ApplyChildren(node.Children, ref updated, ref skipped, errors, log);
        foreach (var c in node.Children)
            ApplySubtree(c, ref updated, ref skipped, errors, log);
    }

    private static void ApplyChildren(
        IEnumerable<FolderTreeNode> children,
        ref int updated,
        ref int skipped,
        List<string> errors,
        Action<string>? log)
    {
        var index = 1;
        foreach (var ch in children)
        {
            if (ApplyOneRow(ch.Row, index, ref updated, ref skipped, errors, log))
                index++;
        }
    }

    /// <returns>True if the next sibling should use the next sequential InfoTip number.</returns>
    private static bool ApplyOneRow(
        FolderRow row,
        int index,
        ref int updated,
        ref int skipped,
        List<string> errors,
        Action<string>? log)
    {
        if (row.IsExcluded)
        {
            skipped++;
            log?.Invoke($"skip (excluded): {row.Name}");
            return false;
        }

        var folder = row.FullPath;
        var iniPath = DesktopIniService.DesktopIniPath(folder);

        if (!Directory.Exists(folder))
        {
            errors.Add($"Missing folder: {folder}");
            skipped++;
            log?.Invoke($"error: missing {folder}");
            return false;
        }

        try
        {
                var iniExisted = DesktopIniService.DesktopIniFileExistsAtPath(iniPath);
            var wasHidden = iniExisted && FolderCustomizationService.IsDesktopIniHidden(iniPath);
            if (wasHidden)
            {
                FolderCustomizationService.SetDesktopIniVisible(iniPath);
                log?.Invoke($"unhide ini (temp) → {row.Name}");
            }

            var infoTipValue = $"#{index}";

                if (DesktopIniService.DesktopIniFileExistsAtPath(iniPath))
                {
                    var raw = DesktopIniService.ReadAllTextLenient(iniPath);
                var merged = DesktopIniService.MergeInfoTipIntoIni(raw, infoTipValue);
                DesktopIniService.WriteAllTextUnicode(iniPath, merged);
                log?.Invoke($"merge InfoTip {infoTipValue} → {row.Name}");
            }
            else
            {
                var template = DesktopIniService.BuildTemplate(index);
                DesktopIniService.WriteAllTextUnicode(iniPath, template);
                FolderCustomizationService.SetDesktopIniVisible(iniPath);
                log?.Invoke($"create desktop.ini {infoTipValue} → {row.Name}");
            }

            FolderCustomizationService.EnsureSystemFolder(folder);
            updated++;

                if (DesktopIniService.DesktopIniFileExistsAtPath(iniPath))
                {
                    FolderCustomizationService.SetDesktopIniHidden(iniPath);
                log?.Invoke(wasHidden ? $"restore hidden ini → {row.Name}" : $"hide ini → {row.Name}");
            }

            return true;
        }
        catch (Exception ex)
        {
            errors.Add($"{row.Name}: {ex.Message}");
            skipped++;
            log?.Invoke($"error: {row.Name}: {ex.Message}");
            return false;
        }
    }
}
