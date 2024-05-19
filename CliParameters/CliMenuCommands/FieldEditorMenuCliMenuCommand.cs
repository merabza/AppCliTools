using CliMenu;
using CliParameters.FieldEditors;
using LibDataInput;
using LibParameters;

namespace CliParameters.CliMenuCommands;

public sealed class FieldEditorMenuCliMenuCommand : CliMenuCommand
{
    private readonly Cruder _cruder;
    private readonly FieldEditor _fieldEditor;
    private readonly ItemData _recordForUpdate;
    private readonly string _recordKey;

    // ReSharper disable once ConvertToPrimaryConstructor
    public FieldEditorMenuCliMenuCommand(string fieldName, FieldEditor fieldEditor, ItemData recordForUpdate,
        Cruder cruder,
        string recordKey) : base(fieldName, EMenuAction.Reload, EMenuAction.Reload, null, false, EStatusView.Table)
    {
        _fieldEditor = fieldEditor;
        _recordForUpdate = recordForUpdate;
        _cruder = cruder;
        _recordKey = recordKey;
    }

    protected override bool RunBody()
    {
        _fieldEditor.UpdateField(_recordKey, _recordForUpdate);
        _cruder.UpdateRecordWithKey(_recordKey, _recordForUpdate);

        if (!_cruder.CheckValidation(_recordForUpdate) &&
            !Inputer.InputBool($"{_recordKey} is not valid, continue input data?", false, false))
            return false;

        //_recordKey = _cruder.FixRecordName(_recordKey, _recordForUpdate);
        //პარამეტრების შენახვა (ცვლილებების გათვალისწინებით)
        _cruder.Save($"{_cruder.CrudName} {_recordKey} Updated {Name}");
        return true;
    }

    protected override string GetStatus()
    {
        return _fieldEditor.GetValueStatus(_recordForUpdate);
    }
}