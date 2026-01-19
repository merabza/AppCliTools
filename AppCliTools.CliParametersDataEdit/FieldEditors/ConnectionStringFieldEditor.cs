//using System;
//using CliMenu;
//using CliParameters.FieldEditors;
//using CliParametersDataEdit.ParametersEditors;
//using CliParametersDataEdit.ParametersManagers;
//using LibDatabaseParameters;
//using LibParameters;
//using Microsoft.Extensions.Logging;

//namespace CliParametersDataEdit.FieldEditors;

//public sealed class ConnectionStringFieldEditor : FieldEditor<string>
//{
//    private readonly string _dataProviderParameterName;
//    private readonly ILogger _logger;

//    private readonly IParametersManager _parametersManager;

//    private ConnectionStringParametersManager? _memoryParametersManager;

//    // ReSharper disable once ConvertToPrimaryConstructor
//    public ConnectionStringFieldEditor(ILogger logger, string connectionStringParameterName,
//        string dataProviderParameterName, IParametersManager parametersManager) : base(connectionStringParameterName,
//        true, null, true)
//    {
//        _logger = logger;
//        _dataProviderParameterName = dataProviderParameterName;
//        _parametersManager = parametersManager;
//    }

//    public override string GetValueStatus(object? record)
//    {
//        if (record is null)
//            return "(Nothing)";
//        var val = GetValue(record);
//        var dataProvider = GetValue<EDatabaseProvider>(record, _dataProviderParameterName);
//        var dbConnectionParameters = DbConnectionFactory.GetDbConnectionParameters(dataProvider, val);
//        if (val == null)
//            return "(empty)";
//        return dbConnectionParameters == null ? "(invalid)" : dbConnectionParameters.GetStatus();
//    }

//    public override CliMenuSet? GetSubMenu(object record)
//    {
//        var dataProvider = GetValue<EDatabaseProvider>(record, _dataProviderParameterName);
//        if (_memoryParametersManager == null)
//        {
//            var val = GetValue(record);
//            var currentDbConnectionParameters = DbConnectionFactory.GetDbConnectionParameters(dataProvider, val) ??
//                                                new DbConnectionParameters();

//            _memoryParametersManager =
//                new ConnectionStringParametersManager(currentDbConnectionParameters, _parametersManager, this, record);
//        }

//        switch (dataProvider)
//        {
//            case EDatabaseProvider.None:
//                return null;
//            case EDatabaseProvider.SqlServer:
//                SqlServerDatabaseConnectionParametersEditor sqlDatabaseConnectionParametersEditor =
//                    new(_logger, _memoryParametersManager);
//                return sqlDatabaseConnectionParametersEditor.GetParametersMainMenu();
//            case EDatabaseProvider.SqLite:
//                SqLiteDatabaseConnectionParametersEditor sqLiteDatabaseConnectionParametersEditor =
//                    new(_memoryParametersManager);
//                return sqLiteDatabaseConnectionParametersEditor.GetParametersMainMenu();
//            case EDatabaseProvider.OleDb:
//                OleDbConnectionParametersEditor msAccessDatabaseConnectionParametersEditor =
//                    new(_memoryParametersManager);
//                return msAccessDatabaseConnectionParametersEditor.GetParametersMainMenu();
//            default:
//                throw new ArgumentOutOfRangeException();
//        }
//    }
//}


