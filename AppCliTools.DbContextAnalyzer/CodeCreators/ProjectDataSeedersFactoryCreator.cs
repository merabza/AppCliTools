using System;
using AppCliTools.CodeTools;
using AppCliTools.DbContextAnalyzer.Domain;
using AppCliTools.DbContextAnalyzer.Models;
using Microsoft.Extensions.Logging;
using SystemTools.SystemToolsShared;

namespace AppCliTools.DbContextAnalyzer.CodeCreators;

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
            "using BackendCarcass.DataSeeding", "using BackendCarcass.MasterData.Models",
            "using SystemTools.DatabaseToolsShared", "using Microsoft.AspNetCore.Identity",
            $"using {_parameters.ProjectNamespace}.{_parameters.CarcassSeedersFolderName}",
            $"using {_parameters.ProjectNamespace}.{_parameters.ProjectSeedersFolderName}",
            "using SystemTools.DomainShared.Repositories", string.Empty, $"namespace {_parameters.ProjectNamespace}",
            string.Empty, new CodeBlock(
                $"public /*open*/ class {_parameters.ProjectDataSeedersFactoryClassName} : CarcassDataSeedersFactory",
                $"protected readonly {_parameters.DataSeederRepositoryInterfaceName} Repo",
                new OneLineComment(" ReSharper disable once ConvertToPrimaryConstructor"), new CodeBlock($"""
                     protected {_parameters.ProjectDataSeedersFactoryClassName}(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager, 
                       string secretDataFolder, string dataSeedFolder, ICarcassDataSeederRepository carcassRepo, 
                       {_parameters.DataSeederRepositoryInterfaceName} repo, IUnitOfWork unitOfWork) : base(userManager, roleManager, secretDataFolder, dataSeedFolder, carcassRepo, 
                       repo, unitOfWork)
                     """, "Repo = repo"), _carcassRegion, _projectRegion));
        CodeFile.AddRange(block.CodeItems);
    }

    public void UseEntity(EntityData entityData, bool isCarcassType)
    {
        string tableName = GetNewTableName(entityData.TableName);
        string tableNameCapitalCamel = tableName.CapitalizeCamel();
        string seederClassName = tableNameCapitalCamel + "Seeder";
        string newClassName = (isCarcassType ? _parameters.ProjectPrefixShort : string.Empty) + seederClassName;

        string additionalParameters = tableName switch
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
        {
            _carcassRegion.Add(block);
        }
        else
        {
            _projectRegion.Add(block);
        }
    }
}
