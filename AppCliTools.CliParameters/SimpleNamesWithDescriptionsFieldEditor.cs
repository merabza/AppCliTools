using System;
using System.Collections.Generic;
using System.Linq;
using AppCliTools.CliMenu;
using AppCliTools.CliParameters.Cruders;
using AppCliTools.CliParameters.FieldEditors;
using Microsoft.Extensions.Logging;
using ParametersManagement.LibParameters;

namespace AppCliTools.CliParameters;

public sealed class SimpleNamesWithDescriptionsFieldEditor<TCruder> : FieldEditor<Dictionary<string, string>>
    where TCruder : Cruder
{
    private readonly ILogger? _logger;
    private readonly ParametersManager? _parametersManager;

    public SimpleNamesWithDescriptionsFieldEditor(string propertyName, ILogger logger,
        ParametersManager parametersManager) : base(propertyName, false, null, false, null, true)
    {
        _logger = logger;
        _parametersManager = parametersManager;
    }

    public SimpleNamesWithDescriptionsFieldEditor(string propertyName, ParametersManager parametersManager) : base(
        propertyName, false, null, false, null, true)
    {
        _parametersManager = parametersManager;
    }

    public SimpleNamesWithDescriptionsFieldEditor(string propertyName) : base(propertyName, false, null, false, null,
        true)
    {
    }

    public override CliMenuSet GetSubMenu(object record)
    {
        Cruder cruder;
        Dictionary<string, string> currentValuesDict = GetValue(record) ?? [];

        if (_logger is not null && _parametersManager is not null)
        {
            cruder = (TCruder)Activator.CreateInstance(typeof(TCruder), _logger, _parametersManager,
                currentValuesDict)!;
            return cruder.GetListMenu();
        }

        cruder = _parametersManager is not null
            ? (TCruder)Activator.CreateInstance(typeof(TCruder), _parametersManager, currentValuesDict)!
            : (TCruder)Activator.CreateInstance(typeof(TCruder), currentValuesDict)!;

        return cruder.GetListMenu();
    }

    public override string GetValueStatus(object? record)
    {
        Dictionary<string, string>? val = GetValue(record);

        if (val is not { Count: > 0 })
        {
            return "No Details";
        }

        if (val.Count > 1)
        {
            return $"{val.Count} Details";
        }

        KeyValuePair<string, string> kvp = val.Single();
        return kvp.Key;
    }
}
