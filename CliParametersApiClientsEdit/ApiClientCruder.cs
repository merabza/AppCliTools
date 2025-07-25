using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using CliParameters;
using CliParameters.FieldEditors;
using LibApiClientParameters;
using LibParameters;
using Microsoft.Extensions.Logging;
using SystemToolsShared.Errors;
using TestApiContracts;

namespace CliParametersApiClientsEdit;

public sealed class ApiClientCruder : ParCruder<ApiClientSettings>
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger _logger;

    private ApiClientCruder(ILogger logger, IHttpClientFactory httpClientFactory, IParametersManager parametersManager,
        Dictionary<string, ApiClientSettings> currentValuesDictionary) : base(parametersManager,
        currentValuesDictionary, "Api Client", "Api Clients")
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        FieldEditors.Add(new TextFieldEditor(nameof(ApiClientSettings.Server)));
        FieldEditors.Add(new TextFieldEditor(nameof(ApiClientSettings.ApiKey)));
    }

    public static ApiClientCruder Create(ILogger logger, IHttpClientFactory httpClientFactory,
        IParametersManager parametersManager)
    {
        var parameters = (IParametersWithApiClients)parametersManager.Parameters;
        return new ApiClientCruder(logger, httpClientFactory, parametersManager, parameters.ApiClients);
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
            var apiClient = new TestApiClient(_logger, _httpClientFactory, apiClientSettings.Server, true);

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
}