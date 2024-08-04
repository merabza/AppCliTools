using CliMenu;

namespace CliParameters.CliMenuCommands;

public sealed class NewItemCliMenuCommand : CliMenuCommand
{
    private readonly Cruder _cruder;

    // ReSharper disable once ConvertToPrimaryConstructor
    public NewItemCliMenuCommand(Cruder cruder, string parentMenuName, string commandName) : base(commandName,
        EMenuAction.Reload, EMenuAction.Reload, parentMenuName)
    {
        _cruder = cruder;
    }

    protected override bool RunBody()
    {
        MenuAction = EMenuAction.Reload;
        return _cruder.CreateNewRecord() is not null;
    }
}