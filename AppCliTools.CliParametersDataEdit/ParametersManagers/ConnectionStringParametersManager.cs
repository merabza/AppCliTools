//using System;
//using CliParameters.FieldEditors;
//using LibParameters;

//namespace CliParametersDataEdit.ParametersManagers;

//public sealed class ConnectionStringParametersManager : IParametersManager
//{
//    private readonly FieldEditor<string> _fieldEditor;
//    private readonly IParametersManager _parametersManager;

//    // ReSharper disable once ConvertToPrimaryConstructor
//    public ConnectionStringParametersManager(IParameters parameters, IParametersManager parametersManager,
//        FieldEditor<string> fieldEditor, object record)
//    {
//        _parametersManager = parametersManager;
//        _fieldEditor = fieldEditor;
//        Parameters = parameters;
//        Record = record;
//    }

//    private object Record { get; }
//    public IParameters Parameters { get; set; }

//    public void Save(IParameters parameters, string message, string? saveAsFilePath = null)
//    {
//        Parameters = parameters;
//        if (parameters is not DbConnectionParameters dbp)
//            return;
//        _fieldEditor.SetValue(Record, DbConnectionFactory.GetDbConnectionString(dbp));
//        if (_parametersManager.Parameters is null)
//            throw new Exception(
//                "_parametersManager.Parameters is null in ConnectionStringParametersManager.Save");
//        _parametersManager.Save(_parametersManager.Parameters, message, saveAsFilePath);
//    }
//}

