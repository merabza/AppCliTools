﻿using CliParameters.FieldEditors;
using Installer.AgentClients;
using LibParameters;
using Microsoft.Extensions.Logging;

namespace CliParametersApiClientsEdit.FieldEditors;

public sealed class ApiClientNameFieldEditor : FieldEditor<string>
{
    private readonly IApiClientsFabric _apiClientsFabric;
    private readonly ILogger _logger;
    private readonly IParametersManager _parametersManager;
    private readonly bool _useNone;

    public ApiClientNameFieldEditor(ILogger logger, string propertyName, IParametersManager parametersManager,
        IApiClientsFabric apiClientsFabric, bool useNone = false) : base(propertyName)
    {
        _logger = logger;
        _parametersManager = parametersManager;
        _apiClientsFabric = apiClientsFabric;
        _useNone = useNone;
    }

    public override void UpdateField(string? recordName, object recordForUpdate)
    {
        var currentApiClientName = GetValue(recordForUpdate);

        ApiClientCruder apiClientCruder = new(_parametersManager, _logger, _apiClientsFabric);

        var newValue = apiClientCruder.GetNameWithPossibleNewName(FieldName, currentApiClientName, null, _useNone);

        SetValue(recordForUpdate, newValue);
    }

    public override string GetValueStatus(object? record)
    {
        var val = GetValue(record);

        if (val == null)
            return "";

        ApiClientCruder apiClientCruder = new(_parametersManager, _logger, _apiClientsFabric);

        var status = apiClientCruder.GetStatusFor(val);
        return $"{val} {(string.IsNullOrWhiteSpace(status) ? "" : $"({status})")}";
    }
}