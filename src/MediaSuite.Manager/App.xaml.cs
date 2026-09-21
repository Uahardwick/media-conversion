using System.Windows;
using MediaSuite.Manager.Tools;

namespace MediaSuite.Manager;

public partial class App : Application
{
    /// <summary>
    /// Supports two silent, headless CLI modes the installer calls directly
    /// (no GUI, no admin prompt beyond the one the installer already
    /// carries): registering every tool's right-click verb right after
    /// install so the suite works immediately, and unregistering all of
    /// them before uninstall removes the executables. Everything else
    /// launches the normal dashboard via App.xaml's StartupUri.
    /// </summary>
    protected override void OnStartup(StartupEventArgs e)
    {
        if (e.Args.Contains("--register-tools", StringComparer.OrdinalIgnoreCase))
        {
            ForEachTool(tool =>
            {
                if (tool.ExecutableExists)
                    tool.Register();
            });
            Shutdown();
            return;
        }

        if (e.Args.Contains("--unregister-tools", StringComparer.OrdinalIgnoreCase))
        {
            ForEachTool(tool => tool.Unregister());
            Shutdown();
            return;
        }

        base.OnStartup(e);
    }

    private static void ForEachTool(Action<ToolComponent> action)
    {
        foreach (var definition in ToolCatalog.All)
            action(new ToolComponent(definition));
    }
}
