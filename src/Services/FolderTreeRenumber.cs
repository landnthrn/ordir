using System.Collections.ObjectModel;
using Ordir.Models;

namespace Ordir.Services;

public static class FolderTreeRenumber
{
    public static void RenumberAll(ObservableCollection<FolderTreeNode> roots)
    {
        var i = 1;
        foreach (var c in roots)
        {
            if (c.Row.IsExcluded) c.Row.DisplayOrder = 0;
            else c.Row.DisplayOrder = i++;
            RenumberNode(c);
        }
    }

    private static void RenumberNode(FolderTreeNode node)
    {
        var i = 1;
        foreach (var c in node.Children)
        {
            if (c.Row.IsExcluded) c.Row.DisplayOrder = 0;
            else c.Row.DisplayOrder = i++;
            RenumberNode(c);
        }
    }
}
