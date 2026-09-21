using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using MediaSuite.Core.Updates;
using MediaSuite.Manager.Dependencies;
using MediaSuite.Manager.Tools;

namespace MediaSuite.Manager;

internal enum ComponentKind
{
    Dependency,
    Tool,
}

internal sealed class ComponentRow : INotifyPropertyChanged
{
    private string _installedVersion = "-";
    private string _latestVersion = "-";
    private string _status = "Not checked yet";
    private bool _isBusy;
    private string _actionLabel = "Install";
    private bool _canRemove;

    private ComponentRow(ComponentKind kind, string displayName)
    {
        Kind = kind;
        DisplayName = displayName;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public static ComponentRow ForDependency(IManagedDependency dependency) =>
        new(ComponentKind.Dependency, dependency.DisplayName) { Dependency = dependency };

    public static ComponentRow ForTool(ToolComponent tool) =>
        new(ComponentKind.Tool, tool.DisplayName) { Tool = tool };

    public ComponentKind Kind { get; }

    public string DisplayName { get; }

    public IManagedDependency? Dependency { get; private init; }

    public ToolComponent? Tool { get; private init; }

    public ResolvedRelease? PendingRelease { get; set; }

    public string InstalledVersion
    {
        get => _installedVersion;
        set => SetField(ref _installedVersion, value);
    }

    public string LatestVersion
    {
        get => _latestVersion;
        set => SetField(ref _latestVersion, value);
    }

    public string Status
    {
        get => _status;
        set => SetField(ref _status, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (SetField(ref _isBusy, value))
                OnPropertyChanged(nameof(IsNotBusy));
        }
    }

    public bool IsNotBusy => !_isBusy;

    public string ActionLabel
    {
        get => _actionLabel;
        set => SetField(ref _actionLabel, value);
    }

    public bool CanRemove
    {
        get => _canRemove;
        set
        {
            if (SetField(ref _canRemove, value))
                OnPropertyChanged(nameof(RemoveVisibility));
        }
    }

    public Visibility RemoveVisibility => _canRemove ? Visibility.Visible : Visibility.Collapsed;

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void OnPropertyChanged(string? propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
