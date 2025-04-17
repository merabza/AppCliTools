using System;
using System.Collections.Generic;
using System.Linq;
using CodeTools;
using DbContextAnalyzer.Domain;
using DbContextAnalyzer.Models;
using Microsoft.Extensions.Logging;
using SystemToolsShared;

namespace DbContextAnalyzer.CodeCreators;

public sealed class SeederCreator : CodeCreator
{
    private readonly ExcludesRulesParametersDomain _excludesRulesParameters;
    private readonly SeederCodeCreatorParameters _parameters;

    // ReSharper disable once ConvertToPrimaryConstructor
    public SeederCreator(ILogger logger, SeederCodeCreatorParameters parameters,
        ExcludesRulesParametersDomain excludesRulesParameters, string placePath) : base(logger, placePath)
    {
        _parameters = parameters;
        _excludesRulesParameters = excludesRulesParameters;
    }

    public override void CreateFileStructure()
    {
    }

    private string GetRightValue(FieldData fieldData)
    {
        if (fieldData.SubstituteField == null)
            return $"s.{fieldData.FullName}";

        var substituteTableNameCapitalCamel = GetTableNameSingularCapitalizeCamel(
            _excludesRulesParameters.SingularityExceptions, fieldData.SubstituteField.TableName);
        if (fieldData.SubstituteField.Fields.Count == 0)
            return fieldData.IsNullableByParents
                ? $"tempData.GetIntNullableIdByOldId<{substituteTableNameCapitalCamel}>(s.{fieldData.FullName})"
                : $"tempData.GetIntIdByOldId<{substituteTableNameCapitalCamel}>(s.{fieldData.FullName})";
        var keyParametersList = string.Join(", ", fieldData.SubstituteField.Fields.Select(GetRightValue));
        return fieldData.IsNullableByParents
            ? $"tempData.GetIntNullableIdByKey<{substituteTableNameCapitalCamel}>({keyParametersList})"
            : $"tempData.GetIntIdByKey<{substituteTableNameCapitalCamel}>({keyParametersList})";
    }

