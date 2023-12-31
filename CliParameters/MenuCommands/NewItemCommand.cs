using System;
using CliMenu;
using LibDataInput;
using SystemToolsShared;
// ReSharper disable ConvertToPrimaryConstructor

namespace CliParameters.MenuCommands;

public sealed class NewItemCommand : CliMenuCommand
{
    private readonly Cruder _cruder;

    public NewItemCommand(Cruder cruder, string parentMenuName, string commandName) : base(commandName,
        parentMenuName)
    {
        _cruder = cruder;
    }

    protected override void RunAction()
    {
        try
        {
            MenuAction = EMenuAction.Reload;
            _cruder.CreateNewRecord();
        }
        catch (DataInputEscapeException)
        {
            Console.WriteLine();
            Console.WriteLine("Escape... ");
            StShared.Pause();
        }
        catch (Exception e)
        {
            StShared.WriteException(e, true);
        }
    }
}