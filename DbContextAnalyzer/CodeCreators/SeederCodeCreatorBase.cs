using System;
using System.Collections.Generic;
using System.Linq;
using CodeTools;
using DbContextAnalyzer.Domain;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;
using SystemToolsShared;

namespace DbContextAnalyzer.CodeCreators;

public class SeederCodeCreatorBase : CodeCreator
{
    private readonly ExcludesRulesParametersDomain _excludesRulesParameters;

    protected SeederCodeCreatorBase(ILogger logger, ExcludesRulesParametersDomain excludesRulesParameters,
        string placePath, string? codeFileName = null) : base(logger, placePath, codeFileName)
    {
        _excludesRulesParameters = excludesRulesParameters;
    }

    protected string GetTableName(string tableName)
    {
        return _excludesRulesParameters.GetReplaceTablesName(tableName);
    }

    private string GetTableNameSingular(string tableName)
    {
        if (_excludesRulesParameters.SingularityExceptions.TryGetValue(tableName, out var singular))
            return singular;
        var unCapTableName = tableName.UnCapitalize();
        return _excludesRulesParameters.SingularityExceptions.TryGetValue(unCapTableName, out var exception)
            ? exception
            : tableName.Singularize();
    }

    protected string GetTableNameSingularCapitalizeCamel(string tableName)
    {
        return GetTableNameSingular(tableName).CapitalizeCamel();
    }

    protected static (bool, List<IProperty>) GetFieldsProperties(IEntityType entityType, List<string> ignoreFields)
    {
        var props = entityType.GetProperties()
            .Where(p => p.ValueGenerated == ValueGenerated.Never && !ignoreFields.Contains(p.Name)).ToList();

        if (props.Count > 0)
            return (false, props);

        //თუ ცხრილის ველები ყველა ავტომატურად გენერირდება, მაშინ ავიღოთ მხოლოდ ძირითადი გასაღების ველები და ის გავიტანოთ json-ში
        //და ის დაგვჭირდება SeederModel-ში
        props = entityType.GetKeys().SelectMany(s => s.Properties).ToList();
        if (props.Count != 1) throw new Exception("გასაღები ზუსტად ერთი უნდა იყოს. სხვა ვარიანტები ჯერ არ განიხილება");

        return (true, props);
    }
}