    public void UseEntity(EntityData entityData, bool isCarcassType)
    {
        var usedList = false;
        var tableName = entityData.TableName;

        Console.WriteLine("UseEntity tableName = {0}", tableName);

        var replaceFieldsDict = _excludesRulesParameters.GetReplaceFieldsDictByTableName(tableName);

        var tableNameCapitalCamel = tableName.CapitalizeCamel();
        var tableNameCamel = tableName.Camelize();

        var tableNameSingular =
            GetTableNameSingularCapitalizeCamel(_excludesRulesParameters.SingularityExceptions, tableName);
        var className = (isCarcassType ? _parameters.ProjectPrefixShort : string.Empty) + tableNameCapitalCamel +
                        "Seeder";
        var seederModelClassName = tableNameSingular + "SeederModel";
        var baseClassName = isCarcassType
            ? tableNameCapitalCamel + "Seeder"
            : $"{_parameters.DataSeederBaseClassName}<{tableNameSingular}, {seederModelClassName}>";
        var seedDataObjectName = tableName.UnCapitalize() + "SeedData";
        var prPref = isCarcassType ? string.Empty : _parameters.ProjectPrefixShort;

        var isIdentity = tableName is "roles" or "users";
        var isDataTypesOrManyToManyJoins = tableName is "dataTypes" or "manyToManyJoins";

        var additionalParameters = tableName switch
        {
            "dataTypes" => "ICarcassDataSeederRepository carcassRepo, ",
            "manyToManyJoins" => "string secretDataFolder, ICarcassDataSeederRepository carcassRepo, ",
            "roles" => "RoleManager<AppRole> roleManager, string secretDataFolder, ",
            "users" => "UserManager<AppUser> userManager, string secretDataFolder, ",
            _ => string.Empty
        };

        var additionalParameters2 = tableName switch
        {
            "dataTypes" => "carcassRepo, ",
            "manyToManyJoins" => "secretDataFolder, carcassRepo, ",
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

        var atLeastOneSubstitute = entityData.FieldsData
            .Where(w => entityData.SelfRecursiveField == null || w.Name != entityData.SelfRecursiveField.Name)
            .Any(w => w.SubstituteField != null);

        if (!isCarcassType)
        {
            if (entityData.NeedsToCreateTempData)
            {
                FlatCodeBlock fcb;
                if (entityData.SelfRecursiveField != null)
                {
                    var seederModelObjectName = seederModelClassName.UnCapitalize();
                    fcb = new FlatCodeBlock(
                        //$"var {seedDataObjectName} = LoadFromJsonFile<{seederModelClassName}>()",
                        //$"var {tableNameCamel}Dict = Create{tableNameCapitalCamel}List({seedDataObjectName})",
                        //new CodeBlock($"if (!{prPref}Repo.CreateEntities({tableNameCamel}Dict.Values.ToList()))",
                        //    $"return new Err[] {{ new() {{ ErrorCode = \"{seederModelClassName}EntitiesCannotBeCreated\", ErrorMessage = \"{seederModelClassName} entities cannot be created\" }} }}"),
                        $"var {tableNameCamel}Dict = Create{tableNameCapitalCamel}List(jsonData)",
                        $"var idsDict = {tableNameCamel}Dict.ToDictionary(k => k.Key, v => v.Value.{entityData.PrimaryKeyFieldName})",
                        //$"var dataList = Repo.GetAll<{tableNameSingular}>()",
                        $"DataSeederTempData.Instance.SaveOldIntIdsDictToIntIds<{tableNameSingular}>(idsDict)",
                        new CodeBlock(
                            $"foreach (var {seederModelObjectName} in jsonData.Where(w => w.{entityData.SelfRecursiveField.Name} != null))",
                            $"{tableNameCamel}Dict[{seederModelObjectName}.{entityData.PrimaryKeyFieldName}].{entityData.SelfRecursiveField.Name} = idsDict[{seederModelObjectName}.{entityData.SelfRecursiveField.Name}!.Value]"),
                        "return true");
                    //new CodeBlock($"if (!{prPref}Repo.SaveChanges() )",
                    //    $"return new Err[] {{ new() {{ ErrorCode = \"{seederModelClassName}CannotBeSaved\", ErrorMessage = \"{seederModelClassName} entities cannot be saved\" }} }}"),
                    //"return null");
                }
                else
                {
                    fcb = new FlatCodeBlock(
                        //$"var {tableNameCamel}Dict = Create{tableNameCapitalCamel}List(LoadFromJsonFile<{seederModelClassName}>())",
                        //new CodeBlock($"if (!{prPref}Repo.CreateEntities({tableNameCamel}Dict.Values.ToList()))",
                        //    //$"return new Err[] {{ new() {{ ErrorCode = \"{seederModelClassName}EntitiesCannotBeCreated\", ErrorMessage = \"{seederModelClassName} entities cannot be created\" }} }}",
                        //    "return false"),
                        //$"var dataList = Repo.GetAll<{tableNameSingular}>()",
                        $"DataSeederTempData.Instance.SaveOldIntIdsDictToIntIds<{tableNameSingular}>(Create{tableNameCapitalCamel}List(jsonData).ToDictionary(k=>k.Key, v=>v.Value.{entityData.PrimaryKeyFieldName}))",
                        "return true");
                }

                adaptMethod =
                    new CodeBlock(
                        $"protected override List<{tableNameSingular}> Adapt(List<{seederModelClassName}> jsonData)",
                        $"return Create{tableNameCapitalCamel}List(jsonData).Values.ToList()");

                additionalCheckMethod = additionalCheckMethodHeader;
                additionalCheckMethod.AddRange(fcb.CodeItems);
            }
            else if (entityData.OptimalIndex != null)
            {
                var tupTypeList = string.Join(", ", entityData.OptimalIndexFieldsData.Select(s => s.RealTypeName));
                var keyFieldsList = string.Join(", ", entityData.OptimalIndexFieldsData.Select(s => $"k.{s.Name}"));
                var keyFields = entityData.OptimalIndex.Properties.Count == 1
                    ? $"k.{GetPreferredFieldName(replaceFieldsDict, entityData.OptimalIndexFieldsData[0].Name)}"
                    : $" new Tuple<{tupTypeList}>({keyFieldsList})";
                FlatCodeBlock fcb;
                if (entityData.SelfRecursiveField != null)
                {
                    fcb = new FlatCodeBlock(
                        //$"var {seedDataObjectName} = LoadFromJsonFile<{seederModelClassName}>()",
                        //$"var {tableNameCamel}List = Create{tableNameCapitalCamel}List({seedDataObjectName})",
                        //new CodeBlock($"if (!{prPref}Repo.CreateEntities({tableNameCamel}List))",
                        //$"return new Err[] {{ new() {{ ErrorCode = \"{seederModelClassName}EntitiesCannotBeCreated\", ErrorMessage = \"{seederModelClassName} entities cannot be created\" }} }}",
                        //"return false"),
                        //$"var dataList = Repo.GetAll<{tableNameSingular}>()",
                        $"DataSeederTempData.Instance.SaveIntIdKeys<{tableNameSingular}>(savedData.ToDictionary(k=>{keyFields}, v=>v.{entityData.PrimaryKeyFieldName}))",
                        new CodeBlock($"if (!SetParents({seedDataObjectName}, {tableNameCamel}List))",
                            //$"return new Err[] {{ new() {{ ErrorCode = \"{seederModelClassName}CannotSetParents\", ErrorMessage = \"{seederModelClassName} cannot Set Parents\" }} }}",
                            "return false"), "return true");
                    var seederModelObjectName = seederModelClassName.UnCapitalize();
                    var keyFieldName = entityData.OptimalIndexFieldsData[0].Name;

                    if (entityData.SelfRecursiveField.SubstituteField is null ||
                        entityData.SelfRecursiveField.SubstituteField.Fields.Count == 0)
                        throw new Exception("entityData.SelfRecursiveField.SubstituteField is null or without fields");

                    setParentsMethod = new CodeBlock(
                        $"private bool SetParents(List<{seederModelClassName}> {seedDataObjectName}, List<{tableNameSingular}> {tableNameCamel}List)",
                        "var tempData = DataSeederTempData.Instance",
                        $"var forUpdate = new List<{tableNameSingular}>()",
                        new CodeBlock(
                            $"foreach ({seederModelClassName} {seederModelObjectName} in {seedDataObjectName}.Where(w => w.{entityData.SelfRecursiveField.SubstituteField.Fields[0].FullName} != null))",
                            $"{tableNameSingular} oneRec = {tableNameCamel}List.SingleOrDefault(s => s.{keyFieldName} == {seederModelObjectName}.{keyFieldName})",
                            new CodeBlock("if (oneRec == null)", "continue"),
                            $"oneRec.{entityData.SelfRecursiveField.Name} = tempData.GetIntIdByKey<{tableNameSingular}>({seederModelObjectName}.{entityData.SelfRecursiveField.SubstituteField.Fields[0].FullName})",
                            "forUpdate.Add(oneRec)"), new CodeBlock($"if (!{prPref}Repo.SetUpdates(forUpdate))",
                            //$"return new Err[] {{ new() {{ ErrorCode = \"{seederModelClassName}CannotSetUpdates\", ErrorMessage = \"{seederModelClassName} cannot Set Updates\" }} }}",
                            "return false"), "return true");
                }
                else
                {
                    fcb = new FlatCodeBlock(
                        //$"var {tableNameCamel}List = Create{tableNameCapitalCamel}List(LoadFromJsonFile<{seederModelClassName}>())",
                        //new CodeBlock($"if (!{prPref}Repo.CreateEntities({tableNameCamel}List))",
                        //    //$"return new Err[] {{ new() {{ ErrorCode = \"{seederModelClassName}EntitiesCannotBeCreated\", ErrorMessage = \"{seederModelClassName} entities cannot be created\" }} }}",
                        //    "return false"),
                        //$"var dataList = Create{tableNameCapitalCamel}List(seedData)",
                        $"DataSeederTempData.Instance.SaveIntIdKeys<{tableNameSingular}>(savedData.ToDictionary(k=>{keyFields}, v=>v.{GetPreferredFieldName(replaceFieldsDict, entityData.PrimaryKeyFieldName)}))",
                        "return true");
                }

                adaptMethod =
                    new CodeBlock(
                        $"protected override List<{tableNameSingular}> Adapt(List<{seederModelClassName}> jsonData)",
                        $"return Create{tableNameCapitalCamel}List(jsonData)");

                additionalCheckMethod = additionalCheckMethodHeader;
                additionalCheckMethod.AddRange(fcb.CodeItems);
            }
            //else
            //{
            //    additionalCheckMethod.Add(new FlatCodeBlock(new CodeBlock(
            //        $"if (!{prPref}Repo.CreateEntities(Create{tableNameCapitalCamel}List(LoadFromJsonFile<{seederModelClassName}>())))",
            //        //$"return new Err[] {{ new() {{ ErrorCode = \"{seederModelClassName}EntitiesCannotBeCreated\", ErrorMessage = \"{seederModelClassName} entities cannot be created\" }} }}",
            //        "return false"), "return true"));
            //}

            var fieldsListStr = string.Join(", ",
                entityData.FieldsData
                    .Where(w =>
                        (entityData.SelfRecursiveField == null || w.Name != entityData.SelfRecursiveField.Name) &&
                        (entityData.UsePrimaryKey || entityData.PrimaryKeyFieldName != w.Name))
                    .Select(p => $"{p.Name} = {GetRightValue(p)}"));

            if (entityData.NeedsToCreateTempData)
                createMethod = new CodeBlock(
                    $"protected virtual Dictionary<int, {tableNameSingular}> Create{tableNameCapitalCamel}List(List<{seederModelClassName}> {seedDataObjectName})",
                    atLeastOneSubstitute ? "var tempData = DataSeederTempData.Instance" : null,
                    $"return {seedDataObjectName}.ToDictionary(k => k.{entityData.PrimaryKeyFieldName}, s => new {tableNameSingular}{(fieldsListStr == string.Empty ? "()" : $"{{ {fieldsListStr} }}")})");
            else
                createMethod = new CodeBlock(
                    $"protected virtual List<{tableNameSingular}> Create{tableNameCapitalCamel}List(List<{seederModelClassName}> {seedDataObjectName})",
                    atLeastOneSubstitute ? "var tempData = DataSeederTempData.Instance" : null,
                    $"return {seedDataObjectName}.Select(s => new {tableNameSingular}{{ {fieldsListStr} }}).ToList()");
            usedList = true;
        }

        var block = new CodeBlock(string.Empty, new OneLineComment($"Created by {GetType().Name} at {DateTime.Now}"),
            new OneLineComment($"tableName is {tableName}"),
            !isCarcassType && entityData.OptimalIndex is not null && entityData.OptimalIndexFieldsData.Count > 1
                ? "using System"
                : null, usedList ? "using System.Collections.Generic" : null,
            !isCarcassType ? "using System.Linq" : null,
            //isCarcassType || (!isCarcassType && (entityData.NeedsToCreateTempData || atLeastOneSubstitute ||
            //                                     entityData.OptimalIndex != null ||
            //                                     entityData.SelfRecursiveField != null))
            //    ? "using CarcassDataSeeding"
            //    : null, 
            isDataTypesOrManyToManyJoins || (!isCarcassType && (entityData.NeedsToCreateTempData ||
                                                                atLeastOneSubstitute ||
                                                                entityData.OptimalIndex != null ||
                                                                entityData.SelfRecursiveField != null))
                ? "using CarcassDataSeeding"
                : null, !isCarcassType ? $"using {_parameters.ProjectNamespace}.{_parameters.ModelsFolderName}" : null,
            isCarcassType ? "using CarcassDataSeeding.Seeders" : null,
            isCarcassType ? null : $"using {_parameters.DbProjectNamespace}.{_parameters.DbProjectModelsFolderName}",
            isIdentity ? "using CarcassMasterDataDom.Models" : string.Empty,
            isIdentity ? "using Microsoft.AspNetCore.Identity" : string.Empty,
            "using DatabaseToolsShared",
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
    }

    private static string GetPreferredFieldName(Dictionary<string, string> replaceDict, string oldName)
    {
        return replaceDict.GetValueOrDefault(oldName, oldName);
    }
}