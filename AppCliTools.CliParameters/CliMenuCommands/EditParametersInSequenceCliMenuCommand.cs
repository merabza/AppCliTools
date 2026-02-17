using System.Threading;
using System.Threading.Tasks;
using AppCliTools.CliMenu;
using SystemTools.SystemToolsShared;

// ReSharper disable ConvertToPrimaryConstructor

namespace AppCliTools.CliParameters.CliMenuCommands;

public sealed class EditParametersInSequenceCliMenuCommand : CliMenuCommand
{
    private readonly ParametersEditor _parametersEditor;

    public EditParametersInSequenceCliMenuCommand(ParametersEditor parametersEditor) : base(
        "Edit Parameters in sequence", EMenuAction.LevelUp)
    {
        _parametersEditor = parametersEditor;
    }

    protected override ValueTask<bool> RunBody(CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(ParentMenuName))
        {
            return ValueTask.FromResult(_parametersEditor.EditParametersInSequence());
        }

        StShared.WriteErrorLine("Empty Parent Menu Name ", true);
        return ValueTask.FromResult(false);
    }
}
