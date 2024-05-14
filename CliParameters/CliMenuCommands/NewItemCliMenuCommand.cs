using CliMenu;

namespace CliParameters.CliMenuCommands;

public sealed class NewItemCliMenuCommand : CliMenuCommand
{
    private readonly Cruder _cruder;

    // ReSharper disable once ConvertToPrimaryConstructor
    public NewItemCliMenuCommand(Cruder cruder, string parentMenuName, string commandName) : base(commandName,
        parentMenuName)
    {
        _cruder = cruder;
    }

    protected override void RunAction()
    {
        MenuAction = EMenuAction.Reload;
        _cruder.CreateNewRecord();
    }
}