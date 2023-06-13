using CliMenu;

namespace CliParameters.MenuCommands;

public sealed class ListItemMenuCommand : CliMenuCommand
{
    private readonly Cruder _cruder;

    public ListItemMenuCommand(Cruder cruder, string name) : base(name)
    {
        _cruder = cruder;
    }

    protected override string? GetStatus()
    {
        return Name is null ? null : _cruder.GetStatusFor(Name);
    }
}