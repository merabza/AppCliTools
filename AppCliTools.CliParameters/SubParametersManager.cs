using CliParameters.FieldEditors;
using ParametersManagement.LibParameters;

namespace CliParameters;

public sealed class SubParametersManager<T> : IParametersManager
{
    private readonly FieldEditor<T> _fieldEditor;
    private readonly IParametersManager _parentParametersManager;
    private readonly object _record;

    // ReSharper disable once ConvertToPrimaryConstructor
    public SubParametersManager(IParameters parameters, IParametersManager parentParametersManager,
        FieldEditor<T> fieldEditor, object record)
    {
        _parentParametersManager = parentParametersManager;
        _fieldEditor = fieldEditor;
        _record = record;
        Parameters = parameters;
    }

    public IParameters Parameters { get; set; }

    public void Save(IParameters parameters, string message, string? saveAsFilePath = null)
    {
        Parameters = parameters;
        if (parameters is not T par)
        {
            return;
        }

        _fieldEditor.SetValue(_record, par);

        _parentParametersManager.Save(_parentParametersManager.Parameters, message, saveAsFilePath);
    }
}
