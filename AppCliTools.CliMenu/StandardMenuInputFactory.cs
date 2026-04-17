using System;
using AppCliTools.CliMenu.CliMenuCommands;
using AppCliTools.LibDataInput;
using SystemTools.SystemToolsShared;

namespace AppCliTools.CliMenu;

public static class StandardMenuInputFactory
{
    //public static void AddEscapeCommand(this CliMenuSet cliMenuSet, string commandName = "Exit to level up menu")
    //{
    //    string key = ConsoleKey.Escape.Value().Pascalize();
    //    cliMenuSet.AddMenuItem(key, new CliMenuCommand(commandName), key.Length);
    //}

    public static void AddEscapeCommand(this CliMenuSet cliMenuSet, CliMenuCommand command)
    {
        string key = ConsoleKey.Escape.Value().Pascalize();
        cliMenuSet.AddMenuItem(key, command);
    }

    public static void AddEscapeCommand(this CliMenuSet cliMenuSet, string commandName = "Exit to level up menu")
    {
        string key = ConsoleKey.Escape.Value().Pascalize();
        cliMenuSet.AddMenuItem(key, new ExitToMainMenuCliMenuCommand(commandName, null));
    }
}
