using CliMenu;

namespace CliTools.CliMenuCommands;

public sealed class ExitCliMenuCommand : CliMenuCommand
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public ExitCliMenuCommand() : base("Exit", EMenuAction.Exit)
    {
    }
}