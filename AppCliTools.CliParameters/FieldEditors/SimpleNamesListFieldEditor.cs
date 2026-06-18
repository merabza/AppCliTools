using System;
using System.Collections.Generic;
using System.Linq;
using AppCliTools.CliMenu;
using AppCliTools.CliParameters.Cruders;

namespace AppCliTools.CliParameters.FieldEditors;

public sealed class SimpleNamesListFieldEditor<TCruder> : FieldEditor<List<string>> where TCruder : Cruder
{
    private readonly Func<List<string>, TCruder> _cruderFactory;

    // ReSharper disable once ConvertToPrimaryConstructor
    public SimpleNamesListFieldEditor(string propertyName, Func<List<string>, TCruder> cruderFactory) : base(
        propertyName, false, null, false, null, true)
    {
        _cruderFactory = cruderFactory;
    }

    public override CliMenuSet GetSubMenu(object record)
    {
        List<string> currentValuesList = GetValue(record) ?? [];
        return _cruderFactory(currentValuesList).GetListMenu();
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
