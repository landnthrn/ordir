using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Ordir.Interop;
using Ordir.Models;
using Ordir.Services;
using Ordir.Views;
using WinForms = System.Windows.Forms;

namespace Ordir;

public partial class MainWindow : Window
{
    private readonly ObservableCollection<FolderRow> _folders = new();
    private readonly ObservableCollection<FolderTreeNode> _treeRoots = new();
    private readonly ObservableCollection<FolderTreeNode> _treeFlatView = new();

    private System.Windows.Point _dragStart;
    private FolderRow? _dragSource;
    private bool _dragInProgress;

    private System.Windows.Point _treeDragStart;
    private FolderTreeNode? _treeDragSource;
    private bool _treeDragInProgress;

    private bool _autoIncludeSweep;
    private bool _autoIncludeSweepTarget;
    private bool _autoIncludeSweepIsTree;

    private const int SortIndexInfotips = 3;

    private bool _sortHandlersReady;
    private bool _suspendSelectAll;
    private bool _suppressPathBoxFolderSync;
    private FolderRow? _lastSelectedAutoRow;
    private bool _activityWelcomeWritten;

    public MainWindow()
    {
        InitializeComponent();
        FolderList.ItemsSource = _folders;
        FolderTreeList.ItemsSource = _treeFlatView;
        Loaded += OnLoaded;
    }

