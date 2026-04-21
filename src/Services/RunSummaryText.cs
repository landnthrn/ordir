using System.Linq;
using System.Text;

namespace Ordir.Services;

/// <summary>User-facing sentences for the small summary strip above the activity log.</summary>
internal static class RunSummaryText
{
    internal static string ForApplyOrder(ApplyOrganizeService.ApplyResult r)
    {
        if (r.Errors.Count == 0)
        {
            if (r.Updated == 0 && r.Skipped == 0)
                return "Nothing was changed—there were no folders to process in this view.";

            var sb = new StringBuilder();
            if (r.Updated > 0)
                sb.Append($"Saved Explorer sort order into desktop.ini for {r.Updated} folder(s). ");
            if (r.Skipped > 0)
                sb.Append($"{r.Skipped} folder(s) were skipped (excluded, unchanged, or not needed).");
            return sb.ToString().Trim();
        }

        if (r.Updated > 0)
            return $"Some folders were updated ({r.Updated}), but {r.Errors.Count} could not be finished. Scroll the log above for each path and reason.";
        return $"The run stopped with {r.Errors.Count} problem(s). Scroll the log above for each folder and reason.";
    }

    internal static string ForManualOperation(ManualOperationsService.OpResult r)
    {
        if (r.Messages.Any(m => m.Contains("No included folders", StringComparison.OrdinalIgnoreCase)))
            return "There were no included folders to normalize in this scope. Turn on the checkboxes for the folders you want, then try again.";

        if (r.Messages.Any(IsHardFailureMessage))
            return $"{ActionHeadline(r.Title)} did not complete. See the red lines in the log for what went wrong.";

        if (r.Ok == 0 && r.Skipped == 0)
            return $"{ActionHeadline(r.Title)} finished; nothing needed changing.";

        if (r.Ok > 0 && r.Skipped == 0)
            return $"{ActionHeadline(r.Title)} finished successfully for {r.Ok} folder(s).";

        if (r.Ok == 0 && r.Skipped > 0)
            return $"{ActionHeadline(r.Title)} finished; {r.Skipped} folder(s) were skipped because they were already correct or did not apply.";

        return $"{ActionHeadline(r.Title)} changed {r.Ok} folder(s) and skipped {r.Skipped} that did not need a change.";
    }

    internal static string ForListExport(ManualOperationsService.OpResult r)
    {
        if (r.Ok == 0 && r.Messages.Any(m => m.Contains("does not exist", StringComparison.OrdinalIgnoreCase)))
            return "No list file was created because the active folder path is missing or invalid. Fix the path and try again.";

        var saved = r.Messages.FirstOrDefault(m => m.StartsWith("saved list", StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrEmpty(saved))
            return "Your current folder order was written to a list file in the lists folder. " + saved.TrimEnd('.') + ".";
        return "Your current folder order was written to a list file.";
    }

    internal static string ForListImport(string rootPath)
    {
        var t = rootPath.TrimEnd(System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar);
        var name = System.IO.Path.GetFileName(t);
        if (string.IsNullOrEmpty(name))
            name = rootPath;
        return $"Imported the list file and switched the active folder to “{name}”. You can review the tree on the left.";
    }

    private static bool IsHardFailureMessage(string m)
    {
        var t = m.TrimStart();
        return t.StartsWith('!')
            || t.Contains("does not exist", StringComparison.OrdinalIgnoreCase)
            || t.Contains("not found", StringComparison.OrdinalIgnoreCase)
            || t.Contains("no longer exists", StringComparison.OrdinalIgnoreCase);
    }

    private static string ActionHeadline(string title) =>
        title switch
        {
            "Create desktop.ini (first level)" => "Creating desktop.ini in first-level subfolders",
            "Create desktop.ini (recursive)" => "Creating desktop.ini in nested folders",
            "Create desktop.ini (target folder)" => "Creating desktop.ini in the target folder",
            "System folders (first level)" => "Marking system folders (first level)",
            "System folders (recursive)" => "Marking system folders (nested)",
            "Hide desktop.ini (first level)" => "Hiding desktop.ini (first level)",
            "Hide desktop.ini (recursive)" => "Hiding desktop.ini (nested)",
            "Hide desktop.ini (target folder)" => "Hiding desktop.ini in the target folder",
            "Unhide desktop.ini (first level)" => "Unhiding desktop.ini (first level)",
            "Unhide desktop.ini (recursive)" => "Unhiding desktop.ini (nested)",
            "Unhide desktop.ini (target folder)" => "Unhiding desktop.ini in the target folder",
            "Delete desktop.ini (first level)" => "Deleting desktop.ini (first level)",
            "Delete desktop.ini (all nested)" => "Deleting desktop.ini (all nested)",
            "Delete desktop.ini (target folder)" => "Deleting desktop.ini in the target folder",
            "Unhide desktop.ini (all nested)" => "Unhiding desktop.ini (all nested)",
            "Remove System attribute (first level)" => "Removing the system-folder attribute (first level)",
            "Remove System attribute (all nested)" => "Removing the system-folder attribute (nested)",
            "Remove System attribute (target folder)" => "Removing the system-folder attribute in the target folder",
            "Make System folder (target folder)" => "Making the target folder a system folder",
            "Write list (first level)" => "Writing the folder list",
            "Write list" => "Writing the folder list",
            "Apply list file" => "Applying order from a list file",
            "Normalize folder(s)" => "Normalizing folders",
            _ => title.Trim()
        };
}
