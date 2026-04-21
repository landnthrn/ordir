using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Ordir.Models;
using Ordir.Services;
using WinForms = System.Windows.Forms;

namespace Ordir.Views;

public partial class ManualTab : UserControl
{
    private const int SortIndexInfotips = 3;

    private enum ScopeKind { TargetOnly, Subfolders, AllNested }

    private enum ManualActionKind
    {
        None,
        CreateIni,
        MakeSystem,
        UndoSystem,
        HideIni,
        UnhideIni,
        DeleteIni,
        Normalize
    }

    private readonly ObservableCollection<FolderRow> _manualFolders = new();
    private readonly ObservableCollection<FolderTreeNode> _manualTreeRoots = new();
    private readonly ObservableCollection<FolderTreeNode> _manualTreeFlatView = new();

    private ManualActionKind _pendingAction = ManualActionKind.None;
    private bool _sortHandlersReady;
    private bool _suspendSelectAll;
    private bool _suppressTargetFolderPathNotify;
    private FolderRow? _lastSelectedManualRow;
    private bool _manualActivityWelcomeWritten;

    private System.Windows.Point _dragStart;
    private FolderRow? _dragSource;
    private bool _dragInProgress;

    private System.Windows.Point _treeDragStart;
    private FolderTreeNode? _treeDragSource;
    private bool _treeDragInProgress;

    private bool _manualIncludeSweep;
    private bool _manualIncludeSweepTarget;
    private bool _manualIncludeSweepIsTree;

    /// <summary>Fired when the manual target path is set (browse) or the path box loses focus, so Auto tab can stay in sync.</summary>
    public event EventHandler<string>? TargetFolderPathChanged;

    public ManualTab()
    {
        InitializeComponent();
        ManualFolderList.ItemsSource = _manualFolders;
        ManualFolderTreeList.ItemsSource = _manualTreeFlatView;
        Loaded += (_, _) =>
        {
            _sortHandlersReady = true;
            AppendManualActivityWelcome();
            TryRefreshManualList(logIfEmptyPath: false);
            ApplyIniTipVisibilityToAllRows();
            UpdateManualSummary();
        };
    }

    private bool IsManualTreeMode() => ManualModeAllNested.IsChecked == true;

    private ScopeKind GetScope()
    {
        if (ManualModeTargetOnly.IsChecked == true) return ScopeKind.TargetOnly;
        if (ManualModeAllNested.IsChecked == true) return ScopeKind.AllNested;
        return ScopeKind.Subfolders;
    }

    private void ApplyIniTipVisibilityToAllRows()
    {
        var show = ManualShowIniTipsToggle.IsChecked == true;
        var sortForcesTips = ManualSortCombo.SelectedIndex == SortIndexInfotips;
        foreach (var r in _manualFolders)
        {
            r.SetShowExistingTipNumbers(show);
            r.SetSortForcesInfotipColumn(sortForcesTips);
        }

        foreach (var n in EnumerateManualTreeNodes())
        {
            n.Row.SetShowExistingTipNumbers(show);
            n.Row.SetSortForcesInfotipColumn(sortForcesTips);
        }
    }

    private void ManualShowIniTips_Changed(object sender, RoutedEventArgs e)
    {
        if (!_sortHandlersReady) return;
        ApplyIniTipVisibilityToAllRows();
    }

    private void EnsureManualActivityLogDocument()
    {
        if (ManualActivityLog.Document == null)
            ManualActivityLog.Document = new FlowDocument { PagePadding = new Thickness(0) };
        if (ManualActivityLog.Document.Blocks.FirstBlock is not Paragraph)
        {
            ManualActivityLog.Document.Blocks.Clear();
            ManualActivityLog.Document.Blocks.Add(new Paragraph { Margin = new Thickness(0) });
        }
    }

    private Paragraph ManualActivityLogParagraph
    {
        get
        {
            EnsureManualActivityLogDocument();
            return (ManualActivityLog.Document.Blocks.FirstBlock as Paragraph)!;
        }
    }

    private void ClearManualActivityLogDocument()
    {
        ManualActivityLog.Document = new FlowDocument { PagePadding = new Thickness(0) };
        ManualActivityLog.Document.Blocks.Add(new Paragraph { Margin = new Thickness(0) });
    }

    private void ManualScrollLogToEnd()
    {
        Dispatcher.BeginInvoke(new Action(() =>
        {
            try
            {
                if (ManualActivityLog.Document?.ContentEnd != null)
                    ManualActivityLog.CaretPosition = ManualActivityLog.Document.ContentEnd;
                ManualActivityLogScroll.UpdateLayout();
                ManualActivityLogScroll.ScrollToVerticalOffset(ManualActivityLogScroll.ExtentHeight);
            }
            catch
            {
                // ignore
            }
        }), DispatcherPriority.Loaded);
    }

