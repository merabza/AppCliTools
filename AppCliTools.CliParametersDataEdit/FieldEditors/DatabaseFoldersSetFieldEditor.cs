//using System.Collections.Generic;
//using System.Linq;
//using CliMenu;
//using CliParameters.FieldEditors;
//using CliParametersDataEdit.Cruders;
//using DbTools.Models;
//using LibParameters;

//namespace CliParametersDataEdit.FieldEditors;

//public sealed class DatabaseFoldersSetFieldEditor : FieldEditor<Dictionary<string, DatabaseFoldersSet>>
//{
//    private readonly IParametersManager _parametersManager;

//    // ReSharper disable once ConvertToPrimaryConstructor
//    public DatabaseFoldersSetFieldEditor(IParametersManager parametersManager, string propertyName,
//        bool enterFieldDataOnCreate = false) : base(propertyName, enterFieldDataOnCreate, null, false, null, true)
//    {
//        _parametersManager = parametersManager;
//    }

//    public override CliMenuSet GetSubMenu(object record)
//    {
//        var currentValuesDict = GetValue(record) ?? [];

//        var databaseFoldersSetCruder = new DatabaseFoldersSetCruder(_parametersManager, currentValuesDict);
//        var menuSet = databaseFoldersSetCruder.GetListMenu();
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
//        return $"{kvp.Key} - {kvp.Value.Backup} {kvp.Value.Data} {kvp.Value.DataLog}";
//    }
//}


