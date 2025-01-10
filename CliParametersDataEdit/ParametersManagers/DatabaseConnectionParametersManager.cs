//using System;
//using CliParameters.FieldEditors;
//using CliParametersDataEdit.Models;
//using LibParameters;

//namespace CliParametersDataEdit.ParametersManagers;

//public sealed class DatabaseConnectionParametersManager : IParametersManager
//{
//    private readonly FieldEditor<DatabaseConnectionParameters> _fieldEditor;
//    private readonly IParametersManager _parentParametersManager;

//    // ReSharper disable once ConvertToPrimaryConstructor
//    public DatabaseConnectionParametersManager(IParameters parameters, IParametersManager parentParametersManager,
//        FieldEditor<DatabaseConnectionParameters> fieldEditor, object record)
//    {
//        _parentParametersManager = parentParametersManager;
//        _fieldEditor = fieldEditor;
//        Parameters = parameters;
//        Record = record;
//    }

//    private object Record { get; }

//    public IParameters Parameters { get; set; }

//    public void Save(IParameters parameters, string message, string? saveAsFilePath = null)
//    {
//        Parameters = parameters;
//        if (parameters is not DatabaseConnectionParameters dbp)
//            return;
//        _fieldEditor.SetValue(Record, dbp);
//        if (_parentParametersManager.Parameters is null)
//            throw new Exception(
//                "_parentParametersManager.Parameters is null in DatabaseConnectionParametersManager.Save");
//        _parentParametersManager.Save(_parentParametersManager.Parameters, message, saveAsFilePath);
//    }
//}