namespace Ordir.Services;

public static class FolderCustomizationService
{
    /// <summary>Same as <c>old-version/OrganizeFolders.ps1</c> (<c>attrib +s</c>): system attribute on the folder for <c>desktop.ini</c>.</summary>
    public static void EnsureSystemFolder(string folderPath)
    {
        var di = new DirectoryInfo(folderPath);
        di.Attributes |= FileAttributes.System;
    }

    /// <summary>Clear the system attribute (matches script undo). Also clears read-only on the directory if present from legacy Ordir builds.</summary>
    public static void RemoveSystemFolderAttribute(string folderPath)
    {
        try
        {
            var di = new DirectoryInfo(folderPath);
            if (!di.Exists) return;
            di.Attributes &= ~(FileAttributes.System | FileAttributes.ReadOnly);
        }
        catch
        {
            // ignore
        }
    }

    public static bool IsSystemFolder(string folderPath)
    {
        try
        {
            var di = new DirectoryInfo(folderPath);
            return (di.Attributes & FileAttributes.System) != 0;
        }
        catch
        {
            return false;
        }
    }

    public static void SetDesktopIniVisible(string iniPath)
    {
        if (!DesktopIniService.DesktopIniFileExistsAtPath(iniPath)) return;
        var fi = new FileInfo(iniPath);
        fi.Attributes &= ~(FileAttributes.Hidden | FileAttributes.System);
        fi.Attributes |= FileAttributes.Archive;
    }

    public static void SetDesktopIniHidden(string iniPath)
    {
        if (!DesktopIniService.DesktopIniFileExistsAtPath(iniPath)) return;
        var fi = new FileInfo(iniPath);
        fi.Attributes |= FileAttributes.System | FileAttributes.Hidden;
    }

    public static bool IsDesktopIniHidden(string iniPath)
    {
        try
        {
            if (!DesktopIniService.DesktopIniFileExistsAtPath(iniPath)) return false;
            var fi = new FileInfo(iniPath);
            fi.Refresh();
            return (fi.Attributes & FileAttributes.Hidden) != 0;
        }
        catch
        {
            return false;
        }
    }
}