    private void RootTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // SelectionChanged bubbles from ComboBoxes and ListBoxes inside tabs; ignore those.
        if (!ReferenceEquals(e.Source, RootTabs))
            return;
        ApplyTitleArtVisibilityForSelectedTab();
    }

    private void ApplyTitleArtVisibilityForSelectedTab()
    {
        var onInfo = RootTabs.SelectedItem is TabItem { Content: InfoTab };
        TitleArtImage.Visibility = onInfo ? Visibility.Collapsed : Visibility.Visible;
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        ImmersiveTitleBar.TryApplyDark(this);
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        Title = $"Ordir {DisplayVersionLabel()}";
        try
        {
            TitleArtImage.Source = new BitmapImage(
                new Uri("pack://application:,,,/Ordir;component/Assets/ordir-title.png", UriKind.Absolute));
        }
        catch
        {
            // ignore missing or invalid title image (e.g. resource not included)
        }

        LastRunSummaryBlock.Text = string.Empty;
        _sortHandlersReady = true;
        AppendActivityWelcome();
        ApplyIniTipVisibilityToAllRows();
        SyncAutoSelectAllToggle();
        UpdateSummary();

        ManualPage.TargetFolderPathChanged += OnManualTargetFolderPathChanged;
        if (!string.IsNullOrWhiteSpace(PathBox.Text))
            ManualPage.ApplySyncedTargetPathFromAuto(PathBox.Text.Trim());
        TryApplyLaunchPathArgument();

        ApplyTitleArtVisibilityForSelectedTab();
    }

    private static string DisplayVersionLabel()
    {
        var v = Assembly.GetExecutingAssembly().GetName().Version;
        return v is null ? "v2.0.0" : $"v{v.Major}.{v.Minor}.{v.Build}";
    }

    private void TryApplyLaunchPathArgument()
    {
        string[] args;
        try
        {
            args = Environment.GetCommandLineArgs();
        }
        catch
        {
            return;
        }

        if (args.Length < 2) return;
        var candidate = (args[1] ?? string.Empty).Trim().Trim('"');
        if (string.IsNullOrWhiteSpace(candidate)) return;

        var resolved = candidate;
        if (File.Exists(resolved))
            resolved = Path.GetDirectoryName(resolved) ?? string.Empty;

        if (!Directory.Exists(resolved)) return;
        if (string.Equals(PathBox.Text.Trim(), resolved, StringComparison.OrdinalIgnoreCase))
            return;

        PathBox.Text = resolved;
        ManualPage.ApplySyncedTargetPathFromAuto(resolved);
        RefreshList();
        AppendLog($"> Launch path: {resolved}");
    }

    private void OnManualTargetFolderPathChanged(object? sender, string path)
    {
        if (_suppressPathBoxFolderSync) return;
        if (string.Equals(PathBox.Text.Trim(), path, StringComparison.OrdinalIgnoreCase)) return;
        _suppressPathBoxFolderSync = true;
        try
        {
            PathBox.Text = path;
        }
        finally
        {
            _suppressPathBoxFolderSync = false;
        }
    }

    private void PathBox_LostFocus(object sender, RoutedEventArgs e)
    {
        if (_suppressPathBoxFolderSync) return;
        ManualPage.ApplySyncedTargetPathFromAuto(PathBox.Text.Trim());
    }

    private bool IsAutoTreeMode() => AutoScopeAllNested.IsChecked == true;

    private void ApplyIniTipVisibilityToAllRows()
    {
        var show = ShowIniTipsToggle.IsChecked == true;
        var sortForcesTips = SortCombo.SelectedIndex == SortIndexInfotips;
        foreach (var r in _folders)
        {
            r.SetShowExistingTipNumbers(show);
            r.SetSortForcesInfotipColumn(sortForcesTips);
        }

        foreach (var n in EnumerateTreeNodes())
        {
            n.Row.SetShowExistingTipNumbers(show);
            n.Row.SetSortForcesInfotipColumn(sortForcesTips);
        }
    }

    private void ShowIniTipsToggle_Changed(object sender, RoutedEventArgs e)
    {
        if (!_sortHandlersReady) return;
        ApplyIniTipVisibilityToAllRows();
    }

    private void EnsureActivityLogDocument()
    {
        if (ActivityLog.Document == null)
            ActivityLog.Document = new FlowDocument { PagePadding = new Thickness(0) };
        if (ActivityLog.Document.Blocks.FirstBlock is not Paragraph)
        {
            ActivityLog.Document.Blocks.Clear();
            ActivityLog.Document.Blocks.Add(new Paragraph { Margin = new Thickness(0) });
        }
    }

    private Paragraph ActivityLogParagraph
    {
        get
        {
            EnsureActivityLogDocument();
            return (ActivityLog.Document.Blocks.FirstBlock as Paragraph)!;
        }
    }

    private void ClearActivityLogDocument()
    {
        ActivityLog.Document = new FlowDocument { PagePadding = new Thickness(0) };
        ActivityLog.Document.Blocks.Add(new Paragraph { Margin = new Thickness(0) });
    }

    private void AppendActivityWelcome()
    {
        if (_activityWelcomeWritten)
            return;
        _activityWelcomeWritten = true;
        ActivityLogParagraph.Inlines.Add(new Run("> ") { Foreground = ActivityLogFormatting.PromptBrush });
        ActivityLogParagraph.Inlines.Add(new Run("Ordir launched. Welcome to folder directory freedom." + Environment.NewLine)
        {
            Foreground = ActivityLogFormatting.LineBrush
        });
        ScrollLogToEnd();
    }

    private void AppendLog(string line)
    {
        if (!Dispatcher.CheckAccess())
        {
            Dispatcher.Invoke(() => AppendLog(line));
            return;
        }

        ActivityLogFormatting.AppendLogLineInlines(ActivityLogParagraph.Inlines, line, null);
        ScrollLogToEnd();
    }

    private void AppendLogError(string line)
    {
        if (!Dispatcher.CheckAccess())
        {
            Dispatcher.Invoke(() => AppendLogError(line));
            return;
        }

        ActivityLogFormatting.AppendLogLineInlines(ActivityLogParagraph.Inlines, line, ActivityLogFormatting.ErrorBrush);
        ScrollLogToEnd();
    }

    private void ScrollLogToEnd()
    {
        Dispatcher.BeginInvoke(new Action(() =>
        {
            try
            {
                if (ActivityLog.Document?.ContentEnd != null)
                    ActivityLog.CaretPosition = ActivityLog.Document.ContentEnd;
                ActivityLogScroll.UpdateLayout();
                ActivityLogScroll.ScrollToVerticalOffset(ActivityLogScroll.ExtentHeight);
            }
            catch
            {
                // ignore
            }
        }), DispatcherPriority.Loaded);
    }

    private void BrowseBtn_Click(object sender, RoutedEventArgs e)
    {
        using var dlg = new WinForms.FolderBrowserDialog
        {
            Description = "Select the target folder.",
            UseDescriptionForTitle = true,
            ShowNewFolderButton = true
        };
        if (!string.IsNullOrWhiteSpace(PathBox.Text) && Directory.Exists(PathBox.Text))
            dlg.SelectedPath = PathBox.Text;

        if (dlg.ShowDialog() == WinForms.DialogResult.OK)
        {
            PathBox.Text = dlg.SelectedPath;
            ManualPage.ApplySyncedTargetPathFromAuto(dlg.SelectedPath);
        }
    }

    private void RefreshBtn_Click(object sender, RoutedEventArgs e) => RefreshList(logIfEmptyPath: true);

    private void AutoScope_Changed(object sender, RoutedEventArgs e)
    {
        if (!_sortHandlersReady) return;
        RefreshList();
    }

    private void RefreshList(bool logIfEmptyPath = false)
    {
        _lastSelectedAutoRow = null;
        _folders.Clear();
        _treeRoots.Clear();
        _treeFlatView.Clear();

        var path = PathBox.Text.Trim();
        if (string.IsNullOrEmpty(path))
        {
            FolderList.Visibility = Visibility.Visible;
            FolderTreeList.Visibility = Visibility.Collapsed;
            if (logIfEmptyPath)
                AppendLog("> Refresh: no folder path entered. The list is empty.");
            UpdateSummary();
            SyncAutoSelectAllToggle();
            return;
        }

        if (!Directory.Exists(path))
        {
            FolderList.Visibility = Visibility.Visible;
            FolderTreeList.Visibility = Visibility.Collapsed;
            AppendLogError(ActivityRequestLog.FormatRequestLine(path, "! Refresh: that folder path does not exist."));
            UpdateSummary();
            SyncAutoSelectAllToggle();
            return;
        }

        if (IsAutoTreeMode())
        {
            foreach (var node in FolderTreeBuilder.LoadRoots(path))
                _treeRoots.Add(node);

            ApplyIniTipVisibilityToAllRows();
            var idx = SortCombo.SelectedIndex;
            if (idx < 0) idx = 0;
            FolderTreeSort.SortRecursive(_treeRoots, idx);
            FolderTreeRenumber.RenumberAll(_treeRoots);
            WireTreeExpansionHandlers();
            SyncTreeFlatView();

            FolderList.Visibility = Visibility.Collapsed;
            FolderTreeList.Visibility = Visibility.Visible;

            var n = CountTreeFolders();
            AppendLog(ActivityRequestLog.FormatRequestLine(path, $"> Refresh: loaded {n} folder(s) from the full tree under this target."));
            UpdateSummary();
            SyncAutoSelectAllToggle();
            return;
        }

        FolderList.Visibility = Visibility.Visible;
        FolderTreeList.Visibility = Visibility.Collapsed;

        IEnumerable<FolderRow> rows = AutoScopeTargetOnly.IsChecked == true
            ? FolderScanner.LoadTargetFolderOnly(path)
            : FolderScanner.LoadFirstLevelChildren(path);

        foreach (var row in rows)
            _folders.Add(row);

        ApplyIniTipVisibilityToAllRows();
        ApplyCurrentSort();
        RenumberDisplayOrders();
        AppendLog(ActivityRequestLog.FormatRequestLine(path, $"> Refresh: loaded {_folders.Count} folder(s) for this scope."));
        UpdateSummary();
        SyncAutoSelectAllToggle();
    }

    private void SyncAutoSelectAllToggle()
    {
        _suspendSelectAll = true;
        try
        {
            IEnumerable<FolderRow> rows = IsAutoTreeMode() ? EnumerateTreeNodes().Select(n => n.Row) : _folders;
            var list = rows.ToList();
            AutoSelectAllToggle.IsChecked = list.Count == 0 || list.All(r => !r.IsExcluded);
        }
        finally
        {
            _suspendSelectAll = false;
        }
    }

    private void AutoSelectAllToggle_Click(object sender, RoutedEventArgs e)
    {
        if (_suspendSelectAll) return;
        var on = AutoSelectAllToggle.IsChecked == true;
        if (IsAutoTreeMode())
        {
            foreach (var n in EnumerateTreeNodes())
                n.Row.IsExcluded = !on;
            FolderTreeRenumber.RenumberAll(_treeRoots);
            SyncTreeFlatView();
        }
        else
        {
            foreach (var r in _folders)
                r.IsExcluded = !on;
            RenumberDisplayOrders();
        }

        UpdateSummary();
    }

    private void FolderRowIncludeToggle_Click(object sender, RoutedEventArgs e)
    {
        SyncAutoSelectAllToggle();
        RenumberDisplayOrders();
        UpdateSummary();
    }

    private void TreeRowIncludeToggle_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not System.Windows.Controls.Primitives.ToggleButton { DataContext: FolderTreeNode node })
            return;
        PropagateIncludeToDescendants(node, node.Row.IsIncluded);
        SyncAutoSelectAllToggle();
        FolderTreeRenumber.RenumberAll(_treeRoots);
        SyncTreeFlatView();
        UpdateSummary();
    }

    private static void PropagateIncludeToDescendants(FolderTreeNode parent, bool included)
    {
        foreach (var c in parent.Children)
        {
            c.Row.IsExcluded = !included;
            PropagateIncludeToDescendants(c, included);
        }
    }

    private static int CountTreeFolders(IEnumerable<FolderTreeNode> roots)
    {
        var c = 0;
        void Walk(FolderTreeNode n)
        {
            c++;
            foreach (var ch in n.Children)
                Walk(ch);
        }

        foreach (var r in roots)
            Walk(r);
        return c;
    }

    private int CountTreeFolders() => CountTreeFolders(_treeRoots);

    private void SyncTreeFlatView() => FolderTreeFlattener.FillVisible(_treeRoots, _treeFlatView);

    private void WireTreeExpansionHandlers()
    {
        void Walk(FolderTreeNode n)
        {
            n.PropertyChanged -= TreeNode_FlatRefreshChannel;
            n.PropertyChanged += TreeNode_FlatRefreshChannel;
            foreach (var c in n.Children)
                Walk(c);
        }

        foreach (var r in _treeRoots)
            Walk(r);
    }

    private void TreeNode_FlatRefreshChannel(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(FolderTreeNode.IsExpanded))
            SyncTreeFlatView();
    }

    private void FolderTreeExpand_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not System.Windows.Controls.Button { DataContext: FolderTreeNode n })
            return;
        if (!n.HasChildren)
            return;
        n.IsExpanded = !n.IsExpanded;
    }

    private IEnumerable<FolderTreeNode> EnumerateTreeNodes()
    {
        foreach (var r in _treeRoots)
        {
            foreach (var n in Walk(r))
                yield return n;
        }
    }

    private static IEnumerable<FolderTreeNode> Walk(FolderTreeNode n)
    {
        yield return n;
        foreach (var c in n.Children)
        {
            foreach (var x in Walk(c))
                yield return x;
        }
    }

    private IEnumerable<FolderRow> FlatTreeRows() => EnumerateTreeNodes().Select(n => n.Row);

    private void RenumberDisplayOrders()
    {
        var n = 1;
        foreach (var r in _folders)
        {
            if (r.IsExcluded) r.DisplayOrder = 0;
            else r.DisplayOrder = n++;
        }
    }

    private void UpdateSummary()
    {
        IEnumerable<FolderRow> rows = IsAutoTreeMode() ? FlatTreeRows() : _folders;
        var list = rows.ToList();
        var (withIni, without, ready, misconfigured, excluded) = FolderScanner.Summarize(list);
        var sbInfo = new StringBuilder();
        sbInfo.AppendLine($"Folders: {list.Count}");
        sbInfo.AppendLine($"Excluded this run: {excluded}");
        sbInfo.AppendLine($"With desktop.ini: {withIni}");
        sbInfo.AppendLine($"Without desktop.ini: {without}");
        sbInfo.AppendLine($"Complete: {ready}");
        sbInfo.AppendLine($"Misconfigured: {misconfigured}");
        SummaryInfoBlock.Text = sbInfo.ToString().TrimEnd();

        var sbSel = new StringBuilder();
        FolderRow.AppendSelectedFolderDetails(sbSel, _lastSelectedAutoRow);
        SummarySelectedBlock.Text = sbSel.ToString().TrimEnd();
    }

    private void SortCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_sortHandlersReady) return;
        if (IsAutoTreeMode())
        {
            var idx = SortCombo.SelectedIndex;
            if (idx < 0) idx = 0;
            FolderTreeSort.SortRecursive(_treeRoots, idx);
            FolderTreeRenumber.RenumberAll(_treeRoots);
            SyncTreeFlatView();
            ApplyIniTipVisibilityToAllRows();
            UpdateSummary();
            return;
        }

        ApplyCurrentSort();
        RenumberDisplayOrders();
        ApplyIniTipVisibilityToAllRows();
        UpdateSummary();
    }

    private void ApplyCurrentSort()
    {
        if (_folders.Count == 0) return;
        var idx = SortCombo.SelectedIndex;
        if (idx < 0) idx = 0;

        List<FolderRow> sorted = idx switch
        {
            1 => _folders.OrderByDescending(r => SafeLastWriteUtc(r.FullPath)).ToList(),
            2 => _folders.OrderByDescending(r => FolderFileBytesOnly(r.FullPath)).ToList(),
            SortIndexInfotips => _folders
                .OrderBy(r => DesktopIniService.InfoTipNumericSortKey(r.CurrentInfoTip))
                .ThenBy(r => r.Name, StringComparer.OrdinalIgnoreCase)
                .ToList(),
            _ => _folders.OrderBy(r => r.Name, StringComparer.OrdinalIgnoreCase).ToList()
        };

        _folders.Clear();
        foreach (var r in sorted)
            _folders.Add(r);
    }

    private static DateTime SafeLastWriteUtc(string path)
    {
        try
        {
            return new DirectoryInfo(path).LastWriteTimeUtc;
        }
        catch
        {
            return DateTime.MinValue;
        }
    }

    private static long FolderFileBytesOnly(string dir)
    {
        try
        {
            return Directory.EnumerateFiles(dir).Sum(f => new FileInfo(f).Length);
        }
        catch
        {
            return 0;
        }
    }

    private void FolderList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _lastSelectedAutoRow = FolderList.SelectedItem as FolderRow;
        UpdateSummary();
    }

    private void FolderTreeList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _lastSelectedAutoRow = FolderTreeList.SelectedItem is FolderTreeNode tn ? tn.Row : null;
        UpdateSummary();
    }

    private void FolderList_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (RowIncludeSweep.TryBegin(FolderList, e, expectTree: false, out var target))
        {
            _autoIncludeSweep = true;
            _autoIncludeSweepTarget = target;
            _autoIncludeSweepIsTree = false;
            return;
        }

        if (IsClickFromRowChrome(e.OriginalSource)) return;
        _dragStart = e.GetPosition(null);
        _dragSource = HitTestRowAt(e.GetPosition(FolderList));
        _dragInProgress = false;
    }

    private void FolderList_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (_autoIncludeSweep && ReferenceEquals(Mouse.Captured, FolderList))
        {
            UpdateAutoIncludeSweep(FolderList, tree: false, e.GetPosition(FolderList));
            EndAutoIncludeSweep();
        }
    }

    private void FolderList_LostMouseCapture(object sender, MouseEventArgs e)
    {
        if (!_autoIncludeSweep)
            return;
        UpdateAutoIncludeSweep(FolderList, tree: false, Mouse.GetPosition(FolderList), requireCapture: false);
        EndAutoIncludeSweep();
    }

    private void FolderList_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (_autoIncludeSweep && ReferenceEquals(Mouse.Captured, FolderList))
        {
            UpdateAutoIncludeSweep(FolderList, tree: false, e.GetPosition(FolderList));
            return;
        }

        if (e.LeftButton != MouseButtonState.Pressed || _dragSource is null || _dragInProgress)
            return;

        var pos = e.GetPosition(null);
        if (Math.Abs(pos.X - _dragStart.X) < SystemParameters.MinimumHorizontalDragDistance
            && Math.Abs(pos.Y - _dragStart.Y) < SystemParameters.MinimumVerticalDragDistance)
            return;

        _dragInProgress = true;
        System.Windows.DragDrop.DoDragDrop(FolderList, _dragSource, System.Windows.DragDropEffects.Move);
        _dragInProgress = false;
        _dragSource = null;
        HideDropIndicator();
    }

    private void FolderList_DragOver(object sender, System.Windows.DragEventArgs e)
    {
        if (!e.Data.GetDataPresent(typeof(FolderRow)))
        {
            e.Effects = System.Windows.DragDropEffects.None;
            e.Handled = true;
            HideDropIndicator();
            return;
        }

        var pos = e.GetPosition(FolderList);
        if (pos.Y < 0 || pos.Y > FolderList.ActualHeight)
        {
            HideDropIndicator();
            e.Effects = System.Windows.DragDropEffects.None;
            e.Handled = true;
            return;
        }

        e.Effects = System.Windows.DragDropEffects.Move;
        e.Handled = true;
        UpdateDropIndicator(pos);
    }

    private void FolderList_DragLeave(object sender, System.Windows.DragEventArgs e) =>
        HideDropIndicator();

    private void FolderList_Drop(object sender, System.Windows.DragEventArgs e)
    {
        HideDropIndicator();
        if (e.Data.GetData(typeof(FolderRow)) is not FolderRow source)
            return;

        var pos = e.GetPosition(FolderList);
        var insertIndex = GetInsertIndexFromPoint(pos);
        var oldIndex = _folders.IndexOf(source);
        if (oldIndex < 0) return;

        var newIndex = oldIndex < insertIndex ? insertIndex - 1 : insertIndex;
        if (newIndex < 0 || newIndex >= _folders.Count) return;
        if (newIndex == oldIndex) return;

        _folders.Move(oldIndex, newIndex);
        RenumberDisplayOrders();
        UpdateSummary();
    }

    private int GetInsertIndexFromPoint(System.Windows.Point positionOverList)
    {
        if (_folders.Count == 0) return 0;
        for (var i = 0; i < _folders.Count; i++)
        {
            if (FolderList.ItemContainerGenerator.ContainerFromIndex(i) is not ListBoxItem item)
                return i;
            var top = item.TranslatePoint(new System.Windows.Point(0, 0), FolderList).Y;
            var h = item.ActualHeight > 1 ? item.ActualHeight : 28;
            if (positionOverList.Y < top + h / 2)
                return i;
        }

        return _folders.Count;
    }

    private void UpdateDropIndicator(System.Windows.Point positionOverList)
    {
        if (_folders.Count == 0)
        {
            HideDropIndicator();
            return;
        }

        var insert = GetInsertIndexFromPoint(positionOverList);
        double y;
        if (insert >= _folders.Count)
        {
            var last = FolderList.ItemContainerGenerator.ContainerFromIndex(_folders.Count - 1) as ListBoxItem;
            if (last == null)
            {
                HideDropIndicator();
                return;
            }

            y = last.TranslatePoint(new System.Windows.Point(0, last.ActualHeight), ExplorerItemsGrid).Y;
        }
        else
        {
            var item = FolderList.ItemContainerGenerator.ContainerFromIndex(insert) as ListBoxItem;
            if (item == null)
            {
                HideDropIndicator();
                return;
            }

            y = item.TranslatePoint(new System.Windows.Point(0, 0), ExplorerItemsGrid).Y;
        }

        DropInsertLine.Margin = new Thickness(8, y, 8, 0);
        DropInsertLine.Visibility = Visibility.Visible;
    }

    private void HideDropIndicator()
    {
        DropInsertLine.Visibility = Visibility.Collapsed;
    }

    private FolderRow? HitTestRowAt(System.Windows.Point pointRelativeToListBox)
    {
        var hit = VisualTreeHelper.HitTest(FolderList, pointRelativeToListBox);
        DependencyObject? current = hit?.VisualHit;
        while (current != null)
        {
            if (current is ListBoxItem item)
                return item.DataContext as FolderRow;
            current = VisualTreeHelper.GetParent(current);
        }

        return null;
    }

    private FolderTreeNode? HitTestTreeNode(System.Windows.Point pointRelativeToTree)
    {
        var hit = VisualTreeHelper.HitTest(FolderTreeList, pointRelativeToTree);
        DependencyObject? current = hit?.VisualHit;
        while (current != null)
        {
            if (current is ListBoxItem item && item.DataContext is FolderTreeNode n)
                return n;
            current = VisualTreeHelper.GetParent(current);
        }

        return null;
    }

    private ObservableCollection<FolderTreeNode> SiblingList(FolderTreeNode node) =>
        node.Parent?.Children ?? _treeRoots;

    private static bool IsClickFromRowChrome(object? src)
    {
        for (var o = src as DependencyObject; o != null; o = VisualTreeHelper.GetParent(o))
        {
            if (o is System.Windows.Controls.Primitives.ToggleButton or System.Windows.Controls.Button)
                return true;
        }

        return false;
    }

    private void UpdateAutoIncludeSweep(System.Windows.Controls.ListBox listBox, bool tree, System.Windows.Point positionOverList, bool requireCapture = true)
    {
        if (!_autoIncludeSweep)
            return;
        if (requireCapture && !ReferenceEquals(Mouse.Captured, listBox))
            return;
        if (!RowIncludeSweep.TryHitRow(listBox, positionOverList, tree, out var flat, out var node))
            return;
        if (tree && node != null)
        {
            if (node.Row.IsIncluded == _autoIncludeSweepTarget)
                return;
            node.Row.IsIncluded = _autoIncludeSweepTarget;
        }
        else if (!tree && flat != null)
        {
            if (flat.IsIncluded == _autoIncludeSweepTarget)
                return;
            flat.IsIncluded = _autoIncludeSweepTarget;
        }
    }

    private void EndAutoIncludeSweep()
    {
        if (!_autoIncludeSweep)
            return;
        var wasTree = _autoIncludeSweepIsTree;
        _autoIncludeSweep = false;
        if (ReferenceEquals(Mouse.Captured, FolderList) || ReferenceEquals(Mouse.Captured, FolderTreeList))
            Mouse.Capture(null);

        SyncAutoSelectAllToggle();
        if (wasTree)
        {
            FolderTreeRenumber.RenumberAll(_treeRoots);
            SyncTreeFlatView();
        }
        else
        {
            RenumberDisplayOrders();
        }

        UpdateSummary();
    }

    private void FolderTreeList_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (RowIncludeSweep.TryBegin(FolderTreeList, e, expectTree: true, out var target))
        {
            _autoIncludeSweep = true;
            _autoIncludeSweepTarget = target;
            _autoIncludeSweepIsTree = true;
            return;
        }

        if (IsClickFromRowChrome(e.OriginalSource)) return;
        _treeDragStart = e.GetPosition(null);
        _treeDragSource = HitTestTreeNode(e.GetPosition(FolderTreeList));
        _treeDragInProgress = false;
    }

    private void FolderTreeList_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (_autoIncludeSweep && ReferenceEquals(Mouse.Captured, FolderTreeList))
        {
            UpdateAutoIncludeSweep(FolderTreeList, tree: true, e.GetPosition(FolderTreeList));
            EndAutoIncludeSweep();
        }
    }

    private void FolderTreeList_LostMouseCapture(object sender, MouseEventArgs e)
    {
        if (!_autoIncludeSweep)
            return;
        UpdateAutoIncludeSweep(FolderTreeList, tree: true, Mouse.GetPosition(FolderTreeList), requireCapture: false);
        EndAutoIncludeSweep();
    }

    private void FolderTreeList_MouseMove(object sender, MouseEventArgs e)
    {
        if (_autoIncludeSweep && ReferenceEquals(Mouse.Captured, FolderTreeList))
        {
            UpdateAutoIncludeSweep(FolderTreeList, tree: true, e.GetPosition(FolderTreeList));
            return;
        }

        if (e.LeftButton != MouseButtonState.Pressed || _treeDragSource is null || _treeDragInProgress)
            return;

        var pos = e.GetPosition(null);
        if (Math.Abs(pos.X - _treeDragStart.X) < SystemParameters.MinimumHorizontalDragDistance
            && Math.Abs(pos.Y - _treeDragStart.Y) < SystemParameters.MinimumVerticalDragDistance)
            return;

        _treeDragInProgress = true;
        System.Windows.DragDrop.DoDragDrop(FolderTreeList, _treeDragSource, System.Windows.DragDropEffects.Move);
        _treeDragInProgress = false;
        _treeDragSource = null;
        HideDropIndicator();
    }

    private void FolderTreeList_DragOver(object sender, System.Windows.DragEventArgs e)
    {
        if (!e.Data.GetDataPresent(typeof(FolderTreeNode)))
        {
            e.Effects = System.Windows.DragDropEffects.None;
            e.Handled = true;
            HideDropIndicator();
            return;
        }

        var pos = e.GetPosition(FolderTreeList);
        if (pos.Y < 0 || pos.Y > FolderTreeList.ActualHeight)
        {
            HideDropIndicator();
            e.Effects = System.Windows.DragDropEffects.None;
            e.Handled = true;
            return;
        }

        e.Effects = System.Windows.DragDropEffects.Move;
        e.Handled = true;
        UpdateTreeDropIndicator(pos);
    }

    private void FolderTreeList_DragLeave(object sender, System.Windows.DragEventArgs e) =>
        HideDropIndicator();

    private void FolderTreeList_Drop(object sender, System.Windows.DragEventArgs e)
    {
        HideDropIndicator();
        if (e.Data.GetData(typeof(FolderTreeNode)) is not FolderTreeNode source)
            return;

        var pos = e.GetPosition(FolderTreeList);
        var insertFlat = FolderTreeFlatReorder.GetInsertIndexFromPoint(FolderTreeList, _treeFlatView.Count, pos);
        if (!FolderTreeFlatReorder.TryReorderAtFlatInsertGap(_treeFlatView, _treeRoots, source, insertFlat))
            return;

        FolderTreeRenumber.RenumberAll(_treeRoots);
        SyncTreeFlatView();
        UpdateSummary();
    }

    private void UpdateTreeDropIndicator(System.Windows.Point positionOverTree)
    {
        if (_treeFlatView.Count == 0)
        {
            HideDropIndicator();
            return;
        }

        var insert = FolderTreeFlatReorder.GetInsertIndexFromPoint(FolderTreeList, _treeFlatView.Count, positionOverTree);
        double y;
        if (insert >= _treeFlatView.Count)
        {
            var last = FolderTreeList.ItemContainerGenerator.ContainerFromIndex(_treeFlatView.Count - 1) as ListBoxItem;
            if (last == null)
            {
                HideDropIndicator();
                return;
            }

            y = last.TranslatePoint(new System.Windows.Point(0, last.ActualHeight), ExplorerItemsGrid).Y;
        }
        else
        {
            var item = FolderTreeList.ItemContainerGenerator.ContainerFromIndex(insert) as ListBoxItem;
            if (item == null)
            {
                HideDropIndicator();
                return;
            }

            y = item.TranslatePoint(new System.Windows.Point(0, 0), ExplorerItemsGrid).Y;
        }

        DropInsertLine.Margin = new Thickness(8, y, 8, 0);
        DropInsertLine.Visibility = Visibility.Visible;
    }

    private async void ApplyBtn_Click(object sender, RoutedEventArgs e)
    {
        var parent = PathBox.Text.Trim();
        if (string.IsNullOrEmpty(parent) || !Directory.Exists(parent))
        {
            ClearActivityLogDocument();
            AppendLogError(ActivityRequestLog.FormatRequestLine(parent, "! Pick a valid target folder first."));
            return;
        }

        if (IsAutoTreeMode())
        {
            if (_treeRoots.Count == 0)
            {
                ClearActivityLogDocument();
                AppendLogError(ActivityRequestLog.FormatRequestLine(parent, "! Nothing to apply. Refresh after choosing a folder."));
                return;
            }
        }
        else if (_folders.Count == 0)
        {
            ClearActivityLogDocument();
            AppendLogError(ActivityRequestLog.FormatRequestLine(parent, "! Nothing to apply. Refresh after choosing a folder."));
            return;
        }

        ApplyBtn.IsEnabled = false;
        RefreshBtn.IsEnabled = false;
        BrowseBtn.IsEnabled = false;
        ClearActivityLogDocument();
        LastRunSummaryBlock.Text = string.Empty;
        AppendLog(ActivityRequestLog.FormatRequestLine(parent, $"> apply {DateTime.Now:HH:mm:ss}"));

        try
        {
            ApplyOrganizeService.ApplyResult result;
            if (IsAutoTreeMode())
            {
                result = await Task.Run(() =>
                    ApplyOrganizeService.ApplyTree(_treeRoots.ToList(), line =>
                        Dispatcher.Invoke(() => AppendLog(line)))).ConfigureAwait(true);

                foreach (var n in EnumerateTreeNodes())
                    FolderScanner.Classify(n.Row);

                FolderTreeRenumber.RenumberAll(_treeRoots);
                SyncTreeFlatView();
            }
            else
            {
                result = await Task.Run(() =>
                    ApplyOrganizeService.Apply(parent, _folders.ToList(), line =>
                        Dispatcher.Invoke(() => AppendLog(line)))).ConfigureAwait(true);

                foreach (var row in _folders)
                    FolderScanner.Classify(row);

                RenumberDisplayOrders();
            }

            UpdateSummary();

            LastRunSummaryBlock.Text = RunSummaryText.ForApplyOrder(result);

            if (result.Errors.Count > 0)
            {
                foreach (var err in result.Errors.Take(20))
                    AppendLogError($"! {err}");
                if (result.Errors.Count > 20)
                    AppendLogError($"! …and {result.Errors.Count - 20} more.");
            }
        }
        catch (Exception ex)
        {
            AppendLogError($"! Apply failed: {ex.Message}");
        }
        finally
        {
            ApplyBtn.IsEnabled = true;
            RefreshBtn.IsEnabled = true;
            BrowseBtn.IsEnabled = true;
        }
    }

    private void AutoListExportBtn_Click(object sender, RoutedEventArgs e)
    {
        var root = PathBox.Text.Trim();
        if (string.IsNullOrEmpty(root) || !Directory.Exists(root))
        {
            AppendLogError("! Choose a valid target folder before exporting a list.");
            return;
        }

        ListPaths.EnsureListsDirectory();
        ManualOperationsService.OpResult r = IsAutoTreeMode()
            ? ManualOperationsService.WriteListOrderedTreeRoots(root, _treeRoots.ToList(), ListPaths.ListsDirectory)
            : ManualOperationsService.WriteListOrderedFirstLevel(root, _folders.ToList(), ListPaths.ListsDirectory);

        AppendLog("> Export list file…");
        foreach (var m in r.Messages.Where(x => !x.StartsWith("Summary —", StringComparison.OrdinalIgnoreCase)))
            AppendLog(m);
        LastRunSummaryBlock.Text = RunSummaryText.ForListExport(r);
    }

    private void AutoListImportBtn_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "Text lists (*.txt)|*.txt|All files (*.*)|*.*"
        };
        if (dlg.ShowDialog() != true)
        {
            AppendLog("> Import list file: cancelled.");
            return;
        }

        AppendLog($"> Import list file → {dlg.FileName}");
        string[] lines;
        try
        {
            lines = File.ReadAllLines(dlg.FileName);
        }
        catch (Exception ex)
        {
            AppendLogError($"! Could not read list file: {ex.Message}");
            return;
        }

        if (!FolderListFile.TryParseRootLine(lines, out var root) || string.IsNullOrWhiteSpace(root))
        {
            AppendLogError("! List file does not contain a root folder path (e.g. D:\\MyFolder\\).");
            return;
        }

        if (!Directory.Exists(root))
        {
            AppendLogError($"! List folder no longer exists: {root}");
            return;
        }

        PathBox.Text = root;
        ManualPage.ApplySyncedTargetPathFromAuto(root);
        RefreshList();
        LastRunSummaryBlock.Text = RunSummaryText.ForListImport(root);
    }

    private void AutoListOpenFolderBtn_Click(object sender, RoutedEventArgs e)
    {
        ListPaths.EnsureListsDirectory();
        AppendLog("> Opened the saved-lists folder in File Explorer.");
        Process.Start(new ProcessStartInfo
        {
            FileName = ListPaths.ListsDirectory,
            UseShellExecute = true
        });
    }
}
