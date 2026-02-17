using System.Threading;
using System.Threading.Tasks;
using AppCliTools.CliMenu;
using AppCliTools.CliParameters.FieldEditors;

// ReSharper disable ConvertToPrimaryConstructor

namespace AppCliTools.CliParameters.CliMenuCommands;

public sealed class ParameterFieldEditorCliMenuCommand : CliMenuCommand
{
    private readonly FieldEditor _fieldEditor;
    private readonly ParametersEditor _parametersEditor;

    public ParameterFieldEditorCliMenuCommand(string fieldName, FieldEditor fieldEditor,
        ParametersEditor parametersEditor) : base(fieldName, EMenuAction.Reload, EMenuAction.Reload, null, false,
        EStatusView.Table)
    {
        _fieldEditor = fieldEditor;
        _parametersEditor = parametersEditor;
    }

    protected override ValueTask<bool> RunBody(CancellationToken cancellationToken = default)
    {
        _fieldEditor.UpdateField(null, _parametersEditor.Parameters);

        ////პარამეტრების შენახვა (ცვლილებების გათვალისწინებით)
        _parametersEditor.Save(ParametersEditor.SaveMessage);
        return ValueTask.FromResult(true);
    }

    protected override string GetStatus()
    {
        return _fieldEditor.GetValueStatus(_parametersEditor.Parameters);
    }
}