    private void AppendManualActivityWelcome()
    {
        if (_manualActivityWelcomeWritten)
            return;
        _manualActivityWelcomeWritten = true;
        ManualActivityLogParagraph.Inlines.Add(new Run("> ") { Foreground = ActivityLogFormatting.PromptBrush });
        ManualActivityLogParagraph.Inlines.Add(new Run("Ordir launched. Welcome to folder directory freedom." + Environment.NewLine)
        {
            Foreground = ActivityLogFormatting.LineBrush
        });
        ManualScrollLogToEnd();
    }

    private void AppendLog(string line)
    {
        if (!Dispatcher.CheckAccess())
        {
            Dispatcher.Invoke(() => AppendLog(line));
            return;
        }

        ActivityLogFormatting.AppendLogLineInlines(ManualActivityLogParagraph.Inlines, line, null);
        ManualScrollLogToEnd();
    }

    private void AppendLogError(string line)
    {
        if (!Dispatcher.CheckAccess())
        {
            Dispatcher.Invoke(() => AppendLogError(line));
            return;
        }

        ActivityLogFormatting.AppendLogLineInlines(ManualActivityLogParagraph.Inlines, line, ActivityLogFormatting.ErrorBrush);
        ManualScrollLogToEnd();
    }

    private void AppendManualOpMessages(ManualOperationsService.OpResult r)
    {
        foreach (var m in r.Messages)
        {
            if (m.StartsWith("Summary —", StringComparison.OrdinalIgnoreCase))
                continue;
            AppendLog(m);
        }

        ManualLastRunSummaryBlock.Text = RunSummaryText.ForManualOperation(r);
    }

    private string? RequireRootLog()
    {
        var p = ManualPathBox.Text.Trim();
        if (string.IsNullOrEmpty(p) || !Directory.Exists(p))
        {
            AppendLogError("! Choose a target folder that exists first.");
            return null;
        }

        return p;
    }

    private void ManualBrowseBtn_Click(object sender, RoutedEventArgs e)
    {
        using var dlg = new WinForms.FolderBrowserDialog
        {
            Description = "Select the target folder.",
            UseDescriptionForTitle = true,
            ShowNewFolderButton = true
        };
        if (!string.IsNullOrWhiteSpace(ManualPathBox.Text) && Directory.Exists(ManualPathBox.Text))
            dlg.SelectedPath = ManualPathBox.Text;

        if (dlg.ShowDialog() == WinForms.DialogResult.OK)
        {
            ManualPathBox.Text = dlg.SelectedPath;
            RaiseTargetFolderPathChangedIfNeeded();
            TryRefreshManualList();
        }
    }

    private void ManualRefreshBtn_Click(object sender, RoutedEventArgs e) => TryRefreshManualList(logIfEmptyPath: true);

    private void ManualPathBox_LostFocus(object sender, RoutedEventArgs e)
    {
        RaiseTargetFolderPathChangedIfNeeded();
        TryRefreshManualList();
    }

    /// <summary>Updates the path box from the Auto tab without notifying back.</summary>
    public void ApplySyncedTargetPathFromAuto(string path)
    {
        var normalized = (path ?? string.Empty).Trim();
        if (string.Equals(ManualPathBox.Text.Trim(), normalized, StringComparison.OrdinalIgnoreCase))
            return;

        _suppressTargetFolderPathNotify = true;
        try
        {
            ManualPathBox.Text = path ?? string.Empty;
            TryRefreshManualList(logIfEmptyPath: false);
        }
        finally
        {
            _suppressTargetFolderPathNotify = false;
        }
    }

    private void RaiseTargetFolderPathChangedIfNeeded()
    {
        if (_suppressTargetFolderPathNotify) return;
        TargetFolderPathChanged?.Invoke(this, ManualPathBox.Text.Trim());
    }

    private void ManualScope_Changed(object sender, RoutedEventArgs e)
    {
        if (!_sortHandlersReady) return;
        TryRefreshManualList();
    }

