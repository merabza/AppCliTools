using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using AppCliTools.CliMenu;
using AppCliTools.CliParameters.Cruders;
using Microsoft.Extensions.Logging;
using ParametersManagement.LibParameters;

namespace AppCliTools.CliParameters.FieldEditors;

public sealed class SimpleNamesListFieldEditor<TCruder> : FieldEditor<List<string>> where TCruder : Cruder
{
    private readonly IHttpClientFactory? _httpClientFactory;
    private readonly ILogger? _logger;
    private readonly ParametersManager _parametersManager;

    public SimpleNamesListFieldEditor(string propertyName, ParametersManager parametersManager) : base(propertyName,
        false, null, false, null, true)
    {
        _parametersManager = parametersManager;
    }

    public SimpleNamesListFieldEditor(string propertyName, ILogger logger, ParametersManager parametersManager) : base(
        propertyName, false, null, false, null, true)
    {
        _logger = logger;
        _parametersManager = parametersManager;
    }

    public SimpleNamesListFieldEditor(string propertyName, ILogger logger, IHttpClientFactory httpClientFactory,
        ParametersManager parametersManager) : base(propertyName, false, null, false, null, true)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _parametersManager = parametersManager;
    }

    public override CliMenuSet GetSubMenu(object record)
    {
        Cruder cruder;
        List<string> currentValuesDict = GetValue(record) ?? [];

        if (_httpClientFactory is not null && _logger is not null)
        {
            cruder = (TCruder)Activator.CreateInstance(typeof(TCruder), _logger, _httpClientFactory, _parametersManager,
                currentValuesDict)!;
            return cruder.GetListMenu();
        }

        cruder = _logger is not null
            ? (TCruder)Activator.CreateInstance(typeof(TCruder), _logger, _parametersManager, currentValuesDict)!
            : (TCruder)Activator.CreateInstance(typeof(TCruder), _parametersManager, currentValuesDict)!;

        return cruder.GetListMenu();
    }

    public override string GetValueStatus(object? record)
    {
        List<string>? val = GetValue(record);

        if (val is not { Count: > 0 })
        {
            return "No Details";
        }

        if (val.Count > 1)
        {
            return $"{val.Count} Details";
        }

        string element = val.Single();
        return element;
    }
}
