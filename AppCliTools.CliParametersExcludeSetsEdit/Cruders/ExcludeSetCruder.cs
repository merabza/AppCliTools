using System.Collections.Generic;
using System.Linq;
using AppCliTools.CliMenu;
using AppCliTools.CliParameters;
using AppCliTools.CliParameters.CliMenuCommands;
using AppCliTools.CliParametersExcludeSetsEdit.MenuCommands;
using ParametersManagement.LibFileParameters.Interfaces;
using ParametersManagement.LibFileParameters.Models;
using ParametersManagement.LibParameters;

namespace AppCliTools.CliParametersExcludeSetsEdit.Cruders;

public sealed class ExcludeSetCruder : ParCruder<ExcludeSet>
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public ExcludeSetCruder(IParametersManager parametersManager,
        Dictionary<string, ExcludeSet> currentValuesDictionary) : base(parametersManager, currentValuesDictionary,
        "Exclude Set", "Exclude Sets", true)
    {
    }

    //public გამოიყენება UsbCopy პროექტში
    public override void FillDetailsSubMenu(CliMenuSet itemSubMenuSet, string itemName)
    {
        base.FillDetailsSubMenu(itemSubMenuSet, itemName);

        var parameters = (IParametersWithExcludeSets)ParametersManager.Parameters;
        Dictionary<string, ExcludeSet> excludeSets = parameters.ExcludeSets;
        ExcludeSet excludeSet = excludeSets[itemName];

        var detailsCruder = new ExcludeSetFileMaskCruder(ParametersManager, itemName);
        var newItemCommand = new NewItemCliMenuCommand(detailsCruder, itemName, $"Create New {detailsCruder.CrudName}");
        itemSubMenuSet.AddMenuItem(newItemCommand);

        //if (excludeSet.FolderFileMasks == null)
        //    return;

        foreach (ItemSubMenuCliMenuCommand detailListCommand in excludeSet.FolderFileMasks.Select(mask =>
                     new ItemSubMenuCliMenuCommand(detailsCruder, mask, itemName, true)))
        {
            itemSubMenuSet.AddMenuItem(detailListCommand);
        }
    }

    public static ExcludeSetCruder Create(IParametersManager parametersManager)
    {
        var parameters = (IParametersWithExcludeSets)parametersManager.Parameters;
        return new ExcludeSetCruder(parametersManager, parameters.ExcludeSets);
    }

    //protected override Dictionary<string, ItemData> GetCrudersDictionary()
    //{
    //    var parameters = (IParametersWithExcludeSets)ParametersManager.Parameters;
    //    return parameters.ExcludeSets.ToDictionary(p => p.Key, p => (ItemData)p.Value);
    //}

    //public override bool ContainsRecordWithKey(string recordKey)
    //{
    //    var parameters = (IParametersWithExcludeSets)ParametersManager.Parameters;
    //    var excludeSets = parameters.ExcludeSets;
    //    return excludeSets.ContainsKey(recordKey);
    //}

    //public override void UpdateRecordWithKey(string recordName, ItemData newRecord)
    //{
    //    var newExcludeSet = newRecord as ExcludeSet;
    //    var parameters = (IParametersWithExcludeSets)ParametersManager.Parameters;
    //    if (newExcludeSet is null)
    //    {
    //        StShared.WriteErrorLine("ExcludeSet is empty and cannot be saved", true);
    //        return;
    //    }

    //    parameters.ExcludeSets[recordName] = newExcludeSet;
    //}

    //protected override void AddRecordWithKey(string recordName, ItemData newRecord)
    //{
    //    var newExcludeSet = newRecord as ExcludeSet;
    //    var parameters = (IParametersWithExcludeSets)ParametersManager.Parameters;
    //    if (newExcludeSet is null)
    //    {
    //        StShared.WriteErrorLine("ExcludeSet is empty and cannot be saved", true);
    //        return;
    //    }

    //    parameters.ExcludeSets.Add(recordName, newExcludeSet);
    //}

    //protected override void RemoveRecordWithKey(string recordKey)
    //{
    //    var parameters = (IParametersWithExcludeSets)ParametersManager.Parameters;
    //    var excludeSets = parameters.ExcludeSets;
    //    excludeSets.Remove(recordKey);
    //}

    //protected override ItemData CreateNewItem(string? recordKey, ItemData? defaultItemData)
    //{
    //    return new ExcludeSet();
    //}

    protected override void FillListMenuAdditional(CliMenuSet cruderSubMenuSet)
    {
        var generateCommand = new GenerateExcludeSetsCommand(ParametersManager);
        cruderSubMenuSet.AddMenuItem(generateCommand);
    }
}
