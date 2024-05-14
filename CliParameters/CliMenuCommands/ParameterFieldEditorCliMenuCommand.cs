using CliMenu;
using CliParameters.FieldEditors;

// ReSharper disable ConvertToPrimaryConstructor

namespace CliParameters.CliMenuCommands;

public sealed class ParameterFieldEditorCliMenuCommand : CliMenuCommand
{
    private readonly FieldEditor _fieldEditor;
    private readonly ParametersEditor _parametersEditor;

    public ParameterFieldEditorCliMenuCommand(string fieldName, FieldEditor fieldEditor,
        ParametersEditor parametersEditor)
        : base(fieldName, null, false, EStatusView.Table)
    {
        _fieldEditor = fieldEditor;
        _parametersEditor = parametersEditor;
    }

    protected override void RunAction()
    {
        MenuAction = EMenuAction.Reload;

        _fieldEditor.UpdateField(null, _parametersEditor.Parameters);

        ////პარამეტრების შენახვა (ცვლილებების გათვალისწინებით)
        _parametersEditor.Save(ParametersEditor.GetSaveMessage());
    }

    protected override string GetStatus()
    {
        return _fieldEditor.GetValueStatus(_parametersEditor.Parameters);
    }
}