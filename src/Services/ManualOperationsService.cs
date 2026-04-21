using System.Text;
using System.Text.RegularExpressions;
using Ordir.Models;

namespace Ordir.Services;

public static class ManualOperationsService
{
    public sealed record OpResult(string Title, int Ok, int Skipped, IReadOnlyList<string> Messages);

    private static string[] ReadListLines(string listPath) =>
        File.ReadAllText(listPath, Encoding.UTF8).Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

    public static OpResult CreateDesktopIniFirstLevel(string root)
    {
        if (!Directory.Exists(root))
            return new OpResult("Create desktop.ini (first level)", 0, 0, new[] { "Folder does not exist." });

        var n = 1;
        var ok = 0;
        var skipped = 0;
        var messages = new List<string>();
        foreach (var dir in Directory.GetDirectories(root).OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
        {
            var name = Path.GetFileName(dir);
            var ini = DesktopIniService.DesktopIniPath(dir);
            try
            {
                if (DesktopIniService.DesktopIniExistsInFolder(dir))
                {
                    skipped++;
                    messages.Add($"skip (desktop.ini already exists): {name}");
                }
                else
                {
                    DesktopIniService.WriteAllTextUnicode(ini, DesktopIniService.BuildTemplate(n));
                    FolderCustomizationService.SetDesktopIniVisible(ini);
                    ok++;
                    messages.Add($"created desktop.ini #{n} → {name}");
                }
            }
            catch (Exception ex)
            {
                messages.Add($"error {name}: {ex.Message}");
            }

            n++;
        }

        messages.Add($"Summary — Create desktop.ini (first level): OK {ok}, skipped {skipped}.");
        return new OpResult("Create desktop.ini (first level)", ok, skipped, messages);
    }

    public static OpResult CreateDesktopIniRecursive(string root)
    {
        if (!Directory.Exists(root))
            return new OpResult("Create desktop.ini (recursive)", 0, 0, new[] { "Folder does not exist." });

        var parents = new List<string> { root };
        parents.AddRange(Directory.GetDirectories(root, "*", SearchOption.AllDirectories));
        var ok = 0;
        var skipped = 0;
        var messages = new List<string>();

        foreach (var parent in parents)
        {
            var children = Directory.GetDirectories(parent).OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToList();
            if (children.Count == 0) continue;

            var counter = 1;
            foreach (var child in children)
            {
                var cname = Path.GetFileName(child);
                var ini = DesktopIniService.DesktopIniPath(child);
                try
                {
                    if (DesktopIniService.DesktopIniExistsInFolder(child))
                    {
                        skipped++;
                        messages.Add($"skip (ini exists): {child}");
                    }
                    else
                    {
                        DesktopIniService.WriteAllTextUnicode(ini, DesktopIniService.BuildTemplate(counter));
                        FolderCustomizationService.SetDesktopIniVisible(ini);
                        ok++;
                        messages.Add($"created desktop.ini #{counter} → {cname}");
                    }
                }
                catch (Exception ex)
                {
                    messages.Add($"error {cname}: {ex.Message}");
                }

                counter++;
            }
        }

        messages.Add($"Summary — Create desktop.ini (recursive): OK {ok}, skipped {skipped}.");
        return new OpResult("Create desktop.ini (recursive)", ok, skipped, messages);
    }

    public static OpResult EnsureSystemFoldersFirstLevel(string root)
    {
        if (!Directory.Exists(root))
            return new OpResult("System folders (first level)", 0, 0, new[] { "Folder does not exist." });

        var ok = 0;
        var skipped = 0;
        var messages = new List<string>();
        foreach (var dir in Directory.GetDirectories(root))
        {
            var name = Path.GetFileName(dir);
            if (DesktopIniService.DesktopIniExistsInFolder(dir))
            {
                FolderCustomizationService.EnsureSystemFolder(dir);
                ok++;
                messages.Add($"make system → {name}");
            }
            else
            {
                skipped++;
                messages.Add($"skip (no desktop.ini): {name}");
            }
        }

        messages.Add($"Summary — System folders (first level): OK {ok}, skipped {skipped}.");
        return new OpResult("System folders (first level)", ok, skipped, messages);
    }

    public static OpResult EnsureSystemFoldersRecursive(string root)
    {
        if (!Directory.Exists(root))
            return new OpResult("System folders (recursive)", 0, 0, new[] { "Folder does not exist." });

        var ok = 0;
        var skipped = 0;
        var messages = new List<string>();
        foreach (var dir in Directory.GetDirectories(root, "*", SearchOption.AllDirectories))
        {
            var name = Path.GetFileName(dir);
            if (DesktopIniService.DesktopIniExistsInFolder(dir))
            {
                FolderCustomizationService.EnsureSystemFolder(dir);
                ok++;
                messages.Add($"make system → {name}");
            }
            else
            {
                skipped++;
            }
        }

        messages.Add($"Summary — System folders (recursive): OK {ok}, skipped {skipped}.");
        return new OpResult("System folders (recursive)", ok, skipped, messages);
    }

    public static OpResult WriteListFirstLevel(string root, string outputDirectory)
    {
        if (!Directory.Exists(root))
            return new OpResult("Write list (first level)", 0, 0, new[] { "Source folder does not exist." });

        Directory.CreateDirectory(outputDirectory);
        var folderName = Path.GetFileName(root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        var baseName = $"List_For_First_Subfolders_Inside_{folderName}.txt";
        var outPath = Path.Combine(outputDirectory, baseName);
        for (var i = 2; File.Exists(outPath); i++)
            outPath = Path.Combine(outputDirectory, $"{Path.GetFileNameWithoutExtension(baseName)}_{i}.txt");

        var lines = new List<string>
        {
            $"{folderName} Folder",
            root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar
        };

        foreach (var d in Directory.GetDirectories(root).OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
        {
            var tip = FolderScanner.TryReadInfoTipForFolder(d);
            lines.Add($"{Path.GetFileName(d)} {tip}".TrimEnd());
        }

        File.WriteAllLines(outPath, lines, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        var messages = new List<string> { $"saved list → {outPath}", "Summary — Write list (first level): OK 1." };
        return new OpResult("Write list (first level)", 1, 0, messages);
    }

    public static OpResult WriteListOrderedFirstLevel(string root, IReadOnlyList<FolderRow> foldersInDisplayOrder, string outputDirectory)
    {
        if (!Directory.Exists(root))
            return new OpResult("Write list", 0, 0, new[] { "Source folder does not exist." });

        Directory.CreateDirectory(outputDirectory);
        var folderName = Path.GetFileName(root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        var baseName = $"List_For_First_Subfolders_Inside_{folderName}.txt";
        var outPath = Path.Combine(outputDirectory, baseName);
        for (var i = 2; File.Exists(outPath); i++)
            outPath = Path.Combine(outputDirectory, $"{Path.GetFileNameWithoutExtension(baseName)}_{i}.txt");

        var lines = new List<string>
        {
            $"{folderName} Folder",
            root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar
        };

        foreach (var row in foldersInDisplayOrder)
        {
            var tip = FolderScanner.TryReadInfoTipForFolder(row.FullPath);
            lines.Add($"{row.Name} {tip}".TrimEnd());
        }

        File.WriteAllLines(outPath, lines, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        var messages = new List<string> { $"saved list → {outPath}", "Summary — Write list: OK 1." };
        return new OpResult("Write list", 1, 0, messages);
    }

    public static OpResult WriteListOrderedTreeRoots(string root, IReadOnlyList<FolderTreeNode> rootsInOrder, string outputDirectory)
    {
        if (!Directory.Exists(root))
            return new OpResult("Write list", 0, 0, new[] { "Source folder does not exist." });

        Directory.CreateDirectory(outputDirectory);
        var folderName = Path.GetFileName(root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        var baseName = $"List_Tree_Roots_Inside_{folderName}.txt";
        var outPath = Path.Combine(outputDirectory, baseName);
        for (var i = 2; File.Exists(outPath); i++)
            outPath = Path.Combine(outputDirectory, $"{Path.GetFileNameWithoutExtension(baseName)}_{i}.txt");

        var lines = new List<string>
        {
            $"{folderName} Folder",
            root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar
        };

        foreach (var node in rootsInOrder)
        {
            var tip = FolderScanner.TryReadInfoTipForFolder(node.Row.FullPath);
            lines.Add($"{node.Row.Name} {tip}".TrimEnd());
        }

        File.WriteAllLines(outPath, lines, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        var messages = new List<string> { $"saved list → {outPath}", "Summary — Write list: OK 1." };
        return new OpResult("Write list", 1, 0, messages);
    }

    public static OpResult ApplyListFile(string listPath)
    {
        if (!File.Exists(listPath))
            return new OpResult("Apply list file", 0, 0, new[] { "List file not found." });

        var lines = ReadListLines(listPath);
        string? currentParent = null;
        var updated = 0;
        var skipped = 0;
        var messages = new List<string>();
        var lineRx = new Regex(@"^(.+?)\s+#(\d+)$", RegexOptions.CultureInvariant);

        foreach (var line in lines)
        {
            var trim = line.Trim();
            if (trim.Length >= 3 && char.IsLetter(trim[0]) && trim[1] == ':' && trim[2] == '\\')
            {
                currentParent = trim.TrimEnd('\\', '/');
                continue;
            }

            if (string.IsNullOrEmpty(trim) || trim.EndsWith("Folder", StringComparison.Ordinal)) continue;

            var m = lineRx.Match(trim);
            if (!m.Success || currentParent is null) continue;

            var folderName = m.Groups[1].Value.Trim();
            var infoTip = $"#{m.Groups[2].Value}";
            var target = Path.Combine(currentParent, folderName);
            var ini = DesktopIniService.DesktopIniPath(target);

            if (!Directory.Exists(target) || !DesktopIniService.DesktopIniFileExistsAtPath(ini))
            {
                skipped++;
                continue;
            }

            var raw = DesktopIniService.ReadAllTextLenient(ini);
            var merged = DesktopIniService.MergeInfoTipIntoIni(raw, infoTip);
            DesktopIniService.WriteAllTextUnicode(ini, merged);
            updated++;
            messages.Add($"merge {infoTip} → {folderName}");
        }

        messages.Add($"Summary — Apply list file: OK {updated}, skipped {skipped}.");
        return new OpResult("Apply list file", updated, skipped, messages);
    }

    public static OpResult HideIniFirstLevel(string root)
    {
        if (!Directory.Exists(root))
            return new OpResult("Hide desktop.ini (first level)", 0, 0, new[] { "Folder does not exist." });

        var ok = 0;
        var skipped = 0;
        var messages = new List<string>();
        foreach (var dir in Directory.GetDirectories(root))
        {
            var name = Path.GetFileName(dir);
            if (!DesktopIniService.DesktopIniExistsInFolder(dir))
            {
                skipped++;
                messages.Add($"skip (no desktop.ini): {name}");
                continue;
            }

            var ini = DesktopIniService.DesktopIniPath(dir);
            FolderCustomizationService.SetDesktopIniHidden(ini);
            ok++;
            messages.Add($"hide desktop.ini → {name}");
        }

        messages.Add($"Summary — Hide desktop.ini (first level): OK {ok}, skipped {skipped}.");
        return new OpResult("Hide desktop.ini (first level)", ok, skipped, messages);
    }

    public static OpResult HideIniRecursive(string root)
    {
        if (!Directory.Exists(root))
            return new OpResult("Hide desktop.ini (recursive)", 0, 0, new[] { "Folder does not exist." });

        var ok = 0;
        var skipped = 0;
        var messages = new List<string>();
        foreach (var dir in Directory.GetDirectories(root, "*", SearchOption.AllDirectories))
        {
            var name = Path.GetFileName(dir);
            if (!DesktopIniService.DesktopIniExistsInFolder(dir))
            {
                skipped++;
                continue;
            }

            var ini = DesktopIniService.DesktopIniPath(dir);
            FolderCustomizationService.SetDesktopIniHidden(ini);
            ok++;
            messages.Add($"hide desktop.ini → {dir}");
        }

        messages.Add($"Summary — Hide desktop.ini (recursive): OK {ok}, skipped {skipped}.");
        return new OpResult("Hide desktop.ini (recursive)", ok, skipped, messages);
    }

    public static OpResult UnhideIniFirstLevel(string root)
    {
        if (!Directory.Exists(root))
            return new OpResult("Unhide desktop.ini (first level)", 0, 0, new[] { "Folder does not exist." });

        var ok = 0;
        var skipped = 0;
        var messages = new List<string>();
        foreach (var dir in Directory.GetDirectories(root))
        {
            var name = Path.GetFileName(dir);
            if (!DesktopIniService.DesktopIniExistsInFolder(dir))
            {
                skipped++;
                messages.Add($"skip (no desktop.ini): {name}");
                continue;
            }

            var ini = DesktopIniService.DesktopIniPath(dir);
            FolderCustomizationService.SetDesktopIniVisible(ini);
            ok++;
            messages.Add($"unhide desktop.ini → {name}");
        }

        messages.Add($"Summary — Unhide desktop.ini (first level): OK {ok}, skipped {skipped}.");
        return new OpResult("Unhide desktop.ini (first level)", ok, skipped, messages);
    }

    public static OpResult DeleteIniFirstLevel(string root)
    {
        if (!Directory.Exists(root))
            return new OpResult("Delete desktop.ini (first level)", 0, 0, new[] { "Folder does not exist." });

        var ok = 0;
        var skipped = 0;
        var messages = new List<string>();
        foreach (var dir in Directory.GetDirectories(root))
        {
            var name = Path.GetFileName(dir);
            if (!DesktopIniService.DesktopIniExistsInFolder(dir))
            {
                skipped++;
                messages.Add($"skip (no desktop.ini): {name}");
                continue;
            }

            File.Delete(DesktopIniService.DesktopIniPath(dir));
            ok++;
            messages.Add($"delete desktop.ini → {name}");
        }

        messages.Add($"Summary — Delete desktop.ini (first level): OK {ok}, skipped {skipped}.");
        return new OpResult("Delete desktop.ini (first level)", ok, skipped, messages);
    }

    public static OpResult DeleteIniRecursive(string root)
    {
        if (!Directory.Exists(root))
            return new OpResult("Delete desktop.ini (all nested)", 0, 0, new[] { "Folder does not exist." });

        var ok = 0;
        var skipped = 0;
        var messages = new List<string>();
        foreach (var dir in Directory.GetDirectories(root, "*", SearchOption.AllDirectories))
        {
            if (!DesktopIniService.DesktopIniExistsInFolder(dir))
            {
                skipped++;
                continue;
            }

            File.Delete(DesktopIniService.DesktopIniPath(dir));
            ok++;
            messages.Add($"delete desktop.ini → {dir}");
        }

        messages.Add($"Summary — Delete desktop.ini (all nested): OK {ok}, skipped {skipped}.");
        return new OpResult("Delete desktop.ini (all nested)", ok, skipped, messages);
    }

    public static OpResult UnhideIniRecursive(string root)
    {
        if (!Directory.Exists(root))
            return new OpResult("Unhide desktop.ini (all nested)", 0, 0, new[] { "Folder does not exist." });

        var ok = 0;
        var skipped = 0;
        var messages = new List<string>();
        foreach (var dir in Directory.GetDirectories(root, "*", SearchOption.AllDirectories))
        {
            if (!DesktopIniService.DesktopIniExistsInFolder(dir))
            {
                skipped++;
                continue;
            }

            FolderCustomizationService.SetDesktopIniVisible(DesktopIniService.DesktopIniPath(dir));
            ok++;
            messages.Add($"unhide desktop.ini → {dir}");
        }

        messages.Add($"Summary — Unhide desktop.ini (all nested): OK {ok}, skipped {skipped}.");
        return new OpResult("Unhide desktop.ini (all nested)", ok, skipped, messages);
    }

    public static OpResult RemoveSystemFromIniFoldersFirstLevel(string root)
    {
        if (!Directory.Exists(root))
            return new OpResult("Remove System attribute (first level)", 0, 0, new[] { "Folder does not exist." });

        var ok = 0;
        var skipped = 0;
        var messages = new List<string>();
        foreach (var dir in Directory.GetDirectories(root))
        {
            var name = Path.GetFileName(dir);
            if (!FolderCustomizationService.IsSystemFolder(dir))
            {
                skipped++;
                messages.Add($"skip (not a system folder): {name}");
                continue;
            }

            FolderCustomizationService.RemoveSystemFolderAttribute(dir);
            ok++;
            messages.Add($"undo system → {name}");
        }

        messages.Add($"Summary — Remove System attribute (first level): OK {ok}, skipped {skipped}.");
        return new OpResult("Remove System attribute (first level)", ok, skipped, messages);
    }

    public static OpResult RemoveSystemFromIniFoldersRecursive(string root)
    {
        if (!Directory.Exists(root))
            return new OpResult("Remove System attribute (all nested)", 0, 0, new[] { "Folder does not exist." });

        var ok = 0;
        var skipped = 0;
        var messages = new List<string>();
        foreach (var dir in Directory.GetDirectories(root, "*", SearchOption.AllDirectories))
        {
            var name = Path.GetFileName(dir);
            if (!FolderCustomizationService.IsSystemFolder(dir))
            {
                skipped++;
                continue;
            }

            FolderCustomizationService.RemoveSystemFolderAttribute(dir);
            ok++;
            messages.Add($"undo system → {dir}");
        }

        messages.Add($"Summary — Remove System attribute (all nested): OK {ok}, skipped {skipped}.");
        return new OpResult("Remove System attribute (all nested)", ok, skipped, messages);
    }

    public static OpResult CreateDesktopIniTargetFolderOnly(string root)
    {
        if (!Directory.Exists(root))
            return new OpResult("Create desktop.ini (target folder)", 0, 0, new[] { "Folder does not exist." });

        var ini = DesktopIniService.DesktopIniPath(root);
        try
        {
            if (DesktopIniService.DesktopIniExistsInFolder(root))
            {
                return new OpResult("Create desktop.ini (target folder)", 0, 1,
                    new[] { "skip: desktop.ini already exists in target folder.", "Summary — OK 0, skipped 1." });
            }

            DesktopIniService.WriteAllTextUnicode(ini, DesktopIniService.BuildTemplate(1));
            FolderCustomizationService.SetDesktopIniVisible(ini);
            return new OpResult("Create desktop.ini (target folder)", 1, 0,
                new[] { $"created desktop.ini #1 → {Path.GetFileName(root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))}", "Summary — OK 1." });
        }
        catch (Exception ex)
        {
            return new OpResult("Create desktop.ini (target folder)", 0, 0, new[] { ex.Message });
        }
    }

    public static OpResult HideIniTargetFolderOnly(string root)
    {
        if (!Directory.Exists(root))
            return new OpResult("Hide desktop.ini (target folder)", 0, 0, new[] { "Folder does not exist." });

        if (!DesktopIniService.DesktopIniExistsInFolder(root))
            return new OpResult("Hide desktop.ini (target folder)", 0, 0, new[] { "No desktop.ini in that folder." });

        FolderCustomizationService.SetDesktopIniHidden(DesktopIniService.DesktopIniPath(root));
        return new OpResult("Hide desktop.ini (target folder)", 1, 0,
            new[] { $"hide desktop.ini → {Path.GetFileName(root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))}", "Summary — OK 1." });
    }

    public static OpResult UnhideIniTargetFolderOnly(string root)
    {
        if (!Directory.Exists(root))
            return new OpResult("Unhide desktop.ini (target folder)", 0, 0, new[] { "Folder does not exist." });

        if (!DesktopIniService.DesktopIniExistsInFolder(root))
            return new OpResult("Unhide desktop.ini (target folder)", 0, 0, new[] { "No desktop.ini in that folder." });

        FolderCustomizationService.SetDesktopIniVisible(DesktopIniService.DesktopIniPath(root));
        return new OpResult("Unhide desktop.ini (target folder)", 1, 0,
            new[] { $"unhide desktop.ini → {Path.GetFileName(root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))}", "Summary — OK 1." });
    }

    public static OpResult DeleteIniTargetFolderOnly(string root)
    {
        if (!Directory.Exists(root))
            return new OpResult("Delete desktop.ini (target folder)", 0, 0, new[] { "Folder does not exist." });

        if (!DesktopIniService.DesktopIniExistsInFolder(root))
            return new OpResult("Delete desktop.ini (target folder)", 0, 0, new[] { "No desktop.ini in that folder." });

        File.Delete(DesktopIniService.DesktopIniPath(root));
        return new OpResult("Delete desktop.ini (target folder)", 1, 0,
            new[] { $"delete desktop.ini → {Path.GetFileName(root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))}", "Summary — OK 1." });
    }

    public static OpResult RemoveSystemTargetFolderOnly(string root)
    {
        if (!Directory.Exists(root))
            return new OpResult("Remove System attribute (target folder)", 0, 0, new[] { "Folder does not exist." });

        if (!FolderCustomizationService.IsSystemFolder(root))
            return new OpResult("Remove System attribute (target folder)", 0, 1,
                new[] { "skip: target folder is not marked as a system folder.", "Summary — OK 0, skipped 1." });

        FolderCustomizationService.RemoveSystemFolderAttribute(root);
        return new OpResult("Remove System attribute (target folder)", 1, 0,
            new[] { $"undo system → {Path.GetFileName(root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))}", "Summary — OK 1." });
    }

    public static OpResult EnsureSystemTargetFolderOnly(string root)
    {
        if (!Directory.Exists(root))
            return new OpResult("Make System folder (target folder)", 0, 0, new[] { "Folder does not exist." });

        if (!DesktopIniService.DesktopIniExistsInFolder(root))
            return new OpResult("Make System folder (target folder)", 0, 0, new[] { "No desktop.ini in that folder." });

        FolderCustomizationService.EnsureSystemFolder(root);
        return new OpResult("Make System folder (target folder)", 1, 0,
            new[] { $"make system → {Path.GetFileName(root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))}", "Summary — OK 1." });
    }

    /// <summary>Removes <c>desktop.ini</c> when present and clears system / legacy read-only marks on each folder.</summary>
    public static OpResult NormalizeFolderPaths(IEnumerable<string> folderPaths)
    {
        var messages = new List<string>();
        var ok = 0;
        var skipped = 0;
        foreach (var folder in folderPaths.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(folder) || !Directory.Exists(folder))
            {
                skipped++;
                messages.Add($"skip (missing): {folder}");
                continue;
            }

            var name = Path.GetFileName(folder.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            try
            {
                var ini = DesktopIniService.DesktopIniPath(folder);
                if (DesktopIniService.DesktopIniFileExistsAtPath(ini))
                {
                    FolderCustomizationService.SetDesktopIniVisible(ini);
                    File.Delete(ini);
                }

                FolderCustomizationService.RemoveSystemFolderAttribute(folder);
                ok++;
                messages.Add($"normalize → {name}");
            }
            catch (Exception ex)
            {
                skipped++;
                messages.Add($"error {name}: {ex.Message}");
            }
        }

        messages.Add($"Summary — Normalize: OK {ok}, skipped/errors {skipped}.");
        return new OpResult("Normalize folder(s)", ok, skipped, messages);
    }
}
