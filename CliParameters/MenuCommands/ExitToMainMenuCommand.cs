using CliMenu;

namespace CliParameters.MenuCommands;

public sealed class ExitToMainMenuCommand : CliMenuCommand
{
    public ExitToMainMenuCommand(string? name, string? parentMenuName) : base(name, parentMenuName)
    {
    }

    protected override void RunAction()
    {
        MenuAction = EMenuAction.LevelUp;
    }
}