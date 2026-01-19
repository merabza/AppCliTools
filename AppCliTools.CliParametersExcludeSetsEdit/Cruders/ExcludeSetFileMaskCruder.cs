using System.Collections.Generic;
using System.Linq;
using AppCliTools.CliParameters;
using AppCliTools.CliParameters.Cruders;
using ParametersManagement.LibFileParameters.Interfaces;
using ParametersManagement.LibFileParameters.Models;
using ParametersManagement.LibParameters;

namespace AppCliTools.CliParametersExcludeSetsEdit.Cruders;

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
        Dictionary<string, ExcludeSet> excludeSets = parameters.ExcludeSets;
        return !excludeSets.TryGetValue(_excludeSetName, out ExcludeSet? excludeSet) ? [] : excludeSet.FolderFileMasks;
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
        List<string> fileMasks = GetFileMasks();
        return fileMasks.Contains(recordKey);
    }

    protected override void RemoveRecordWithKey(string recordKey)
    {
        List<string> fileMasks = GetFileMasks();
        fileMasks.Remove(recordKey);
    }

    protected override void AddRecordWithKey(string recordKey, ItemData newRecord)
    {
        var parameters = (IParametersWithExcludeSets)_parametersManager.Parameters;
        Dictionary<string, ExcludeSet> excludeSets = parameters.ExcludeSets;
        if (!excludeSets.TryGetValue(_excludeSetName, out ExcludeSet? excludeSet))
        {
            return;
        }

        excludeSet.FolderFileMasks.Add(recordKey);
    }
}
