using CliMenu;
using LibDataInput;

namespace CliParameters.CliMenuCommands;

public sealed class RecordKeyEditorCliMenuCommand : CliMenuCommand
{
    private readonly Cruder _cruder;
    private readonly string _recordKey;

    // ReSharper disable once ConvertToPrimaryConstructor
    public RecordKeyEditorCliMenuCommand(string fieldName, Cruder cruder, string recordKey) : base(fieldName, null,
        false, EStatusView.Table)
    {
        _cruder = cruder;
        _recordKey = recordKey;
    }

    protected override void RunAction()
    {
        MenuAction = EMenuAction.LevelUp;

        var newRecordName =
            Inputer.InputTextRequired($"New {_cruder.CrudName} Name for {_recordKey}", _recordKey);

        if (!_cruder.ChangeRecordKey(_recordKey, newRecordName))
            return;

        //პარამეტრების შენახვა (ცვლილებების გათვალისწინებით)
        _cruder.Save($"{_cruder.CrudName} {_recordKey} Updated {Name}");
    }

    protected override string GetStatus()
    {
        return _recordKey;
    }
}