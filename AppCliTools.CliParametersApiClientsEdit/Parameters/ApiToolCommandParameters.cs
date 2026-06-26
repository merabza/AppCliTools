using System;
using System.Collections.Generic;
using ParametersManagement.LibApiClientParameters;
using ParametersManagement.LibParameters;
using SystemTools.SystemToolsShared;
using ToolsManagement.ApiClientsManagement;

namespace AppCliTools.CliParametersApiClientsEdit.Parameters;

public /*open*/ class ApiToolCommandParameters : IParameters
{
    protected ApiToolCommandParameters(ApiClientSettingsDomain api)
    {
        Api = api;
    }

    public ApiClientSettingsDomain Api { get; }

    public bool CheckBeforeSave()
    {
        return true;
    }

    public static ApiToolCommandParameters? Create(IParametersWithApiClients parameters, string apiName)
    {
        if (string.IsNullOrWhiteSpace(apiName))
        {
            StShared.WriteErrorLine("apiName does not specified for Task", true);
            return null;
        }

        ApiClientSettingsDomain api = GetApiClientRequired(parameters, apiName);

        return new ApiToolCommandParameters(api);
    }

    protected static ApiClientSettingsDomain GetApiClientRequired(IParametersWithApiClients parameters, string apiKey)
    {
        ApiClientSettings apiClientSettings = GetApiClient(parameters, apiKey) ??
                                              throw new InvalidOperationException(
                                                  $"ApiClient with name {apiKey} does not exists");
        return string.IsNullOrWhiteSpace(apiClientSettings.Server)
            ? throw new InvalidOperationException($"Server does not specified for ApiClient with name {apiKey}")
            : new ApiClientSettingsDomain(apiClientSettings.Server, apiClientSettings.ApiKey);
    }

    private static ApiClientSettings? GetApiClient(IParametersWithApiClients parameters, string apiKey)
    {
        return parameters.ApiClients.GetValueOrDefault(apiKey);
    }
}
