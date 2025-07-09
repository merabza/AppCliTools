using System.Collections.Generic;
using System.Linq;
using CliMenu;
using CliParameters;
using CliParameters.FieldEditors;
using CliParametersEdit.CliMenuCommands;
using CliParametersEdit.FieldEditors;
using LibFileParameters.Interfaces;
using LibFileParameters.Models;
using LibParameters;

namespace CliParametersEdit.Cruders;

public sealed class SmartSchemaCruder : ParCruder
{
    public SmartSchemaCruder(IParametersManager parametersManager) : base(parametersManager, "Smart Schema",
        "Smart Schemas")
    {
        FieldEditors.Add(new IntFieldEditor(nameof(SmartSchema.LastPreserveCount), 1));
        FieldEditors.Add(new SmartSchemaDetailsFieldEditor(nameof(SmartSchema.Details)));
    }

    protected override Dictionary<string, ItemData> GetCrudersDictionary()
    {
        var parameters = (IParametersWithSmartSchemas)ParametersManager.Parameters;
        return parameters.SmartSchemas.ToDictionary(p => p.Key, p => (ItemData)p.Value);
    }

    public override bool ContainsRecordWithKey(string recordKey)
    {
        var parameters = (IParametersWithSmartSchemas)ParametersManager.Parameters;
        var smartSchemas = parameters.SmartSchemas;
        return smartSchemas.ContainsKey(recordKey);
    }

    public override void UpdateRecordWithKey(string recordKey, ItemData newRecord)
    {
        var newFileStorage = (SmartSchema)newRecord;
        var parameters = (IParametersWithSmartSchemas)ParametersManager.Parameters;
        parameters.SmartSchemas[recordKey] = newFileStorage;
    }

    protected override void AddRecordWithKey(string recordKey, ItemData newRecord)
    {
        var newFileStorage = (SmartSchema)newRecord;
        var parameters = (IParametersWithSmartSchemas)ParametersManager.Parameters;
        parameters.SmartSchemas.Add(recordKey, newFileStorage);
    }

    protected override void RemoveRecordWithKey(string recordKey)
    {
        var parameters = (IParametersWithSmartSchemas)ParametersManager.Parameters;
        var smartSchemas = parameters.SmartSchemas;
        smartSchemas.Remove(recordKey);
    }

    protected override ItemData CreateNewItem(ItemData? defaultItemData)
    {
        return new SmartSchema();
    }

    protected override void FillListMenuAdditional(CliMenuSet cruderSubMenuSet)
    {
        GenerateStandardSmartSchemasCliMenuCommand generateCommand = new(ParametersManager);
        cruderSubMenuSet.AddMenuItem(generateCommand);
    }
}