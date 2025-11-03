using System;
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
        try
        {
            var currentRemoteDbConnectionName = GetValue(recordForUpdate);
            var databaseApiClientName = GetValue<string>(recordForUpdate, _databaseApiClientNameFieldName);
            var acParameters = (IParametersWithApiClients)_parametersManager.Parameters;
            var apiClients = new ApiClients(acParameters.ApiClients);

            // ReSharper disable once using
            // ReSharper disable once DisposableConstructor
            using var cts = new CancellationTokenSource();
            var token = cts.Token;
            token.ThrowIfCancellationRequested();
            var createDatabaseManagerResult = DatabaseManagersFactory.CreateRemoteDatabaseManager(_logger,
                _httpClientFactory, true, databaseApiClientName, apiClients, null, null, token).Result;

            var databaseConnectionNames = new List<string>();
            if (createDatabaseManagerResult.IsT1)
            {
                Err.PrintErrorsOnConsole(createDatabaseManagerResult.AsT1);
            }
            else
            {
                var apiClient = ((RemoteDatabaseManager)createDatabaseManagerResult.AsT0).ApiClient;
                var getDatabaseFoldersSetsResult = apiClient.GetDatabaseConnectionNames(token).Result;
                if (getDatabaseFoldersSetsResult.IsT0)
                    databaseConnectionNames = getDatabaseFoldersSetsResult.AsT0;
                else
                    Err.PrintErrorsOnConsole(getDatabaseFoldersSetsResult.AsT1);
            }

            var databasesMenuSet = new CliMenuSet();

            foreach (var listItem in databaseConnectionNames)
                databasesMenuSet.AddMenuItem(new CliMenuCommand(listItem));

            var selectedKey = MenuInputer.InputFromMenuList(FieldName, databasesMenuSet, currentRemoteDbConnectionName);

            if (selectedKey is null)
                throw new DataInputException("Selected invalid Item. ");

            SetValue(recordForUpdate, selectedKey);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Operation was canceled.");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error in RemoteDbConnectionNameFieldEditor.UpdateField");
            throw;
        }
    }

    public override string GetValueStatus(object? record)
    {
        return GetValue(record) ?? string.Empty;
    }
}