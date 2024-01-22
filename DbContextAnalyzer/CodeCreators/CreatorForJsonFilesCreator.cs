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
        _runMethodCodeBlock =
            new CodeBlock("public bool Run()", $"Console.WriteLine(\"{_parameters.ProjectNamespace} Started\")");

        var block = new CodeBlock("",
            new OneLineComment($"Created by {GetType().Name} at {DateTime.Now}"),
            "using System",
            "using System.Collections.Generic",
            "using System.IO",
            "using System.Linq",
            $"using {_parameters.ProjectNamespace}.{_parameters.ModelsFolderName}",
            $"using {_parameters.DbContextProjectName}",
            "using Newtonsoft.Json",
            "using Microsoft.EntityFrameworkCore",
            $"namespace {_parameters.ProjectNamespace}",
            "",
            new CodeBlock("public sealed class JsonFilesCreator",
                $"private readonly {_parameters.DbContextClassName} _context",
                "private readonly string _seederCreatorJsonFolderName",
                "",
                new OneLineComment(" ReSharper disable once ConvertToPrimaryConstructor"),
                new CodeBlock(
                    $"public JsonFilesCreator({_parameters.DbContextClassName} context, string seederCreatorJsonFolderName)",
                    "_context = context",
                    "_seederCreatorJsonFolderName = seederCreatorJsonFolderName"),
                new CodeBlock("private void SaveJson(object obj, string name)",
                    "var sampleParamsJsonText = JsonConvert.SerializeObject(obj, Formatting.Indented)",
                    "var fileName = $\"{name}.json\"",
                    "File.WriteAllText(Path.Combine(_seederCreatorJsonFolderName, fileName), sampleParamsJsonText)",
                    "Console.WriteLine($\"{fileName} created\")"),
                _runMethodCodeBlock));
        CodeFile.AddRange(block.CodeItems);
    }

    private static string GetIncludes(FieldData fieldData, int level = 0)
    {
        if (fieldData.SubstituteField == null || fieldData.SubstituteField.Fields.Count == 0)
            return "";

        //if (level > 0)
        //{
        //  string result = "";
        //  foreach (FieldData field in fieldData.SubstituteField.Fields)
        //    result = result +
        //             $".{(level > 0 ? "Then" : "")}Include(i{level}=>i{level}.{fieldData.NavigationFieldName}){GetIncludes(field, level + 1)}";
        //  return result;
        //}

        var res = fieldData.SubstituteField.Fields.Select(field =>
                $".{(level > 0 ? "Then" : "")}Include(i{level}=>i{level}.{fieldData.NavigationFieldName}){GetIncludes(field, level + 1)}")
            .ToList();

        if (res.Count > 1)
        {
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
        }

        return res.Aggregate("", (current, includeString) => current + includeString);
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


    //private static Dictionary<string, string> GetFieldNames(FieldData fieldData)
    //{

    //    if (fieldData.SubstituteField?.Fields == null)
    //        return new Dictionary<string, string> { { fieldData.FullName, fieldData.OldName } };

    //    Dictionary<string, string> dictionary = new Dictionary<string, string>();
    //    foreach (Dictionary<string, string> fieldNames in fieldData.SubstituteField.Fields.Select(GetFieldNames))
    //    {
    //        foreach (KeyValuePair<string, string> pair in fieldNames)
    //        {
    //            var newField = $"s.{fieldData.NavigationFieldName}.{pair.Value}";
    //            if (fieldData.IsNullable)
    //            {
    //                newField =
    //                    $"s.{fieldData.NavigationFieldName} == null ? null : s.{fieldData.NavigationFieldName}.{pair.Value}";
    //            }
    //            dictionary.Add(pair.Key, newField);
    //        }
    //    }
    //    return dictionary;
    //}


    public override void UseEntity(EntityData entityData, bool isCarcassType)
    {
        var tableName = entityData.TableName;
        var tableNameCapitalCamel = tableName.CapitalizeCamel();
        var tableNameSingular =
            GetTableNameSingularCapitalizeCamel(_excludesRulesParameters.SingularityExceptions, tableName);
        var seederModelClassName = tableNameSingular + "SeederModel";

        var includes = entityData.FieldsData.Aggregate("", (current, field) => current + GetIncludes(field));


        //string strFieldsList = string.Join(", ", entityData.GetFlatFieldData().Select(p => $"{p.FullName} = s.{p.FullName}"));

        var strFieldsList = "";
        var atLeastOneAdded = false;
        //foreach (var kvp in entityData.FieldsData.SelectMany(GetFieldNames))
        //{
        //    if (atLeastOneAdded)
        //        strFieldsList += ", ";
        //    strFieldsList += $"{kvp.Key} = s.{kvp.Value}";
        //    atLeastOneAdded = true;
        //}

        foreach (var (_, fieldList) in entityData.FieldsData.SelectMany(GetFields))
        {
            if (atLeastOneAdded)
                strFieldsList += ", ";

            var rightPart = FieldName(fieldList, fieldList.Length);

            if (fieldList.Length > 1)
                for (var i = fieldList.Length - 2; i >= 0; i--)
                    if (fieldList[i].IsNullable)
                        rightPart = $"{FieldName(fieldList, i + 1)} == null ? null : ({rightPart})";
            //strFieldsList += $"{key} = {rightPart}";
            strFieldsList += rightPart;

            atLeastOneAdded = true;
        }


        var block = new CodeBlock("",
            "",
            $"Console.WriteLine(\"Working on {tableNameCapitalCamel}\")",
            "",
            $"var {tableName} = _context.{tableNameCapitalCamel}{includes}.Select(s => new {seederModelClassName} ({strFieldsList})).ToList()",
            $"SaveJson({tableName}, \"{tableNameCapitalCamel}\")");

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

    //public override void UseEntity(IEntityType entityType, bool isCarcassType, List<string> ignoreFields)
    //{
    //  (_, List<IProperty> props) = GetFieldsProperties(entityType, ignoreFields);

    //  string tableName = entityType.GetTableName();
    //  string tableNameCapitalCamel = tableName.CapitalizeCamel();
    //  string tableNameSingular = GetTableNameSingular(_excludesRulesParameters.SingularityExceptions, tableName);
    //  string seederModelClassName = tableNameSingular + "SeederModel";

    //  string strFieldsList = string.Join(",", props.Select(p => $"{p.Name} = s.{p.Name}"));
    //  CodeBlock block = new CodeBlock("",
    //    "",
    //    $"Console.WriteLine(\"Working on {tableNameCapitalCamel}\")",
    //    "",
    //    $"List<{seederModelClassName}> {tableName} = _context.{tableNameCapitalCamel}.Select(s => new {seederModelClassName} {{{strFieldsList}}}).ToList()",
    //    $"SaveJson({tableName}, \"{tableNameCapitalCamel}\")");

    //  _runMethodCodeBlock.AddRange(block.CodeItems);
    //  //Logger.LogInformation($"used table -> {tableName}");
    //}


    public override void FinishAndSave()
    {
        var block = new CodeBlock("", "", "Console.WriteLine(\"DataSeederCreator.Run Finished\")",
            "return true");
        if (_runMethodCodeBlock is null)
            throw new Exception("_runMethodCodeBlock is null");
        _runMethodCodeBlock.AddRange(block.CodeItems);
        CreateFile();
    }
}