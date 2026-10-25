using System;
using System.Collections.Generic;
using System.Linq;
using AppCliTools.CliMenu;
using AppCliTools.CliParameters.Cruders;
using SystemTools.SystemToolsShared;

namespace AppCliTools.CliParameters.FieldEditors;

public sealed class SimpleNamesListFieldEditor<TCruder> : FieldEditor<List<string>> where TCruder : Cruder
{
    private readonly Func<List<string>, TCruder> _cruderFactory;

    public SimpleNamesListFieldEditor(string propertyName, Func<List<string>, TCruder> cruderFactory) : base(
        propertyName, false, null, false, null, true)
    {
        _cruderFactory = cruderFactory;
    }

    public override CliMenuSet GetSubMenu(object record)
    {
        List<string>? currentValuesList = GetValue(record);
        if (currentValuesList is null)
        {
            //თუ ველის მნიშვნელობა null-ია, შევქმნათ ცარიელი სია და დავუბრუნოთ record-ს,
            //რომ შემდგომი ცვლილებები რეფერენსით აისახოს თვით თვისებაში
            currentValuesList = [];
            SetValue(record, currentValuesList);
        }

        return _cruderFactory(currentValuesList).GetListMenu();
    }

    public override void SetDefault(ItemData currentItem)
    {
        //ნაგულისხმევად ცარიელი სია, რომ ახლადშექმნილ ჩანაწერს არ ჰქონდეს null
        SetValue(currentItem, []);
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
