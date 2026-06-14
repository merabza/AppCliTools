//using CliParameters.FieldEditors;
//using CliParametersDataEdit.ParametersEditors;
//using LibDatabaseParameters;
//using LibParameters;
//using Microsoft.Extensions.Logging;

//namespace CliParametersDataEdit.FieldEditors;

//public sealed class
//    DatabaseBackupParametersFieldEditor : ParametersFieldEditor<DatabaseBackupParametersModel,
//    DatabaseBackupParametersEditor>
//{
//    
//    public DatabaseBackupParametersFieldEditor(ILogger logger, string propertyName,
//        IParametersManager parametersManager) : base(logger, propertyName, parametersManager)
//    {
//    }

//    protected override DatabaseBackupParametersEditor CreateEditor(object record,
//        DatabaseBackupParametersModel currentValue)
//    {
//        return new DatabaseBackupParametersEditor(currentValue, ParametersManager);
//    }
//}


