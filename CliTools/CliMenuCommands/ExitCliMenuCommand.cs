using CliMenu;

namespace CliTools.CliMenuCommands;

public sealed class ExitCliMenuCommand : CliMenuCommand
{
    protected override void RunAction()
    {
        MenuAction = EMenuAction.Exit;
    }
}