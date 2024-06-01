using CliMenu;

namespace CliTools.CliMenuCommands;

public sealed class ExitCliMenuCommand : CliMenuCommand
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public ExitCliMenuCommand() : base(null, EMenuAction.Exit)
    {
    }
}