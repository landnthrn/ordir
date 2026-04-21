using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Ordir.Models;

/// <summary>Recursive folder node for Auto “all nested” scope: expand/collapse, sibling order, Apply per parent.</summary>
public sealed class FolderTreeNode : INotifyPropertyChanged
{
    private bool _isExpanded = true;

    /// <summary>0 for roots under the target folder; used as left gutter width in the flattened tree list.</summary>
    public int TreeDepth { get; internal set; }

    public bool HasChildren => Children.Count > 0;

    public FolderTreeNode(FolderRow row)
    {
        Row = row;
        Children = new ObservableCollection<FolderTreeNode>();
        Children.CollectionChanged += OnChildrenChanged;
        Row.PropertyChanged += (_, e) =>
        {
            // Ensure bindings like Row.IniIsHidden refresh reliably on the tree row chrome.
            if (e.PropertyName is nameof(FolderRow.IniIsHidden)
                or nameof(FolderRow.ShowBadgeIni)
                or nameof(FolderRow.ShowBadgeSystem)
                or nameof(FolderRow.ShowBadgeNoIniNotSystem)
                or nameof(FolderRow.ShowBadgeMisconfigured)
                or nameof(FolderRow.ShowOrderCurrentTipColumn)
                or nameof(FolderRow.OrderColumnCurrentTipText)
                or nameof(FolderRow.OrderColumnNewOrderText)
                or nameof(FolderRow.IsExcluded)
                or nameof(FolderRow.IsIncluded))
                OnPropertyChanged(nameof(Row));
        };
    }

    private void OnChildrenChanged(object? sender, NotifyCollectionChangedEventArgs e) =>
        OnPropertyChanged(nameof(HasChildren));

    public FolderRow Row { get; }

    /// <summary>Null when this node is a top-level item under the target folder.</summary>
    public FolderTreeNode? Parent { get; private set; }

    public ObservableCollection<FolderTreeNode> Children { get; }

    internal void SetParent(FolderTreeNode? parent) => Parent = parent;

    public bool IsExpanded
    {
        get => _isExpanded;
        set
        {
            if (_isExpanded == value) return;
            _isExpanded = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
