using AppCliTools.CliMenu;
using AppCliTools.CliParameters.FieldEditors;
using ParametersManagement.LibParameters;

namespace AppCliTools.CliParameters.CliMenuCommands;

public sealed class SubObjectFieldEditorCliMenuCommand : CliMenuCommand
{
    private readonly FieldEditor _fieldEditor;

    private readonly ItemData _recordForUpdate;

    // ReSharper disable once ConvertToPrimaryConstructor
    public SubObjectFieldEditorCliMenuCommand(string fieldName, FieldEditor fieldEditor, ItemData recordForUpdate) :
        base(fieldName, EMenuAction.LoadSubMenu, EMenuAction.LoadSubMenu, null, false, EStatusView.Table)
    {
        _fieldEditor = fieldEditor;
        _recordForUpdate = recordForUpdate;
    }

    public override CliMenuSet? GetSubMenu()
    {
        return _fieldEditor.GetSubMenu(_recordForUpdate);
    }

    protected override string GetStatus()
    {
        return _fieldEditor.GetValueStatus(_recordForUpdate);
    }
}
