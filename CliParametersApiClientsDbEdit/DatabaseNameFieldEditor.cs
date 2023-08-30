using System.Collections.Generic;
using System.Linq;
using CliMenu;
using CliParameters.FieldEditors;
using CliParameters.MenuCommands;
using CliParametersApiClientsEdit;
using CliParametersDataEdit.Cruders;
using DatabaseApiClients;
using DatabaseManagementClients;
using DbTools.Models;
using LibApiClientParameters;
using LibDatabaseParameters;
using LibDataInput;
using LibMenuInput;
using LibParameters;
using Microsoft.Extensions.Logging;

namespace CliParametersApiClientsDbEdit;

public sealed class DatabaseNameFieldEditor : FieldEditor<string>
{
    private readonly bool _canUseNewDatabaseName;
    private readonly string _databaseApiClientNameFieldName;
    private readonly string _databaseConnectionNamePropertyName;
    private readonly ILogger _logger;
    private readonly IParametersManager _parametersManager;

    public DatabaseNameFieldEditor(ILogger logger, string propertyName, IParametersManager parametersManager,
        string databaseConnectionNamePropertyName, string databaseApiClientNameFieldName, bool canUseNewDatabaseName) :
        base(propertyName)
    {
        _logger = logger;
        _parametersManager = parametersManager;
        _databaseConnectionNamePropertyName = databaseConnectionNamePropertyName;
        _databaseApiClientNameFieldName = databaseApiClientNameFieldName;
        _canUseNewDatabaseName = canUseNewDatabaseName;
    }

    public override void UpdateField(string? recordKey, object recordForUpdate) //, object currentRecord
    {
        var currentDatabaseName = GetValue(recordForUpdate);

        var databaseServerConnectionName =
            GetValue<string>(recordForUpdate, _databaseConnectionNamePropertyName);
        var databaseApiClientName = GetValue<string>(recordForUpdate, _databaseApiClientNameFieldName);
        //var databaseServerName = GetValue<string>(recordForUpdate, _databaseServerNameFieldName);

        DatabaseServerConnectionCruder databaseServerConnectionCruder = new(_parametersManager, _logger);

        var databaseServerConnectionData =
            string.IsNullOrWhiteSpace(databaseServerConnectionName)
                ? null
                : (DatabaseServerConnectionData?)databaseServerConnectionCruder.GetItemByName(
                    databaseServerConnectionName);

        ApiClientCruder apiClientCruder = new(_parametersManager, _logger);

        var apiClientSettings = string.IsNullOrWhiteSpace(databaseApiClientName)
            ? null
            : (ApiClientSettings?)apiClientCruder.GetItemByName(databaseApiClientName);

        DatabaseManagementClient? databaseClient = null;
        if (databaseServerConnectionData != null)
            databaseClient =
                DatabaseAgentClientsFabric.CreateDatabaseManagementClient(true, _logger, databaseServerConnectionData,
                    null, null);

        if (databaseClient == null && apiClientSettings != null)
            databaseClient = DatabaseApiClient.Create(_logger, true, apiClientSettings, null, null);

        var databaseInfos = databaseClient is null
            ? new List<DatabaseInfoModel>()
            : databaseClient.GetDatabaseNames().Result;

        CliMenuSet databasesMenuSet = new();
        if (_canUseNewDatabaseName)
            databasesMenuSet.AddMenuItem(new MenuCommandWithStatus(null), "New Database Name");

        var keys = databaseInfos.Select(s => s.Name).ToList();
        foreach (var listItem in keys)
            databasesMenuSet.AddMenuItem(new MenuCommandWithStatus(listItem), listItem);

        var selectedId = MenuInputer.InputIdFromMenuList(FieldName, databasesMenuSet, currentDatabaseName);

        if (_canUseNewDatabaseName && selectedId == 0)
        {
            var newDatabaseName = Inputer.InputTextRequired("New Database Name"); // nameInput.Text;
            SetValue(recordForUpdate, newDatabaseName);
            return;
        }

        var index = selectedId - (_canUseNewDatabaseName ? 1 : 0);
        if (index < 0 || index >= keys.Count)
            throw new DataInputException("Selected invalid ID. ");

        SetValue(recordForUpdate, keys[index]);
    }

    public override string GetValueStatus(object? record)
    {
        return GetValue(record) ?? "";
    }
}