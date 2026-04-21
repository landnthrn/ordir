using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Ordir.Models;

namespace Ordir.Services;

/// <summary>Flat-list drag reorder for <see cref="FolderTreeNode"/> items among siblings only (same parent collection).</summary>
public static class FolderTreeFlatReorder
{
    public static int GetInsertIndexFromPoint(ListBox listBox, int itemCount, System.Windows.Point positionOverList)
    {
        if (itemCount <= 0) return 0;
        const double defaultRow = 28;
        double cumulative = 0;
        for (var i = 0; i < itemCount; i++)
        {
            double top;
            double h;
            if (listBox.ItemContainerGenerator.ContainerFromIndex(i) is ListBoxItem item)
            {
                top = item.TranslatePoint(new System.Windows.Point(0, 0), listBox).Y;
                h = item.ActualHeight > 1 ? item.ActualHeight : defaultRow;
            }
            else
            {
                top = cumulative;
                h = defaultRow;
            }

            if (positionOverList.Y < top + h / 2)
                return i;

            cumulative = top + h;
        }

        return itemCount;
    }

    /// <summary>Moves <paramref name="source"/> among its siblings only, matching a drop gap in flat visible order.</summary>
    public static bool TryReorderAtFlatInsertGap(
        ObservableCollection<FolderTreeNode> flatView,
        ObservableCollection<FolderTreeNode> roots,
        FolderTreeNode source,
        int insertFlat)
    {
        var list = source.Parent?.Children ?? roots;
        var ordered = list.ToList();
        var from = ordered.IndexOf(source);
        if (from < 0) return false;

        insertFlat = Math.Clamp(insertFlat, 0, flatView.Count);

        var without = ordered.Where(n => !ReferenceEquals(n, source)).ToList();
        var insertPos = 0;
        for (var i = 0; i < without.Count; i++)
        {
            if (flatView.IndexOf(without[i]) < insertFlat)
                insertPos = i + 1;
        }

        without.Insert(insertPos, source);
        if (ordered.Count != without.Count)
            return false;

        for (var i = 0; i < ordered.Count; i++)
        {
            if (!ReferenceEquals(ordered[i], without[i]))
            {
                SyncListOrder(list, without);
                return true;
            }
        }

        return false;
    }

    private static void SyncListOrder(ObservableCollection<FolderTreeNode> list, IReadOnlyList<FolderTreeNode> desired)
    {
        for (var guard = 0; guard < 512; guard++)
        {
            var moved = false;
            for (var i = 0; i < desired.Count; i++)
            {
                var cur = IndexOfReference(list, desired[i]);
                if (cur < 0 || cur == i)
                    continue;
                list.Move(cur, i);
                moved = true;
                break;
            }

            if (!moved)
                break;
        }
    }

    private static int IndexOfReference(ObservableCollection<FolderTreeNode> list, FolderTreeNode node)
    {
        for (var i = 0; i < list.Count; i++)
        {
            if (ReferenceEquals(list[i], node))
                return i;
        }

        return -1;
    }
}
