using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using CliMenu;
using CliParameters.CliMenuCommands;
using CliParameters.FieldEditors;
using DatabasesManagement;
using DbTools.Models;
using LibApiClientParameters;
using LibDatabaseParameters;
using LibDataInput;
using LibMenuInput;
using LibParameters;
using Microsoft.Extensions.Logging;
using SystemToolsShared.Errors;

namespace CliParametersDataEdit.FieldEditors;

public sealed class DatabaseNameFieldEditor : FieldEditor<string>
{
    private readonly bool _canUseNewDatabaseName;
    private readonly string _databaseConnectionNamePropertyName;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger _logger;
    private readonly IParametersManager _parametersManager;

    // ReSharper disable once ConvertToPrimaryConstructor
    public DatabaseNameFieldEditor(ILogger logger, IHttpClientFactory httpClientFactory, string propertyName,
        IParametersManager parametersManager, string databaseConnectionNamePropertyName,
        bool canUseNewDatabaseName) : base(propertyName)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _parametersManager = parametersManager;
        _databaseConnectionNamePropertyName = databaseConnectionNamePropertyName;
        _canUseNewDatabaseName = canUseNewDatabaseName;
    }

    public override void UpdateField(string? recordKey, object recordForUpdate)
    {
        var currentDatabaseName = GetValue(recordForUpdate);
        var databaseServerConnectionName = GetValue<string>(recordForUpdate, _databaseConnectionNamePropertyName);
        var dscParameters = (IParametersWithDatabaseServerConnections)_parametersManager.Parameters;
        var databaseServerConnections = new DatabaseServerConnections(dscParameters.DatabaseServerConnections);
        var acParameters = (IParametersWithApiClients)_parametersManager.Parameters;
        var apiClients = new ApiClients(acParameters.ApiClients);
        var databaseInfos = new List<DatabaseInfoModel>();

        var createDatabaseManagerResult = DatabaseManagersFactory.CreateDatabaseManager(_logger, true,
            databaseServerConnectionName, databaseServerConnections, apiClients, _httpClientFactory, null, null,
            CancellationToken.None).Result;

        if (createDatabaseManagerResult.IsT1)
        {
            Err.PrintErrorsOnConsole(createDatabaseManagerResult.AsT1);
            return;
        }

        var getDatabaseNamesResult = createDatabaseManagerResult.AsT0.GetDatabaseNames(CancellationToken.None).Result;
        if (getDatabaseNamesResult.IsT0)
            databaseInfos = getDatabaseNamesResult.AsT0;
        else
            Err.PrintErrorsOnConsole(getDatabaseNamesResult.AsT1);

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

        //SetValue(recordForUpdate, Inputer.InputText(FieldName, currentDatabaseName));
    }

    public override string GetValueStatus(object? record)
    {
        return GetValue(record) ?? string.Empty;
    }
}