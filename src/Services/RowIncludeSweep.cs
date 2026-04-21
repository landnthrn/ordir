using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Ordir.Models;

namespace Ordir.Services;

/// <summary>Detects row include-toggle press and hit-tests rows during a click-drag to paint the same include state.</summary>
internal static class RowIncludeSweep
{
    internal static bool IsRowIncludeTogglePress(MouseButtonEventArgs e, out ListBoxItem? rowItem)
    {
        rowItem = null;
        if (e.ChangedButton != MouseButton.Left)
            return false;

        ToggleButton? toggle = null;
        for (var o = e.OriginalSource as DependencyObject; o != null; o = VisualTreeHelper.GetParent(o))
        {
            if (o is ToggleButton tb)
                toggle = tb;
            if (o is ListBoxItem li)
            {
                if (toggle == null)
                    return false;
                rowItem = li;
                return true;
            }
        }

        return false;
    }

    internal static bool TryBegin(ListBox listBox, MouseButtonEventArgs e, bool expectTree, out bool targetIncluded)
    {
        targetIncluded = false;
        if (!IsRowIncludeTogglePress(e, out var li))
            return false;
        if (li is null || li.DataContext is null)
            return false;

        if (expectTree)
        {
            if (li.DataContext is not FolderTreeNode tn)
                return false;
            targetIncluded = !tn.Row.IsIncluded;
        }
        else
        {
            if (li.DataContext is not FolderRow r)
                return false;
            targetIncluded = !r.IsIncluded;
        }

        Mouse.Capture(listBox);
        return true;
    }

    internal static bool TryHitRow(ListBox listBox, System.Windows.Point positionOverList, bool expectTree, out FolderRow? flat,
        out FolderTreeNode? tree)
    {
        flat = null;
        tree = null;
        var hit = VisualTreeHelper.HitTest(listBox, positionOverList);
        for (var o = hit?.VisualHit as DependencyObject; o != null; o = VisualTreeHelper.GetParent(o))
        {
            if (o is not ListBoxItem li)
                continue;
            if (expectTree && li.DataContext is FolderTreeNode tn)
            {
                tree = tn;
                return true;
            }

            if (!expectTree && li.DataContext is FolderRow r)
            {
                flat = r;
                return true;
            }

            return false;
        }

        return false;
    }
}