    private void TryRefreshManualList(bool logIfEmptyPath = false)
    {
        _lastSelectedManualRow = null;
        _manualFolders.Clear();
        _manualTreeRoots.Clear();
        _manualTreeFlatView.Clear();

        var path = ManualPathBox.Text.Trim();
        if (string.IsNullOrEmpty(path))
        {
            ManualFolderList.Visibility = Visibility.Visible;
            ManualFolderTreeList.Visibility = Visibility.Collapsed;
            if (logIfEmptyPath)
                AppendLog("> Refresh: no folder path entered. The list is empty.");
            UpdateManualSummary();
            SyncManualSelectAllToggle();
            return;
        }

        if (!Directory.Exists(path))
        {
            ManualFolderList.Visibility = Visibility.Visible;
            ManualFolderTreeList.Visibility = Visibility.Collapsed;
            AppendLogError(ActivityRequestLog.FormatRequestLine(path, "! Refresh: that folder path does not exist."));
            UpdateManualSummary();
            SyncManualSelectAllToggle();
            return;
        }

        if (IsManualTreeMode())
        {
            foreach (var node in FolderTreeBuilder.LoadRoots(path))
                _manualTreeRoots.Add(node);

            ApplyIniTipVisibilityToAllRows();
            var idx = ManualSortCombo.SelectedIndex;
            if (idx < 0) idx = 0;
            FolderTreeSort.SortRecursive(_manualTreeRoots, idx);
            FolderTreeRenumber.RenumberAll(_manualTreeRoots);
            WireManualTreeExpansionHandlers();
            SyncManualTreeFlatView();

            ManualFolderList.Visibility = Visibility.Collapsed;
            ManualFolderTreeList.Visibility = Visibility.Visible;

            var n = CountTreeFolders(_manualTreeRoots);
            AppendLog(ActivityRequestLog.FormatRequestLine(path, $"> Refresh: loaded {n} folder(s) from the full tree under this target."));
            UpdateManualSummary();
            SyncManualSelectAllToggle();
            return;
        }

        ManualFolderList.Visibility = Visibility.Visible;
        ManualFolderTreeList.Visibility = Visibility.Collapsed;

        IEnumerable<FolderRow> rows = GetScope() == ScopeKind.TargetOnly
            ? FolderScanner.LoadTargetFolderOnly(path)
            : FolderScanner.LoadFirstLevelChildren(path);

        foreach (var row in rows)
            _manualFolders.Add(row);

        ApplyIniTipVisibilityToAllRows();
        ApplyManualSort();
        RenumberManualOrders();
        AppendLog(ActivityRequestLog.FormatRequestLine(path, $"> Refresh: loaded {_manualFolders.Count} folder(s) for this scope."));
        UpdateManualSummary();
        SyncManualSelectAllToggle();
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

    private void SyncManualTreeFlatView() =>
        FolderTreeFlattener.FillVisible(_manualTreeRoots, _manualTreeFlatView);

    private void WireManualTreeExpansionHandlers()
    {
        void Walk(FolderTreeNode n)
        {
            n.PropertyChanged -= ManualTreeNode_FlatRefreshChannel;
            n.PropertyChanged += ManualTreeNode_FlatRefreshChannel;
            foreach (var c in n.Children)
                Walk(c);
        }

        foreach (var r in _manualTreeRoots)
            Walk(r);
    }

    private void ManualTreeNode_FlatRefreshChannel(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(FolderTreeNode.IsExpanded))
            SyncManualTreeFlatView();
    }

