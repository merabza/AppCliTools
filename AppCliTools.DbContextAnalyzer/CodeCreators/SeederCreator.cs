using System;
using System.Collections.Generic;
using System.Linq;
using AppCliTools.CodeTools;
using AppCliTools.DbContextAnalyzer.Domain;
using AppCliTools.DbContextAnalyzer.Models;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;
using SystemTools.SystemToolsShared;

namespace AppCliTools.DbContextAnalyzer.CodeCreators;

public sealed class SeederCreator : SeederCodeCreatorBase
{
    private readonly ExcludesRulesParametersDomain _excludesRulesParameters;
    private readonly SeederCodeCreatorParameters _parameters;

    // ReSharper disable once ConvertToPrimaryConstructor
    public SeederCreator(ILogger logger, SeederCodeCreatorParameters parameters,
        ExcludesRulesParametersDomain excludesRulesParameters, string placePath) : base(logger, excludesRulesParameters,
        placePath)
    {
        _parameters = parameters;
        _excludesRulesParameters = excludesRulesParameters;
    }

    private string GetRightValue(FieldData fieldData, bool devFieldIsNullable)
    {
        if (fieldData.SubstituteField is null)
        {
            return
                $"s.{fieldData.FullName}{(!devFieldIsNullable && fieldData is { IsValueType: true, IsNullable: true } ? ".Value" : string.Empty)}";
        }

        string substituteTableNameCapitalCamel =
            GetTableNameSingularCapitalizeCamel(GetNewTableName(fieldData.SubstituteField.TableName));

        string realTypeName = GetRealTypeNameForMethodName(fieldData);
        string dotValue = devFieldIsNullable ? string.Empty : ".Value";

        if (fieldData.SubstituteField.Fields.Count == 0)
        {
            return fieldData.IsNullableByParents
                ? $"tempData.Get{realTypeName}NullableIdByOldId<{substituteTableNameCapitalCamel}>(s.{fieldData.FullName}){dotValue}"
                : $"tempData.Get{realTypeName}IdByOldId<{substituteTableNameCapitalCamel}>(s.{fieldData.FullName})";
        }

        string keyParametersList = string.Join(", ",
            fieldData.SubstituteField.Fields.Select(s => GetRightValue(s, fieldData.IsNullableByParents)));
        return fieldData.IsNullableByParents
            ? $"tempData.Get{realTypeName}NullableIdByKey<{substituteTableNameCapitalCamel}>({keyParametersList}){dotValue}"
            : $"tempData.Get{realTypeName}IdByKey<{substituteTableNameCapitalCamel}>({keyParametersList})";
    }

    private static string GetRealTypeNameForDictionaryGeneric(FieldData? fieldData)
    {
        if (fieldData is null)
        {
            return "int";
        }

        string? realTypeName = fieldData.RealTypeName;
        if (realTypeName.EndsWith('?'))
        {
            realTypeName = realTypeName[..^1];
        }

        switch (realTypeName.ToUpperInvariant())
        {
            case "INT":
                return "int";
            case "DATETIME":
                return "DateTime";
            default:
                realTypeName = realTypeName.UnCapitalize();
                return realTypeName;
        }
    }

    private static string GetRealTypeNameForMethodName(FieldData? fieldData)
    {
        if (fieldData is null)
        {
            return "Int";
        }

        string? realTypeName = fieldData.RealTypeName;
        if (realTypeName.EndsWith('?'))
        {
            realTypeName = realTypeName[..^1];
        }

        realTypeName = realTypeName.Capitalize();
        return realTypeName;
    }

