using System.Collections.Generic;
using System.Linq;
using CliParameters;
using LibFileParameters.Interfaces;
using LibParameters;

namespace CliParametersExcludeSetsEdit.Cruders;

public sealed class ExcludeSetFileMaskCruder : ParCruder
{
    private readonly string _excludeSetName;

    public ExcludeSetFileMaskCruder(IParametersManager parametersManager, string excludeSetName) : base(
        parametersManager, "File Mask", "File Masks", true)
    {
        _excludeSetName = excludeSetName;
    }

    private List<string> GetFileMasks()
    {
        var parameters = (IParametersWithExcludeSets)ParametersManager.Parameters;
        var excludeSets = parameters.ExcludeSets;
        if (!excludeSets.ContainsKey(_excludeSetName))
            return new List<string>();
        var excludeSet = excludeSets[_excludeSetName];
        return excludeSet.FolderFileMasks;
    }

    protected override Dictionary<string, ItemData> GetCrudersDictionary()
    {
<<<<<<< HEAD
        return GetFileMasks().ToDictionary(k => k, v => (ItemData)new TextItemData {Text = v});
=======
        return GetFileMasks().ToDictionary(k => k, v => (ItemData)new TextItemData{ Text = v });
>>>>>>> 6df0052617f86e0293c4f5e73053c2b20579e2ec
    }

    protected override ItemData CreateNewItem(ItemData? defaultItemData)
    {
<<<<<<< HEAD
        return new TextItemData();
=======
        return new TextItemData {Text = ""};
>>>>>>> 6df0052617f86e0293c4f5e73053c2b20579e2ec
    }


    public override bool ContainsRecordWithKey(string recordKey)
    {
        var fileMasks = GetFileMasks();
        return fileMasks.Contains(recordKey);
    }

    protected override void RemoveRecordWithKey(string recordKey)
    {
        var fileMasks = GetFileMasks();
        fileMasks.Remove(recordKey);
    }

    protected override void AddRecordWithKey(string recordName, ItemData newRecord)
    {
        var parameters = (IParametersWithExcludeSets)ParametersManager.Parameters;
        var excludeSets = parameters.ExcludeSets;
        if (!excludeSets.ContainsKey(_excludeSetName))
            return;
        var excludeSet = excludeSets[_excludeSetName];

        excludeSet.FolderFileMasks.Add(recordName);
    }
}