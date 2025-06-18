using System;
using CodeTools;
using DbContextAnalyzer.Domain;
using DbContextAnalyzer.Models;
using Microsoft.Extensions.Logging;
using SystemToolsShared;

namespace DbContextAnalyzer.CodeCreators;

public sealed class ProjectDataSeedersFactoryCreator : SeederCodeCreatorBase
{
    private readonly CodeRegion _carcassRegion;
    private readonly SeederCodeCreatorParameters _parameters;
    private readonly CodeRegion _projectRegion;

    // ReSharper disable once ConvertToPrimaryConstructor
    public ProjectDataSeedersFactoryCreator(ILogger logger, SeederCodeCreatorParameters parameters,
        ExcludesRulesParametersDomain excludesRulesParameters) : base(logger, excludesRulesParameters,
        parameters.PlacePath, $"{parameters.ProjectDataSeedersFactoryClassName}.cs")
    {
        _parameters = parameters;
        _carcassRegion = new CodeRegion("Carcass");
        _projectRegion = new CodeRegion(_parameters.ProjectPrefix);
    }

    public override void CreateFileStructure()
    {
        var block = new CodeBlock(string.Empty, new OneLineComment($"Created by {GetType().Name} at {DateTime.Now}"),
            "using CarcassDataSeeding", "using CarcassMasterDataDom.Models", "using DatabaseToolsShared",
            "using Microsoft.AspNetCore.Identity",
            $"using {_parameters.ProjectNamespace}.{_parameters.CarcassSeedersFolderName}",
            $"using {_parameters.ProjectNamespace}.{_parameters.ProjectSeedersFolderName}",
            $"namespace {_parameters.ProjectNamespace}", string.Empty, new CodeBlock(
                $"public /*open*/ class {_parameters.ProjectDataSeedersFactoryClassName} : CarcassDataSeedersFactory",
                $"protected readonly {_parameters.DataSeederRepositoryInterfaceName} Repo",
                new OneLineComment(" ReSharper disable once ConvertToPrimaryConstructor"), new CodeBlock($"""
                     protected {_parameters.ProjectDataSeedersFactoryClassName}(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager, 
                       string secretDataFolder, string dataSeedFolder, ICarcassDataSeederRepository carcassRepo, 
                       {_parameters.DataSeederRepositoryInterfaceName} repo) : base(userManager, roleManager, secretDataFolder, dataSeedFolder, carcassRepo, 
                       repo)
                     """, "Repo = repo"), _carcassRegion, _projectRegion));
        CodeFile.AddRange(block.CodeItems);
    }

    public void UseEntity(EntityData entityData, bool isCarcassType)
    {
        var tableName = GetNewTableName(entityData.TableName);
        var tableNameCapitalCamel = tableName.CapitalizeCamel();
        var seederClassName = tableNameCapitalCamel + "Seeder";
        var newClassName = (isCarcassType ? _parameters.ProjectPrefixShort : string.Empty) + seederClassName;

        var additionalParameters = tableName switch
        {
            "roles" => "MyRoleManager, SecretDataFolder, ",
            "users" => "MyUserManager, SecretDataFolder, ",
            "manyToManyJoins" => "SecretDataFolder, CarcassRepo, ",
            "dataTypes" => "CarcassRepo, ",
            _ => string.Empty
        };

        var block = new CodeBlock(
            $"public {(isCarcassType ? "override " : "virtual ")}ITableDataSeeder Create{seederClassName}()",
            $"return new {newClassName}({additionalParameters}DataSeedFolder, Repo)");

        if (isCarcassType)
            _carcassRegion.Add(block);
        else
            _projectRegion.Add(block);
    }
}