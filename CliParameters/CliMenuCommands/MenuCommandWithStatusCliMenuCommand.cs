using CliMenu;

namespace CliParameters.CliMenuCommands;

public sealed class MenuCommandWithStatusCliMenuCommand : CliMenuCommand
{
    private readonly string? _status;

    // ReSharper disable once ConvertToPrimaryConstructor
    public MenuCommandWithStatusCliMenuCommand(string name, string status = "") : base(name)
    {
        _status = status;
    }

    protected override string? GetStatus()
    {
        return _status;
    }
}