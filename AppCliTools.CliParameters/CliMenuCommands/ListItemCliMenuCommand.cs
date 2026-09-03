using AppCliTools.CliMenu;
using AppCliTools.CliParameters.Cruders;

namespace AppCliTools.CliParameters.CliMenuCommands;

public sealed class ListItemCliMenuCommand : CliMenuCommand
{
    private readonly Cruder _cruder;

    
    public ListItemCliMenuCommand(Cruder cruder, string name) : base(name)
    {
        _cruder = cruder;
    }

    protected override string? GetStatus()
    {
        return _cruder.GetStatusFor(Name);
    }
}
