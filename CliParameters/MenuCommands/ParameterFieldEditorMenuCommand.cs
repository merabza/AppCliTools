using System;
using CliMenu;
using CliParameters.FieldEditors;
using LibDataInput;
using SystemToolsShared;

namespace CliParameters.MenuCommands;

public sealed class ParameterFieldEditorMenuCommand : CliMenuCommand
{
    private readonly FieldEditor _fieldEditor;
    private readonly ParametersEditor _parametersEditor;

    public ParameterFieldEditorMenuCommand(string fieldName, FieldEditor fieldEditor, ParametersEditor parametersEditor)
        : base(fieldName, null, false, EStatusView.Table)
    {
        _fieldEditor = fieldEditor;
        _parametersEditor = parametersEditor;
    }

    protected override void RunAction()
    {
        try
        {
            MenuAction = EMenuAction.Reload;

            _fieldEditor.UpdateField(null, _parametersEditor.Parameters);

            ////პარამეტრების შენახვა (ცვლილებების გათვალისწინებით)
            _parametersEditor.Save(_parametersEditor.GetSaveMessage());
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
        return _fieldEditor.GetValueStatus(_parametersEditor.Parameters);
    }
}