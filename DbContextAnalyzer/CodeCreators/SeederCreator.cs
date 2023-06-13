using System;
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
            _excludesRulesParameters.SingularityExceptions,
            fieldData.SubstituteField.TableName);
        if (fieldData.SubstituteField.Fields.Count == 0)
            return fieldData.IsNullableByParents
                ? $"tempData.GetIntNullableIdByOldId<{substituteTableNameCapitalCamel}>(s.{fieldData.FullName})"
                : $"tempData.GetIntIdByOldId<{substituteTableNameCapitalCamel}>(s.{fieldData.FullName})";
        var keyParametersList = string.Join(", ",
            fieldData.SubstituteField.Fields.Select(GetRightValue));
        return fieldData.IsNullableByParents
            ? $"tempData.GetIntNullableIdByKey<{substituteTableNameCapitalCamel}>({keyParametersList})"
            : $"tempData.GetIntIdByKey<{substituteTableNameCapitalCamel}>({keyParametersList})";
    }

    public override void UseEntity(EntityData entityData, bool isCarcassType)
    {
        var usedList = false;
        var tableName = entityData.TableName;
        var tableNameCapitalCamel = tableName.CapitalizeCamel();
        var tableNameCamel = tableName.Camelize();

        var tableNameSingular =
            GetTableNameSingularCapitalizeCamel(_excludesRulesParameters.SingularityExceptions, tableName);
        var className = (isCarcassType ? _parameters.ProjectPrefixShort : "") + tableNameCapitalCamel + "Seeder";
        var baseClassName = isCarcassType
            ? tableNameCapitalCamel + "Seeder"
            : $"{_parameters.DataSeederBaseClassName}<{tableNameSingular}>";
        var seederModelClassName = tableNameSingular + "SeederModel";
        var seedDataObjectName = tableName.UnCapitalize() + "SeedData";
        var prPref = isCarcassType ? "" : _parameters.ProjectPrefixShort;

        var isIdentity = tableName is "roles" or "users";

        var additionalParameters = tableName switch
        {
            //"dataRights" => "string secretDataFolder, ",
            "manyToManyJoins" => "string secretDataFolder, ",
            "roles" => "RoleManager<AppRole> roleManager, string secretDataFolder, ",
            "users" => "UserManager<AppUser> userManager, string secretDataFolder, ",
            _ => ""
        };


        var additionalParameters2 = tableName switch
        {
            //"dataRights" => "secretDataFolder, ",
            "manyToManyJoins" => "secretDataFolder, ",
            "roles" => "roleManager, secretDataFolder, ",
            "users" => "userManager, secretDataFolder, ",
            _ => ""
        };

        CodeBlock? setParentsMethod = null;
        CodeBlock? createByJsonFile = null;
        CodeBlock? createMethod = null;

        var atLeastOneSubstitute = entityData.FieldsData
            .Where(w => entityData.SelfRecursiveField == null || w.Name != entityData.SelfRecursiveField.Name)
            .Any(w => w.SubstituteField != null);


        if (!isCarcassType)
        {
            createByJsonFile = new CodeBlock("protected override bool CreateByJsonFile()");
            if (entityData.NeedsToCreateTempData)
            {
                FlatCodeBlock fcb;
                if (entityData.SelfRecursiveField != null)
                {
                    var seederModelObjectName = seederModelClassName.UnCapitalize();
                    fcb = new FlatCodeBlock(
                        $"List<{seederModelClassName}> {seedDataObjectName} = LoadFromJsonFile<{seederModelClassName}>()",
                        $"Dictionary<int, {tableNameSingular}> {tableNameCamel}Dict = Create{tableNameCapitalCamel}List({seedDataObjectName})",
                        new CodeBlock($"if (!{prPref}Repo.CreateEntities({tableNameCamel}Dict.Values.ToList()))",
                            "return false"),
                        $"Dictionary<int, int> idsDict = {tableNameCamel}Dict.ToDictionary(k => k.Key, v => v.Value.{entityData.PrimaryKeyFieldName})",
                        $"DataSeederTempData.Instance.SaveOldIntIdsDictToIntIds<{tableNameSingular}>(idsDict)",
                        new CodeBlock(
                            $"foreach ({seederModelClassName} {seederModelObjectName} in {seedDataObjectName}.Where(w => w.{entityData.SelfRecursiveField.Name} != null))",
                            $"{tableNameCamel}Dict[{seederModelObjectName}.{entityData.PrimaryKeyFieldName}].{entityData.SelfRecursiveField.Name} = idsDict[{seederModelObjectName}.{entityData.SelfRecursiveField.Name}!.Value]"),
                        $"return {prPref}Repo.SaveChanges()");
                }
                else
                {
                    fcb = new FlatCodeBlock(
                        $"Dictionary<int, {tableNameSingular}> {tableNameCamel}Dict = Create{tableNameCapitalCamel}List(LoadFromJsonFile<{seederModelClassName}>())",
                        new CodeBlock($"if (!{prPref}Repo.CreateEntities({tableNameCamel}Dict.Values.ToList()))",
                            "return false"),
                        $"DataSeederTempData.Instance.SaveOldIntIdsDictToIntIds<{tableNameSingular}>({tableNameCamel}Dict.ToDictionary(k=>k.Key, v=>v.Value.{entityData.PrimaryKeyFieldName}))",
                        "return true");
                }

                createByJsonFile.AddRange(fcb.CodeItems);
            }
            else if (entityData.OptimalIndex != null)
            {
                var tupTypeList =
                    string.Join(", ", entityData.OptimalIndexFieldsData.Select(s => s.RealTypeName));
                var keyFieldsList =
                    string.Join(", ", entityData.OptimalIndexFieldsData.Select(s => $"k.{s.Name}"));
                var keyFields = entityData.OptimalIndex.Properties.Count == 1
                    ? $"k.{entityData.OptimalIndexFieldsData[0].Name}"
                    : $" new Tuple<{tupTypeList}>({keyFieldsList})";
                FlatCodeBlock fcb;
                if (entityData.SelfRecursiveField != null)
                {
                    fcb = new FlatCodeBlock(
                        $"List<{seederModelClassName}> {seedDataObjectName} = LoadFromJsonFile<{seederModelClassName}>()",
                        $"List<{tableNameSingular}> {tableNameCamel}List = Create{tableNameCapitalCamel}List({seedDataObjectName})",
                        new CodeBlock($"if (!{prPref}Repo.CreateEntities({tableNameCamel}List))",
                            "return false"),
                        $"DataSeederTempData.Instance.SaveIntIdKeys<{tableNameSingular}>({tableNameCamel}List.ToDictionary(k=>{keyFields}, v=>v.{entityData.PrimaryKeyFieldName}))",
                        $"return SetParents({seedDataObjectName}, {tableNameCamel}List)");
                    var seederModelObjectName = seederModelClassName.UnCapitalize();
                    var keyFieldName = entityData.OptimalIndexFieldsData[0].Name;

                    if (entityData.SelfRecursiveField.SubstituteField is null ||
                        entityData.SelfRecursiveField.SubstituteField.Fields.Count == 0)
                        throw new Exception("entityData.SelfRecursiveField.SubstituteField is null or without fields");

                    setParentsMethod = new CodeBlock(
                        $"private bool SetParents(List<{seederModelClassName}> {seedDataObjectName}, List<{tableNameSingular}> {tableNameCamel}List)",
                        "DataSeederTempData tempData = DataSeederTempData.Instance",
                        $"List<{tableNameSingular}> forUpdate = new List<{tableNameSingular}>()",
                        new CodeBlock(
                            $"foreach ({seederModelClassName} {seederModelObjectName} in {seedDataObjectName}.Where(w => w.{entityData.SelfRecursiveField.SubstituteField.Fields[0].FullName} != null))",
                            $"{tableNameSingular} oneRec = {tableNameCamel}List.SingleOrDefault(s => s.{keyFieldName} == {seederModelObjectName}.{keyFieldName})",
                            new CodeBlock("if (oneRec == null)",
                                "continue"),
                            $"oneRec.{entityData.SelfRecursiveField.Name} = tempData.GetIntIdByKey<{tableNameSingular}>({seederModelObjectName}.{entityData.SelfRecursiveField.SubstituteField.Fields[0].FullName})",
                            "forUpdate.Add(oneRec)"),
                        $"return {prPref}Repo.SetUpdates(forUpdate)");
                }
                else
                {
                    fcb = new FlatCodeBlock(
                        $"List<{tableNameSingular}> {tableNameCamel}List = Create{tableNameCapitalCamel}List(LoadFromJsonFile<{seederModelClassName}>())",
                        new CodeBlock($"if (!{prPref}Repo.CreateEntities({tableNameCamel}List))",
                            "return false"),
                        $"DataSeederTempData.Instance.SaveIntIdKeys<{tableNameSingular}>({tableNameCamel}List.ToDictionary(k=>{keyFields}, v=>v.{entityData.PrimaryKeyFieldName}))",
                        "return true");
                }

                createByJsonFile.AddRange(fcb.CodeItems);
            }
            else
            {
                createByJsonFile.Add(new CodeCommand(
                    $"return {prPref}Repo.CreateEntities(Create{tableNameCapitalCamel}List(LoadFromJsonFile<{seederModelClassName}>()))"));
            }

            var fieldsListStr = string.Join(", ",
                entityData.FieldsData
                    .Where(w => (entityData.SelfRecursiveField == null ||
                                 w.Name != entityData.SelfRecursiveField.Name) &&
                                (entityData.UsePrimaryKey || entityData.PrimaryKeyFieldName != w.Name))
                    .Select(p => $"{p.Name} = {GetRightValue(p)}"));

            if (entityData.NeedsToCreateTempData)
                createMethod = new CodeBlock(
                    $"protected virtual Dictionary<int, {tableNameSingular}> Create{tableNameCapitalCamel}List(List<{seederModelClassName}> {seedDataObjectName})",
                    atLeastOneSubstitute ? "DataSeederTempData tempData = DataSeederTempData.Instance" : null,
                    $"return {seedDataObjectName}.ToDictionary(k => k.{entityData.PrimaryKeyFieldName}, s => new {tableNameSingular}{(fieldsListStr == string.Empty ? "()" : $"{{ {fieldsListStr} }}")})");
            else
                createMethod = new CodeBlock(
                    $"protected virtual List<{tableNameSingular}> Create{tableNameCapitalCamel}List(List<{seederModelClassName}> {seedDataObjectName})",
                    atLeastOneSubstitute ? "DataSeederTempData tempData = DataSeederTempData.Instance" : null,
                    $"return {seedDataObjectName}.Select(s => new {tableNameSingular}{{ {fieldsListStr} }}).ToList()");
            usedList = true;
        }

        var block = new CodeBlock("",
            new OneLineComment($"Created by {GetType().Name} at {DateTime.Now}"),
            !isCarcassType && entityData.OptimalIndex != null && entityData.OptimalIndexFieldsData.Count > 1
                ? "using System"
                : null,
            usedList ? "using System.Collections.Generic" : null,
            !isCarcassType ? "using System.Linq" : null,
            isCarcassType || (!isCarcassType && (entityData.NeedsToCreateTempData || atLeastOneSubstitute ||
                                                 entityData.OptimalIndex != null ||
                                                 entityData.SelfRecursiveField != null))
                ? "using CarcassDataSeeding"
                : null,
            !isCarcassType ? $"using {_parameters.ProjectNamespace}.{_parameters.ModelsFolderName}" : null,
            isCarcassType ? "using CarcassDataSeeding.Seeders" : null,
            isCarcassType
                ? null // "using CarcassDb.Models"
                : $"using {_parameters.DbProjectNamespace}.{_parameters.DbProjectModelsFolderName}",
            isIdentity ? "using CarcassMasterDataDom.Models" : "",
            isIdentity ? "using Microsoft.AspNetCore.Identity" : "",
            $"namespace {_parameters.ProjectNamespace}.{(isCarcassType ? _parameters.CarcassSeedersFolderName : _parameters.ProjectSeedersFolderName)}",
            "",
            new CodeBlock($"public /*open*/ class {className} : {baseClassName}",
                new CodeBlock(
                    $"public {className}({additionalParameters}string dataSeedFolder, {_parameters.DataSeederRepositoryInterfaceName} repo) : base({additionalParameters2}dataSeedFolder, repo)"),
                createByJsonFile,
                createMethod,
                setParentsMethod));
        CodeFile.FileName = className + ".cs";
        CodeFile.AddRange(block.CodeItems);

        CreateFile();
    }
}