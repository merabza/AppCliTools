using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using ApiClientsManagement;
using CliParameters;
using CliParameters.FieldEditors;
using LibApiClientParameters;
using LibParameters;
using Microsoft.Extensions.Logging;
using SystemToolsShared;
using TestApiContracts;

namespace CliParametersApiClientsEdit;

public sealed class ApiClientCruder : ParCruder
{
    private readonly ILogger _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public ApiClientCruder(IParametersManager parametersManager, ILogger logger, IHttpClientFactory httpClientFactory) :
        base(parametersManager, "Api Client", "Api Clients")
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        FieldEditors.Add(new TextFieldEditor(nameof(ApiClientSettings.Server)));
        FieldEditors.Add(new TextFieldEditor(nameof(ApiClientSettings.ApiKey)));
        FieldEditors.Add(new BoolFieldEditor(nameof(ApiClientSettings.WithMessaging), false));
    }

    protected override Dictionary<string, ItemData> GetCrudersDictionary()
    {
        var parameters = (IParametersWithApiClients)ParametersManager.Parameters;
        return parameters.ApiClients.ToDictionary(p => p.Key, p => (ItemData)p.Value);
    }

    public override bool ContainsRecordWithKey(string recordKey)
    {
        var parameters = (IParametersWithApiClients)ParametersManager.Parameters;
        var apiClients = parameters.ApiClients;
        return apiClients.ContainsKey(recordKey);
    }

    public override void UpdateRecordWithKey(string recordKey, ItemData newRecord)
    {
        var newApiClient = (ApiClientSettings)newRecord;
        var parameters = (IParametersWithApiClients)ParametersManager.Parameters;
        parameters.ApiClients[recordKey] = newApiClient;
    }

    protected override void AddRecordWithKey(string recordKey, ItemData newRecord)
    {
        var newApiClient = (ApiClientSettings)newRecord;
        var parameters = (IParametersWithApiClients)ParametersManager.Parameters;
        parameters.ApiClients.Add(recordKey, newApiClient);
    }

    protected override void RemoveRecordWithKey(string recordKey)
    {
        var parameters = (IParametersWithApiClients)ParametersManager.Parameters;
        var apiClients = parameters.ApiClients;
        apiClients.Remove(recordKey);
    }

    public override bool CheckValidation(ItemData item)
    {
        try
        {
            if (item is not ApiClientSettings apiClientSettings)
                return false;

            if (string.IsNullOrWhiteSpace(apiClientSettings.Server))
                return false;

            Console.WriteLine("Try connect to Test Api Client...");

            //კლიენტის შექმნა ვერსიის შესამოწმებლად
            var apiClient = new TestApiClient(_logger, _httpClientFactory, apiClientSettings.Server);

            var getVersionResult = apiClient.GetVersion(CancellationToken.None).Result;

            if (getVersionResult.IsT1)
            {
                Err.PrintErrorsOnConsole(getVersionResult.AsT1);
                return false;
            }

            var version = getVersionResult.AsT0;


            if (string.IsNullOrWhiteSpace(version))
                return false;

            Console.WriteLine($"Connected successfully, Test Api Client version is {version}");

            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error in method CheckValidation");
            return false;
        }
    }

    public override string? GetStatusFor(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;
        var apiClient = (ApiClientSettings?)GetItemByName(name);
        return apiClient?.Server;
    }

    protected override ItemData CreateNewItem(ItemData? defaultItemData)
    {
        return new ApiClientSettings();
    }
}