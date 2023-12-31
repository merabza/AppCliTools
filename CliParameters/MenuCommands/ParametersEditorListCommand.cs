using CliMenu;
// ReSharper disable ConvertToPrimaryConstructor

namespace CliParameters.MenuCommands;

public sealed class ParametersEditorListCommand : CliMenuCommand
{
    private readonly ParametersEditor _parametersEditor;

    public ParametersEditorListCommand(ParametersEditor parametersEditor) : base(parametersEditor.Name)
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
        return _parametersEditor.GetStatus();
    }
}