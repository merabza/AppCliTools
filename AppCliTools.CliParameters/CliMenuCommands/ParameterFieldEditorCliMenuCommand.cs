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

    protected override async ValueTask<bool> RunBody(CancellationToken cancellationToken = default)
    {
        await _fieldEditor.UpdateField(null, _parametersEditor.Parameters, cancellationToken);

        ////პარამეტრების შენახვა (ცვლილებების გათვალისწინებით)
        await _parametersEditor.Save(ParametersEditor.SaveMessage, null, cancellationToken);
        return true;
    }

    protected override string GetStatus()
    {
        return _fieldEditor.GetValueStatus(_parametersEditor.Parameters);
    }
}
