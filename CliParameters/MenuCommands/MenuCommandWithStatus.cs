using CliMenu;

namespace CliParameters.MenuCommands;

public sealed class MenuCommandWithStatus : CliMenuCommand
{
    private readonly string? _status;

    public MenuCommandWithStatus(string? status)
    {
        _status = status;
    }

    protected override string? GetStatus()
    {
        return _status;
    }
}