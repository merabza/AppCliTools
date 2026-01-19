using CliMenu;
using SystemTools.SystemToolsShared;

// ReSharper disable ConvertToPrimaryConstructor

namespace CliParameters.CliMenuCommands;

public sealed class EditParametersInSequenceCliMenuCommand : CliMenuCommand
{
    private readonly ParametersEditor _parametersEditor;

    public EditParametersInSequenceCliMenuCommand(ParametersEditor parametersEditor) : base(
        "Edit Parameters in sequence", EMenuAction.LevelUp)
    {
        _parametersEditor = parametersEditor;
    }

    protected override bool RunBody()
    {
        if (!string.IsNullOrWhiteSpace(ParentMenuName))
        {
            return _parametersEditor.EditParametersInSequence();
        }

        StShared.WriteErrorLine("Empty Parent Menu Name ", true);
        return false;
    }
}
