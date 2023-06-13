using LibFileParameters.Interfaces;
using LibFileParameters.Models;
using LibParameters;
using SystemToolsShared;

namespace CliParametersEdit.Generators;

public sealed class StandardSmartSchemas
{
    public StandardSmartSchemas(string smartSchemaDailyStandardName, string smartSchemaReduceName,
        string smartSchemaHourlyName)
    {
        SmartSchemaDailyStandardName = smartSchemaDailyStandardName;
        SmartSchemaReduceName = smartSchemaReduceName;
        SmartSchemaHourlyName = smartSchemaHourlyName;
    }

    public string SmartSchemaDailyStandardName { get; } //DailyStandard
    public string SmartSchemaReduceName { get; } //Reduce
    public string SmartSchemaHourlyName { get; } //Hourly

    public static StandardSmartSchemas Generate(IParametersManager parametersManager)
    {
        var parameters = (IParametersWithSmartSchemas)parametersManager.Parameters;

        //თუ არ არსებობს დაემატოს ჭკვიანი სქემები: DailyStandard, Reduce, Hourly
        return new StandardSmartSchemas(CreateSmartSchemaDailyStandard(parameters),
            CreateSmartSchemaReduce(parameters), CreateSmartSchemaHourly(parameters));
    }

    private static string CreateSmartSchemaHourly(IParametersWithSmartSchemas parameters)
    {
        const string smartSchemaName = "Hourly";
        if (parameters.SmartSchemas.ContainsKey(smartSchemaName))
            return smartSchemaName;

        var smartSchema = new SmartSchema { LastPreserveCount = 1 };

        var ep = EPeriodType.Hour;
        smartSchema.Details.Add(new SmartSchemaDetail { PeriodType = ep, PreserveCount = 48 });

        parameters.SmartSchemas.Add(smartSchemaName, smartSchema);
        return smartSchemaName;
    }

    private static string CreateSmartSchemaReduce(IParametersWithSmartSchemas parameters)
    {
        const string smartSchemaName = "Reduce";
        if (parameters.SmartSchemas.ContainsKey(smartSchemaName))
            return smartSchemaName;

        var smartSchema = new SmartSchema { LastPreserveCount = 1 };
        for (var ep = EPeriodType.Year; ep < EPeriodType.Hour; ep++)
            smartSchema.Details.Add(
                new SmartSchemaDetail { PeriodType = ep, PreserveCount = ep == EPeriodType.Day ? 2 : 1 });

        parameters.SmartSchemas.Add(smartSchemaName, smartSchema);
        return smartSchemaName;
    }

    private static string CreateSmartSchemaDailyStandard(IParametersWithSmartSchemas parameters)
    {
        const string smartSchemaName = "DailyStandard";
        if (parameters.SmartSchemas.ContainsKey(smartSchemaName))
            return smartSchemaName;

        var smartSchema = new SmartSchema { LastPreserveCount = 1 };
        for (var ep = EPeriodType.Year; ep < EPeriodType.Hour; ep++)
            smartSchema.Details.Add(new SmartSchemaDetail { PeriodType = ep, PreserveCount = 3 });

        parameters.SmartSchemas.Add(smartSchemaName, smartSchema);
        return smartSchemaName;
    }
}