using System.Collections.Generic;
using CliMenu;
using CliParameters.FieldEditors;
using CliParametersEdit.Cruders;
using LibFileParameters.Models;

namespace CliParametersEdit.FieldEditors;

public sealed class SmartSchemaDetailsFieldEditor : FieldEditor<List<SmartSchemaDetail>>
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public SmartSchemaDetailsFieldEditor(string propertyName, bool enterFieldDataOnCreate = false) : base(propertyName,
        enterFieldDataOnCreate, null, false, null, true)
    {
    }

    public override CliMenuSet GetSubMenu(object record)
    {
        var currentValuesDict = GetValue(record) ?? [];

        SmartSchemaDetailCruder smartSchemaDetailCruder = new(currentValuesDict);
        var menuSet = smartSchemaDetailCruder.GetListMenu();

        return menuSet;
    }

    public override string GetValueStatus(object? record)
    {
        var val = GetValue(record);

        if (val is null || val.Count <= 0)
            return "No Details";

        var result = $"{val[0].PeriodType}-{val[0].PreserveCount}";
        for (var i = 1; i < val.Count; i++)
            result += $"/{val[i].PeriodType}-{val[i].PreserveCount}";

        return result;
    }
}