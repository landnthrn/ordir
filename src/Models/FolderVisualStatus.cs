namespace Ordir.Models;

/// <summary>Summary / internal state after scanning a folder.</summary>
public enum FolderVisualStatus
{
    NoIni,
    IniIncomplete,
    NeedsSystemFolder,
    HealthyVisibleIni,
    HealthyHiddenIni,
    Error
}
