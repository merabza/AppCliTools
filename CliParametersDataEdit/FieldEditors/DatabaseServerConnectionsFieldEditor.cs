using System.Collections.Generic;
using System.Linq;
using CliMenu;
using CliParameters.FieldEditors;
using CliParametersDataEdit.Cruders;
using LibDatabaseParameters;
using LibParameters;
using Microsoft.Extensions.Logging;

namespace CliParametersDataEdit.FieldEditors;

public sealed class DatabaseServerConnectionsFieldEditor : FieldEditor<Dictionary<string, DatabaseServerConnectionData>>
{
    private readonly ILogger _logger;
    private readonly IParametersManager _parametersManager;

    // ReSharper disable once ConvertToPrimaryConstructor
    public DatabaseServerConnectionsFieldEditor(string propertyName, IParametersManager parametersManager,
        ILogger logger, bool enterFieldDataOnCreate = false) : base(propertyName, enterFieldDataOnCreate, null, true)
    {
        _parametersManager = parametersManager;
        _logger = logger;
    }

    public override CliMenuSet GetSubMenu(object record)
    {
        DatabaseServerConnectionCruder databaseServerConnectionsCruder = new(_parametersManager, _logger);
        var menuSet = databaseServerConnectionsCruder.GetListMenu();

        return menuSet;
    }


    public override string GetValueStatus(object? record)
    {
        var val = GetValue(record);

        if (val == null || val.Count <= 0)
            return "No Details";

        if (val.Count > 1)
            return $"{val.Count} Details";

        var kvp = val.Single();
        return $"{kvp.Key} - {kvp.Value.DataProvider} {kvp.Value.ServerAddress}";
    }
}