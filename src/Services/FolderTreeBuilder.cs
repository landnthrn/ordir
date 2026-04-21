using System.Collections.ObjectModel;
using System.IO;
using Ordir.Models;

namespace Ordir.Services;

public static class FolderTreeBuilder
{
    public static ObservableCollection<FolderTreeNode> LoadRoots(string parentPath)
    {
        var roots = new ObservableCollection<FolderTreeNode>();
        if (!Directory.Exists(parentPath)) return roots;

        foreach (var path in SafeEnumerateDirectories(parentPath))
            roots.Add(BuildNode(path, parent: null, depth: 0));

        return roots;
    }

    private static IEnumerable<string> SafeEnumerateDirectories(string directory)
    {
        string[] dirs;
        try
        {
            dirs = Directory.GetDirectories(directory);
        }
        catch (UnauthorizedAccessException)
        {
            yield break;
        }
        catch (IOException)
        {
            yield break;
        }

        foreach (var d in dirs.OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
            yield return d;
    }

    private static FolderTreeNode BuildNode(string path, FolderTreeNode? parent, int depth)
    {
        var name = Path.GetFileName(path);
        if (string.IsNullOrEmpty(name)) name = path;

        var row = new FolderRow
        {
            Name = name,
            FullPath = path,
            DisplayOrder = 0
        };
        FolderScanner.Classify(row);

        var node = new FolderTreeNode(row)
        {
            TreeDepth = depth
        };
        node.SetParent(parent);

        foreach (var sub in SafeEnumerateDirectories(path))
        {
            try
            {
                var child = BuildNode(sub, node, depth + 1);
                node.Children.Add(child);
            }
            catch (UnauthorizedAccessException)
            {
                // Skip protected / redirected special folders (e.g. "My Music") under user libraries.
            }
            catch (IOException)
            {
                // Skip unreadable subtrees.
            }
        }

        return node;
    }
}
