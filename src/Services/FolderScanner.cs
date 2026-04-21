using Ordir.Models;

namespace Ordir.Services;

public static class FolderScanner
{
    public static string TryReadInfoTipForFolder(string folderPath)
    {
        if (!DesktopIniService.DesktopIniExistsInFolder(folderPath)) return string.Empty;
        var ini = DesktopIniService.DesktopIniPath(folderPath);
        try
        {
            var raw = DesktopIniService.ReadAllTextLenient(ini);
            return DesktopIniService.ParseInfoTipRaw(raw) ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    /// <summary>Single row for the target folder itself (not its subfolders).</summary>
    public static IReadOnlyList<FolderRow> LoadTargetFolderOnly(string folderPath)
    {
        if (!Directory.Exists(folderPath))
            return Array.Empty<FolderRow>();

        var trimmed = folderPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var name = Path.GetFileName(trimmed);
        if (string.IsNullOrEmpty(name)) name = trimmed;

        var row = new FolderRow
        {
            Name = name,
            FullPath = trimmed,
            DisplayOrder = 1
        };
        Classify(row);
        return new[] { row };
    }

    public static IReadOnlyList<FolderRow> LoadFirstLevelChildren(string parentPath)
    {
        if (!Directory.Exists(parentPath))
            return Array.Empty<FolderRow>();

        var dirs = Directory.GetDirectories(parentPath);
        Array.Sort(dirs, StringComparer.OrdinalIgnoreCase);

        var list = new List<FolderRow>();
        var order = 1;
        foreach (var path in dirs)
        {
            var name = Path.GetFileName(path);
            if (string.IsNullOrEmpty(name)) continue;

            var row = new FolderRow
            {
                Name = name,
                FullPath = path,
                DisplayOrder = order++
            };
            Classify(row);
            list.Add(row);
        }

        return list;
    }

    public static void Classify(FolderRow row)
    {
        try
        {
            var iniPath = DesktopIniService.DesktopIniPath(row.FullPath);
            var hasIni = DesktopIniService.DesktopIniExistsInFolder(row.FullPath);
            var sys = FolderCustomizationService.IsSystemFolder(row.FullPath);

            if (!hasIni)
            {
                row.SetScanState(FolderVisualStatus.NoIni, null, false, sys, false, false);
                return;
            }

            string raw;
            try
            {
                raw = DesktopIniService.ReadAllTextLenient(iniPath);
            }
            catch
            {
                row.SetScanState(FolderVisualStatus.Error, null, true, sys, false, false);
                return;
            }

            var tip = DesktopIniService.ParseInfoTipRaw(raw);
            var hiddenIni = FolderCustomizationService.IsDesktopIniHidden(iniPath);
            var tipOk = DesktopIniService.IsWellFormedOrderTip(tip);

            if (!tipOk)
            {
                row.SetScanState(FolderVisualStatus.IniIncomplete, tip, true, sys, hiddenIni, false);
                return;
            }

            if (!sys)
            {
                // Valid order tip + ini present: show blue ini badge (hidden eye / slash), not "misconfigured" red.
                row.SetScanState(
                    hiddenIni ? FolderVisualStatus.HealthyHiddenIni : FolderVisualStatus.HealthyVisibleIni,
                    tip, true, false, hiddenIni, true);
                return;
            }

            row.SetScanState(
                hiddenIni ? FolderVisualStatus.HealthyHiddenIni : FolderVisualStatus.HealthyVisibleIni,
                tip, true, true, hiddenIni, true);
        }
        catch
        {
            row.SetScanState(FolderVisualStatus.Error, null, false, false, false, false);
        }
    }

    public static (int WithIni, int WithoutIni, int Ready, int Misconfigured, int Excluded) Summarize(
        IEnumerable<FolderRow> rows)
    {
        var withIni = 0;
        var without = 0;
        var ready = 0;
        var misconfigured = 0;
        var excluded = 0;

        foreach (var r in rows)
        {
            if (r.IsExcluded) excluded++;

            if (r.Status == FolderVisualStatus.NoIni) without++;
            else withIni++;

            if (r.Status is FolderVisualStatus.HealthyVisibleIni or FolderVisualStatus.HealthyHiddenIni)
                ready++;
            else if (r.Status is FolderVisualStatus.IniIncomplete
                     or FolderVisualStatus.NeedsSystemFolder
                     or FolderVisualStatus.Error)
                misconfigured++;
        }

        return (withIni, without, ready, misconfigured, excluded);
    }
}
