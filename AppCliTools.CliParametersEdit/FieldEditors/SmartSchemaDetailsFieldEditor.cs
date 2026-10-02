using System.Collections.Generic;
using System.Globalization;
using System.Text;
using AppCliTools.CliMenu;
using AppCliTools.CliParameters.FieldEditors;
using AppCliTools.CliParametersEdit.Cruders;
using ParametersManagement.LibFileParameters.Models;
using ParametersManagement.LibParameters;

namespace AppCliTools.CliParametersEdit.FieldEditors;

public sealed class SmartSchemaDetailsFieldEditor : FieldEditor<List<SmartSchemaDetail>>
{
    private readonly IParametersManager? _parametersManager;

    //ამ კონსტრუქტორით შექმნილი რედაქტორი დეტალების ცვლილებას არ ინახავს
    public SmartSchemaDetailsFieldEditor(string propertyName, bool enterFieldDataOnCreate = false) : base(propertyName,
        enterFieldDataOnCreate, null, false, null, true)
    {
    }

    //დეტალების ყოველი ცვლილება parametersManager-ის ძირეული ობიექტის შენახვით სრულდება
    public SmartSchemaDetailsFieldEditor(string propertyName, IParametersManager parametersManager,
        bool enterFieldDataOnCreate = false) : this(propertyName, enterFieldDataOnCreate)
    {
        _parametersManager = parametersManager;
    }

    public override CliMenuSet GetSubMenu(object record)
    {
        List<SmartSchemaDetail> currentValuesDict = GetValue(record) ?? [];

        SmartSchemaDetailCruder smartSchemaDetailCruder = _parametersManager is null
            ? new SmartSchemaDetailCruder(currentValuesDict)
            : new SmartSchemaDetailCruder(_parametersManager, currentValuesDict);
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
