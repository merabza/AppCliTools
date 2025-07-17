using System;
using System.Collections.Generic;
using System.Linq;
using CliMenu;
using LibParameters;
using Microsoft.Extensions.Logging;

namespace CliParameters.FieldEditors;

public sealed class SimpleNamesListFieldEditor<T> : FieldEditor<List<string>> where T : Cruder
{
    private readonly ILogger? _logger;
    private readonly ParametersManager _parametersManager;

    // ReSharper disable once ConvertToPrimaryConstructor
    public SimpleNamesListFieldEditor(string propertyName, ParametersManager parametersManager) : base(
        propertyName, false, null, false, null, true)
    {
        _parametersManager = parametersManager;
    }

    // ReSharper disable once ConvertToPrimaryConstructor
    public SimpleNamesListFieldEditor(ILogger logger, string propertyName,
        ParametersManager parametersManager) : base(propertyName, false, null, false, null, true)
    {
        _logger = logger;
        _parametersManager = parametersManager;
    }

    public override CliMenuSet GetSubMenu(object record)
    {
        var cruder = _logger is null
            ? (T)Activator.CreateInstance(typeof(T), _parametersManager, record)!
            : (T)Activator.CreateInstance(typeof(T), _logger, _parametersManager, record)!;
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