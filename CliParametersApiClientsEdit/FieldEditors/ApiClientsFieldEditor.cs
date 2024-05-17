using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using CliMenu;
using CliParameters.FieldEditors;
using LibApiClientParameters;
using LibParameters;
using Microsoft.Extensions.Logging;

namespace CliParametersApiClientsEdit.FieldEditors;

public sealed class ApiClientsFieldEditor : FieldEditor<Dictionary<string, ApiClientSettings>>
{
    private readonly ILogger _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IParametersManager _parametersManager;

    // ReSharper disable once ConvertToPrimaryConstructor
    public ApiClientsFieldEditor(ILogger logger, IHttpClientFactory httpClientFactory, string propertyName,
        IParametersManager parametersManager, bool enterFieldDataOnCreate = false) : base(propertyName,
        enterFieldDataOnCreate, null, true)
    {
        _parametersManager = parametersManager;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    public override CliMenuSet GetSubMenu(object record)
    {
        ApiClientCruder apiClientCruder = new(_parametersManager, _logger, _httpClientFactory);
        var menuSet = apiClientCruder.GetListMenu();
        return menuSet;
    }


    public override string GetValueStatus(object? record)
    {
        var val = GetValue(record);

        if (val is null || val.Count <= 0)
            return "No Details";

        if (val.Count > 1)
            return $"{val.Count} Details";

        var kvp = val.Single();
        return $"{kvp.Key} - {kvp.Value.Server} {kvp.Value.ApiKey} {(kvp.Value.WithMessaging ? "WithMessaging" : "")}";
    }
}