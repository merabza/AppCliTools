using System;
using CliMenu;
using LibDataInput;
using SystemToolsShared;

// ReSharper disable ConvertToPrimaryConstructor

namespace CliParameters.CliMenuCommands;

public sealed class EditParametersInSequenceCliMenuCommand : CliMenuCommand
{
    private readonly ParametersEditor _parametersEditor;

    public EditParametersInSequenceCliMenuCommand(ParametersEditor parametersEditor) : base(
        "Edit Parameters in sequence")
    {
        _parametersEditor = parametersEditor;
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

            if (_parametersEditor.EditParametersInSequence())
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