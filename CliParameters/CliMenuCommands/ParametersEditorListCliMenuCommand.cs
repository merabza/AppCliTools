using CliMenu;

// ReSharper disable ConvertToPrimaryConstructor

namespace CliParameters.CliMenuCommands;

public sealed class ParametersEditorListCliMenuCommand : CliMenuCommand
{
    private readonly ParametersEditor _parametersEditor;

    public ParametersEditorListCliMenuCommand(ParametersEditor parametersEditor) : base(parametersEditor.Name)
    {
        _parametersEditor = parametersEditor;
    }

    protected override void RunAction()
    {
        MenuAction = EMenuAction.LoadSubMenu;
    }

    public override CliMenuSet GetSubmenu()
    {
        return _parametersEditor.GetParametersMainMenu();
    }


    protected override string? GetStatus()
    {
        return ParametersEditor.GetStatus();
    }
}