using System;
using CliParameters.FieldEditors;
using LibParameters;

namespace CliParametersDataEdit;

public sealed class ConnectionStringParametersManager : IParametersManager
{
    private readonly FieldEditor<string> _fieldEditor;
    private readonly IParametersManager _parametersManager;

    public ConnectionStringParametersManager(IParameters parameters, IParametersManager parametersManager,
        FieldEditor<string> fieldEditor, object record)
    {
        _parametersManager = parametersManager;
        _fieldEditor = fieldEditor;
        Parameters = parameters;
        Record = record;
    }

    public object Record { get; }
    public IParameters Parameters { get; set; }

    //public string? ParametersFileName
    //{
    //    get => throw new NotImplementedException();
    //    set => throw new NotImplementedException();
    //}

    public void Save(IParameters parameters, string message, bool pauseAfterMessage = true,
        string? saveAsFilePath = null)
    {
        Parameters = parameters;
        if (parameters is not DbConnectionParameters dbp)
            return;
        _fieldEditor.SetValue(Record, DbConnectionFabric.GetDbConnectionString(dbp));
        if (_parametersManager.Parameters is null)
            throw new Exception(
                "_parametersManager.Parameters is null in ConnectionStringParametersManager.Save");
        _parametersManager.Save(_parametersManager.Parameters, message, true, saveAsFilePath);
    }
}