using CliMenu;
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
        if (string.IsNullOrWhiteSpace(ParentMenuName))
        {
            StShared.WriteErrorLine("Empty Parent Menu Name ", true);
            return;
        }

        if (!_parametersEditor.EditParametersInSequence())
            return;

        MenuAction = EMenuAction.LevelUp;
    }
}