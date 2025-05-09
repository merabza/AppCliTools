using CliMenu;

// ReSharper disable ConvertToPrimaryConstructor

namespace CliParameters.CliMenuCommands;

public sealed class ParametersEditorListCliMenuCommand : CliMenuCommand
{
    private readonly ParametersEditor _parametersEditor;

    public ParametersEditorListCliMenuCommand(ParametersEditor parametersEditor) : base(parametersEditor.Name,
        EMenuAction.LoadSubMenu)
    {
        _parametersEditor = parametersEditor;
    }

    public override CliMenuSet GetSubMenu()
    {
        return _parametersEditor.GetParametersMainMenu();
    }

    protected override string? GetStatus()
    {
        return ParametersEditor.GetStatus();
    }
}