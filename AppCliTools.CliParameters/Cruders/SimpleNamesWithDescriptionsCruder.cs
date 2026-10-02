using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AppCliTools.CliParameters.FieldEditors;
using ParametersManagement.LibParameters;
using SystemTools.SystemToolsShared;

namespace AppCliTools.CliParameters.Cruders;

public abstract class SimpleNamesWithDescriptionsCruder : Cruder
{
    private readonly IParametersManager? _parametersManager;

    //ამ კონსტრუქტორით შექმნილი cruder ცვლილებას არ ინახავს, თუ მემკვიდრე Save-ს თვითონ არ გადაფარავს
    protected SimpleNamesWithDescriptionsCruder(string crudName, string crudNamePlural,
        string descriptionFieldRealName = "Description") : base(crudName, crudNamePlural)
    {
        FieldEditors.Add(new OptionalTextFieldEditor(nameof(TextItemData.Text), true, descriptionFieldRealName));
    }

    //ლექსიკონი პარამეტრების ნაწილია, ამიტომ ყოველი ცვლილება parametersManager-ის ძირეული ობიექტის შენახვით სრულდება
    protected SimpleNamesWithDescriptionsCruder(IParametersManager parametersManager, string crudName,
        string crudNamePlural, string descriptionFieldRealName = "Description") : this(crudName, crudNamePlural,
        descriptionFieldRealName)
    {
        _parametersManager = parametersManager;
    }

    protected abstract Dictionary<string, string> GetDictionary();

    protected override Dictionary<string, ItemData> GetCrudersDictionary()
    {
        return GetDictionary().ToDictionary(k => k.Key, ItemData (v) => new TextItemData { Text = v.Value });
    }

    protected override ItemData CreateNewItem(string? recordKey, ItemData? defaultItemData)
    {
        return new TextItemData();
    }

    public override bool ContainsRecordWithKey(string recordKey)
    {
        Dictionary<string, string> reactAppTemplateNames = GetDictionary();
        return reactAppTemplateNames.ContainsKey(recordKey);
    }

    protected override ValueTask RemoveRecordWithKey(string recordKey, CancellationToken cancellationToken = default)
    {
        Dictionary<string, string> reactAppTemplateNames = GetDictionary();
        reactAppTemplateNames.Remove(recordKey);
        return ValueTask.CompletedTask;
    }

    public override ValueTask UpdateRecordWithKey(string recordKey, ItemData newRecord,
        CancellationToken cancellationToken = default)
    {
        if (newRecord is not TextItemData newTextItemData)
        {
            throw new Exception("newRecord is null in UpdateRecordWithKey");
        }

        if (string.IsNullOrWhiteSpace(newTextItemData.Text))
        {
            throw new Exception("newReactAppType.Description is empty in UpdateRecordWithKey");
        }

        GetDictionary()[recordKey] = newTextItemData.Text;
        return ValueTask.CompletedTask;
    }

    protected override ValueTask AddRecordWithKey(string recordKey, ItemData newRecord,
        CancellationToken cancellationToken = default)
    {
        if (newRecord is not TextItemData newTextItemData)
        {
            throw new Exception("newRecord is null in AddRecordWithKey");
        }

        if (string.IsNullOrWhiteSpace(newTextItemData.Text))
        {
            throw new Exception("Description is empty in AddRecordWithKey");
        }

        GetDictionary().Add(recordKey, newTextItemData.Text);
        return ValueTask.CompletedTask;
    }

    public override string? GetStatusFor(string name)
    {
        if (GetDictionary().TryGetValue(name, out string? description) && !string.IsNullOrWhiteSpace(description))
        {
            return description;
        }

        return null;
    }

    public override ValueTask<bool> Save(string message, CancellationToken cancellationToken = default)
    {
        return _parametersManager is null
            ? base.Save(message, cancellationToken)
            : _parametersManager.Save(_parametersManager.Parameters, message, null, cancellationToken);
    }
}
