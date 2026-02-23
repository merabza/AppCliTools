using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AppCliTools.CliParameters.Cruders;
using AppCliTools.CliParameters.FieldEditors;
using DatabaseTools.DbTools.Models;
using ParametersManagement.LibParameters;
using SystemTools.SystemToolsShared;

namespace AppCliTools.CliParametersDataEdit.Cruders;

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

    public override async ValueTask UpdateRecordWithKey(string recordKey, ItemData newRecord,
        CancellationToken cancellationToken = default)

    {
        if (newRecord is not DatabaseFoldersSet newSmartSchemaDetail)
        {
            return;
        }

        if (!_currentValuesDictionary.TryGetValue(recordKey, out DatabaseFoldersSet? cv))
        {
            return;
        }

        cv.Backup = newSmartSchemaDetail.Backup;
        cv.Data = newSmartSchemaDetail.Data;
        cv.DataLog = newSmartSchemaDetail.DataLog;
        await _parametersManager.Save(_parametersManager.Parameters, $"record {recordKey} saved", null,
            cancellationToken);
    }

    protected override async ValueTask AddRecordWithKey(string recordKey, ItemData newRecord,
        CancellationToken cancellationToken = default)
    {
        if (newRecord is not DatabaseFoldersSet sid)
        {
            return;
        }

        _currentValuesDictionary.Add(recordKey, sid);
        await _parametersManager.Save(_parametersManager.Parameters, $"record {recordKey} Added", null,
            cancellationToken);
    }

    protected override async ValueTask RemoveRecordWithKey(string recordKey,
        CancellationToken cancellationToken = default)
    {
        _currentValuesDictionary.Remove(recordKey);
        await _parametersManager.Save(_parametersManager.Parameters, $"record {recordKey} Removed", null,
            cancellationToken);
    }

    public override List<string> GetKeys()
    {
        return _currentValuesDictionary.Keys.ToList();
    }

    public override string? GetStatusFor(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        var databaseFoldersSet = (DatabaseFoldersSet?)GetItemByName(name);
        return databaseFoldersSet?.GetItemKey();
    }

    protected override ItemData CreateNewItem(string? recordKey, ItemData? defaultItemData)
    {
        return new DatabaseFoldersSet();
    }
}
