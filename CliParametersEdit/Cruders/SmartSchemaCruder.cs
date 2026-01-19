using System.Collections.Generic;
using CliMenu;
using CliParameters;
using CliParameters.FieldEditors;
using CliParametersEdit.CliMenuCommands;
using CliParametersEdit.FieldEditors;
using ParametersManagement.LibFileParameters.Interfaces;
using ParametersManagement.LibFileParameters.Models;
using ParametersManagement.LibParameters;

namespace CliParametersEdit.Cruders;

public sealed class SmartSchemaCruder : ParCruder<SmartSchema>
{
    //public საჭიროა რეფლექსიით კონსტრუირებისას DictionaryFieldEditor კლასიდან
    // ReSharper disable once MemberCanBePrivate.Global
    public SmartSchemaCruder(IParametersManager parametersManager,
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

    protected override void FillListMenuAdditional(CliMenuSet cruderSubMenuSet)
    {
        var generateCommand = new GenerateStandardSmartSchemasCliMenuCommand(ParametersManager);
        cruderSubMenuSet.AddMenuItem(generateCommand);
    }
}
