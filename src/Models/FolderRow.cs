using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using Ordir.Services;

namespace Ordir.Models;

public sealed class FolderRow : INotifyPropertyChanged
{
    private int _displayOrder;
    private FolderVisualStatus _status;
    private bool _isExcluded;
    private bool _hasDesktopIni;
    private bool _isSystemFolder;
    private bool _iniHidden;
    private bool _hasWellFormedOrderTip;
    private bool _showExistingTipNumbers = true;
    private bool _sortForcesInfotipColumn;

    public required string Name { get; init; }
    public required string FullPath { get; init; }

    public bool IsExcluded
    {
        get => _isExcluded;
        set
        {
            if (_isExcluded == value) return;
            _isExcluded = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsIncluded));
            NotifyOrderColumn();
        }
    }

    /// <summary>When true, this folder is included for Apply and manual batch actions.</summary>
    public bool IsIncluded
    {
        get => !_isExcluded;
        set => IsExcluded = !value;
    }

    public int DisplayOrder
    {
        get => _displayOrder;
        set
        {
            if (_displayOrder == value) return;
            _displayOrder = value;
            NotifyOrderColumn();
        }
    }

    /// <summary>Existing InfoTip from desktop.ini when well-formed; shown left of new order #.</summary>
    public bool ShowOrderCurrentTipColumn =>
        !IsExcluded &&
        _hasWellFormedOrderTip &&
        !string.IsNullOrWhiteSpace(CurrentInfoTip) &&
        (_showExistingTipNumbers || _sortForcesInfotipColumn);

    public void SetShowExistingTipNumbers(bool show)
    {
        if (_showExistingTipNumbers == show) return;
        _showExistingTipNumbers = show;
        NotifyOrderColumn();
    }

    public void SetSortForcesInfotipColumn(bool force)
    {
        if (_sortForcesInfotipColumn == force) return;
        _sortForcesInfotipColumn = force;
        NotifyOrderColumn();
    }

    public string OrderColumnCurrentTipText =>
        ShowOrderCurrentTipColumn ? CurrentInfoTip!.Trim() : "";

    /// <summary>New apply order # for this run (right column).</summary>
    public string OrderColumnNewOrderText =>
        IsExcluded ? "--" : (_displayOrder > 0 ? $"#{_displayOrder}" : "--");

    public bool ShowBadgeSystem => _isSystemFolder;

    public bool ShowBadgeIni => _hasDesktopIni;

    public bool ShowBadgeNoIniNotSystem =>
        !_hasDesktopIni && !_isSystemFolder && _status != FolderVisualStatus.Error;

    public bool ShowBadgeMisconfigured =>
        !IsExcluded && _hasDesktopIni &&
        (_status == FolderVisualStatus.IniIncomplete
         || _status == FolderVisualStatus.NeedsSystemFolder
         || _status == FolderVisualStatus.Error);

    public bool IniIsHidden => _hasDesktopIni && _iniHidden;

    public FolderVisualStatus Status
    {
        get => _status;
        set
        {
            if (_status == value) return;
            _status = value;
            OnPropertyChanged();
            NotifyBadgeProps();
        }
    }

    public string? CurrentInfoTip { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;

    internal void SetScanState(
        FolderVisualStatus status,
        string? currentInfoTip,
        bool hasDesktopIni,
        bool isSystemFolder,
        bool iniHidden,
        bool hasWellFormedOrderTip)
    {
        _status = status;
        CurrentInfoTip = currentInfoTip;
        _hasDesktopIni = hasDesktopIni;
        _isSystemFolder = isSystemFolder;
        _iniHidden = iniHidden;
        _hasWellFormedOrderTip = hasWellFormedOrderTip;
        OnPropertyChanged(nameof(Status));
        OnPropertyChanged(nameof(CurrentInfoTip));
        NotifyBadgeProps();
        NotifyOrderColumn();
    }

    private void NotifyBadgeProps()
    {
        OnPropertyChanged(nameof(ShowBadgeSystem));
        OnPropertyChanged(nameof(ShowBadgeIni));
        OnPropertyChanged(nameof(ShowBadgeNoIniNotSystem));
        OnPropertyChanged(nameof(ShowBadgeMisconfigured));
        OnPropertyChanged(nameof(IniIsHidden));
    }

    private void NotifyOrderColumn()
    {
        OnPropertyChanged(nameof(ShowOrderCurrentTipColumn));
        OnPropertyChanged(nameof(OrderColumnCurrentTipText));
        OnPropertyChanged(nameof(OrderColumnNewOrderText));
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    /// <summary>Appends the multi-line &quot;Selected Folder Info&quot; block for the summary panel.</summary>
    public static void AppendSelectedFolderDetails(StringBuilder sb, FolderRow? r)
    {
        if (r is null)
        {
            var dash = "\u2014";
            sb.AppendLine($"System folder: {dash}");
            sb.AppendLine($"desktop.ini: {dash}");
            sb.AppendLine($"Hidden: {dash}");
            sb.AppendLine($"Infotip: {dash}");
            sb.AppendLine($"New order: {dash}");
            return;
        }

        // True: checkmark. False: circled slash (⊘), not ballot X (✗).
        sb.AppendLine($"System folder: {(r.ShowBadgeSystem ? "\u2713" : "\u2298")}");
        sb.AppendLine($"desktop.ini: {(r.ShowBadgeIni ? "\u2713" : "\u2298")}");
        var hidden = !r.ShowBadgeIni ? "\u2014" : (r.IniIsHidden ? "\u2713" : "\u2298");
        sb.AppendLine($"Hidden: {hidden}");
        var tip = !string.IsNullOrWhiteSpace(r.CurrentInfoTip) && DesktopIniService.IsWellFormedOrderTip(r.CurrentInfoTip)
            ? r.CurrentInfoTip!.Trim()
            : "\u2014";
        sb.AppendLine($"Infotip: {tip}");
        sb.AppendLine($"New order: {r.OrderColumnNewOrderText}");
    }
}
