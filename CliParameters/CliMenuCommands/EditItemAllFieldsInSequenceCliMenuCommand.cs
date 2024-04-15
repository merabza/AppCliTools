using System;
using CliMenu;
using LibDataInput;
using SystemToolsShared;

// ReSharper disable ConvertToPrimaryConstructor

namespace CliParameters.CliMenuCommands;

public sealed class EditItemAllFieldsInSequenceCliMenuCommand : CliMenuCommand
{
    private readonly Cruder _cruder;

    public EditItemAllFieldsInSequenceCliMenuCommand(Cruder cruder, string itemName) : base("Edit", itemName)
    {
        _cruder = cruder;
    }

    protected override void RunAction()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(ParentMenuName))
            {
                StShared.WriteErrorLine("Empty Parent Menu Name ", true);
                return;
            }

            if (_cruder.EditItemAllFieldsInSequence(ParentMenuName))
            {
                MenuAction = EMenuAction.LevelUp;
                return;
            }
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
}