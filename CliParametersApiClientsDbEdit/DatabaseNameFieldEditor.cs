using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using CliMenu;
using CliParameters.CliMenuCommands;
using CliParameters.FieldEditors;
using CliParametersApiClientsEdit;
using CliParametersDataEdit.Cruders;
using DatabasesManagement;
using DbTools.Models;
using LibApiClientParameters;
using LibDatabaseParameters;
using LibDataInput;
using LibMenuInput;
using LibParameters;
using Microsoft.Extensions.Logging;
using SystemToolsShared.Errors;

// ReSharper disable ConvertToPrimaryConstructor

namespace CliParametersApiClientsDbEdit;

public sealed class DatabaseNameFieldEditor : FieldEditor<string>
{
    private readonly bool _canUseNewDatabaseName;
    private readonly string _databaseApiClientNameFieldName;
    private readonly string _databaseConnectionNamePropertyName;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger _logger;
    private readonly IParametersManager _parametersManager;

    public DatabaseNameFieldEditor(ILogger logger, IHttpClientFactory httpClientFactory, string propertyName,
        IParametersManager parametersManager, string databaseConnectionNamePropertyName,
        string databaseApiClientNameFieldName, bool canUseNewDatabaseName) : base(propertyName)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _parametersManager = parametersManager;
        _databaseConnectionNamePropertyName = databaseConnectionNamePropertyName;
        _databaseApiClientNameFieldName = databaseApiClientNameFieldName;
        _canUseNewDatabaseName = canUseNewDatabaseName;
    }

    public override void UpdateField(string? recordKey, object recordForUpdate) //, object currentRecord
    {
        var currentDatabaseName = GetValue(recordForUpdate);

        var databaseServerConnectionName = GetValue<string>(recordForUpdate, _databaseConnectionNamePropertyName);
        var databaseApiClientName = GetValue<string>(recordForUpdate, _databaseApiClientNameFieldName);
        //var databaseServerName = GetValue<string>(recordForUpdate, _databaseServerNameFieldName);

        DatabaseServerConnectionCruder databaseServerConnectionCruder = new(_parametersManager, _logger);

        var databaseServerConnectionData = string.IsNullOrWhiteSpace(databaseServerConnectionName)
            ? null
            : (DatabaseServerConnectionData?)databaseServerConnectionCruder.GetItemByName(databaseServerConnectionName);

        ApiClientCruder apiClientCruder = new(_parametersManager, _logger, _httpClientFactory);

        var apiClientSettings = string.IsNullOrWhiteSpace(databaseApiClientName)
            ? null
            : (ApiClientSettings?)apiClientCruder.GetItemByName(databaseApiClientName);

        IDatabaseManager? databaseManager = null;
        if (databaseServerConnectionData != null)
            databaseManager = DatabaseAgentClientsFabric.CreateDatabaseManager(true, _logger,
                databaseServerConnectionData, null, null, CancellationToken.None).GetAwaiter().GetResult();


        if (databaseManager == null && apiClientSettings != null)
            databaseManager = DatabaseAgentClientsFabric.CreateDatabaseManager(_logger, _httpClientFactory,
                apiClientSettings, null, null, true, CancellationToken.None).GetAwaiter().GetResult();

        var databaseInfos = new List<DatabaseInfoModel>();
        if (databaseManager is not null)
        {
            var getDatabaseNamesResult = databaseManager.GetDatabaseNames(CancellationToken.None).Result;
            if (getDatabaseNamesResult.IsT0)
                databaseInfos = getDatabaseNamesResult.AsT0;
            else
                Err.PrintErrorsOnConsole(getDatabaseNamesResult.AsT1);
            //+
            //databaseClient.Dispose();
        }

        CliMenuSet databasesMenuSet = new();
        if (_canUseNewDatabaseName)
            databasesMenuSet.AddMenuItem(new MenuCommandWithStatusCliMenuCommand("New Database Name"));

        var keys = databaseInfos.Select(s => s.Name).ToList();
        foreach (var listItem in keys)
            databasesMenuSet.AddMenuItem(new MenuCommandWithStatusCliMenuCommand(listItem));

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
        return GetValue(record) ?? string.Empty;
    }
}