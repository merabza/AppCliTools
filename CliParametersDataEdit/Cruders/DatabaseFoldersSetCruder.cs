using System.Collections.Generic;
using System.Linq;
using CliParameters.Cruders;
using CliParameters.FieldEditors;
using DbTools.Models;
using LibParameters;

namespace CliParametersDataEdit.Cruders;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class DatabaseFoldersSetCruder : Cruder
{
    private readonly Dictionary<string, DatabaseFoldersSet> _currentValuesDictionary;
    private readonly IParametersManager _parametersManager;

    public DatabaseFoldersSetCruder(IParametersManager parametersManager,
        Dictionary<string, DatabaseFoldersSet> currentValuesDictionary) : base("Database Folders Set",
        "Database Folders Sets")
    {
        _parametersManager = parametersManager;
        _currentValuesDictionary = currentValuesDictionary;
        FieldEditors.Add(new FolderPathFieldEditor(nameof(DatabaseFoldersSet.Backup)));
        FieldEditors.Add(new FolderPathFieldEditor(nameof(DatabaseFoldersSet.Data)));
        FieldEditors.Add(new FolderPathFieldEditor(nameof(DatabaseFoldersSet.DataLog)));
    }

    protected override Dictionary<string, ItemData> GetCrudersDictionary()
    {
        return _currentValuesDictionary.ToDictionary(k => k.Key, ItemData (v) => v.Value);
    }

    public override bool ContainsRecordWithKey(string recordKey)
    {
        return _currentValuesDictionary.ContainsKey(recordKey);
    }

    public override void UpdateRecordWithKey(string recordKey, ItemData newRecord)
    {
        if (newRecord is not DatabaseFoldersSet newSmartSchemaDetail)
            return;
        if (!_currentValuesDictionary.TryGetValue(recordKey, out var cv))
            return;

        cv.Backup = newSmartSchemaDetail.Backup;
        cv.Data = newSmartSchemaDetail.Data;
        cv.DataLog = newSmartSchemaDetail.DataLog;
        _parametersManager.Save(_parametersManager.Parameters, $"record {recordKey} saved");
    }

    protected override void AddRecordWithKey(string recordKey, ItemData newRecord)
    {
        if (newRecord is not DatabaseFoldersSet sid)
            return;
        _currentValuesDictionary.Add(recordKey, sid);
        _parametersManager.Save(_parametersManager.Parameters, $"record {recordKey} Added");
    }

    protected override void RemoveRecordWithKey(string recordKey)
    {
        _currentValuesDictionary.Remove(recordKey);
        _parametersManager.Save(_parametersManager.Parameters, $"record {recordKey} Removed");
    }

    public override List<string> GetKeys()
    {
        return _currentValuesDictionary.Keys.ToList();
    }

    public override string? GetStatusFor(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;
        var databaseFoldersSet = (DatabaseFoldersSet?)GetItemByName(name);
        return databaseFoldersSet?.GetItemKey();
    }

    protected override ItemData CreateNewItem(string? recordKey, ItemData? defaultItemData)
    {
        return new DatabaseFoldersSet();
    }
}