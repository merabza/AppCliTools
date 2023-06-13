using CliParameters;
using CliParameters.FieldEditors;
using CliParametersDataEdit.Models;
using LibParameters;
using Microsoft.Extensions.Logging;
using SystemToolsShared;

namespace CliParametersDataEdit.FieldEditors;

public sealed class SqlServerDatabaseConnectionParametersEditor : ParametersEditor
{
    public SqlServerDatabaseConnectionParametersEditor(ILogger logger, IParametersManager parametersManager) : base(
        nameof(SqlServerDatabaseConnectionParametersEditor).SplitWithSpacesCamelParts(), parametersManager)
    {
        FieldEditors.Add(new TextFieldEditor(nameof(SqlServerConnectionParameters.ServerAddress)));
        FieldEditors.Add(new BoolFieldEditor(nameof(SqlServerConnectionParameters.WindowsNtIntegratedSecurity),
            false));
        FieldEditors.Add(new TextFieldEditor(nameof(SqlServerConnectionParameters.ServerUser)));
        FieldEditors.Add(new TextFieldEditor(nameof(SqlServerConnectionParameters.ServerPass), default, '*'));
        FieldEditors.Add(new SqlServerDatabaseNameFieldEditor(logger,
            nameof(SqlServerConnectionParameters.DatabaseName),
            nameof(SqlServerConnectionParameters.ServerAddress),
            nameof(SqlServerConnectionParameters.WindowsNtIntegratedSecurity),
            nameof(SqlServerConnectionParameters.ServerUser), nameof(SqlServerConnectionParameters.ServerPass)));
        FieldEditors.Add(new IntFieldEditor(nameof(SqlServerConnectionParameters.ConnectionTimeOut)));
        FieldEditors.Add(new BoolFieldEditor(nameof(SqlServerConnectionParameters.Encrypt),
            false));
        FieldEditors.Add(new BoolFieldEditor(nameof(SqlServerConnectionParameters.TrustServerCertificate),
            false));
    }
}