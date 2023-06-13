using CliMenu;

namespace CliTools.Commands;

public sealed class ExitCommand : CliMenuCommand
{
    protected override void RunAction()
    {
        MenuAction = EMenuAction.Exit;
    }
}