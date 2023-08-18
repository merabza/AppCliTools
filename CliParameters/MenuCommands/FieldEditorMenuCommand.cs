using System;
using CliMenu;
using CliParameters.FieldEditors;
using LibDataInput;
using LibParameters;
using SystemToolsShared;

namespace CliParameters.MenuCommands;

public sealed class FieldEditorMenuCommand : CliMenuCommand
{
    private readonly Cruder _cruder;
    private readonly FieldEditor _fieldEditor;
    private readonly ItemData _recordForUpdate;
    private readonly string _recordKey;

    public FieldEditorMenuCommand(string fieldName, FieldEditor fieldEditor, ItemData recordForUpdate,
        Cruder cruder,
        string recordKey) : base(fieldName, null, false, EStatusView.Table)
    {
        _fieldEditor = fieldEditor;
        _recordForUpdate = recordForUpdate;
        _cruder = cruder;
        _recordKey = recordKey;
    }

    protected override void RunAction()
    {
        try
        {
            MenuAction = EMenuAction.Reload;

            _fieldEditor.UpdateField(_recordKey, _recordForUpdate);
            _cruder.UpdateRecordWithKey(_recordKey, _recordForUpdate);

            if (!_cruder.CheckValidation(_recordForUpdate))
                if (!Inputer.InputBool($"{_recordKey} is not valid, continue input data?", false, false))
                    return;

            //_recordKey = _cruder.FixRecordName(_recordKey, _recordForUpdate);
            //პარამეტრების შენახვა (ცვლილებების გათვალისწინებით)
            _cruder.Save($"{_cruder.CrudName} {_recordKey} Updated {Name}");
        }
        catch (DataInputEscapeException e)
        {
            Console.WriteLine();
            Console.WriteLine($"{e.Message}... ");
            StShared.Pause();
        }
        catch (Exception e)
        {
            StShared.WriteException(e, true);
        }
    }

    protected override string GetStatus()
    {
        return _fieldEditor.GetValueStatus(_recordForUpdate);
    }
}