using System;
using CliMenu;
using LibDataInput;
using SystemToolsShared;

// ReSharper disable ConvertToPrimaryConstructor

namespace CliParameters.MenuCommands;

public sealed class DeleteCommand : CliMenuCommand
{
    private readonly Cruder _cruder;

    public DeleteCommand(Cruder cruder, string fileStorageName) : base("Delete", fileStorageName)
    {
        _cruder = cruder;
    }

    protected override void RunAction()
    {
        try
        {
            if (RunBody())
                return;
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

        MenuAction = EMenuAction.Reload;
    }

    private bool RunBody()
    {
        if (string.IsNullOrWhiteSpace(ParentMenuName))
        {
            StShared.WriteErrorLine("Empty Parent Menu Name ", true);
            return false;
        }

        if (!_cruder.DeleteRecord(ParentMenuName))
            return false;

        MenuAction = EMenuAction.LevelUp;
        return true;
    }
}