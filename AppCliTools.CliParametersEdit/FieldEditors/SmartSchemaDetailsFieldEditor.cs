using System.Collections.Generic;
using System.Globalization;
using System.Text;
using AppCliTools.CliMenu;
using AppCliTools.CliParameters.FieldEditors;
using AppCliTools.CliParametersEdit.Cruders;
using ParametersManagement.LibFileParameters.Models;

namespace AppCliTools.CliParametersEdit.FieldEditors;

public sealed class SmartSchemaDetailsFieldEditor : FieldEditor<List<SmartSchemaDetail>>
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public SmartSchemaDetailsFieldEditor(string propertyName, bool enterFieldDataOnCreate = false) : base(propertyName,
        enterFieldDataOnCreate, null, false, null, true)
    {
    }

    public override CliMenuSet GetSubMenu(object record)
    {
        List<SmartSchemaDetail> currentValuesDict = GetValue(record) ?? [];

        var smartSchemaDetailCruder = new SmartSchemaDetailCruder(currentValuesDict);
        CliMenuSet menuSet = smartSchemaDetailCruder.GetListMenu();

        return menuSet;
    }

    public override string GetValueStatus(object? record)
    {
        List<SmartSchemaDetail>? val = GetValue(record);

        if (val is null || val.Count <= 0)
        {
            return "No Details";
        }

        var sb = new StringBuilder();
        CultureInfo culture = CultureInfo.InvariantCulture;
        sb.Append(culture, $"{val[0].PeriodType}-{val[0].PreserveCount}");
        for (int i = 1; i < val.Count; i++)
        {
            sb.Append(culture, $"/{val[i].PeriodType}-{val[i].PreserveCount}");
        }

        return sb.ToString();
    }
}
