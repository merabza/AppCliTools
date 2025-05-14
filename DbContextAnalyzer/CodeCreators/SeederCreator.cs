using System;
using System.Collections.Generic;
using System.Linq;
using CodeTools;
using DbContextAnalyzer.Domain;
using DbContextAnalyzer.Models;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;
using SystemToolsShared;

namespace DbContextAnalyzer.CodeCreators;

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

    public override void CreateFileStructure()
    {
    }

    private string GetRightValue(FieldData fieldData, FieldData? devFieldData)
    {
        if (fieldData.SubstituteField == null)
            return
                $"s.{fieldData.FullName}{(!(devFieldData?.IsNullable ?? false) && fieldData is { IsValueType: true, IsNullable: true } ? ".Value" : string.Empty)}";

        var substituteTableNameCapitalCamel = GetTableNameSingularCapitalizeCamel(fieldData.SubstituteField.TableName);
        if (fieldData.SubstituteField.Fields.Count == 0)
            return fieldData.IsNullableByParents
                ? $"tempData.GetIntNullableIdByOldId<{substituteTableNameCapitalCamel}>(s.{fieldData.FullName})"
                : $"tempData.GetIntIdByOldId<{substituteTableNameCapitalCamel}>(s.{fieldData.FullName})";
        var keyParametersList = string.Join(", ", fieldData.SubstituteField.Fields.Select(s => GetRightValue(s, null)));
        return fieldData.IsNullableByParents
            ? $"tempData.GetIntNullableIdByKey<{substituteTableNameCapitalCamel}>({keyParametersList})"
            : $"tempData.GetIntIdByKey<{substituteTableNameCapitalCamel}>({keyParametersList})";
    }

    public string UseEntity(EntityData entityData, EntityData? entityDataForDev, bool isCarcassType)
    {
        var usedList = false;
        var tableName = GetNewTableName(entityData.TableName);

        //tableName.ToLower() == "MorphemeRanges"
        Console.WriteLine("UseEntity tableName = {0}", tableName);

        var replaceFieldsDict = _excludesRulesParameters.GetReplaceFieldsDictByTableName(tableName);

        var tableNameCapitalCamel = tableName.CapitalizeCamel();
        var tableNameCamel = tableName.Camelize();

        var tableNameSingular = GetTableNameSingularCapitalizeCamel(tableName);
        var className = (isCarcassType ? _parameters.ProjectPrefixShort : string.Empty) + tableNameCapitalCamel +
                        "Seeder";
        var seederModelClassName = tableNameSingular + "SeederModel";
        var baseClassName = isCarcassType
            ? tableNameCapitalCamel + "Seeder"
            : $"{_parameters.DataSeederBaseClassName}<{tableNameSingular}, {seederModelClassName}>";
        var seedDataObjectName = tableName.UnCapitalize() + "SeedData";
        var prPref = isCarcassType ? string.Empty : _parameters.ProjectPrefixShort;

        var isIdentity = tableName.ToLower() is "roles" or "users";
        var isDataTypesOrManyToManyJoins = tableName.ToLower() is "datatypes" or "manytomanyjoins";

        var additionalParameters = tableName.ToLower() switch
        {
            "datatypes" => "ICarcassDataSeederRepository carcassRepo, ",
            "manytomanyjoins" => "string secretDataFolder, ICarcassDataSeederRepository carcassRepo, ",
            "roles" => "RoleManager<AppRole> roleManager, string secretDataFolder, ",
            "users" => "UserManager<AppUser> userManager, string secretDataFolder, ",
            _ => string.Empty
        };

        var additionalParameters2 = tableName.ToLower() switch
        {
            "datatypes" => "carcassRepo, ",
            "manytomanyjoins" => "secretDataFolder, carcassRepo, ",
            "roles" => "roleManager, secretDataFolder, ",
            "users" => "userManager, secretDataFolder, ",
            _ => string.Empty
        };

        CodeBlock? setParentsMethod = null;
        CodeBlock? adaptMethod = null;
        CodeBlock? additionalCheckMethod = null;
        var additionalCheckMethodHeader =
            new CodeBlock(
                $"protected override bool AdditionalCheck(List<{seederModelClassName}> jsonData, List<{tableNameSingular}> savedData)");
        CodeBlock? createMethod = null;

        var atLeastOneSubstitute = false;
        foreach (var w in entityData.FieldsData.Where(w => entityData.SelfRecursiveFields.Count == 0 ||
                                                           !entityData.SelfRecursiveFields.Select(x => x.Name)
                                                               .Contains(w.Name)))
        {
            if (w.SubstituteField == null)
                continue;
            atLeastOneSubstitute = true;
            break;
        }

        if (!isCarcassType)
        {
            if (entityData.NeedsToCreateTempData)
            {
                FlatCodeBlock fcb;
                if (entityData.SelfRecursiveFields.Count > 0)
                {
                    var seederModelObjectName = seederModelClassName.UnCapitalize();

                    var flatCodeBlocks = entityData.SelfRecursiveFields.Select(s =>
                            new FlatCodeBlock($"var {tableNameCamel}Dict = Create{tableNameCapitalCamel}List(jsonData)",
                                $"var idsDict = {tableNameCamel}Dict.ToDictionary(k => k.Key, v => v.Value.{entityData.PrimaryKeyFieldName})",
                                $"DataSeederTempData.Instance.SaveOldIntIdsDictToIntIds<{tableNameSingular}>(idsDict)",
                                new CodeBlock(
                                    $"foreach (var {seederModelObjectName} in jsonData.Where(w => w.{s.Name} != null))",
                                    $"{tableNameCamel}Dict[{seederModelObjectName}.{entityData.PrimaryKeyFieldName}].{s.Name} = idsDict[{seederModelObjectName}.{s.Name}!.Value]")))
                        .ToList();

                    fcb = flatCodeBlocks[0];
                    for (var i = 1; i < flatCodeBlocks.Count; i++)
                        fcb.Add(flatCodeBlocks[i]);
                    fcb.Add(new FlatCodeBlock("return true"));
                }
                else
                {
                    fcb = new FlatCodeBlock(
                        $"DataSeederTempData.Instance.SaveOldIntIdsDictToIntIds<{tableNameSingular}>(Create{tableNameCapitalCamel}List(jsonData).ToDictionary(k=>k.Key, v=>v.Value.{_excludesRulesParameters.GetNewFieldName(tableName, entityData.PrimaryKeyFieldName)}))",
                        "return true");
                }

                additionalCheckMethod = additionalCheckMethodHeader;
                additionalCheckMethod.AddRange(fcb.CodeItems);
            }
            else if (entityData.OptimalIndexProperties.Count > 0)
            {
                var optimalIndexFieldsData = entityData.OptimalIndexProperties
                    .Select(prop => entityData.FieldsData.SingleOrDefault(ss => ss.OldName == prop.Name))
                    .OfType<FieldData>().ToList();

                var tupTypeList = string.Join(", ", optimalIndexFieldsData.Select(s => s.RealTypeName));
                var keyFieldsList = string.Join(", ", optimalIndexFieldsData.Select(s => $"k.{s.Name}"));
                var keyFields = optimalIndexFieldsData.Count == 1
                    ? $"k.{GetPreferredFieldName(replaceFieldsDict, optimalIndexFieldsData[0].Name)}"
                    : $" new Tuple<{tupTypeList}>({keyFieldsList})";
                FlatCodeBlock fcb;
                if (entityData.SelfRecursiveFields.Count > 0)
                {
                    fcb = new FlatCodeBlock(
                        $"DataSeederTempData.Instance.SaveIntIdKeys<{tableNameSingular}>(savedData.ToDictionary(k=>{keyFields}, v=>v.{entityData.PrimaryKeyFieldName}))",
                        new CodeBlock($"if (!SetParents({seedDataObjectName}, {tableNameCamel}List))", "return false"),
                        "return true");
                    var seederModelObjectName = seederModelClassName.UnCapitalize();
                    var keyFieldName = optimalIndexFieldsData[0].Name;

                    setParentsMethod = new CodeBlock(
                        $"private bool SetParents(List<{seederModelClassName}> {seedDataObjectName}, List<{tableNameSingular}> {tableNameCamel}List)",
                        "var tempData = DataSeederTempData.Instance",
                        $"var forUpdate = new List<{tableNameSingular}>()");

                    foreach (var entityDataSelfRecursiveField in entityData.SelfRecursiveFields)
                    {
                        if (entityDataSelfRecursiveField.SubstituteField is null ||
                            entityDataSelfRecursiveField.SubstituteField.Fields.Count == 0)
                            throw new Exception(
                                "entityData.SelfRecursiveField.SubstituteField is null or without fields");

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
                    fcb = new FlatCodeBlock(
                        $"DataSeederTempData.Instance.SaveIntIdKeys<{tableNameSingular}>(savedData.ToDictionary(k=>{keyFields}, v=>v.{GetPreferredFieldName(replaceFieldsDict, entityData.PrimaryKeyFieldName)}))",
                        "return true");
                }

                additionalCheckMethod = additionalCheckMethodHeader;
                additionalCheckMethod.AddRange(fcb.CodeItems);
            }

            var fieldsListStr = string.Join(", ", entityData.FieldsData.Where(w =>
                (entityData.SelfRecursiveFields.Count == 0 ||
                 !entityData.SelfRecursiveFields.Select(s => s.Name).Contains(w.Name)) &&
                (entityData.UsePrimaryKey || entityData.PrimaryKeyFieldName != w.Name)).Select(p =>
            {
                var devFieldData = entityDataForDev?.FieldsData.SingleOrDefault(x =>
                    string.Equals(x.Name, p.Name, StringComparison.CurrentCultureIgnoreCase));
                var result = $"{p.Name} = {GetRightValue(p, devFieldData)}";
                //if (devFieldData is null)
                //    return result;

                //if (devFieldData.IsValueType && p.IsNullable)
                //    return $"{result}.Value";

                return result;
            }));

            if (entityData.NeedsToCreateTempData)
            {
                createMethod = new CodeBlock(
                    $"protected virtual Dictionary<int, {tableNameSingular}> Create{tableNameCapitalCamel}List(List<{seederModelClassName}> {seedDataObjectName})",
                    atLeastOneSubstitute ? "var tempData = DataSeederTempData.Instance" : null,
                    $"return {seedDataObjectName}.ToDictionary(k => k.{_excludesRulesParameters.GetNewFieldName(tableName, entityData.PrimaryKeyFieldName)}, s => new {tableNameSingular}{(fieldsListStr == string.Empty ? "()" : $"{{ {fieldsListStr} }}")})");
                adaptMethod =
                    new CodeBlock(
                        $"protected override List<{tableNameSingular}> Adapt(List<{seederModelClassName}> jsonData)",
                        $"return Create{tableNameCapitalCamel}List(jsonData).Values.ToList()");
            }
            else
            {
                createMethod = new CodeBlock(
                    $"protected virtual List<{tableNameSingular}> Create{tableNameCapitalCamel}List(List<{seederModelClassName}> {seedDataObjectName})",
                    atLeastOneSubstitute ? "var tempData = DataSeederTempData.Instance" : null,
                    $"return {seedDataObjectName}.Select(s => new {tableNameSingular}{{ {fieldsListStr} }}).ToList()");
                adaptMethod =
                    new CodeBlock(
                        $"protected override List<{tableNameSingular}> Adapt(List<{seederModelClassName}> jsonData)",
                        $"return Create{tableNameCapitalCamel}List(jsonData)");
            }

            usedList = true;
        }

        var block = new CodeBlock(string.Empty, new OneLineComment($"Created by {GetType().Name} at {DateTime.Now}"),
            new OneLineComment($"tableName is {tableName}"),
            !isCarcassType && entityData.OptimalIndexProperties.Count > 1 ? "using System" : null,
            usedList ? "using System.Collections.Generic" : null, !isCarcassType ? "using System.Linq" : null,
            //isCarcassType || (!isCarcassType && (entityData.NeedsToCreateTempData || atLeastOneSubstitute ||
            //                                     entityData.OptimalIndex != null ||
            //                                     entityData.SelfRecursiveField != null))
            //    ? "using CarcassDataSeeding"
            //    : null, 
            isDataTypesOrManyToManyJoins || (!isCarcassType && (entityData.NeedsToCreateTempData ||
                                                                atLeastOneSubstitute ||
                                                                entityData.OptimalIndexProperties.Count > 0 ||
                                                                entityData.SelfRecursiveFields.Count > 0))
                ? "using CarcassDataSeeding"
                : null, !isCarcassType ? $"using {_parameters.ProjectNamespace}.{_parameters.ModelsFolderName}" : null,
            isCarcassType ? "using CarcassDataSeeding.Seeders" : null,
            isCarcassType ? null : $"using {_parameters.DbProjectNamespace}.{_parameters.DbProjectModelsFolderName}",
            isIdentity ? "using CarcassMasterDataDom.Models" : string.Empty,
            isIdentity ? "using Microsoft.AspNetCore.Identity" : string.Empty, "using DatabaseToolsShared",
            "using System.Collections.Generic",
            $"namespace {_parameters.ProjectNamespace}.{(isCarcassType ? _parameters.CarcassSeedersFolderName : _parameters.ProjectSeedersFolderName)}",
            string.Empty,
            new CodeBlock($"public /*open*/ class {className} : {baseClassName}",
                new OneLineComment(" ReSharper disable once ConvertToPrimaryConstructor"),
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
        var tableName = Relations.GetTableName(carcassEntityType);
        if (string.IsNullOrWhiteSpace(tableName))
            return;

        Console.WriteLine("UseEntity tableName = {0}", tableName);

        var tableNameCapitalCamel = tableName.CapitalizeCamel();

        var className = _parameters.ProjectPrefixShort + tableNameCapitalCamel + "Seeder";
        var baseClassName = tableNameCapitalCamel + "Seeder";

        var isIdentity = tableName.ToLower() is "roles" or "users";
        var isDataTypesOrManyToManyJoins = tableName.ToLower() is "datatypes" or "manytomanyjoins";

        var additionalParameters = tableName.ToLower() switch
        {
            "datatypes" => "ICarcassDataSeederRepository carcassRepo, ",
            "manytomanyjoins" => "string secretDataFolder, ICarcassDataSeederRepository carcassRepo, ",
            "roles" => "RoleManager<AppRole> roleManager, string secretDataFolder, ",
            "users" => "UserManager<AppUser> userManager, string secretDataFolder, ",
            _ => string.Empty
        };

        var additionalParameters2 = tableName.ToLower() switch
        {
            "datatypes" => "carcassRepo, ",
            "manytomanyjoins" => "secretDataFolder, carcassRepo, ",
            "roles" => "roleManager, secretDataFolder, ",
            "users" => "userManager, secretDataFolder, ",
            _ => string.Empty
        };

        var block = new CodeBlock(string.Empty, new OneLineComment($"Created by {GetType().Name} at {DateTime.Now}"),
            new OneLineComment($"tableName is {tableName}"),
            isDataTypesOrManyToManyJoins ? "using CarcassDataSeeding" : null, "using CarcassDataSeeding.Seeders",
            isIdentity ? "using CarcassMasterDataDom.Models" : string.Empty,
            isIdentity ? "using Microsoft.AspNetCore.Identity" : string.Empty, "using DatabaseToolsShared",
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