using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AppCliTools.CliMenu;
using AppCliTools.CliParameters.FieldEditors;
using AppCliTools.LibDataInput;
using AppCliTools.LibMenuInput;
using Microsoft.Extensions.Logging;
using OneOf;
using ParametersManagement.LibApiClientParameters;
using ParametersManagement.LibParameters;
using SystemTools.SystemToolsShared.Errors;
using ToolsManagement.DatabasesManagement;
using WebAgentContracts.WebAgentDatabasesApiContracts;

// ReSharper disable ConvertToPrimaryConstructor

namespace AppCliTools.CliParametersDataEdit.FieldEditors;

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

    public override async ValueTask UpdateField(string? recordKey, object recordForUpdate,
        CancellationToken cancellationToken = default)
    {
        try
        {
            string? currentRemoteDbConnectionName = GetValue(recordForUpdate);
            string? databaseApiClientName = GetValue<string>(recordForUpdate, _databaseApiClientNameFieldName);
            var acParameters = (IParametersWithApiClients)_parametersManager.Parameters;
            var apiClients = new ApiClients(acParameters.ApiClients);

            OneOf<IDatabaseManager, Error[]> createDatabaseManagerResult =
                await DatabaseManagersFactory.CreateRemoteDatabaseManager(_logger, _httpClientFactory, true,
                    databaseApiClientName, apiClients, null, null, cancellationToken);

            var databaseConnectionNames = new List<string>();
            if (createDatabaseManagerResult.IsT1)
            {
                Error.PrintErrorsOnConsole(createDatabaseManagerResult.AsT1);
            }
            else
            {
                DatabaseApiClient apiClient = ((RemoteDatabaseManager)createDatabaseManagerResult.AsT0).ApiClient;
                OneOf<List<string>, Error[]> getDatabaseFoldersSetsResult =
                    await apiClient.GetDatabaseConnectionNames(cancellationToken);
                if (getDatabaseFoldersSetsResult.IsT0)
                {
                    databaseConnectionNames = getDatabaseFoldersSetsResult.AsT0;
                }
                else
                {
                    Error.PrintErrorsOnConsole(getDatabaseFoldersSetsResult.AsT1);
                }
            }

            var databasesMenuSet = new CliMenuSet();

            foreach (string listItem in databaseConnectionNames)
            {
                databasesMenuSet.AddMenuItem(new CliMenuCommand(listItem));
            }

            string selectedKey =
                MenuInputer.InputFromMenuList(FieldName, databasesMenuSet, currentRemoteDbConnectionName) ??
                throw new DataInputException("Selected invalid Item. ");

            SetValue(recordForUpdate, selectedKey);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Operation was canceled.");
        }
        catch (Exception e)
        {
            string contextualMessage =
                $"Error in RemoteDbConnectionNameFieldEditor.UpdateField for recordKey: {recordKey}, property: {FieldName}";
            _logger.LogError(e,
                "Error in RemoteDbConnectionNameFieldEditor.UpdateField for recordKey: {RecordKey}, property: {FieldName}",
                recordKey, FieldName);
            throw new Exception(contextualMessage, e);
        }
    }

    public override string GetValueStatus(object? record)
    {
        return GetValue(record) ?? string.Empty;
    }
}