    public string UseEntity(EntityData entityData, EntityData? entityDataForDevBase, bool isCarcassType)
    {
        //var usedList = false;
        string tableName = GetNewTableName(entityData.TableName);

        //tableName.ToLower() == "MorphemeRanges"
        Console.WriteLine("UseEntity tableName = {0}", tableName);

        Dictionary<string, string> replaceFieldsDict =
            _excludesRulesParameters.GetReplaceFieldsDictByTableName(tableName);

        string tableNameCapitalCamel = tableName.CapitalizeCamel();
        string tableNameCamel = tableName.Camelize();

        string tableNameSingular = GetTableNameSingularCapitalizeCamel(tableName);
        string className = (isCarcassType ? _parameters.ProjectPrefixShort : string.Empty) + tableNameCapitalCamel +
                           "Seeder";
        string seederModelClassName = tableNameSingular + "SeederModel";
        string baseClassName = isCarcassType
            ? tableNameCapitalCamel + "Seeder"
            : $"{_parameters.DataSeederBaseClassName}<{tableNameSingular}, {seederModelClassName}>";
        string seedDataObjectName = tableName.UnCapitalize() + "SeedData";
        string prPref = isCarcassType ? string.Empty : _parameters.ProjectPrefixShort;

        bool isIdentity = tableName.ToUpperInvariant() is "ROLES" or "USERS";
        bool isDataTypesOrManyToManyJoins = tableName.ToUpperInvariant() is "DATATYPES" or "MANYTOMANYJOINS";

        string additionalParameters = tableName.ToUpperInvariant() switch
        {
            "DATATYPES" => "ICarcassDataSeederRepository carcassRepo, ",
            "MANYTOMANYJOINS" => "string secretDataFolder, ICarcassDataSeederRepository carcassRepo, ",
            "ROLES" => "RoleManager<AppRole> roleManager, string secretDataFolder, ",
            "USERS" => "UserManager<AppUser> userManager, string secretDataFolder, ",
            _ => string.Empty
        };

        string additionalParameters2 = tableName.ToUpperInvariant() switch
        {
            "DATATYPES" => "carcassRepo, ",
            "MANYTOMANYJOINS" => "secretDataFolder, carcassRepo, ",
            "ROLES" => "roleManager, secretDataFolder, ",
            "USERS" => "userManager, secretDataFolder, ",
            _ => string.Empty
        };

        CodeBlock? setParentsMethod = null;
        CodeBlock? adaptMethod = null;
        CodeBlock? additionalCheckMethod = null;
        var additionalCheckMethodHeader =
            new CodeBlock(
                $"public override bool AdditionalCheck(List<{seederModelClassName}> jsonData, List<{tableNameSingular}> savedData)");
        CodeBlock? createMethod = null;

        bool atLeastOneSubstitute = false;
        foreach (FieldData w in entityData.FieldsData.Where(w => entityData.SelfRecursiveFields.Count == 0 ||
                                                                 !entityData.SelfRecursiveFields.Select(x => x.Name)
                                                                     .Contains(w.Name)))
        {
            if (w.SubstituteField == null)
            {
                continue;
            }

            atLeastOneSubstitute = true;
            break;
        }

        string primaryKeyFieldNewName =
            _excludesRulesParameters.GetNewFieldName(tableName, entityData.PrimaryKeyFieldName);
        FieldData? keyFieldData = entityData.FieldsData.FirstOrDefault(f => f.Name == primaryKeyFieldNewName);
        string keyRealTypeName = GetRealTypeNameForMethodName(keyFieldData);
        string keyRealTypeNameForDictionaryGeneric = GetRealTypeNameForDictionaryGeneric(keyFieldData);

        if (!isCarcassType)
        {
            if (entityData.NeedsToCreateTempData)
            {
                FlatCodeBlock flatCodeBlockForAdditionalCheckMethod;
                if (entityData.SelfRecursiveFields.Count > 0)
                {
                    string seederModelObjectName = seederModelClassName.UnCapitalize();

                    List<FlatCodeBlock> flatCodeBlocks = entityData.SelfRecursiveFields.Select(s =>
                            new FlatCodeBlock(
                                $"var idsDict = _tempData.ToDictionary(k => k.Key, v => v.Value.{entityData.PrimaryKeyFieldName})",
                                $"DataSeederTempData.Instance.SaveOld{keyRealTypeName}IdsDictTo{keyRealTypeName}Ids<{tableNameSingular}>(idsDict)",
                                new CodeBlock(
                                    $"foreach (var {seederModelObjectName} in jsonData.Where(w => w.{s.Name} != null))",
                                    $"_tempData[{seederModelObjectName}.{entityData.PrimaryKeyFieldName}].{s.Name} = idsDict[{seederModelObjectName}.{s.Name}!.Value]")))
                        .ToList();

                    flatCodeBlockForAdditionalCheckMethod = flatCodeBlocks[0];
                    for (int i = 1; i < flatCodeBlocks.Count; i++)
                    {
                        flatCodeBlockForAdditionalCheckMethod.Add(flatCodeBlocks[i]);
                    }

                    flatCodeBlockForAdditionalCheckMethod.Add(new FlatCodeBlock("return true"));
                }
                else
                {
                    flatCodeBlockForAdditionalCheckMethod = new FlatCodeBlock(
                        $"DataSeederTempData.Instance.SaveOld{keyRealTypeName}IdsDictTo{keyRealTypeName}Ids<{tableNameSingular}>(_tempData.ToDictionary(k=>k.Key, v=>v.Value.{primaryKeyFieldNewName}))",
                        "return true");
                }

                additionalCheckMethod = additionalCheckMethodHeader;
                additionalCheckMethod.AddRange(flatCodeBlockForAdditionalCheckMethod.CodeItems);
            }
            else if (entityData.OptimalIndexProperties.Count > 0)
            {
                List<FieldData> optimalIndexFieldsData = entityData.OptimalIndexProperties
                    .Select(prop => entityData.FieldsData.SingleOrDefault(ss => ss.OldName == prop.Name))
                    .OfType<FieldData>().ToList();

                string tupTypeList = string.Join(", ", optimalIndexFieldsData.Select(s => s.RealTypeName));
                string keyFieldsList = string.Join(", ", optimalIndexFieldsData.Select(s => $"k.{s.Name}"));
                string keyFields = optimalIndexFieldsData.Count == 1
                    ? $"k.{GetPreferredFieldName(replaceFieldsDict, optimalIndexFieldsData[0].Name)}"
                    : $" new Tuple<{tupTypeList}>({keyFieldsList})";
                FlatCodeBlock flatCodeBlockForAdditionalCheckMethod;
                if (entityData.SelfRecursiveFields.Count > 0)
                {
                    flatCodeBlockForAdditionalCheckMethod = new FlatCodeBlock(
                        $"DataSeederTempData.Instance.SaveIntIdKeys<{tableNameSingular}>(savedData.ToDictionary(k=>{keyFields}, v=>v.{entityData.PrimaryKeyFieldName}))",
                        new CodeBlock($"if (!SetParents({seedDataObjectName}, {tableNameCamel}List))", "return false"),
                        "return true");
                    string seederModelObjectName = seederModelClassName.UnCapitalize();
                    string keyFieldName = optimalIndexFieldsData[0].Name;

                    setParentsMethod = new CodeBlock(
                        $"private bool SetParents(List<{seederModelClassName}> {seedDataObjectName}, List<{tableNameSingular}> {tableNameCamel}List)",
                        "var tempData = DataSeederTempData.Instance",
                        $"var forUpdate = new List<{tableNameSingular}>()");

                    foreach (FieldData entityDataSelfRecursiveField in entityData.SelfRecursiveFields)
                    {
                        if (entityDataSelfRecursiveField.SubstituteField is null ||
                            entityDataSelfRecursiveField.SubstituteField.Fields.Count == 0)
                        {
                            throw new Exception(
                                "entityData.SelfRecursiveField.SubstituteField is null or without fields");
                        }

                        setParentsMethod.Add(new CodeBlock(
                            $"foreach ({seederModelClassName} {seederModelObjectName} in {seedDataObjectName}.Where(w => w.{entityDataSelfRecursiveField.SubstituteField.Fields[0].FullName} != null))",
                            $"{tableNameSingular} oneRec = {tableNameCamel}List.SingleOrDefault(s => s.{keyFieldName} == {seederModelObjectName}.{keyFieldName})",
                            new CodeBlock("if (oneRec == null)", "continue"),
                            $"oneRec.{entityDataSelfRecursiveField.Name} = tempData.GetIntIdByKey<{tableNameSingular}>({seederModelObjectName}.{entityDataSelfRecursiveField.SubstituteField.Fields[0].FullName})",
                            "forUpdate.Add(oneRec)"));
                    }

                    setParentsMethod.Add(new CodeBlock($"if (!{prPref}Repo.SetUpdates(forUpdate))", "return false"));
                    setParentsMethod.Add(new FlatCodeBlock("return true"));
                }
                else
                {
                    flatCodeBlockForAdditionalCheckMethod = new FlatCodeBlock(
                        $"DataSeederTempData.Instance.SaveIntIdKeys<{tableNameSingular}>(savedData.ToDictionary(k=>{keyFields}, v=>v.{GetPreferredFieldName(replaceFieldsDict, entityData.PrimaryKeyFieldName)}))",
                        "return true");
                }

                additionalCheckMethod = additionalCheckMethodHeader;
                additionalCheckMethod.AddRange(flatCodeBlockForAdditionalCheckMethod.CodeItems);
            }

            string fieldsListStr = string.Join(", ", entityData.FieldsData.Where(w =>
                (entityData.SelfRecursiveFields.Count == 0 ||
                 !entityData.SelfRecursiveFields.Select(s => s.Name).Contains(w.Name)) &&
                (entityData.UsePrimaryKey || entityData.PrimaryKeyFieldName != w.OldName)).Select(p =>
            {
                FieldData? devFieldData = entityDataForDevBase?.FieldsData.SingleOrDefault(x =>
                    string.Equals(x.Name, p.Name, StringComparison.Ordinal));
                string result = $"{p.Name} = {GetRightValue(p, devFieldData?.IsNullable ?? false)}";
                //if (devFieldData is null)
                //    return result;

                //if (devFieldData.IsValueType && p.IsNullable)
                //    return $"{result}.Value";

                return result;
            }));

            if (entityData.NeedsToCreateTempData)
            {
                createMethod = new CodeBlock(
                    $"protected virtual Dictionary<{keyRealTypeNameForDictionaryGeneric}, {tableNameSingular}> Create{tableNameCapitalCamel}List(List<{seederModelClassName}> {seedDataObjectName})",
                    atLeastOneSubstitute ? "var tempData = DataSeederTempData.Instance" : null,
                    $"return {seedDataObjectName}.ToDictionary(k => k.{_excludesRulesParameters.GetNewFieldName(tableName, entityData.PrimaryKeyFieldName)}, s => new {tableNameSingular}{(string.IsNullOrWhiteSpace(fieldsListStr) ? "()" : $"{{ {fieldsListStr} }}")})");
                adaptMethod =
                    new CodeBlock(
                        $"public override List<{tableNameSingular}> Adapt(List<{seederModelClassName}> jsonData)",
                        $"_tempData = Create{tableNameCapitalCamel}List(jsonData)", "return _tempData.Values.ToList()");
            }
            else
            {
                createMethod = new CodeBlock(
                    $"protected virtual List<{tableNameSingular}> Create{tableNameCapitalCamel}List(List<{seederModelClassName}> {seedDataObjectName})",
                    atLeastOneSubstitute ? "var tempData = DataSeederTempData.Instance" : null,
                    $"return {seedDataObjectName}.Select(s => new {tableNameSingular}{{ {fieldsListStr} }}).ToList()");
                adaptMethod =
                    new CodeBlock(
                        $"public override List<{tableNameSingular}> Adapt(List<{seederModelClassName}> jsonData)",
                        $"return Create{tableNameCapitalCamel}List(jsonData)");
            }

            //usedList = true;
        }

        var block = new CodeBlock(string.Empty, new OneLineComment($"Created by {GetType().Name} at {DateTime.Now}"),
            new OneLineComment($"tableName is {tableName}"),
            !isCarcassType && entityData.OptimalIndexProperties.Count > 1 ||
            keyRealTypeNameForDictionaryGeneric == "DateTime"
                ? "using System"
                : null, "using System.Collections.Generic", !isCarcassType ? "using System.Linq" : null,
            //isCarcassType || (!isCarcassType && (entityData.NeedsToCreateTempData || atLeastOneSubstitute ||
            //                                     entityData.OptimalIndex != null ||
            //                                     entityData.SelfRecursiveField != null))
            //    ? "using CarcassDataSeeding"
            //    : null, 
            isDataTypesOrManyToManyJoins || !isCarcassType && (entityData.NeedsToCreateTempData ||
                                                               atLeastOneSubstitute ||
                                                               entityData.OptimalIndexProperties.Count > 0 ||
                                                               entityData.SelfRecursiveFields.Count > 0)
                ? "using BackendCarcass.DataSeeding"
                : null, !isCarcassType ? $"using {_parameters.ProjectNamespace}.{_parameters.ModelsFolderName}" : null,
            isCarcassType ? "using BackendCarcass.DataSeeding.Seeders" : null,
            isCarcassType ? null : $"using {_parameters.DbProjectNamespace}.{_parameters.DbProjectModelsFolderName}",
            isIdentity ? "using BackendCarcass.MasterData.Models" : string.Empty,
            isIdentity ? "using Microsoft.AspNetCore.Identity" : string.Empty, "using SystemTools.DatabaseToolsShared",
            $"namespace {_parameters.ProjectNamespace}.{(isCarcassType ? _parameters.CarcassSeedersFolderName : _parameters.ProjectSeedersFolderName)}",
            string.Empty,
            new CodeBlock($"public /*open*/ class {className} : {baseClassName}",
                entityData.NeedsToCreateTempData
                    ? $"private Dictionary<{keyRealTypeNameForDictionaryGeneric}, {tableNameSingular}> _tempData = []"
                    : null, new OneLineComment(" ReSharper disable once ConvertToPrimaryConstructor"),
                new CodeBlock(
                    $"public {className}({additionalParameters}string dataSeedFolder, {_parameters.DataSeederRepositoryInterfaceName} repo, ESeedDataType seedDataType = ESeedDataType.OnlyJson, List<string>? keyFieldNamesList = null) : base({additionalParameters2}dataSeedFolder, repo, seedDataType, keyFieldNamesList)"),
                adaptMethod, additionalCheckMethod, createMethod, setParentsMethod));
        CodeFile.FileName = className + ".cs";
        CodeFile.AddRange(block.CodeItems);

        FinishAndSave();

        return tableName;
    }

