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
    private readonly string _recordName;

    public FieldEditorMenuCommand(string fieldName, FieldEditor fieldEditor, ItemData recordForUpdate,
        Cruder cruder,
        string recordKey) : base(fieldName, null, false, EStatusView.Table)
    {
        _fieldEditor = fieldEditor;
        _recordForUpdate = recordForUpdate;
        _cruder = cruder;
        _recordName = recordKey;
    }

    protected override void RunAction()
    {
        try
        {
            MenuAction = EMenuAction.Reload;

            _fieldEditor.UpdateField(_recordName, _recordForUpdate);
            _cruder.UpdateRecordWithKey(_recordName, _recordForUpdate);

            if (!_cruder.CheckValidation(_recordForUpdate))
                if (!Inputer.InputBool($"{_recordName} is not valid, continue input data?", false, false))
                    return;

            _cruder.FixRecordName(_recordName, _recordForUpdate);
            //პარამეტრების შენახვა (ცვლილებების გათვალისწინებით)
            _cruder.Save($"{_cruder.CrudName} {_recordName} Updated {Name}");
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