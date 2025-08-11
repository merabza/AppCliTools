using System;
using System.Collections.Generic;
using System.Linq;
using CliMenu;
using CliParameters.Cruders;
using LibParameters;
using Microsoft.Extensions.Logging;

namespace CliParameters.FieldEditors;

public sealed class SimpleNamesListFieldEditor<TCruder> : FieldEditor<List<string>> where TCruder : Cruder
{
    private readonly ILogger? _logger;
    private readonly ParametersManager _parametersManager;

    public SimpleNamesListFieldEditor(string propertyName, ParametersManager parametersManager) : base(propertyName,
        false, null, false, null, true)
    {
        _parametersManager = parametersManager;
    }

    public SimpleNamesListFieldEditor(ILogger logger, string propertyName, ParametersManager parametersManager) : base(
        propertyName, false, null, false, null, true)
    {
        _logger = logger;
        _parametersManager = parametersManager;
    }

    public override CliMenuSet GetSubMenu(object record)
    {
        Cruder cruder;
        var currentValuesDict = GetValue(record) ?? [];

        if (_logger is not null)
            cruder = (TCruder)Activator.CreateInstance(typeof(TCruder), _logger, _parametersManager,
                currentValuesDict)!;
        else
            cruder = (TCruder)Activator.CreateInstance(typeof(TCruder), _parametersManager, currentValuesDict)!;

        return cruder.GetListMenu();
    }

    public override string GetValueStatus(object? record)
    {
        var val = GetValue(record);

        if (val is not { Count: > 0 })
            return "No Details";

        if (val.Count > 1)
            return $"{val.Count} Details";

        var element = val.Single();
        return element;
    }
}