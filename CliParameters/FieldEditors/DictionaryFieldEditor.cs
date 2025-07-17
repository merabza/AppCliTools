using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using CliMenu;
using CliParameters.Cruders;
using LibParameters;
using Microsoft.Extensions.Logging;

namespace CliParameters.FieldEditors;

public sealed class DictionaryFieldEditor<TCruder, TItemData> : FieldEditor<Dictionary<string, TItemData>>
    where TCruder : Cruder where TItemData : ItemData
{
    private readonly IHttpClientFactory? _httpClientFactory;
    private readonly ILogger? _logger;
    private readonly IParametersManager _parametersManager;

    public DictionaryFieldEditor(string propertyName, IParametersManager parametersManager,
        bool enterFieldDataOnCreate = false) : base(propertyName, enterFieldDataOnCreate, null, false, null, true)
    {
        _parametersManager = parametersManager;
    }

    public DictionaryFieldEditor(string propertyName, ILogger logger, IParametersManager parametersManager,
        bool enterFieldDataOnCreate = false) : base(propertyName, enterFieldDataOnCreate, null, false, null, true)
    {
        _logger = logger;
        _parametersManager = parametersManager;
    }

    public DictionaryFieldEditor(string propertyName, ILogger logger, IHttpClientFactory httpClientFactory,
        IParametersManager parametersManager, bool enterFieldDataOnCreate = false) : base(propertyName,
        enterFieldDataOnCreate, null, false, null, true)
    {
        _logger = logger;
        _parametersManager = parametersManager;
        _httpClientFactory = httpClientFactory;
    }

    public override CliMenuSet GetSubMenu(object record)
    {
        Cruder cruder;
        var currentValuesDict = GetValue(record) ?? [];

        if (_httpClientFactory is not null && _logger is not null)
            cruder = (TCruder)Activator.CreateInstance(typeof(TCruder), _logger, _httpClientFactory, _parametersManager,
                currentValuesDict)!;
        else if (_logger is not null)
            cruder = (TCruder)Activator.CreateInstance(typeof(TCruder), _logger, _parametersManager,
                currentValuesDict)!;
        else
            cruder = (TCruder)Activator.CreateInstance(typeof(TCruder), _parametersManager, currentValuesDict)!;

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