using System.Collections.ObjectModel;
using System.Net.Http;
using System.Windows;
using MediaSuite.Core.Updates;
using MediaSuite.Manager.Dependencies;
using MediaSuite.Manager.Tools;

namespace MediaSuite.Manager;

public partial class MainWindow : Window
{
    private const string Caption = "Media Tools Suite";

    private readonly HttpClient _httpClient = new();
    private readonly ObservableCollection<ComponentRow> _rows = new();

    public MainWindow()
    {
        InitializeComponent();

        var dependencies = new List<IManagedDependency>
        {
            new GhostscriptDependency(),
            new FfmpegDependency(),
            new ImageMagickDependency(),
        };

        var tools = ToolCatalog.All.Select(definition => new ToolComponent(definition)).ToList();

        foreach (var dependency in dependencies)
            _rows.Add(ComponentRow.ForDependency(dependency));

        foreach (var tool in tools)
            _rows.Add(ComponentRow.ForTool(tool));

        ComponentsGrid.ItemsSource = _rows;

        RefreshToolRowsFromDisk();
    }

    private void RefreshToolRowsFromDisk()
    {
        foreach (var row in _rows.Where(r => r.Kind == ComponentKind.Tool))
        {
            var tool = row.Tool!;
            if (!tool.ExecutableExists)
            {
                row.InstalledVersion = "-";
                row.Status = "Missing (reinstall the suite)";
                row.ActionLabel = "Install";
                row.CanRemove = false;
                continue;
            }

            var registered = tool.IsRegistered;
            row.InstalledVersion = registered ? "Installed" : "-";
            row.Status = registered ? "Installed" : "Not installed";
            row.ActionLabel = registered ? "Reinstall" : "Install";
            row.CanRemove = registered;
        }
    }

    private async void CheckForUpdatesButton_Click(object sender, RoutedEventArgs e)
    {
        CheckForUpdatesButton.IsEnabled = false;
        try
        {
            var dependencyRows = _rows.Where(r => r.Kind == ComponentKind.Dependency).ToList();
            await Task.WhenAll(dependencyRows.Select(CheckDependencyAsync));

            RefreshToolRowsFromDisk();

            LastCheckedText.Text = $"Last checked: {DateTime.Now:t}";
        }
        finally
        {
            CheckForUpdatesButton.IsEnabled = true;
        }
    }

    private async Task CheckDependencyAsync(ComponentRow row)
    {
        var dependency = row.Dependency!;
        row.IsBusy = true;
        row.Status = "Checking...";
        try
        {
            var installed = await dependency.GetInstalledVersionAsync(CancellationToken.None).ConfigureAwait(true);
            var (latest, error) = await TryGetLatestAsync(dependency).ConfigureAwait(true);

            row.InstalledVersion = installed ?? "Not installed";

            if (latest is null)
            {
                row.LatestVersion = "?";
                row.Status = error ?? "Couldn't check for updates";
                row.ActionLabel = installed is null ? "Install" : "Update";
                row.CanRemove = installed is not null;
                return;
            }

            row.PendingRelease = latest;
            row.LatestVersion = latest.Version;

            if (installed is null)
            {
                row.Status = "Not installed";
                row.ActionLabel = "Install";
                row.CanRemove = false;
            }
            else if (DottedVersionComparer.IsNewer(latest.Version, installed))
            {
                row.Status = $"Update available ({installed} -> {latest.Version})";
                row.ActionLabel = "Update";
                row.CanRemove = true;
            }
            else
            {
                row.Status = "Up to date";
                row.ActionLabel = "Reinstall";
                row.CanRemove = true;
            }
        }
        finally
        {
            row.IsBusy = false;
        }
    }

    private async Task<(ResolvedRelease? Release, string? Error)> TryGetLatestAsync(IManagedDependency dependency)
    {
        try
        {
            var release = await dependency.GetLatestReleaseAsync(_httpClient, CancellationToken.None).ConfigureAwait(false);
            return (release, null);
        }
        catch (Exception ex)
        {
            return (null, $"Couldn't check for updates: {ex.Message}");
        }
    }

    private async void ActionButton_Click(object sender, RoutedEventArgs e)
    {
        if (((FrameworkElement)sender).DataContext is not ComponentRow row)
            return;

        row.IsBusy = true;
        try
        {
            if (row.Kind == ComponentKind.Dependency)
                await InstallOrUpdateDependencyAsync(row).ConfigureAwait(true);
            else
                InstallTool(row);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, Caption, MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            row.IsBusy = false;
        }
    }

    private async Task InstallOrUpdateDependencyAsync(ComponentRow row)
    {
        var dependency = row.Dependency!;
        row.Status = "Downloading...";

        var release = row.PendingRelease
            ?? await dependency.GetLatestReleaseAsync(_httpClient, CancellationToken.None).ConfigureAwait(true);
        row.PendingRelease = release;

        await dependency.InstallAsync(_httpClient, release, CancellationToken.None).ConfigureAwait(true);

        row.Status = "Verifying...";
        var installed = await dependency.GetInstalledVersionAsync(CancellationToken.None).ConfigureAwait(true);
        row.InstalledVersion = installed ?? "Unknown";
        row.LatestVersion = release.Version;
        row.Status = installed is not null ? "Up to date" : "Installed (version check failed)";
        row.ActionLabel = "Reinstall";
        row.CanRemove = true;
    }

    private void InstallTool(ComponentRow row)
    {
        row.Tool!.Register();
        row.InstalledVersion = "Installed";
        row.Status = "Installed";
        row.ActionLabel = "Reinstall";
        row.CanRemove = true;
    }

    private async void RemoveButton_Click(object sender, RoutedEventArgs e)
    {
        if (((FrameworkElement)sender).DataContext is not ComponentRow row)
            return;

        var confirm = MessageBox.Show(
            this,
            $"Remove {row.DisplayName}?",
            Caption,
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (confirm != MessageBoxResult.Yes)
            return;

        row.IsBusy = true;
        try
        {
            if (row.Kind == ComponentKind.Dependency)
                await row.Dependency!.RemoveAsync(CancellationToken.None).ConfigureAwait(true);
            else
                row.Tool!.Unregister();

            row.InstalledVersion = "-";
            row.Status = "Not installed";
            row.ActionLabel = "Install";
            row.CanRemove = false;
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, Caption, MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            row.IsBusy = false;
        }
    }
}
