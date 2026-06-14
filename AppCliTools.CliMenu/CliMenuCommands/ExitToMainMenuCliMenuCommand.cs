namespace AppCliTools.CliMenu.CliMenuCommands;

public sealed class ExitToMainMenuCliMenuCommand(string name, string? parentMenuName)
    : CliMenuCommand(name, EMenuAction.LevelUp, EMenuAction.LevelUp, parentMenuName);
