using System.Collections.Generic;
using System.Linq;
using CliMenu;
using CliParameters.FieldEditors;
using Installer.AgentClients;
using LibApiClientParameters;
using LibParameters;
using Microsoft.Extensions.Logging;

namespace CliParametersApiClientsEdit.FieldEditors;

public sealed class ApiClientsFieldEditor : FieldEditor<Dictionary<string, ApiClientSettings>>
{
    private readonly IApiClientsFabric _apiClientsFabric;
    private readonly ILogger _logger;
    private readonly IParametersManager _parametersManager;

    public ApiClientsFieldEditor(ILogger logger, string propertyName, IParametersManager parametersManager,
        IApiClientsFabric apiClientsFabric) : base(propertyName, null, true)
    {
        _parametersManager = parametersManager;
        _apiClientsFabric = apiClientsFabric;
        _logger = logger;
    }

    public override CliMenuSet GetSubMenu(object record)
    {
        ApiClientCruder apiClientCruder = new(_parametersManager, _logger, _apiClientsFabric);
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
        return $"{kvp.Key} - {kvp.Value.Server} {kvp.Value.ApiKey}";
    }
}