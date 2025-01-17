using System;
using System.Collections.Generic;
using System.Linq;
using CodeTools;
using DbContextAnalyzer.Domain;
using DbContextAnalyzer.Models;
using Microsoft.Extensions.Logging;
using SystemToolsShared;

namespace DbContextAnalyzer.CodeCreators;

public sealed class CreatorForJsonFilesCreator : CodeCreator
{
    private readonly ExcludesRulesParametersDomain _excludesRulesParameters;

    private readonly GetJsonCreatorParameters _parameters;
    private CodeBlock? _runMethodCodeBlock;

    // ReSharper disable once ConvertToPrimaryConstructor
    public CreatorForJsonFilesCreator(ILogger logger, GetJsonCreatorParameters getJsonCreatorParameters,
        ExcludesRulesParametersDomain excludesRulesParameters) : base(logger, getJsonCreatorParameters.PlacePath,
        "JsonFilesCreator.cs")
    {
        _parameters = getJsonCreatorParameters;
        _excludesRulesParameters = excludesRulesParameters;
    }

    public override void CreateFileStructure()
    {
        _runMethodCodeBlock = new CodeBlock("public bool Run()",
            $"Console.WriteLine(\"{_parameters.ProjectNamespace} Started\")");

        var block = new CodeBlock(string.Empty, new OneLineComment($"Created by {GetType().Name} at {DateTime.Now}"),
            "using System", "using System.IO", "using System.Linq",
            $"using {_parameters.ProjectNamespace}.{_parameters.ModelsFolderName}",
            $"using {_parameters.DbContextProjectName}", "using Newtonsoft.Json", "using Microsoft.EntityFrameworkCore",
            $"namespace {_parameters.ProjectNamespace}", string.Empty,
            new CodeBlock("public sealed class JsonFilesCreator",
                $"private readonly {_parameters.DbContextClassName} _context",
                "private readonly string _seederCreatorJsonFolderName", string.Empty,
                new OneLineComment(" ReSharper disable once ConvertToPrimaryConstructor"),
                new CodeBlock(
                    $"public JsonFilesCreator({_parameters.DbContextClassName} context, string seederCreatorJsonFolderName)",
                    "_context = context", "_seederCreatorJsonFolderName = seederCreatorJsonFolderName"),
                new CodeBlock("private void SaveJson(object obj, string name)",
                    "var sampleParamsJsonText = JsonConvert.SerializeObject(obj, Formatting.Indented)",
                    "var fileName = $\"{name}.json\"",
                    "File.WriteAllText(Path.Combine(_seederCreatorJsonFolderName, fileName), sampleParamsJsonText)",
                    "Console.WriteLine($\"{fileName} created\")"), _runMethodCodeBlock));
        CodeFile.AddRange(block.CodeItems);
    }

    private static string GetIncludes(FieldData fieldData, int level = 0)
    {
        if (fieldData.SubstituteField == null || fieldData.SubstituteField.Fields.Count == 0)
            return string.Empty;

        var res = fieldData.SubstituteField.Fields.Select(field =>
                $".{(level > 0 ? "Then" : string.Empty)}Include(i{level}=>i{level}.{fieldData.NavigationFieldName}){GetIncludes(field, level + 1)}")
            .ToList();

        if (res.Count <= 1)
            return res.Aggregate(string.Empty, (current, includeString) => current + includeString);

        res.Sort();

        var changed = true;
        while (changed)
        {
            changed = false;
            string? toRemove = null;
            foreach (var includeString in res.Where(includeString =>
                         res.Any(w => w != includeString && w.StartsWith(includeString)) ||
                         res.Count(w => w == includeString) > 1))
                toRemove = includeString;

            if (toRemove == null)
                continue;

            var index = res.FindIndex(f => f == toRemove);
            if (index < 0)
                continue;
            res.RemoveAt(index);
            changed = true;
        }

        return res.Aggregate(string.Empty, (current, includeString) => current + includeString);
    }


    private static Dictionary<string, FieldData[]> GetFields(FieldData fieldData)
    {
        if (fieldData.SubstituteField?.Fields is null || !(fieldData.SubstituteField?.Fields.Count > 0))
            return new Dictionary<string, FieldData[]> { { fieldData.FullName, [fieldData] } };
        var dictionary = new Dictionary<string, FieldData[]>();
        foreach (var fields in fieldData.SubstituteField.Fields.Select(GetFields))
        foreach (var pair in fields)
        {
            var tempList = new List<FieldData> { fieldData };
            tempList.AddRange(pair.Value);
            dictionary.Add(pair.Key, [.. tempList]);
        }

        return dictionary;
    }

    public void UseEntity(EntityData entityData)
    {
        var tableName = entityData.TableName;
        var tableNameCapitalCamel = tableName.CapitalizeCamel();
        var tableNameSingular =
            GetTableNameSingularCapitalizeCamel(_excludesRulesParameters.SingularityExceptions, tableName);
        var seederModelClassName = tableNameSingular + "SeederModel";

        var includes = entityData.FieldsData.Aggregate(string.Empty, (current, field) => current + GetIncludes(field));

        var strFieldsList = string.Empty;
        var atLeastOneAdded = false;

        foreach (var (_, fieldList) in entityData.FieldsData.SelectMany(GetFields))
        {
            if (atLeastOneAdded)
                strFieldsList += ", ";

            var rightPart = FieldName(fieldList, fieldList.Length);

            if (fieldList.Length > 1)
                for (var i = fieldList.Length - 2; i >= 0; i--)
                    if (fieldList[i].IsNullable)
                        rightPart = $"{FieldName(fieldList, i + 1)} == null ? null : ({rightPart})";
            strFieldsList += rightPart;

            atLeastOneAdded = true;
        }


        var tableVarName = tableName.UnCapitalize();
        var block = new CodeBlock(string.Empty, string.Empty,
            $"Console.WriteLine(\"Working on {tableNameCapitalCamel}\")", string.Empty,
            $"var {tableVarName} = _context.{tableNameCapitalCamel}{includes}.Select(s => new {seederModelClassName} ({strFieldsList})).ToList()",
            $"SaveJson({tableVarName}, \"{tableNameCapitalCamel}\")");

        if (_runMethodCodeBlock is null)
            throw new Exception("_runMethodCodeBlock is null");
        _runMethodCodeBlock.AddRange(block.CodeItems);
    }


    private static string FieldName(FieldData[] list, int levelCount)
    {
        var rightPart = "s";

        var needCount = Math.Min(list.Length, levelCount);

        for (var i = 0; i < needCount; i++)
            rightPart += $".{(i == list.Length - 1 ? list[i].OldName : list[i].NavigationFieldName)}";
        return rightPart;
    }

    public override void FinishAndSave()
    {
        var block = new CodeBlock(string.Empty, string.Empty, "Console.WriteLine(\"DataSeederCreator.Run Finished\")",
            "return true");
        if (_runMethodCodeBlock is null)
            throw new Exception("_runMethodCodeBlock is null");
        _runMethodCodeBlock.AddRange(block.CodeItems);
        base.FinishAndSave();
    }
}