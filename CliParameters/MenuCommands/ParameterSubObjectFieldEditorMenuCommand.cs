using CliMenu;
using CliParameters.FieldEditors;
// ReSharper disable ConvertToPrimaryConstructor

namespace CliParameters.MenuCommands;

public sealed class ParameterSubObjectFieldEditorMenuCommand : CliMenuCommand
{
    private readonly FieldEditor _fieldEditor;
    private readonly ParametersEditor _parametersEditor;

    public ParameterSubObjectFieldEditorMenuCommand(string fieldName, FieldEditor fieldEditor,
        ParametersEditor parametersEditor) : base(fieldName, null, false, EStatusView.Table)
    {
        _fieldEditor = fieldEditor;
        _parametersEditor = parametersEditor;
    }


    protected override void RunAction()
    {
        MenuAction = EMenuAction.LoadSubMenu;
    }

    public override CliMenuSet? GetSubmenu()
    {
        return _fieldEditor.GetSubMenu(_parametersEditor.Parameters);
    }

    protected override string GetStatus()
    {
        return _fieldEditor.GetValueStatus(_parametersEditor.Parameters);
    }
}