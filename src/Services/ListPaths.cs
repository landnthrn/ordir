namespace Ordir.Services;

/// <summary>Default location for exported / working list .txt files (user can still pick elsewhere).</summary>
public static class ListPaths
{
    public static string ListsDirectory =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Ordir", "lists");

    public static void EnsureListsDirectory() =>
        Directory.CreateDirectory(ListsDirectory);
}
