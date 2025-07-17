using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using CliMenu;
using LibParameters;
using Microsoft.Extensions.Logging;

namespace CliParameters.FieldEditors;

public sealed class DictionaryFieldEditor<TCrud, TD> : FieldEditor<Dictionary<string, TD>>
    where TCrud : Cruder where TD : ItemData
{
    private readonly IHttpClientFactory? _httpClientFactory;
    private readonly ILogger? _logger;
    private readonly IParametersManager _parametersManager;

    // ReSharper disable once ConvertToPrimaryConstructor
    public DictionaryFieldEditor(string propertyName, IParametersManager parametersManager,
        bool enterFieldDataOnCreate = false) : base(propertyName, enterFieldDataOnCreate, null, false, null, true)
    {
        _parametersManager = parametersManager;
    }

    public DictionaryFieldEditor(ILogger logger, IHttpClientFactory httpClientFactory, string propertyName,
        IParametersManager parametersManager, bool enterFieldDataOnCreate = false) : base(propertyName,
        enterFieldDataOnCreate, null, false, null, true)
    {
        _parametersManager = parametersManager;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    public override CliMenuSet GetSubMenu(object record)
    {
        Cruder cruder;

        if (_httpClientFactory is not null && _logger is not null)
        {
            cruder = (TCrud)Activator.CreateInstance(typeof(TCrud), _parametersManager, _logger, _httpClientFactory)!;
        }
        else if (_logger is not null)
        {
            cruder = (TCrud)Activator.CreateInstance(typeof(TCrud), _logger, _parametersManager)!;
        }
        else
        {
            var currentValuesDict = GetValue(record) ?? [];
            cruder = (TCrud)Activator.CreateInstance(typeof(TCrud), _parametersManager, currentValuesDict)!;
        }

        return cruder.GetListMenu();
    }

    public override string GetValueStatus(object? record)
    {
        var val = GetValue(record);

        if (val is null || val.Count <= 0)
            return "No Details";

        if (val.Count > 1)
            return $"{val.Count} Details";

        var kvp = val.Single();
        return $"{kvp.Key} - {kvp.Value.GetItemKey()}";
    }
}