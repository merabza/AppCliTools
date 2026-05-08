using System;
using System.Collections.Generic;
using AppCliTools.CliMenu;
using AppCliTools.CliParameters;
using AppCliTools.CliParameters.FieldEditors;
using AppCliTools.CliParametersEdit.CliMenuCommands;
using ParametersManagement.LibFileParameters.Interfaces;
using ParametersManagement.LibFileParameters.Models;
using ParametersManagement.LibParameters;
using Polly;

namespace AppCliTools.CliParametersEdit.Cruders;

public sealed class RetryStrategyParametersCruder : ParCruder<RetryStrategyParameters>
{
    //public საჭიროა რეფლექსიით კონსტრუირებისას DictionaryFieldEditor კლასიდან
    // ReSharper disable once MemberCanBePrivate.Global
    public RetryStrategyParametersCruder(IParametersManager parametersManager,
        Dictionary<string, RetryStrategyParameters> currentValuesDictionary) : base(parametersManager,
        currentValuesDictionary, "Smart Schema", "Smart Schemas")
    {
        FieldEditors.Add(new IntFieldEditor(nameof(RetryStrategyParameters.MaxRetryAttempts), 3));
        FieldEditors.Add(new EnumFieldEditor<DelayBackoffType>(nameof(RetryStrategyParameters.BackoffType),
            DelayBackoffType.Constant));
        FieldEditors.Add(new BoolFieldEditor(nameof(RetryStrategyParameters.UseJitter)));
        FieldEditors.Add(new TimeSpanFieldEditor(nameof(RetryStrategyParameters.Delay), new TimeSpan(0, 0, 3)));
        FieldEditors.Add(new TimeSpanFieldEditor(nameof(RetryStrategyParameters.MaxDelay), new TimeSpan(0, 1, 0)));
    }

    public static RetryStrategyParametersCruder Create(IParametersManager parametersManager)
    {
        var parameters = (IParametersWithRetryStrategyParameters)parametersManager.Parameters;
        return new RetryStrategyParametersCruder(parametersManager, parameters.RetryStrategyParameters);
    }

    protected override void FillListMenuAdditional(CliMenuSet cruderSubMenuSet)
    {
        var generateCommand = new GenerateStandardRetryStrategyParametersCliMenuCommand(ParametersManager);
        cruderSubMenuSet.AddMenuItem(generateCommand);
    }
}
