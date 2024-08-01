using System.Collections.Generic;
using System.Linq;
using CliMenu;
using CliParameters;
using CliParameters.CliMenuCommands;
using CliParametersExcludeSetsEdit.MenuCommands;
using LibFileParameters.Interfaces;
using LibFileParameters.Models;
using LibParameters;
using SystemToolsShared;

namespace CliParametersExcludeSetsEdit.Cruders;

public sealed class ExcludeSetCruder : ParCruder
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public ExcludeSetCruder(IParametersManager parametersManager) : base(parametersManager, "Exclude Set",
        "Exclude Sets", true)
    {
    }

    //public გამოიყენება UsbCopy პროექტში
    public override void FillDetailsSubMenu(CliMenuSet itemSubMenuSet, string recordName)
    {
        base.FillDetailsSubMenu(itemSubMenuSet, recordName);

        var parameters = (IParametersWithExcludeSets)ParametersManager.Parameters;
        var excludeSets = parameters.ExcludeSets;
        var excludeSet = excludeSets[recordName];

        var detailsCruder = new ExcludeSetFileMaskCruder(ParametersManager, recordName);
        var newItemCommand =
            new NewItemCliMenuCommand(detailsCruder, recordName, $"Create New {detailsCruder.CrudName}");
        itemSubMenuSet.AddMenuItem(newItemCommand);

        //if (excludeSet.FolderFileMasks == null)
        //    return;

        foreach (var detailListCommand in excludeSet.FolderFileMasks.Select(mask =>
                     new ItemSubMenuCliMenuCommand(detailsCruder, mask, recordName, true)))
            itemSubMenuSet.AddMenuItem(detailListCommand);
    }


    protected override Dictionary<string, ItemData> GetCrudersDictionary()
    {
        var parameters = (IParametersWithExcludeSets)ParametersManager.Parameters;
        return parameters.ExcludeSets.ToDictionary(p => p.Key, p => (ItemData)p.Value);
    }

    public override bool ContainsRecordWithKey(string recordKey)
    {
        var parameters = (IParametersWithExcludeSets)ParametersManager.Parameters;
        var excludeSets = parameters.ExcludeSets;
        return excludeSets.ContainsKey(recordKey);
    }

    public override void UpdateRecordWithKey(string recordName, ItemData newRecord)
    {
        var newExcludeSet = newRecord as ExcludeSet;
        var parameters = (IParametersWithExcludeSets)ParametersManager.Parameters;
        if (newExcludeSet is null)
        {
            StShared.WriteErrorLine("ExcludeSet is empty and cannot be saved", true);
            return;
        }

        parameters.ExcludeSets[recordName] = newExcludeSet;
    }

    protected override void AddRecordWithKey(string recordName, ItemData newRecord)
    {
        var newExcludeSet = newRecord as ExcludeSet;
        var parameters = (IParametersWithExcludeSets)ParametersManager.Parameters;
        if (newExcludeSet is null)
        {
            StShared.WriteErrorLine("ExcludeSet is empty and cannot be saved", true);
            return;
        }

        parameters.ExcludeSets.Add(recordName, newExcludeSet);
    }

    protected override void RemoveRecordWithKey(string recordKey)
    {
        var parameters = (IParametersWithExcludeSets)ParametersManager.Parameters;
        var excludeSets = parameters.ExcludeSets;
        excludeSets.Remove(recordKey);
    }

    protected override ItemData CreateNewItem(string? recordKey, ItemData? defaultItemData)
    {
        return new ExcludeSet();
    }

    protected override void FillListMenuAdditional(CliMenuSet cruderSubMenuSet)
    {
        var generateCommand = new GenerateExcludeSetsCommand(ParametersManager);
        cruderSubMenuSet.AddMenuItem(generateCommand);
    }
}