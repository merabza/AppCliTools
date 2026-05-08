using System;
using ParametersManagement.LibFileParameters.Interfaces;
using ParametersManagement.LibFileParameters.Models;
using ParametersManagement.LibParameters;
using Polly;

namespace AppCliTools.CliParametersEdit.Generators;

public static class StandardRetryStrategyParameters
{
    //public საჭიროა Replicator პროექტში სამივე კონსტანტისთვის
    public const string StandardRetryStrategyParametersName = "Standard";

    public static void Generate(IParametersManager parametersManager)
    {
        var parameters = (IParametersWithRetryStrategyParameters)parametersManager.Parameters;

        //თუ არ არსებობს დაემატოს ჭკვიანი სქემები: DailyStandard, Reduce, Hourly
        CreateRetryStrategyParametersStandard(parameters);
    }

    private static void CreateRetryStrategyParametersStandard(IParametersWithRetryStrategyParameters parameters)
    {
        if (parameters.RetryStrategyParameters.ContainsKey(StandardRetryStrategyParametersName))
        {
            return;
        }

        var retryStrategyParameters = new RetryStrategyParameters
        {
            MaxRetryAttempts = 3,
            BackoffType = DelayBackoffType.Exponential,
            Delay = TimeSpan.FromSeconds(2),
            MaxDelay = TimeSpan.FromMinutes(1),
            UseJitter = true
        };

        parameters.RetryStrategyParameters.Add(StandardRetryStrategyParametersName, retryStrategyParameters);
    }
}
