using MediaSuite.Manager.Shell;

namespace MediaSuite.Manager.Tools;

internal sealed class ToolComponent
{
    private readonly ToolDefinition _definition;

    public ToolComponent(ToolDefinition definition)
    {
        _definition = definition;
    }

    public string DisplayName => _definition.DisplayName;

    public string ExecutablePath => Path.Combine(AppContext.BaseDirectory, _definition.ExecutableFileName);

    public bool ExecutableExists => File.Exists(ExecutablePath);

    public bool IsRegistered => ContextMenuRegistrar.IsRegistered(_definition.Id, _definition.Extensions[0]);

    public void Register()
    {
        if (!ExecutableExists)
            throw new InvalidOperationException($"{_definition.ExecutableFileName} wasn't found beside the Manager; reinstall the suite.");

        ContextMenuRegistrar.Register(_definition.Id, _definition.MenuText, ExecutablePath, _definition.Extensions);
    }

    public void Unregister() => ContextMenuRegistrar.Unregister(_definition.Id, _definition.Extensions);
}
