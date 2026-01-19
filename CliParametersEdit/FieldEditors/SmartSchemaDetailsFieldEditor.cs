using System.Collections.Generic;
using CliMenu;
using CliParameters.FieldEditors;
using CliParametersEdit.Cruders;
using ParametersManagement.LibFileParameters.Models;

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

        var smartSchemaDetailCruder = new SmartSchemaDetailCruder(currentValuesDict);
        var menuSet = smartSchemaDetailCruder.GetListMenu();

        return menuSet;
    }

    public override string GetValueStatus(object? record)
    {
        var val = GetValue(record);

        if (val is null || val.Count <= 0)
        {
            return "No Details";
        }

        var sb = new System.Text.StringBuilder();
        var culture = System.Globalization.CultureInfo.InvariantCulture;
        sb.Append(culture, $"{val[0].PeriodType}-{val[0].PreserveCount}");
        for (var i = 1; i < val.Count; i++)
        {
            sb.Append(culture, $"/{val[i].PeriodType}-{val[i].PreserveCount}");
        }
        return sb.ToString();
    }
}
