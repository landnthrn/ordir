using System.Collections.ObjectModel;
using Ordir.Models;

namespace Ordir.Services;

/// <summary>
/// Builds a pre-order list of visible <see cref="FolderTreeNode"/> rows for a full-width list UI
/// (avoids WPF <see cref="System.Windows.Controls.TreeView"/> indent breaking right-aligned chrome).
/// </summary>
public static class FolderTreeFlattener
{
    public static void FillVisible(IEnumerable<FolderTreeNode> roots, ObservableCollection<FolderTreeNode> sink)
    {
        sink.Clear();
        foreach (var root in roots)
            Walk(root, sink);
    }

    private static void Walk(FolderTreeNode node, ObservableCollection<FolderTreeNode> sink)
    {
        sink.Add(node);
        if (!node.IsExpanded)
            return;
        foreach (var child in node.Children)
            Walk(child, sink);
    }
}
