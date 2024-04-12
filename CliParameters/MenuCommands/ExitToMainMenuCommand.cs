using CliMenu;

namespace CliParameters.MenuCommands;

public sealed class ExitToMainMenuCommand : CliMenuCommand
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public ExitToMainMenuCommand(string? name, string? parentMenuName) : base(name, parentMenuName)
    {
    }

    protected override void RunAction()
    {
        MenuAction = EMenuAction.LevelUp;
    }
}