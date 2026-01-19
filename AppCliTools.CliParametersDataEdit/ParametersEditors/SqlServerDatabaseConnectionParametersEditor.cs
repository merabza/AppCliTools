using AppCliTools.CliParameters;
using AppCliTools.CliParameters.FieldEditors;
using AppCliTools.CliParametersDataEdit.FieldEditors;
using AppCliTools.CliParametersDataEdit.Models;
using Microsoft.Extensions.Logging;
using ParametersManagement.LibParameters;
using SystemTools.SystemToolsShared;

namespace AppCliTools.CliParametersDataEdit.ParametersEditors;

public sealed class SqlServerDatabaseConnectionParametersEditor : ParametersEditor
{
    public SqlServerDatabaseConnectionParametersEditor(ILogger logger, IParametersManager parametersManager) : base(
        nameof(SqlServerDatabaseConnectionParametersEditor).SplitWithSpacesCamelParts(), parametersManager)
    {
        FieldEditors.Add(new TextFieldEditor(nameof(SqlServerConnectionParameters.ServerAddress)));
        FieldEditors.Add(new BoolFieldEditor(nameof(SqlServerConnectionParameters.WindowsNtIntegratedSecurity)));
        FieldEditors.Add(new TextFieldEditor(nameof(SqlServerConnectionParameters.ServerUser)));
        FieldEditors.Add(new TextFieldEditor(nameof(SqlServerConnectionParameters.ServerPass), null, false,
            PasswordChar));
        FieldEditors.Add(new SqlServerDatabaseNameFieldEditor(logger,
            nameof(SqlServerConnectionParameters.DatabaseName), nameof(SqlServerConnectionParameters.ServerAddress),
            nameof(SqlServerConnectionParameters.WindowsNtIntegratedSecurity),
            nameof(SqlServerConnectionParameters.ServerUser), nameof(SqlServerConnectionParameters.ServerPass)));
        FieldEditors.Add(new IntFieldEditor(nameof(SqlServerConnectionParameters.ConnectionTimeOut)));
        FieldEditors.Add(new BoolFieldEditor(nameof(SqlServerConnectionParameters.Encrypt)));
        FieldEditors.Add(new BoolFieldEditor(nameof(SqlServerConnectionParameters.TrustServerCertificate)));
    }
}
