using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using CliMenu;
using CliParameters.FieldEditors;
using DatabasesManagement;
using LibApiClientParameters;
using LibDataInput;
using LibMenuInput;
using LibParameters;
using Microsoft.Extensions.Logging;
using SystemToolsShared.Errors;

// ReSharper disable ConvertToPrimaryConstructor

namespace CliParametersDataEdit.FieldEditors;

public sealed class RemoteDbConnectionNameFieldEditor : FieldEditor<string>
{
    private readonly string _databaseApiClientNameFieldName;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger _logger;
    private readonly IParametersManager _parametersManager;

    public RemoteDbConnectionNameFieldEditor(ILogger logger, IHttpClientFactory httpClientFactory, string propertyName,
        IParametersManager parametersManager, string databaseApiClientNameFieldName) : base(propertyName)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _parametersManager = parametersManager;
        _databaseApiClientNameFieldName = databaseApiClientNameFieldName;
    }

    public override void UpdateField(string? recordKey, object recordForUpdate)
    {
        var currentRemoteDbConnectionName = GetValue(recordForUpdate);
        var databaseApiClientName = GetValue<string>(recordForUpdate, _databaseApiClientNameFieldName);
        var acParameters = (IParametersWithApiClients)_parametersManager.Parameters;
        var apiClients = new ApiClients(acParameters.ApiClients);

        var databaseManager = DatabaseManagersFabric.CreateRemoteDatabaseManager(_logger, _httpClientFactory, true,
            databaseApiClientName, apiClients, null, null, CancellationToken.None).Preserve().GetAwaiter().GetResult();

        var databaseConnectionNames = new List<string>();
        if (databaseManager is not null)
        {
            var apiClient = ((RemoteDatabaseManager)databaseManager).ApiClient;
            var getDatabaseFoldersSetsResult = apiClient.GetDatabaseConnectionNames(CancellationToken.None).Result;
            if (getDatabaseFoldersSetsResult.IsT0)
                databaseConnectionNames = getDatabaseFoldersSetsResult.AsT0;
            else
                Err.PrintErrorsOnConsole(getDatabaseFoldersSetsResult.AsT1);
        }

        CliMenuSet databasesMenuSet = new();

        foreach (var listItem in databaseConnectionNames)
            databasesMenuSet.AddMenuItem(new CliMenuCommand(listItem));

        var selectedKey = MenuInputer.InputFromMenuList(FieldName, databasesMenuSet, currentRemoteDbConnectionName);

        if (selectedKey is null)
            throw new DataInputException("Selected invalid Item. ");

        SetValue(recordForUpdate, selectedKey);
    }

    public override string GetValueStatus(object? record)
    {
        return GetValue(record) ?? string.Empty;
    }
}