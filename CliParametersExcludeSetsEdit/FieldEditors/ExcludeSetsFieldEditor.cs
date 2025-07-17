//using System.Collections.Generic;
//using System.Linq;
//using CliMenu;
//using CliParameters.FieldEditors;
//using CliParametersExcludeSetsEdit.Cruders;
//using LibFileParameters.Models;
//using LibParameters;

//namespace CliParametersExcludeSetsEdit.FieldEditors;

//public sealed class ExcludeSetsFieldEditor : FieldEditor<Dictionary<string, ExcludeSet>>
//{
//    private readonly ParametersManager _parametersManager;

//    // ReSharper disable once ConvertToPrimaryConstructor
//    public ExcludeSetsFieldEditor(string propertyName, ParametersManager parametersManager,
//        bool enterFieldDataOnCreate = false) : base(propertyName, enterFieldDataOnCreate, null, false, null, true)
//    {
//        _parametersManager = parametersManager;
//    }

//    public override CliMenuSet GetSubMenu(object record)
//    {
//        ExcludeSetCruder excludeSetCruder = new(_parametersManager);
//        var menuSet = excludeSetCruder.GetListMenu();
//        return menuSet;
//    }

//    public override string GetValueStatus(object? record)
//    {
//        var val = GetValue(record);

//        if (val is null || val.Count <= 0)
//            return "No Details";

//        if (val.Count > 1)
//            return $"{val.Count} Details";

//        var kvp = val.Single();
//        return kvp.Key;
//    }
//}