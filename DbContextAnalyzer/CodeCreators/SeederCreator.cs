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

    private string GetRightValue(FieldData fieldData, bool devFieldIsNullable)
    {
        if (fieldData.SubstituteField == null)
            return
                $"s.{fieldData.FullName}{(!devFieldIsNullable && fieldData is { IsValueType: true, IsNullable: true } ? ".Value" : string.Empty)}";

        var substituteTableNameCapitalCamel =
            GetTableNameSingularCapitalizeCamel(GetNewTableName(fieldData.SubstituteField.TableName));

        var realTypeName = GetRealTypeNameForMethodName(fieldData);

        if (fieldData.SubstituteField.Fields.Count == 0 && fieldData.SubstituteField.HasAutoNumber)
            return fieldData.IsNullableByParents
                ? $"tempData.Get{realTypeName}NullableIdByOldId<{substituteTableNameCapitalCamel}>(s.{fieldData.FullName}){(devFieldIsNullable ? string.Empty : ".Value")}"
                : $"tempData.Get{realTypeName}IdByOldId<{substituteTableNameCapitalCamel}>(s.{fieldData.FullName})";

        var keyParametersList = string.Join(", ",
            fieldData.SubstituteField.Fields.Select(s => GetRightValue(s, fieldData.IsNullableByParents)));
        return fieldData.IsNullableByParents
            ? $"tempData.Get{realTypeName}NullableIdByKey<{substituteTableNameCapitalCamel}>({keyParametersList}){(devFieldIsNullable ? string.Empty : ".Value")}"
            : $"tempData.Get{realTypeName}IdByKey<{substituteTableNameCapitalCamel}>({keyParametersList})";
    }

    private static string GetRealTypeNameForDictionaryGeneric(FieldData? fieldData)
    {
        if (fieldData is null)
            return "int";
        var realTypeName = fieldData.RealTypeName;
        if (realTypeName.EndsWith('?'))
            realTypeName = realTypeName[..^1];

        switch (realTypeName.ToLower())
        {
            case "int":
                return "int";
            case "datetime":
                return "DateTime";
            default:
                realTypeName = realTypeName.UnCapitalize();
                return realTypeName;
        }
    }

    private static string GetRealTypeNameForMethodName(FieldData? fieldData)
    {
        if (fieldData is null)
            return "Int";
        var realTypeName = fieldData.RealTypeName;
        if (realTypeName.EndsWith('?'))
            realTypeName = realTypeName[..^1];
        realTypeName = realTypeName.Capitalize();
        return realTypeName;
    }

    public string UseEntity(EntityData entityData, EntityData? entityDataForDev, bool isCarcassType)
    {
        //var usedList = false;
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
                $"public override bool AdditionalCheck(List<{seederModelClassName}> jsonData, List<{tableNameSingular}> savedData)");
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

        var primaryKeyFieldNewName =
            _excludesRulesParameters.GetNewFieldName(tableName, entityData.PrimaryKeyFieldName);
        var keyFieldData = entityData.FieldsData.FirstOrDefault(f => f.Name == primaryKeyFieldNewName);
        var keyRealTypeName = GetRealTypeNameForMethodName(keyFieldData);
        var keyRealTypeNameForDictionaryGeneric = GetRealTypeNameForDictionaryGeneric(keyFieldData);

        if (!isCarcassType)
        {
            if (entityData.NeedsToCreateTempData)
            {
                FlatCodeBlock flatCodeBlockForAdditionalCheckMethod;
                if (entityData.SelfRecursiveFields.Count > 0)
                {
                    var seederModelObjectName = seederModelClassName.UnCapitalize();

                    var flatCodeBlocks = entityData.SelfRecursiveFields.Select(s =>
                            new FlatCodeBlock(
                                $"var idsDict = _tempData.ToDictionary(k => k.Key, v => v.Value.{entityData.PrimaryKeyFieldName})",
                                $"DataSeederTempData.Instance.SaveOld{keyRealTypeName}IdsDictTo{keyRealTypeName}Ids<{tableNameSingular}>(idsDict)",
                                new CodeBlock(
                                    $"foreach (var {seederModelObjectName} in jsonData.Where(w => w.{s.Name} != null))",
                                    $"_tempData[{seederModelObjectName}.{entityData.PrimaryKeyFieldName}].{s.Name} = idsDict[{seederModelObjectName}.{s.Name}!.Value]")))
                        .ToList();

                    flatCodeBlockForAdditionalCheckMethod = flatCodeBlocks[0];
                    for (var i = 1; i < flatCodeBlocks.Count; i++)
                        flatCodeBlockForAdditionalCheckMethod.Add(flatCodeBlocks[i]);
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
                var optimalIndexFieldsData = entityData.OptimalIndexProperties
                    .Select(prop => entityData.FieldsData.SingleOrDefault(ss => ss.OldName == prop.Name))
                    .OfType<FieldData>().ToList();

                var tupTypeList = string.Join(", ", optimalIndexFieldsData.Select(s => s.RealTypeName));
                var keyFieldsList = string.Join(", ", optimalIndexFieldsData.Select(s => $"k.{s.Name}"));
                var keyFields = optimalIndexFieldsData.Count == 1
                    ? $"k.{GetPreferredFieldName(replaceFieldsDict, optimalIndexFieldsData[0].Name)}"
                    : $" new Tuple<{tupTypeList}>({keyFieldsList})";
                FlatCodeBlock flatCodeBlockForAdditionalCheckMethod;
                if (entityData.SelfRecursiveFields.Count > 0)
                {
                    flatCodeBlockForAdditionalCheckMethod = new FlatCodeBlock(
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
                    flatCodeBlockForAdditionalCheckMethod = new FlatCodeBlock(
                        $"DataSeederTempData.Instance.SaveIntIdKeys<{tableNameSingular}>(savedData.ToDictionary(k=>{keyFields}, v=>v.{GetPreferredFieldName(replaceFieldsDict, entityData.PrimaryKeyFieldName)}))",
                        "return true");
                }

                additionalCheckMethod = additionalCheckMethodHeader;
                additionalCheckMethod.AddRange(flatCodeBlockForAdditionalCheckMethod.CodeItems);
            }

            var fieldsListStr = string.Join(", ", entityData.FieldsData.Where(w =>
                (entityData.SelfRecursiveFields.Count == 0 ||
                 !entityData.SelfRecursiveFields.Select(s => s.Name).Contains(w.Name)) &&
                (entityData.UsePrimaryKey || entityData.PrimaryKeyFieldName != w.OldName)).Select(p =>
            {
                var devFieldData = entityDataForDev?.FieldsData.SingleOrDefault(x =>
                    string.Equals(x.Name, p.Name, StringComparison.CurrentCultureIgnoreCase));
                var result = $"{p.Name} = {GetRightValue(p, devFieldData?.IsNullable ?? false)}";
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
                    $"return {seedDataObjectName}.ToDictionary(k => k.{_excludesRulesParameters.GetNewFieldName(tableName, entityData.PrimaryKeyFieldName)}, s => new {tableNameSingular}{(fieldsListStr == string.Empty ? "()" : $"{{ {fieldsListStr} }}")})");
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
            (!isCarcassType && entityData.OptimalIndexProperties.Count > 1) ||
            keyRealTypeNameForDictionaryGeneric == "DateTime"
                ? "using System"
                : null, "using System.Collections.Generic", !isCarcassType ? "using System.Linq" : null,
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