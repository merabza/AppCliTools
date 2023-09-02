using System.Collections.Generic;
using System.Linq;
using CliMenu;
using CliParameters.FieldEditors;
using CliParametersEdit.Cruders;
using LibFileParameters.Models;
using LibParameters;

namespace CliParametersEdit.FieldEditors;

public sealed class ArchiversFieldEditor : FieldEditor<Dictionary<string, ArchiverData>>
{
    private readonly ParametersManager _parametersManager;

    public ArchiversFieldEditor(string propertyName, ParametersManager parametersManager) : base(propertyName, false,
        null, true)
    {
        _parametersManager = parametersManager;
    }

    public override CliMenuSet GetSubMenu(object record)
    {
        ArchiverCruder archiversCruder = new(_parametersManager);
        var menuSet = archiversCruder.GetListMenu();
        return menuSet;
    }


    public override string GetValueStatus(object? record)
    {
        var val = GetValue(record);

        if (val is null || val.Count <= 0)
            return "No Details";

        if (val.Count > 1)
            return $"{val.Count} Details";

        var kvp = val.Single();
        return
            $"{kvp.Key} - {kvp.Value.CompressProgramPatch} {kvp.Value.DecompressProgramPatch} {kvp.Value.FileExtension}";
    }
}