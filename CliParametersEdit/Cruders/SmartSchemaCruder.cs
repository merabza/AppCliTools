using System.Collections.Generic;
using CliMenu;
using CliParameters;
using CliParameters.FieldEditors;
using CliParametersEdit.CliMenuCommands;
using CliParametersEdit.FieldEditors;
using LibFileParameters.Interfaces;
using LibFileParameters.Models;
using LibParameters;

namespace CliParametersEdit.Cruders;

public sealed class SmartSchemaCruder : ParCruder<SmartSchema>
{
    private SmartSchemaCruder(IParametersManager parametersManager,
        Dictionary<string, SmartSchema> currentValuesDictionary) : base(parametersManager, currentValuesDictionary,
        "Smart Schema", "Smart Schemas")
    {
        FieldEditors.Add(new IntFieldEditor(nameof(SmartSchema.LastPreserveCount), 1));
        FieldEditors.Add(new SmartSchemaDetailsFieldEditor(nameof(SmartSchema.Details)));
    }

    public static SmartSchemaCruder Create(IParametersManager parametersManager)
    {
        var parameters = (IParametersWithSmartSchemas)parametersManager.Parameters;
        return new SmartSchemaCruder(parametersManager, parameters.SmartSchemas);
    }

    //protected override Dictionary<string, ItemData> GetCrudersDictionary()
    //{
    //    var parameters = (IParametersWithSmartSchemas)ParametersManager.Parameters;
    //    return parameters.SmartSchemas.ToDictionary(p => p.Key, p => (ItemData)p.Value);
    //}

    //public override bool ContainsRecordWithKey(string recordKey)
    //{
    //    var parameters = (IParametersWithSmartSchemas)ParametersManager.Parameters;
    //    var smartSchemas = parameters.SmartSchemas;
    //    return smartSchemas.ContainsKey(recordKey);
    //}

    //public override void UpdateRecordWithKey(string recordKey, ItemData newRecord)
    //{
    //    var newFileStorage = (SmartSchema)newRecord;
    //    var parameters = (IParametersWithSmartSchemas)ParametersManager.Parameters;
    //    parameters.SmartSchemas[recordKey] = newFileStorage;
    //}

    //protected override void AddRecordWithKey(string recordKey, ItemData newRecord)
    //{
    //    var newFileStorage = (SmartSchema)newRecord;
    //    var parameters = (IParametersWithSmartSchemas)ParametersManager.Parameters;
    //    parameters.SmartSchemas.Add(recordKey, newFileStorage);
    //}

    //protected override void RemoveRecordWithKey(string recordKey)
    //{
    //    var parameters = (IParametersWithSmartSchemas)ParametersManager.Parameters;
    //    var smartSchemas = parameters.SmartSchemas;
    //    smartSchemas.Remove(recordKey);
    //}

    //protected override ItemData CreateNewItem(string? recordKey, ItemData? defaultItemData)
    //{
    //    return new SmartSchema();
    //}

    protected override void FillListMenuAdditional(CliMenuSet cruderSubMenuSet)
    {
        var generateCommand = new GenerateStandardSmartSchemasCliMenuCommand(ParametersManager);
        cruderSubMenuSet.AddMenuItem(generateCommand);
    }
}