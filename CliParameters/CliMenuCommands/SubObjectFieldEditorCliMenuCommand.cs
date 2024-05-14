using CliMenu;
using CliParameters.FieldEditors;
using LibParameters;

namespace CliParameters.CliMenuCommands;

public sealed class SubObjectFieldEditorCliMenuCommand : CliMenuCommand
{
    private readonly FieldEditor _fieldEditor;

    private readonly ItemData _recordForUpdate;

    // ReSharper disable once ConvertToPrimaryConstructor
    public SubObjectFieldEditorCliMenuCommand(string fieldName, FieldEditor fieldEditor, ItemData recordForUpdate) :
        base(
            fieldName, null, false, EStatusView.Table)
    {
        _fieldEditor = fieldEditor;
        _recordForUpdate = recordForUpdate;
    }


    protected override void RunAction()
    {
        MenuAction = EMenuAction.LoadSubMenu;
    }

    public override CliMenuSet? GetSubmenu()
    {
        return _fieldEditor.GetSubMenu(_recordForUpdate);
    }

    protected override string GetStatus()
    {
        return _fieldEditor.GetValueStatus(_recordForUpdate);
    }
}