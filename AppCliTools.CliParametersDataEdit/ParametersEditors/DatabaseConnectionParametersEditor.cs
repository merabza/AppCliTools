//using CliParameters;
//using CliParameters.FieldEditors;
//using CliParametersDataEdit.FieldEditors;
//using CliParametersDataEdit.Models;
//using DbTools;
//using LibParameters;
//using Microsoft.Extensions.Logging;
//using SystemToolsShared;

//namespace CliParametersDataEdit.ParametersEditors;

//public sealed class DatabaseConnectionParametersEditor : ParametersEditor
//{
//    public DatabaseConnectionParametersEditor(ILogger logger, IParametersManager parametersManager) : base(
//        nameof(DatabaseConnectionParametersEditor).SplitWithSpacesCamelParts(), parametersManager)
//    {
//        FieldEditors.Add(
//            new EnumFieldEditor<EDataProvider>(nameof(DatabaseConnectionParameters.DataProvider),
//                EDataProvider.Sql));
//        FieldEditors.Add(new ConnectionStringFieldEditor(logger,
//            nameof(DatabaseConnectionParameters.ConnectionString),
//            nameof(DatabaseConnectionParameters.DataProvider), parametersManager));
//        FieldEditors.Add(new IntFieldEditor(nameof(DatabaseConnectionParameters.CommandTimeOut), 10000));
//    }
//}


