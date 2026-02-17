using System.Threading;
using System.Threading.Tasks;
using AppCliTools.CliMenu;
using AppCliTools.CliParameters.Cruders;
using AppCliTools.CliParameters.FieldEditors;
using AppCliTools.LibDataInput;
using SystemTools.SystemToolsShared;

namespace AppCliTools.CliParameters.CliMenuCommands;

public sealed class FieldEditorMenuCliMenuCommand : CliMenuCommand
{
    private readonly Cruder _cruder;
    private readonly FieldEditor _fieldEditor;
    private readonly ItemData _recordForUpdate;
    private readonly string _recordKey;

    // ReSharper disable once ConvertToPrimaryConstructor
    public FieldEditorMenuCliMenuCommand(string fieldName, FieldEditor fieldEditor, ItemData recordForUpdate,
        Cruder cruder, string recordKey) : base(fieldName, EMenuAction.Reload, EMenuAction.Reload, null, false,
        EStatusView.Table)
    {
        _fieldEditor = fieldEditor;
        _recordForUpdate = recordForUpdate;
        _cruder = cruder;
        _recordKey = recordKey;
    }

    protected override ValueTask<bool> RunBody(CancellationToken cancellationToken = default)
    {
        _fieldEditor.UpdateField(_recordKey, _recordForUpdate);
        _cruder.UpdateRecordWithKey(_recordKey, _recordForUpdate);

        if (!_cruder.CheckValidation(_recordForUpdate) &&
            !Inputer.InputBool($"{_recordKey} is not valid, continue input data?", false, false))
        {
            return ValueTask.FromResult(false);
        }

        //_recordKey = _cruder.FixRecordName(_recordKey, _recordForUpdate);
        //პარამეტრების შენახვა (ცვლილებების გათვალისწინებით)
        _cruder.Save($"{_cruder.CrudName} {_recordKey} Updated {Name}");
        return ValueTask.FromResult(true);
    }

    protected override string GetStatus()
    {
        return _fieldEditor.GetValueStatus(_recordForUpdate);
    }
}
