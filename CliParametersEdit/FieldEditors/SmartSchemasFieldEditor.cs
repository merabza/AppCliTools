using System.Collections.Generic;
using System.Linq;
using CliMenu;
using CliParameters.FieldEditors;
using CliParametersEdit.Cruders;
using LibFileParameters.Models;
using LibParameters;

namespace CliParametersEdit.FieldEditors;

public sealed class SmartSchemasFieldEditor : FieldEditor<Dictionary<string, SmartSchema>>
{
    private readonly IParametersManager _parametersManager;

    public SmartSchemasFieldEditor(string propertyName, IParametersManager parametersManager,
        bool enterFieldDataOnCreate = false) : base(propertyName, enterFieldDataOnCreate, null, true)
    {
        _parametersManager = parametersManager;
    }

    public override CliMenuSet GetSubMenu(object record)
    {
        SmartSchemaCruder smartSchemasCruder = new(_parametersManager);
        var menuSet = smartSchemasCruder.GetListMenu();
        return menuSet;
    }


    public override string GetValueStatus(object? record)
    {
        var val = GetValue(record);

        if (val == null || val.Count <= 0)
            return "No Details";

        if (val.Count > 1)
            return $"{val.Count} Details";

        var kvp = val.Single();
        return kvp.Key;
    }
}