using AppCliTools.CliMenu;

namespace AppCliTools.CliParameters.CliMenuCommands;

public sealed class MenuCommandWithStatusCliMenuCommand : CliMenuCommand
{
    private readonly string? _status;

    
    public MenuCommandWithStatusCliMenuCommand(string name, string status = "") : base(name)
    {
        _status = status;
    }

    protected override string? GetStatus()
    {
        return _status;
    }
}
