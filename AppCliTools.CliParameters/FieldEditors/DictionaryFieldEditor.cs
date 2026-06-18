using System;
using System.Collections.Generic;
using System.Linq;
using AppCliTools.CliMenu;
using AppCliTools.CliParameters.Cruders;
using SystemTools.SystemToolsShared;

namespace AppCliTools.CliParameters.FieldEditors;

public sealed class DictionaryFieldEditor<TCruder, TItemData> : FieldEditor<Dictionary<string, TItemData>>
    where TCruder : Cruder where TItemData : ItemData
{
    private readonly Func<Dictionary<string, TItemData>, TCruder> _cruderFactory;

    // ReSharper disable once ConvertToPrimaryConstructor
    public DictionaryFieldEditor(string propertyName, Func<Dictionary<string, TItemData>, TCruder> cruderFactory,
        bool enterFieldDataOnCreate = false) : base(propertyName, enterFieldDataOnCreate, null, false, null, true)
    {
        _cruderFactory = cruderFactory;
    }

    public override CliMenuSet GetSubMenu(object record)
    {
        Dictionary<string, TItemData> currentValuesDict = GetValue(record) ?? [];
        return _cruderFactory(currentValuesDict).GetListMenu();
    }

    public override string GetValueStatus(object? record)
    {
        Dictionary<string, TItemData>? val = GetValue(record);

        if (val is null || val.Count <= 0)
        {
            return "No Details";
        }

        if (val.Count > 1)
        {
            return $"{val.Count} Details";
        }

        KeyValuePair<string, TItemData> kvp = val.Single();
        return $"{kvp.Key} - {kvp.Value.GetItemKey()}";
    }
}
