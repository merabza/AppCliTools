using System;
using System.Collections.Generic;
using AppCliTools.CliParametersApiClientsEdit.Models;
using ParametersManagement.LibApiClientParameters;
using ParametersManagement.LibParameters;
using SystemTools.SystemToolsShared;

namespace AppCliTools.CliParametersApiClientsEdit.Parameters;

public /*open*/ class ApiToolCommandParameters : IParameters
{
    // ReSharper disable once ConvertToPrimaryConstructor
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
        //var parameters = (ProcessorWorkerConsoleParameters)parametersManager.Parameters;
        //TaskModel? task = parameters.GetTask(taskName);
        //if (task == null)
        //{
        //    StShared.WriteErrorLine($"Task {taskName} does not found", true);
        //    return null;
        //}

        //string? apiName = task.ApiName;
        if (string.IsNullOrWhiteSpace(apiName))
        {
            StShared.WriteErrorLine("apiName does not specified for Task", true);
            return null;
        }

        ApiClientSettingsDomain api = GetApiClientRequired(parameters, apiName);
        //string? folderForLocalStorage = parameters.FolderForLocalStorage;

        //if (!string.IsNullOrWhiteSpace(folderForLocalStorage))
        //{
        return new ApiToolCommandParameters(api);
        //}

        //StShared.WriteErrorLine("FolderForLocalStorage does not specified in parameters", true);
        //return null;
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
