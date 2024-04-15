using CliMenu;

namespace CliParameters.CliMenuCommands;

public sealed class ExitToMainMenuCliMenuCommand : CliMenuCommand
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public ExitToMainMenuCliMenuCommand(string? name, string? parentMenuName) : base(name, parentMenuName)
    {
    }

    protected override void RunAction()
    {
        MenuAction = EMenuAction.LevelUp;
    }
}