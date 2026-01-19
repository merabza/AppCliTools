using System;
using System.Collections.Generic;
using System.Linq;
using CliParameters.Cruders;
using ParametersManagement.LibParameters;

namespace CliParameters;

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

    public override void Save(string message)
    {
        ParametersManager.Save(ParametersManager.Parameters, message);
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

    protected override void RemoveRecordWithKey(string recordKey)
    {
        CheckKey(recordKey);
        _currentValuesDictionary.Remove(recordKey);
        Save($"record {recordKey} Removed");
    }

    public override void UpdateRecordWithKey(string recordKey, ItemData newRecord)
    {
        CheckKey(recordKey);

        _currentValuesDictionary[recordKey] = GetTItem(newRecord);
        Save($"record {recordKey} saved");
    }

    protected override void AddRecordWithKey(string recordKey, ItemData newRecord)
    {
        _currentValuesDictionary.Add(recordKey, GetTItem(newRecord));
        Save($"record {recordKey} Added");
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
