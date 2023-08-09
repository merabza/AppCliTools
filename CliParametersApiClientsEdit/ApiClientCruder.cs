using System;
using System.Collections.Generic;
using System.Linq;
using CliParameters;
using CliParameters.FieldEditors;
using Installer.AgentClients;
using LibApiClientParameters;
using LibParameters;
using Microsoft.Extensions.Logging;

namespace CliParametersApiClientsEdit;

public sealed class ApiClientCruder : ParCruder
{
    private readonly IApiClientsFabric _apiClientsFabric;
    private readonly ILogger _logger;

    public ApiClientCruder(IParametersManager parametersManager, ILogger logger, IApiClientsFabric apiClientsFabric)
        : base(parametersManager, "Api Client", "Api Clients")
    {
        _logger = logger;
        _apiClientsFabric = apiClientsFabric;
        FieldEditors.Add(new TextFieldEditor(nameof(ApiClientSettings.Server)));
        FieldEditors.Add(new TextFieldEditor(nameof(ApiClientSettings.ApiKey)));
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

            //კლიენტის შექმნა ვერსიის შესამოწმებლად
            var apiClient =
                _apiClientsFabric.CreateApiClient(_logger, apiClientSettings.Server, apiClientSettings.ApiKey);

            return apiClient.CheckValidation().Result;
        }
        catch (Exception e)
        {
            _logger.LogError(e, null);
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