using CliMenu;
using CliParameters.FieldEditors;
using CliParametersDataEdit.Models;
using LibParameters;
using Microsoft.Extensions.Logging;

namespace CliParametersDataEdit.FieldEditors;

public sealed class DatabaseConnectionParametersFieldEditor : FieldEditor<DatabaseConnectionParameters>
{
    private readonly ILogger _logger;

    private readonly ParametersManager _parametersManager;

    public DatabaseConnectionParametersFieldEditor(ILogger logger, string devDatabaseConnectionParametersParameterName,
        ParametersManager parametersManager, bool enterFieldDataOnCreate = false) : base(
        devDatabaseConnectionParametersParameterName, enterFieldDataOnCreate, null, true)
    {
        _logger = logger;
        _parametersManager = parametersManager;
    }

    public override string GetValueStatus(object? record)
    {
        var val = GetValue(record);
        if (val is null)
            return "(empty)";
        var dataProvider = val.DataProvider;
        var status = $"Data Provider: {dataProvider}";
        var dbConnectionParameters =
            DbConnectionFabric.GetDbConnectionParameters(dataProvider, val.ConnectionString);
        status +=
            $", Connection: {(dbConnectionParameters == null ? "(invalid)" : dbConnectionParameters.GetStatus())}";
        status += $", CommandTimeOut: {val.CommandTimeOut}";
        return status;
    }

    public override CliMenuSet GetSubMenu(object record)
    {
        var databaseConnectionParameters =
            GetValue(record) ?? new DatabaseConnectionParameters();

        DatabaseConnectionParametersManager memoryParametersManager =
            new(databaseConnectionParameters, _parametersManager, this, record);

        DatabaseConnectionParametersEditor databaseConnectionParametersEditor = new(_logger, memoryParametersManager);
        return databaseConnectionParametersEditor.GetParametersMainMenu();
    }
}