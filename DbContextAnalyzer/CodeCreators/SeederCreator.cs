using System;
using System.Linq;
using CodeTools;
using DbContextAnalyzer.Domain;
using DbContextAnalyzer.Models;
using Microsoft.Extensions.Logging;
using SystemToolsShared;

namespace DbContextAnalyzer.CodeCreators;

public sealed class SeederCreator(
    ILogger logger,
    SeederCodeCreatorParameters parameters,
    ExcludesRulesParametersDomain excludesRulesParameters,
    string placePath) : CodeCreator(logger, placePath)
{
    public override void CreateFileStructure()
    {
    }

    private string GetRightValue(FieldData fieldData)
    {
        if (fieldData.SubstituteField == null)
            return $"s.{fieldData.FullName}";

        var substituteTableNameCapitalCamel = GetTableNameSingularCapitalizeCamel(
            excludesRulesParameters.SingularityExceptions, fieldData.SubstituteField.TableName);
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
        var tableNameCapitalCamel = tableName.CapitalizeCamel();
        var tableNameCamel = tableName.Camelize();

        var tableNameSingular =
            GetTableNameSingularCapitalizeCamel(excludesRulesParameters.SingularityExceptions, tableName);
        var className = (isCarcassType ? parameters.ProjectPrefixShort : string.Empty) + tableNameCapitalCamel +
                        "Seeder";
        var baseClassName = isCarcassType
            ? tableNameCapitalCamel + "Seeder"
            : $"{parameters.DataSeederBaseClassName}<{tableNameSingular}>";
        var seederModelClassName = tableNameSingular + "SeederModel";
        var seedDataObjectName = tableName.UnCapitalize() + "SeedData";
        var prPref = isCarcassType ? string.Empty : parameters.ProjectPrefixShort;

        var isIdentity = tableName is "roles" or "users";

        var additionalParameters = tableName switch
        {
            "manyToManyJoins" => "string secretDataFolder, ",
            "roles" => "RoleManager<AppRole> roleManager, string secretDataFolder, ",
            "users" => "UserManager<AppUser> userManager, string secretDataFolder, ",
            _ => string.Empty
        };


        var additionalParameters2 = tableName switch
        {
            "manyToManyJoins" => "secretDataFolder, ",
            "roles" => "roleManager, secretDataFolder, ",
            "users" => "userManager, secretDataFolder, ",
            _ => string.Empty
        };

        CodeBlock? setParentsMethod = null;
        CodeBlock? createByJsonFile = null;
        CodeBlock? createMethod = null;

        var atLeastOneSubstitute = entityData.FieldsData
            .Where(w => entityData.SelfRecursiveField == null || w.Name != entityData.SelfRecursiveField.Name)
            .Any(w => w.SubstituteField != null);

        if (!isCarcassType)
        {
            createByJsonFile = new CodeBlock("protected override Option<IEnumerable<Err>> CreateByJsonFile()");
            if (entityData.NeedsToCreateTempData)
            {
                FlatCodeBlock fcb;
                if (entityData.SelfRecursiveField != null)
                {
                    var seederModelObjectName = seederModelClassName.UnCapitalize();
                    fcb = new FlatCodeBlock($"var {seedDataObjectName} = LoadFromJsonFile<{seederModelClassName}>()",
                        $"var {tableNameCamel}Dict = Create{tableNameCapitalCamel}List({seedDataObjectName})",
                        new CodeBlock($"if (!{prPref}Repo.CreateEntities({tableNameCamel}Dict.Values.ToList()))",
                            $"return new Err[] {{ new() {{ ErrorCode = \"{seederModelClassName}EntitiesCannotBeCreated\", ErrorMessage = \"{seederModelClassName} entities cannot be created\" }} }}"),
                        $"var idsDict = {tableNameCamel}Dict.ToDictionary(k => k.Key, v => v.Value.{entityData.PrimaryKeyFieldName})",
                        $"DataSeederTempData.Instance.SaveOldIntIdsDictToIntIds<{tableNameSingular}>(idsDict)",
                        new CodeBlock(
                            $"foreach ({seederModelClassName} {seederModelObjectName} in {seedDataObjectName}.Where(w => w.{entityData.SelfRecursiveField.Name} != null))",
                            $"{tableNameCamel}Dict[{seederModelObjectName}.{entityData.PrimaryKeyFieldName}].{entityData.SelfRecursiveField.Name} = idsDict[{seederModelObjectName}.{entityData.SelfRecursiveField.Name}!.Value]"),
                        new CodeBlock($"if (!{prPref}Repo.SaveChanges() )",
                            $"return new Err[] {{ new() {{ ErrorCode = \"{seederModelClassName}CannotBeSaved\", ErrorMessage = \"{seederModelClassName} entities cannot be saved\" }} }}"),
                        "return null");
                }
                else
                {
                    fcb = new FlatCodeBlock(
                        $"var {tableNameCamel}Dict = Create{tableNameCapitalCamel}List(LoadFromJsonFile<{seederModelClassName}>())",
                        new CodeBlock($"if (!{prPref}Repo.CreateEntities({tableNameCamel}Dict.Values.ToList()))",
                            $"return new Err[] {{ new() {{ ErrorCode = \"{seederModelClassName}EntitiesCannotBeCreated\", ErrorMessage = \"{seederModelClassName} entities cannot be created\" }} }}"),
                        $"DataSeederTempData.Instance.SaveOldIntIdsDictToIntIds<{tableNameSingular}>({tableNameCamel}Dict.ToDictionary(k=>k.Key, v=>v.Value.{entityData.PrimaryKeyFieldName}))",
                        "return null");
                }

                createByJsonFile.AddRange(fcb.CodeItems);
            }
            else if (entityData.OptimalIndex != null)
            {
                var tupTypeList = string.Join(", ", entityData.OptimalIndexFieldsData.Select(s => s.RealTypeName));
                var keyFieldsList = string.Join(", ", entityData.OptimalIndexFieldsData.Select(s => $"k.{s.Name}"));
                var keyFields = entityData.OptimalIndex.Properties.Count == 1
                    ? $"k.{entityData.OptimalIndexFieldsData[0].Name}"
                    : $" new Tuple<{tupTypeList}>({keyFieldsList})";
                FlatCodeBlock fcb;
                if (entityData.SelfRecursiveField != null)
                {
                    fcb = new FlatCodeBlock($"var {seedDataObjectName} = LoadFromJsonFile<{seederModelClassName}>()",
                        $"var {tableNameCamel}List = Create{tableNameCapitalCamel}List({seedDataObjectName})",
                        new CodeBlock($"if (!{prPref}Repo.CreateEntities({tableNameCamel}List))",
                            $"return new Err[] {{ new() {{ ErrorCode = \"{seederModelClassName}EntitiesCannotBeCreated\", ErrorMessage = \"{seederModelClassName} entities cannot be created\" }} }}"),
                        $"DataSeederTempData.Instance.SaveIntIdKeys<{tableNameSingular}>({tableNameCamel}List.ToDictionary(k=>{keyFields}, v=>v.{entityData.PrimaryKeyFieldName}))",
                        new CodeBlock($"if (!SetParents({seedDataObjectName}, {tableNameCamel}List))",
                            $"return new Err[] {{ new() {{ ErrorCode = \"{seederModelClassName}CannotSetParents\", ErrorMessage = \"{seederModelClassName} cannot Set Parents\" }} }}"),
                        "return null");
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
                            "forUpdate.Add(oneRec)"),
                        new CodeBlock($"if (!{prPref}Repo.SetUpdates(forUpdate))",
                            $"return new Err[] {{ new() {{ ErrorCode = \"{seederModelClassName}CannotSetUpdates\", ErrorMessage = \"{seederModelClassName} cannot Set Updates\" }} }}"),
                        "return null");
                }
                else
                {
                    fcb = new FlatCodeBlock(
                        $"var {tableNameCamel}List = Create{tableNameCapitalCamel}List(LoadFromJsonFile<{seederModelClassName}>())",
                        new CodeBlock($"if (!{prPref}Repo.CreateEntities({tableNameCamel}List))",
                            $"return new Err[] {{ new() {{ ErrorCode = \"{seederModelClassName}EntitiesCannotBeCreated\", ErrorMessage = \"{seederModelClassName} entities cannot be created\" }} }}"),
                        $"DataSeederTempData.Instance.SaveIntIdKeys<{tableNameSingular}>({tableNameCamel}List.ToDictionary(k=>{keyFields}, v=>v.{entityData.PrimaryKeyFieldName}))",
                        "return null");
                }

                createByJsonFile.AddRange(fcb.CodeItems);
            }
            else
            {
                createByJsonFile.Add(new FlatCodeBlock(
                    new CodeBlock(
                        $"if (!{prPref}Repo.CreateEntities(Create{tableNameCapitalCamel}List(LoadFromJsonFile<{seederModelClassName}>())))",
                        $"return new Err[] {{ new() {{ ErrorCode = \"{seederModelClassName}EntitiesCannotBeCreated\", ErrorMessage = \"{seederModelClassName} entities cannot be created\" }} }}"),
                    "return null"));
            }

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
            !isCarcassType && entityData.OptimalIndex is not null && entityData.OptimalIndexFieldsData.Count > 1
                ? "using System"
                : null, usedList ? "using System.Collections.Generic" : null,
            !isCarcassType ? "using System.Linq" : null,
            isCarcassType || (!isCarcassType && (entityData.NeedsToCreateTempData || atLeastOneSubstitute ||
                                                 entityData.OptimalIndex != null ||
                                                 entityData.SelfRecursiveField != null))
                ? "using CarcassDataSeeding"
                : null, !isCarcassType ? $"using {parameters.ProjectNamespace}.{parameters.ModelsFolderName}" : null,
            isCarcassType ? "using CarcassDataSeeding.Seeders" : null, isCarcassType
                ? "using CarcassDb.Models"
                : $"using {parameters.DbProjectNamespace}.{parameters.DbProjectModelsFolderName}",
            isIdentity ? "using CarcassMasterDataDom.Models" : string.Empty,
            isIdentity ? "using Microsoft.AspNetCore.Identity" : string.Empty, "using LanguageExt",
            "using SystemToolsShared.Errors",
            $"namespace {parameters.ProjectNamespace}.{(isCarcassType ? parameters.CarcassSeedersFolderName : parameters.ProjectSeedersFolderName)}",
            string.Empty,
            new CodeBlock($"public /*open*/ class {className} : {baseClassName}",
                new OneLineComment(" ReSharper disable once ConvertToPrimaryConstructor"),
                new CodeBlock(
                    $"public {className}({additionalParameters}string dataSeedFolder, {parameters.DataSeederRepositoryInterfaceName} repo) : base({additionalParameters2}dataSeedFolder, repo)"),
                createByJsonFile, createMethod, setParentsMethod));
        CodeFile.FileName = className + ".cs";
        CodeFile.AddRange(block.CodeItems);

        FinishAndSave();
    }
}