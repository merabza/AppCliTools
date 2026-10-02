using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ParametersManagement.LibParameters;
using SystemTools.SystemToolsShared;

namespace AppCliTools.CliParameters.Cruders;

public abstract class SimpleNamesListCruder : Cruder
{
    private readonly IParametersManager? _parametersManager;

    //ამ კონსტრუქტორით შექმნილი cruder ცვლილებას არ ინახავს, თუ მემკვიდრე Save-ს თვითონ არ გადაფარავს
    protected SimpleNamesListCruder(string crudName, string crudNamePlural, bool fieldKeyFromItem = false,
        bool canEditFieldsInSequence = true) : base(crudName, crudNamePlural, fieldKeyFromItem, canEditFieldsInSequence)
    {
    }

    //სია პარამეტრების ნაწილია, ამიტომ ყოველი ცვლილება parametersManager-ის ძირეული ობიექტის შენახვით სრულდება
    protected SimpleNamesListCruder(IParametersManager parametersManager, string crudName, string crudNamePlural,
        bool fieldKeyFromItem = false, bool canEditFieldsInSequence = true) : this(crudName, crudNamePlural,
        fieldKeyFromItem, canEditFieldsInSequence)
    {
        _parametersManager = parametersManager;
    }

    protected abstract List<string> GetList();

    protected override Dictionary<string, ItemData> GetCrudersDictionary()
    {
        return GetList().ToDictionary(k => k, ItemData (v) => new TextItemData { Text = v });
    }

    protected override ItemData CreateNewItem(string? recordKey, ItemData? defaultItemData)
    {
        return new TextItemData();
    }

    public override bool ContainsRecordWithKey(string recordKey)
    {
        List<string> reactAppTemplateNames = GetList();
        return reactAppTemplateNames.Contains(recordKey);
    }

    protected override ValueTask RemoveRecordWithKey(string recordKey, CancellationToken cancellationToken = default)
    {
        List<string> reactAppTemplateNames = GetList();
        reactAppTemplateNames.Remove(recordKey);
        return ValueTask.CompletedTask;
    }

    protected override ValueTask AddRecordWithKey(string recordKey, ItemData newRecord,
        CancellationToken cancellationToken = default)
    {
        GetList().Add(recordKey);
        return ValueTask.CompletedTask;
    }

    public override string? GetStatusFor(string name)
    {
        return null;
    }

    public override ValueTask<bool> Save(string message, CancellationToken cancellationToken = default)
    {
        return _parametersManager is null
            ? base.Save(message, cancellationToken)
            : _parametersManager.Save(_parametersManager.Parameters, message, null, cancellationToken);
    }
}
