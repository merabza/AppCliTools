using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AppCliTools.CliParameters.Cruders;
using ParametersManagement.LibParameters;
using SystemTools.SystemToolsShared;

namespace AppCliTools.CliParameters;

public /*open*/ class ParCruder<T> : Cruder where T : ItemData, new()
{
    private readonly Dictionary<string, T> _currentValuesDictionary;
    protected readonly IParametersManager ParametersManager;

    protected ParCruder(IParametersManager parametersManager, Dictionary<string, T> currentValuesDictionary,
        string crudName, string crudNamePlural, bool fieldKeyFromItem = false,
        bool canEditFieldsInSequence = true) : base(crudName, crudNamePlural, fieldKeyFromItem, canEditFieldsInSequence)
    {
        ParametersManager = parametersManager;
        _currentValuesDictionary = currentValuesDictionary;
    }

    public override ValueTask Save(string message, CancellationToken cancellationToken = default)
    {
        return ParametersManager.Save(ParametersManager.Parameters, message, null, cancellationToken);
    }

    protected override Dictionary<string, ItemData> GetCrudersDictionary()
    {
        return _currentValuesDictionary.ToDictionary(k => k.Key, ItemData (v) => v.Value);
    }

    protected override ItemData CreateNewItem(string? recordKey, ItemData? defaultItemData)
    {
        return new T();
    }

    public override bool ContainsRecordWithKey(string recordKey)
    {
        return _currentValuesDictionary.ContainsKey(recordKey);
    }

    protected override async ValueTask RemoveRecordWithKey(string recordKey,
        CancellationToken cancellationToken = default)
    {
        CheckKey(recordKey);
        _currentValuesDictionary.Remove(recordKey);
        await Save($"record {recordKey} Removed", cancellationToken);
    }

    public override async ValueTask UpdateRecordWithKey(string recordKey, ItemData newRecord,
        CancellationToken cancellationToken = default)
    {
        CheckKey(recordKey);

        _currentValuesDictionary[recordKey] = GetTItem(newRecord);
        await Save($"record {recordKey} saved", cancellationToken);
    }

    protected override async ValueTask AddRecordWithKey(string recordKey, ItemData newRecord,
        CancellationToken cancellationToken = default)
    {
        _currentValuesDictionary.Add(recordKey, GetTItem(newRecord));
        await Save($"record {recordKey} Added", cancellationToken);
    }

    private void CheckKey(string recordKey)
    {
        if (!ContainsRecordWithKey(recordKey))
        {
            throw new Exception($"No Record Found with key {recordKey}");
        }
    }

    protected T GetTItem(ItemData item)
    {
        return item as T ?? throw new Exception("newRecord is null");
    }
}
