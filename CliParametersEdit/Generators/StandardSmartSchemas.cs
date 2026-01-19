using ParametersManagement.LibFileParameters.Interfaces;
using ParametersManagement.LibFileParameters.Models;
using ParametersManagement.LibParameters;
using SystemTools.SystemToolsShared;

namespace CliParametersEdit.Generators;

public static class StandardSmartSchemas
{
    //public საჭიროა ApAgent პროექტში სამივე კონსტანტისთვის
    public const string HourlySmartSchemaName = "Hourly";
    public const string ReduceSmartSchemaName = "Reduce";
    public const string DailyStandardSmartSchemaName = "DailyStandard";

    public static void Generate(IParametersManager parametersManager)
    {
        var parameters = (IParametersWithSmartSchemas)parametersManager.Parameters;

        //თუ არ არსებობს დაემატოს ჭკვიანი სქემები: DailyStandard, Reduce, Hourly
        CreateSmartSchemaDailyStandard(parameters);
        CreateSmartSchemaReduce(parameters);
        CreateSmartSchemaHourly(parameters);
    }

    private static void CreateSmartSchemaHourly(IParametersWithSmartSchemas parameters)
    {
        if (parameters.SmartSchemas.ContainsKey(HourlySmartSchemaName))
        {
            return;
        }

        var smartSchema = new SmartSchema { LastPreserveCount = 1 };

        const EPeriodType ep = EPeriodType.Hour;
        smartSchema.Details.Add(new SmartSchemaDetail { PeriodType = ep, PreserveCount = 48 });

        parameters.SmartSchemas.Add(HourlySmartSchemaName, smartSchema);
    }

    private static void CreateSmartSchemaReduce(IParametersWithSmartSchemas parameters)
    {
        if (parameters.SmartSchemas.ContainsKey(ReduceSmartSchemaName))
        {
            return;
        }

        var smartSchema = new SmartSchema { LastPreserveCount = 1 };
        for (var ep = EPeriodType.Year; ep < EPeriodType.Hour; ep++)
        {
            smartSchema.Details.Add(new SmartSchemaDetail
            {
                PeriodType = ep, PreserveCount = ep == EPeriodType.Day ? 2 : 1
            });
        }

        parameters.SmartSchemas.Add(ReduceSmartSchemaName, smartSchema);
    }

    private static void CreateSmartSchemaDailyStandard(IParametersWithSmartSchemas parameters)
    {
        if (parameters.SmartSchemas.ContainsKey(DailyStandardSmartSchemaName))
        {
            return;
        }

        var smartSchema = new SmartSchema { LastPreserveCount = 1 };
        for (var ep = EPeriodType.Year; ep < EPeriodType.Hour; ep++)
        {
            smartSchema.Details.Add(new SmartSchemaDetail { PeriodType = ep, PreserveCount = 3 });
        }

        parameters.SmartSchemas.Add(DailyStandardSmartSchemaName, smartSchema);
    }
}
