using AppCliTools.CliMenu;
using AppCliTools.CliParameters.FieldEditors;

namespace AppCliTools.CliParameters.CliMenuCommands;

public sealed class ParameterSubObjectFieldEditorCliMenuCommand : CliMenuCommand
{
    private readonly FieldEditor _fieldEditor;
    private readonly ParametersEditor _parametersEditor;

    // ReSharper disable once ConvertToPrimaryConstructor
    public ParameterSubObjectFieldEditorCliMenuCommand(string fieldName, FieldEditor fieldEditor,
        ParametersEditor parametersEditor) : base(fieldName, EMenuAction.LoadSubMenu, EMenuAction.LoadSubMenu, null,
        false, EStatusView.Table)
    {
        _fieldEditor = fieldEditor;
        _parametersEditor = parametersEditor;
    }

    public override CliMenuSet? GetSubMenu()
    {
        return _fieldEditor.GetSubMenu(_parametersEditor.Parameters);
    }

    protected override string GetStatus()
    {
        return _fieldEditor.GetValueStatus(_parametersEditor.Parameters);
    }
}
