using CliMenu;
using CliParameters.Cruders;

namespace CliParameters.CliMenuCommands;

public sealed class ListItemCliMenuCommand : CliMenuCommand
{
    private readonly Cruder _cruder;

    // ReSharper disable once ConvertToPrimaryConstructor
    public ListItemCliMenuCommand(Cruder cruder, string name) : base(name)
    {
        _cruder = cruder;
    }

    protected override string? GetStatus()
    {
        return _cruder.GetStatusFor(Name);
    }
}