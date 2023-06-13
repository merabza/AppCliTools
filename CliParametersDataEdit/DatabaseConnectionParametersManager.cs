using System;
using CliParameters.FieldEditors;
using CliParametersDataEdit.Models;
using LibParameters;

namespace CliParametersDataEdit;

public sealed class DatabaseConnectionParametersManager : IParametersManager
{
    private readonly FieldEditor<DatabaseConnectionParameters> _fieldEditor;
    private readonly IParametersManager _parentParametersManager;

    public DatabaseConnectionParametersManager(IParameters parameters,
        IParametersManager parentParametersManager,
        FieldEditor<DatabaseConnectionParameters> fieldEditor, object record)
    {
        _parentParametersManager = parentParametersManager;
        _fieldEditor = fieldEditor;
        Parameters = parameters;
        Record = record;
    }

    public object Record { get; }

    public IParameters Parameters { get; set; }
    //public string? ParametersFileName { get; set; }

    public void Save(IParameters parameters, string message, bool pauseAfterMessage = true,
        string? saveAsFilePath = null)
    {
        Parameters = parameters;
        if (parameters is not DatabaseConnectionParameters dbp)
            return;
        _fieldEditor.SetValue(Record, dbp);
        if (_parentParametersManager.Parameters is null)
            throw new Exception(
                "_parentParametersManager.Parameters is null in DatabaseConnectionParametersManager.Save");
        _parentParametersManager.Save(_parentParametersManager.Parameters, message, true, saveAsFilePath);
    }
}