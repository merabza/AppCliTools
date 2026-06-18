using System;
using System.Collections.Generic;
using System.Linq;
using AppCliTools.CliMenu;
using AppCliTools.CliParameters.Cruders;
using AppCliTools.CliParameters.FieldEditors;
using SystemTools.SystemToolsShared;

namespace AppCliTools.CliParameters;

public sealed class SimpleNamesWithDescriptionsFieldEditor<TCruder> : FieldEditor<Dictionary<string, string>>
    where TCruder : Cruder
{
    private readonly Func<Dictionary<string, string>, TCruder> _cruderFactory;

    // ReSharper disable once ConvertToPrimaryConstructor
    public SimpleNamesWithDescriptionsFieldEditor(string propertyName,
        Func<Dictionary<string, string>, TCruder> cruderFactory) : base(propertyName, false, null, false, null, true)
    {
        _cruderFactory = cruderFactory;
    }

    public override CliMenuSet GetSubMenu(object record)
    {
        Dictionary<string, string>? currentValuesDict = GetValue(record);
        if (currentValuesDict is null)
        {
            //თუ ველის მნიშვნელობა null-ია, შევქმნათ ცარიელი ლექსიკონი და დავუბრუნოთ record-ს,
            //რომ შემდგომი ცვლილებები რეფერენსით აისახოს თვით თვისებაში
            currentValuesDict = [];
            SetValue(record, currentValuesDict);
        }

        return _cruderFactory(currentValuesDict).GetListMenu();
    }

    public override void SetDefault(ItemData currentItem)
    {
        //ნაგულისხმევად ცარიელი ლექსიკონი, რომ ახლადშექმნილ ჩანაწერს არ ჰქონდეს null
        SetValue(currentItem, []);
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
