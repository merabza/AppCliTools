using System;
using CliMenu;
using LibDataInput;
using SystemToolsShared;

namespace CliParameters.MenuCommands;

public sealed class RecordNameEditorMenuCommand : CliMenuCommand
{
    private readonly Cruder _cruder;
    private readonly string _recordName;

    public RecordNameEditorMenuCommand(string fieldName, Cruder cruder, string recordName) : base(fieldName, null,
        false, EStatusView.Table)
    {
        _cruder = cruder;
        _recordName = recordName;
    }

    protected override void RunAction()
    {
        try
        {
            MenuAction = EMenuAction.LevelUp;

            ////ახალი ჩანაწერის სახელის შეტანა პროგრამაში
            //TextDataInput nameInput = new TextDataInput($"New {_cruder.CrudName} Name for {_recordName}");
            //if (!nameInput.DoInput())
            //    return false;
            //string newRecordName = nameInput.Text;

            var newRecordName =
                Inputer.InputTextRequired($"New {_cruder.CrudName} Name for {_recordName}", _recordName);

            if (!_cruder.ChangeRecordKey(_recordName, newRecordName))
                return;

            //პარამეტრების შენახვა (ცვლილებების გათვალისწინებით)
            _cruder.Save($"{_cruder.CrudName} {_recordName} Updated {Name}");

            //ცვლილებების შენახვა დასრულდა
            //StShared.WriteSuccessMessage($"{_cruder.CrudName} {_recordName} Updated {Name}");

            //მენიუს შესახებ სტატუსის დაფიქსირება
            //ცვლილებების გამო მენიუს თავიდან ჩატვირთვა და აწყობა
            //რადგან მენიუ თავიდან აეწყობა, საჭიროა მიეთითოს რომელ პროექტში ვიყავით, რომ ისევ იქ დავბრუნდეთ
            //MenuState = new MenuState { RebuildMenu = true, NextMenu = new List<string> { ParentMenuName } };

            //პაუზა იმისათვის, რომ პროცესის მიმდინარეობის შესახებ წაკითხვა მოვასწროთ და მივხვდეთ, რომ პროცესი დასრულდა
            //StShared.Pause();
        }
        catch (DataInputEscapeException)
        {
            Console.WriteLine();
            Console.WriteLine("Escape... ");
            StShared.Pause();
        }
        catch (Exception e)
        {
            StShared.WriteException(e, true);
        }
    }

    protected override string GetStatus()
    {
        return _recordName;
    }
}