    private static string GetPreferredFieldName(Dictionary<string, string> replaceDict, string oldName)
    {
        return replaceDict.GetValueOrDefault(oldName, oldName);
    }

    public void UseCarcassEntity(IEntityType carcassEntityType)
    {
        string? tableName = Relations.GetTableName(carcassEntityType);
        if (string.IsNullOrWhiteSpace(tableName))
        {
            return;
        }

        Console.WriteLine("UseEntity tableName = {0}", tableName);

        string tableNameCapitalCamel = tableName.CapitalizeCamel();

        string className = _parameters.ProjectPrefixShort + tableNameCapitalCamel + "Seeder";
        string baseClassName = tableNameCapitalCamel + "Seeder";

        // FIX: Use ToLowerInvariant() to address CA1304 and CA1311
        bool isIdentity = tableName.ToUpperInvariant() is "ROLES" or "USERS";
        bool isDataTypesOrManyToManyJoins = tableName.ToUpperInvariant() is "DATATYPES" or "MANYTOMANYJOINS";

        string additionalParameters = tableName.ToUpperInvariant() switch
        {
            "DATATYPES" => "ICarcassDataSeederRepository carcassRepo, ",
            "MANYTOMANYJOINS" => "string secretDataFolder, ICarcassDataSeederRepository carcassRepo, ",
            "ROLES" => "RoleManager<AppRole> roleManager, string secretDataFolder, ",
            "USERS" => "UserManager<AppUser> userManager, string secretDataFolder, ",
            _ => string.Empty
        };

        string additionalParameters2 = tableName.ToUpperInvariant() switch
        {
            "DATATYPES" => "carcassRepo, ",
            "MANYTOMANYJOINS" => "secretDataFolder, carcassRepo, ",
            "ROLES" => "roleManager, secretDataFolder, ",
            "USERS" => "userManager, secretDataFolder, ",
            _ => string.Empty
        };

        var block = new CodeBlock(string.Empty, new OneLineComment($"Created by {GetType().Name} at {DateTime.Now}"),
            new OneLineComment($"tableName is {tableName}"),
            isDataTypesOrManyToManyJoins ? "using BackendCarcass.DataSeeding" : null, "using BackendCarcass.DataSeeding.Seeders",
            isIdentity ? "using BackendCarcass.MasterData.Models" : string.Empty,
            isIdentity ? "using Microsoft.AspNetCore.Identity" : string.Empty, "using SystemTools.DatabaseToolsShared",
            "using System.Collections.Generic",
            $"namespace {_parameters.ProjectNamespace}.{_parameters.CarcassSeedersFolderName}", string.Empty,
            new CodeBlock($"public /*open*/ class {className} : {baseClassName}",
                new OneLineComment(" ReSharper disable once ConvertToPrimaryConstructor"),
                new CodeBlock(
                    $"public {className}({additionalParameters}string dataSeedFolder, {_parameters.DataSeederRepositoryInterfaceName} repo, ESeedDataType seedDataType = ESeedDataType.OnlyJson, List<string>? keyFieldNamesList = null) : base({additionalParameters2}dataSeedFolder, repo, seedDataType, keyFieldNamesList)")));
        CodeFile.FileName = className + ".cs";
        CodeFile.AddRange(block.CodeItems);

        FinishAndSave();
    }
}
