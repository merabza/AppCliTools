using System.Collections.Generic;
using System.Linq;
using CliParameters;
using LibFileParameters.Interfaces;
using LibParameters;

namespace CliParametersExcludeSetsEdit.Cruders;

public sealed class ExcludeSetFileMaskCruder : Cruder
{
    private readonly string _excludeSetName;
    private readonly IParametersManager _parametersManager;

    // ReSharper disable once ConvertToPrimaryConstructor
    public ExcludeSetFileMaskCruder(IParametersManager parametersManager, string excludeSetName) : base("File Mask",
        "File Masks")
    {
        _parametersManager = parametersManager;
        _excludeSetName = excludeSetName;
    }

    private List<string> GetFileMasks()
    {
        var parameters = (IParametersWithExcludeSets)_parametersManager.Parameters;
        var excludeSets = parameters.ExcludeSets;
        return !excludeSets.TryGetValue(_excludeSetName, out var excludeSet) ? [] : excludeSet.FolderFileMasks;
    }

    protected override Dictionary<string, ItemData> GetCrudersDictionary()
    {
        return GetFileMasks().ToDictionary(k => k, ItemData (v) => new TextItemData { Text = v });
    }

    protected override ItemData CreateNewItem(string? recordKey, ItemData? defaultItemData)
    {
        return new TextItemData { Text = string.Empty };
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
        var parameters = (IParametersWithExcludeSets)_parametersManager.Parameters;
        var excludeSets = parameters.ExcludeSets;
        if (!excludeSets.TryGetValue(_excludeSetName, out var excludeSet))
            return;

        excludeSet.FolderFileMasks.Add(recordName);
    }
}