    private void ManualFolderTreeExpand_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { DataContext: FolderTreeNode n })
            return;
        if (!n.HasChildren)
            return;
        n.IsExpanded = !n.IsExpanded;
    }

    private IEnumerable<FolderTreeNode> EnumerateManualTreeNodes()
    {
        foreach (var r in _manualTreeRoots)
        {
            foreach (var n in WalkTree(r))
                yield return n;
        }
    }

    private static IEnumerable<FolderTreeNode> WalkTree(FolderTreeNode n)
    {
        yield return n;
        foreach (var c in n.Children)
        {
            foreach (var x in WalkTree(c))
                yield return x;
        }
    }

    private IEnumerable<FolderRow> FlatManualTreeRows() => EnumerateManualTreeNodes().Select(n => n.Row);

    private void UpdateManualSummary()
    {
        IEnumerable<FolderRow> rows = IsManualTreeMode() ? FlatManualTreeRows() : _manualFolders;
        var list = rows.ToList();
        var (withIni, without, ready, misconfigured, excluded) = FolderScanner.Summarize(list);
        var sbInfo = new StringBuilder();
        sbInfo.AppendLine($"Folders: {list.Count}");
        sbInfo.AppendLine($"Excluded this run: {excluded}");
        sbInfo.AppendLine($"With desktop.ini: {withIni}");
        sbInfo.AppendLine($"Without desktop.ini: {without}");
        sbInfo.AppendLine($"Complete: {ready}");
        sbInfo.AppendLine($"Misconfigured: {misconfigured}");
        ManualSummaryInfoBlock.Text = sbInfo.ToString().TrimEnd();

        var sbSel = new StringBuilder();
        FolderRow.AppendSelectedFolderDetails(sbSel, _lastSelectedManualRow);
        ManualSummarySelectedBlock.Text = sbSel.ToString().TrimEnd();
    }

    private void ManualSortCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_sortHandlersReady) return;
        if (IsManualTreeMode())
        {
            var idx = ManualSortCombo.SelectedIndex;
            if (idx < 0) idx = 0;
            FolderTreeSort.SortRecursive(_manualTreeRoots, idx);
            FolderTreeRenumber.RenumberAll(_manualTreeRoots);
            SyncManualTreeFlatView();
            ApplyIniTipVisibilityToAllRows();
            UpdateManualSummary();
            return;
        }

        ApplyManualSort();
        RenumberManualOrders();
        ApplyIniTipVisibilityToAllRows();
        UpdateManualSummary();
    }

    private void ApplyManualSort()
    {
        if (_manualFolders.Count == 0) return;
        var idx = ManualSortCombo.SelectedIndex;
        if (idx < 0) idx = 0;

        List<FolderRow> sorted = idx switch
        {
            1 => _manualFolders.OrderByDescending(r => SafeLastWriteUtc(r.FullPath)).ToList(),
            2 => _manualFolders.OrderByDescending(r => FolderFileBytesOnly(r.FullPath)).ToList(),
            SortIndexInfotips => _manualFolders
                .OrderBy(r => DesktopIniService.InfoTipNumericSortKey(r.CurrentInfoTip))
                .ThenBy(r => r.Name, StringComparer.OrdinalIgnoreCase)
                .ToList(),
            _ => _manualFolders.OrderBy(r => r.Name, StringComparer.OrdinalIgnoreCase).ToList()
        };

        _manualFolders.Clear();
        foreach (var r in sorted)
            _manualFolders.Add(r);
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

    private void RenumberManualOrders()
    {
        var n = 1;
        foreach (var r in _manualFolders)
        {
            if (r.IsExcluded) r.DisplayOrder = 0;
            else r.DisplayOrder = n++;
        }
    }

    private void SyncManualSelectAllToggle()
    {
        _suspendSelectAll = true;
        try
        {
            IEnumerable<FolderRow> rows = IsManualTreeMode() ? FlatManualTreeRows() : _manualFolders;
            var list = rows.ToList();
            ManualSelectAllToggle.IsChecked = list.Count == 0 || list.All(r => !r.IsExcluded);
        }
        finally
        {
            _suspendSelectAll = false;
        }
    }

    private void ManualSelectAllToggle_Click(object sender, RoutedEventArgs e)
    {
        if (_suspendSelectAll) return;
        var on = ManualSelectAllToggle.IsChecked == true;
        if (IsManualTreeMode())
        {
            foreach (var n in EnumerateManualTreeNodes())
                n.Row.IsExcluded = !on;
            FolderTreeRenumber.RenumberAll(_manualTreeRoots);
            SyncManualTreeFlatView();
        }
        else
        {
            foreach (var r in _manualFolders)
                r.IsExcluded = !on;
            RenumberManualOrders();
        }

        UpdateManualSummary();
    }

    private void ManualFolderRowIncludeToggle_Click(object sender, RoutedEventArgs e)
    {
        SyncManualSelectAllToggle();
        RenumberManualOrders();
        UpdateManualSummary();
    }

    private void ManualTreeRowIncludeToggle_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not System.Windows.Controls.Primitives.ToggleButton { DataContext: FolderTreeNode node })
            return;
        PropagateManualIncludeToDescendants(node, node.Row.IsIncluded);
        SyncManualSelectAllToggle();
        FolderTreeRenumber.RenumberAll(_manualTreeRoots);
        SyncManualTreeFlatView();
        UpdateManualSummary();
    }

    private static void PropagateManualIncludeToDescendants(FolderTreeNode parent, bool included)
    {
        foreach (var c in parent.Children)
        {
            c.Row.IsExcluded = !included;
            PropagateManualIncludeToDescendants(c, included);
        }
    }

    private static bool IsClickFromRowChrome(object? src)
    {
        for (var o = src as DependencyObject; o != null; o = VisualTreeHelper.GetParent(o))
        {
            if (o is System.Windows.Controls.Primitives.ToggleButton or System.Windows.Controls.Button)
                return true;
        }

        return false;
    }

    private void UpdateManualIncludeSweep(ListBox listBox, bool tree, System.Windows.Point positionOverList, bool requireCapture = true)
    {
        if (!_manualIncludeSweep)
            return;
        if (requireCapture && !ReferenceEquals(Mouse.Captured, listBox))
            return;
        if (!RowIncludeSweep.TryHitRow(listBox, positionOverList, tree, out var flat, out var node))
            return;
        if (tree && node != null)
        {
            if (node.Row.IsIncluded == _manualIncludeSweepTarget)
                return;
            node.Row.IsIncluded = _manualIncludeSweepTarget;
        }
        else if (!tree && flat != null)
        {
            if (flat.IsIncluded == _manualIncludeSweepTarget)
                return;
            flat.IsIncluded = _manualIncludeSweepTarget;
        }
    }

    private void EndManualIncludeSweep()
    {
        if (!_manualIncludeSweep)
            return;
        var wasTree = _manualIncludeSweepIsTree;
        _manualIncludeSweep = false;
        if (ReferenceEquals(Mouse.Captured, ManualFolderList) || ReferenceEquals(Mouse.Captured, ManualFolderTreeList))
            Mouse.Capture(null);

        SyncManualSelectAllToggle();
        if (wasTree)
        {
            FolderTreeRenumber.RenumberAll(_manualTreeRoots);
            SyncManualTreeFlatView();
        }
        else
        {
            RenumberManualOrders();
        }

        UpdateManualSummary();
    }

    private void ManualActionSelect_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: string tag }) return;
        if (!Enum.TryParse<ManualActionKind>(tag, out var kind))
            return;

        _pendingAction = kind;
        foreach (var b in new[] { BtnCreateIni, BtnDeleteIni, BtnMakeSystem, BtnUndoSystem, BtnHideIni, BtnUnhideIni, BtnNormalizeFolders })
            b.FontWeight = FontWeights.Normal;

        if (sender is Button sel)
            sel.FontWeight = FontWeights.SemiBold;

        AppendLog($"> selected action: {kind}");
    }

    private void ManualApplyActionBtn_Click(object sender, RoutedEventArgs e)
    {
        var root = RequireRootLog();
        if (root is null) return;

        if (_pendingAction == ManualActionKind.None)
        {
            AppendLogError("! Select an action button first, then click Apply.");
            return;
        }

        var scope = GetScope();
        AppendLog(ActivityRequestLog.FormatRequestLine(root, $"> apply: {_pendingAction}"));
        try
        {
            ManualOperationsService.OpResult r = _pendingAction switch
            {
                ManualActionKind.CreateIni => scope switch
                {
                    ScopeKind.TargetOnly => ManualOperationsService.CreateDesktopIniTargetFolderOnly(root),
                    ScopeKind.AllNested => ManualOperationsService.CreateDesktopIniRecursive(root),
                    _ => ManualOperationsService.CreateDesktopIniFirstLevel(root)
                },
                ManualActionKind.MakeSystem => scope switch
                {
                    ScopeKind.TargetOnly => ManualOperationsService.EnsureSystemTargetFolderOnly(root),
                    ScopeKind.AllNested => ManualOperationsService.EnsureSystemFoldersRecursive(root),
                    _ => ManualOperationsService.EnsureSystemFoldersFirstLevel(root)
                },
                ManualActionKind.UndoSystem => scope switch
                {
                    ScopeKind.TargetOnly => ManualOperationsService.RemoveSystemTargetFolderOnly(root),
                    ScopeKind.AllNested => ManualOperationsService.RemoveSystemFromIniFoldersRecursive(root),
                    _ => ManualOperationsService.RemoveSystemFromIniFoldersFirstLevel(root)
                },
                ManualActionKind.HideIni => scope switch
                {
                    ScopeKind.TargetOnly => ManualOperationsService.HideIniTargetFolderOnly(root),
                    ScopeKind.AllNested => ManualOperationsService.HideIniRecursive(root),
                    _ => ManualOperationsService.HideIniFirstLevel(root)
                },
                ManualActionKind.UnhideIni => scope switch
                {
                    ScopeKind.TargetOnly => ManualOperationsService.UnhideIniTargetFolderOnly(root),
                    ScopeKind.AllNested => ManualOperationsService.UnhideIniRecursive(root),
                    _ => ManualOperationsService.UnhideIniFirstLevel(root)
                },
                ManualActionKind.DeleteIni => scope switch
                {
                    ScopeKind.TargetOnly => ManualOperationsService.DeleteIniTargetFolderOnly(root),
                    ScopeKind.AllNested => ManualOperationsService.DeleteIniRecursive(root),
                    _ => ManualOperationsService.DeleteIniFirstLevel(root)
                },
                ManualActionKind.Normalize => RunNormalizeForScope(root, scope),
                _ => new ManualOperationsService.OpResult("Unknown", 0, 0, Array.Empty<string>())
            };

            AppendManualOpMessages(r);
            TryRefreshManualList();
        }
        catch (Exception ex)
        {
            AppendLog($"! {ex.Message}");
        }
    }

    private ManualOperationsService.OpResult RunNormalizeForScope(string root, ScopeKind scope)
    {
        var paths = new List<string>();
        if (scope == ScopeKind.TargetOnly)
            paths.Add(root);
        else if (IsManualTreeMode())
        {
            foreach (var n in EnumerateManualTreeNodes())
            {
                if (!n.Row.IsExcluded)
                    paths.Add(n.Row.FullPath);
            }
        }
        else
        {
            foreach (var row in _manualFolders)
            {
                if (!row.IsExcluded)
                    paths.Add(row.FullPath);
            }
        }

        if (paths.Count == 0)
            return new ManualOperationsService.OpResult("Normalize folder(s)", 0, 0,
                new[] { "! No included folders to normalize.", "Summary — OK 0." });

        return ManualOperationsService.NormalizeFolderPaths(paths);
    }

    private void ManualFolderList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _lastSelectedManualRow = ManualFolderList.SelectedItem as FolderRow;
        UpdateManualSummary();
    }

    private void ManualFolderTreeList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _lastSelectedManualRow = ManualFolderTreeList.SelectedItem is FolderTreeNode tn ? tn.Row : null;
        UpdateManualSummary();
    }

    private async void ManualApplyOrderBtn_Click(object sender, RoutedEventArgs e)
    {
        var parent = ManualPathBox.Text.Trim();
        if (string.IsNullOrEmpty(parent) || !Directory.Exists(parent))
        {
            AppendLogError(ActivityRequestLog.FormatRequestLine(parent, "! Pick a valid target folder first."));
            return;
        }

        if (IsManualTreeMode())
        {
            if (_manualTreeRoots.Count == 0)
            {
                AppendLogError(ActivityRequestLog.FormatRequestLine(parent, "! Nothing to apply. Refresh after choosing a folder."));
                return;
            }
        }
        else if (_manualFolders.Count == 0)
        {
            AppendLogError(ActivityRequestLog.FormatRequestLine(parent, "! Nothing to apply. Refresh after choosing a folder."));
            return;
        }

        ClearManualActivityLogDocument();
        ManualLastRunSummaryBlock.Text = string.Empty;
        AppendLog(ActivityRequestLog.FormatRequestLine(parent, $"> apply order {DateTime.Now:HH:mm:ss}"));

        try
        {
            ApplyOrganizeService.ApplyResult result;
            if (IsManualTreeMode())
            {
                result = await Task.Run(() =>
                    ApplyOrganizeService.ApplyTree(_manualTreeRoots.ToList(), line =>
                        Dispatcher.Invoke(() => AppendLog(line)))).ConfigureAwait(true);

                foreach (var n in EnumerateManualTreeNodes())
                    FolderScanner.Classify(n.Row);

                FolderTreeRenumber.RenumberAll(_manualTreeRoots);
                SyncManualTreeFlatView();
            }
            else
            {
                result = await Task.Run(() =>
                    ApplyOrganizeService.Apply(parent, _manualFolders.ToList(), line =>
                        Dispatcher.Invoke(() => AppendLog(line)))).ConfigureAwait(true);

                foreach (var row in _manualFolders)
                    FolderScanner.Classify(row);

                RenumberManualOrders();
            }

            UpdateManualSummary();
            ManualLastRunSummaryBlock.Text = RunSummaryText.ForApplyOrder(result);
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
            AppendLogError($"! {ex.Message}");
        }
    }

    private void ManualListExportBtn_Click(object sender, RoutedEventArgs e)
    {
        var root = ManualPathBox.Text.Trim();
        if (string.IsNullOrEmpty(root) || !Directory.Exists(root))
        {
            AppendLogError("! Choose a valid target folder before exporting a list.");
            return;
        }

        ListPaths.EnsureListsDirectory();
        ManualOperationsService.OpResult r = IsManualTreeMode()
            ? ManualOperationsService.WriteListOrderedTreeRoots(root, _manualTreeRoots.ToList(), ListPaths.ListsDirectory)
            : ManualOperationsService.WriteListOrderedFirstLevel(root, _manualFolders.ToList(), ListPaths.ListsDirectory);

        AppendLog("> Export list file…");
        foreach (var m in r.Messages.Where(x => !x.StartsWith("Summary —", StringComparison.OrdinalIgnoreCase)))
            AppendLog(m);
        ManualLastRunSummaryBlock.Text = RunSummaryText.ForListExport(r);
    }

    private void ManualListImportBtn_Click(object sender, RoutedEventArgs e)
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

        if (!FolderListFile.TryParseRootLine(lines, out var pathRoot) || string.IsNullOrWhiteSpace(pathRoot))
        {
            AppendLogError("! List file does not contain a root folder path (e.g. D:\\MyFolder\\).");
            return;
        }

        if (!Directory.Exists(pathRoot))
        {
            AppendLogError($"! List folder no longer exists: {pathRoot}");
            return;
        }

        ManualPathBox.Text = pathRoot;
        RaiseTargetFolderPathChangedIfNeeded();
        TryRefreshManualList();
        ManualLastRunSummaryBlock.Text = RunSummaryText.ForListImport(pathRoot);
    }

    private void ManualListOpenFolderBtn_Click(object sender, RoutedEventArgs e)
    {
        ListPaths.EnsureListsDirectory();
        AppendLog("> Opened the saved-lists folder in File Explorer.");
        Process.Start(new ProcessStartInfo
        {
            FileName = ListPaths.ListsDirectory,
            UseShellExecute = true
        });
    }

    private void ManualFolderList_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (RowIncludeSweep.TryBegin(ManualFolderList, e, expectTree: false, out var target))
        {
            _manualIncludeSweep = true;
            _manualIncludeSweepTarget = target;
            _manualIncludeSweepIsTree = false;
            return;
        }

        if (IsClickFromRowChrome(e.OriginalSource)) return;
        _dragStart = e.GetPosition(null);
        _dragSource = HitTestRowAt(e.GetPosition(ManualFolderList));
        _dragInProgress = false;
    }

    private void ManualFolderList_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (_manualIncludeSweep && ReferenceEquals(Mouse.Captured, ManualFolderList))
        {
            UpdateManualIncludeSweep(ManualFolderList, tree: false, e.GetPosition(ManualFolderList));
            EndManualIncludeSweep();
        }
    }

    private void ManualFolderList_LostMouseCapture(object sender, MouseEventArgs e)
    {
        if (!_manualIncludeSweep)
            return;
        UpdateManualIncludeSweep(ManualFolderList, tree: false, Mouse.GetPosition(ManualFolderList), requireCapture: false);
        EndManualIncludeSweep();
    }

    private void ManualFolderList_MouseMove(object sender, MouseEventArgs e)
    {
        if (_manualIncludeSweep && ReferenceEquals(Mouse.Captured, ManualFolderList))
        {
            UpdateManualIncludeSweep(ManualFolderList, tree: false, e.GetPosition(ManualFolderList));
            return;
        }

        if (e.LeftButton != MouseButtonState.Pressed || _dragSource is null || _dragInProgress)
            return;

        var pos = e.GetPosition(null);
        if (Math.Abs(pos.X - _dragStart.X) < SystemParameters.MinimumHorizontalDragDistance
            && Math.Abs(pos.Y - _dragStart.Y) < SystemParameters.MinimumVerticalDragDistance)
            return;

        _dragInProgress = true;
        DragDrop.DoDragDrop(ManualFolderList, _dragSource, DragDropEffects.Move);
        _dragInProgress = false;
        _dragSource = null;
        HideDropIndicator();
    }

    private void ManualFolderList_DragOver(object sender, DragEventArgs e)
    {
        if (!e.Data.GetDataPresent(typeof(FolderRow)))
        {
            e.Effects = DragDropEffects.None;
            e.Handled = true;
            HideDropIndicator();
            return;
        }

        var pos = e.GetPosition(ManualFolderList);
        if (pos.Y < 0 || pos.Y > ManualFolderList.ActualHeight)
        {
            HideDropIndicator();
            e.Effects = DragDropEffects.None;
            e.Handled = true;
            return;
        }

        e.Effects = DragDropEffects.Move;
        e.Handled = true;
        UpdateDropIndicator(pos);
    }

    private void ManualFolderList_DragLeave(object sender, DragEventArgs e) => HideDropIndicator();

    private void ManualFolderList_Drop(object sender, DragEventArgs e)
    {
        HideDropIndicator();
        if (e.Data.GetData(typeof(FolderRow)) is not FolderRow source)
            return;

        var pos = e.GetPosition(ManualFolderList);
        var insertIndex = GetInsertIndexFromPoint(pos);
        var oldIndex = _manualFolders.IndexOf(source);
        if (oldIndex < 0) return;

        var newIndex = oldIndex < insertIndex ? insertIndex - 1 : insertIndex;
        if (newIndex < 0 || newIndex >= _manualFolders.Count) return;
        if (newIndex == oldIndex) return;

        _manualFolders.Move(oldIndex, newIndex);
        RenumberManualOrders();
        UpdateManualSummary();
    }

    private int GetInsertIndexFromPoint(System.Windows.Point positionOverList)
    {
        if (_manualFolders.Count == 0) return 0;
        for (var i = 0; i < _manualFolders.Count; i++)
        {
            if (ManualFolderList.ItemContainerGenerator.ContainerFromIndex(i) is not ListBoxItem item)
                return i;
            var top = item.TranslatePoint(new System.Windows.Point(0, 0), ManualFolderList).Y;
            var h = item.ActualHeight > 1 ? item.ActualHeight : 28;
            if (positionOverList.Y < top + h / 2)
                return i;
        }

        return _manualFolders.Count;
    }

    private void UpdateDropIndicator(System.Windows.Point positionOverList)
    {
        if (_manualFolders.Count == 0)
        {
            HideDropIndicator();
            return;
        }

        var insert = GetInsertIndexFromPoint(positionOverList);
        double y;
        if (insert >= _manualFolders.Count)
        {
            var last = ManualFolderList.ItemContainerGenerator.ContainerFromIndex(_manualFolders.Count - 1) as ListBoxItem;
            if (last == null)
            {
                HideDropIndicator();
                return;
            }

            y = last.TranslatePoint(new System.Windows.Point(0, last.ActualHeight), ManualExplorerItemsGrid).Y;
        }
        else
        {
            var item = ManualFolderList.ItemContainerGenerator.ContainerFromIndex(insert) as ListBoxItem;
            if (item == null)
            {
                HideDropIndicator();
                return;
            }

            y = item.TranslatePoint(new System.Windows.Point(0, 0), ManualExplorerItemsGrid).Y;
        }

        ManualDropInsertLine.Margin = new Thickness(8, y, 8, 0);
        ManualDropInsertLine.Visibility = Visibility.Visible;
    }

    private void HideDropIndicator() => ManualDropInsertLine.Visibility = Visibility.Collapsed;

    private FolderRow? HitTestRowAt(System.Windows.Point pointRelativeToListBox)
    {
        var hit = VisualTreeHelper.HitTest(ManualFolderList, pointRelativeToListBox);
        DependencyObject? current = hit?.VisualHit;
        while (current != null)
        {
            if (current is ListBoxItem item)
                return item.DataContext as FolderRow;
            current = VisualTreeHelper.GetParent(current);
        }

        return null;
    }

    private FolderTreeNode? HitTestManualTreeNode(System.Windows.Point pointRelativeToTree)
    {
        var hit = VisualTreeHelper.HitTest(ManualFolderTreeList, pointRelativeToTree);
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
        node.Parent?.Children ?? _manualTreeRoots;

    private void ManualFolderTreeList_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (RowIncludeSweep.TryBegin(ManualFolderTreeList, e, expectTree: true, out var target))
        {
            _manualIncludeSweep = true;
            _manualIncludeSweepTarget = target;
            _manualIncludeSweepIsTree = true;
            return;
        }

        if (IsClickFromRowChrome(e.OriginalSource)) return;
        _treeDragStart = e.GetPosition(null);
        _treeDragSource = HitTestManualTreeNode(e.GetPosition(ManualFolderTreeList));
        _treeDragInProgress = false;
    }

    private void ManualFolderTreeList_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (_manualIncludeSweep && ReferenceEquals(Mouse.Captured, ManualFolderTreeList))
        {
            UpdateManualIncludeSweep(ManualFolderTreeList, tree: true, e.GetPosition(ManualFolderTreeList));
            EndManualIncludeSweep();
        }
    }

    private void ManualFolderTreeList_LostMouseCapture(object sender, MouseEventArgs e)
    {
        if (!_manualIncludeSweep)
            return;
        UpdateManualIncludeSweep(ManualFolderTreeList, tree: true, Mouse.GetPosition(ManualFolderTreeList), requireCapture: false);
        EndManualIncludeSweep();
    }

    private void ManualFolderTreeList_MouseMove(object sender, MouseEventArgs e)
    {
        if (_manualIncludeSweep && ReferenceEquals(Mouse.Captured, ManualFolderTreeList))
        {
            UpdateManualIncludeSweep(ManualFolderTreeList, tree: true, e.GetPosition(ManualFolderTreeList));
            return;
        }

        if (e.LeftButton != MouseButtonState.Pressed || _treeDragSource is null || _treeDragInProgress)
            return;

        var pos = e.GetPosition(null);
        if (Math.Abs(pos.X - _treeDragStart.X) < SystemParameters.MinimumHorizontalDragDistance
            && Math.Abs(pos.Y - _treeDragStart.Y) < SystemParameters.MinimumVerticalDragDistance)
            return;

        _treeDragInProgress = true;
        DragDrop.DoDragDrop(ManualFolderTreeList, _treeDragSource, DragDropEffects.Move);
        _treeDragInProgress = false;
        _treeDragSource = null;
        HideDropIndicator();
    }

    private void ManualFolderTreeList_DragOver(object sender, DragEventArgs e)
    {
        if (!e.Data.GetDataPresent(typeof(FolderTreeNode)))
        {
            e.Effects = DragDropEffects.None;
            e.Handled = true;
            HideDropIndicator();
            return;
        }

        var pos = e.GetPosition(ManualFolderTreeList);
        if (pos.Y < 0 || pos.Y > ManualFolderTreeList.ActualHeight)
        {
            HideDropIndicator();
            e.Effects = DragDropEffects.None;
            e.Handled = true;
            return;
        }

        e.Effects = DragDropEffects.Move;
        e.Handled = true;
        UpdateManualTreeDropIndicator(pos);
    }

    private void ManualFolderTreeList_DragLeave(object sender, DragEventArgs e) =>
        HideDropIndicator();

    private void ManualFolderTreeList_Drop(object sender, DragEventArgs e)
    {
        HideDropIndicator();
        if (e.Data.GetData(typeof(FolderTreeNode)) is not FolderTreeNode source)
            return;

        var pos = e.GetPosition(ManualFolderTreeList);
        var insertFlat = FolderTreeFlatReorder.GetInsertIndexFromPoint(ManualFolderTreeList, _manualTreeFlatView.Count, pos);
        if (!FolderTreeFlatReorder.TryReorderAtFlatInsertGap(_manualTreeFlatView, _manualTreeRoots, source, insertFlat))
            return;

        FolderTreeRenumber.RenumberAll(_manualTreeRoots);
        SyncManualTreeFlatView();
        UpdateManualSummary();
    }

    private void UpdateManualTreeDropIndicator(System.Windows.Point positionOverTree)
    {
        if (_manualTreeFlatView.Count == 0)
        {
            HideDropIndicator();
            return;
        }

        var insert = FolderTreeFlatReorder.GetInsertIndexFromPoint(ManualFolderTreeList, _manualTreeFlatView.Count, positionOverTree);
        double y;
        if (insert >= _manualTreeFlatView.Count)
        {
            var last = ManualFolderTreeList.ItemContainerGenerator.ContainerFromIndex(_manualTreeFlatView.Count - 1) as ListBoxItem;
            if (last == null)
            {
                HideDropIndicator();
                return;
            }

            y = last.TranslatePoint(new System.Windows.Point(0, last.ActualHeight), ManualExplorerItemsGrid).Y;
        }
        else
        {
            var item = ManualFolderTreeList.ItemContainerGenerator.ContainerFromIndex(insert) as ListBoxItem;
            if (item == null)
            {
                HideDropIndicator();
                return;
            }

            y = item.TranslatePoint(new System.Windows.Point(0, 0), ManualExplorerItemsGrid).Y;
        }

        ManualDropInsertLine.Margin = new Thickness(8, y, 8, 0);
        ManualDropInsertLine.Visibility = Visibility.Visible;
    }
}
