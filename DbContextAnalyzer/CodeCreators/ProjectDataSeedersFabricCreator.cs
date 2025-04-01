using System;
using CodeTools;
using DbContextAnalyzer.Models;
using Microsoft.Extensions.Logging;
using SystemToolsShared;

namespace DbContextAnalyzer.CodeCreators;

public sealed class ProjectDataSeedersFabricCreator : CodeCreator
{
    private readonly bool _isAnyCarcassType;

    private readonly SeederCodeCreatorParameters _parameters;

    private readonly CodeRegion _carcassRegion;
    private readonly CodeRegion _projectRegion;
    //private readonly FlatCodeBlock _carcassCodeBlock;
    //private readonly FlatCodeBlock _projectCodeBlock;

    // ReSharper disable once ConvertToPrimaryConstructor
    public ProjectDataSeedersFabricCreator(ILogger logger, SeederCodeCreatorParameters parameters,
        bool isAnyCarcassType) : base(logger, parameters.PlacePath,
        $"{parameters.ProjectDataSeedersFabricClassName}.cs")
    {
        _parameters = parameters;
        _isAnyCarcassType = isAnyCarcassType;
        _carcassRegion = new CodeRegion("Carcass");
        _projectRegion = new CodeRegion(_parameters.ProjectPrefix);
    }

    public override void CreateFileStructure()
    {
        //_carcassRegion = new CodeRegion("Carcass");
        //_projectRegion = new CodeRegion(_parameters.ProjectPrefix);

        var block = new CodeBlock(string.Empty, new OneLineComment($"Created by {GetType().Name} at {DateTime.Now}"),
            "using CarcassDataSeeding", "using CarcassMasterDataDom.Models", "using DatabaseToolsShared",
            "using Microsoft.AspNetCore.Identity",
            _isAnyCarcassType
                ? $"using {_parameters.ProjectNamespace}.{_parameters.CarcassSeedersFolderName}"
                : string.Empty, $"using {_parameters.ProjectNamespace}.{_parameters.ProjectSeedersFolderName}",
            $"namespace {_parameters.ProjectNamespace}", string.Empty, new CodeBlock(
                $"public /*open*/ class {_parameters.ProjectDataSeedersFabricClassName} : CarcassDataSeedersFabric",
                $"protected readonly {_parameters.DataSeederRepositoryInterfaceName} Repo", new CodeBlock($"""
                     public {_parameters.ProjectDataSeedersFabricClassName}(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager, 
                       string secretDataFolder, string dataSeedFolder, ICarcassDataSeederRepository carcassRepo, {_parameters.DataSeederRepositoryInterfaceName} repo) : base(userManager, roleManager, 
                       secretDataFolder, dataSeedFolder, carcassRepo, repo)
                     """, "Repo = repo"), _carcassRegion, _projectRegion));
        CodeFile.AddRange(block.CodeItems);
    }

    public void UseEntity(EntityData entityData, bool isCarcassType)
    {
        var tableName = entityData.TableName;
        var tableNameCapitalCamel = tableName.CapitalizeCamel();
        var seederClassName = tableNameCapitalCamel + "Seeder";
        var newClassName = (isCarcassType ? _parameters.ProjectPrefixShort : string.Empty) + seederClassName;

        var additionalParameters = tableName switch
        {
            "roles" => "MyRoleManager, SecretDataFolder, ",
            "users" => "MyUserManager, SecretDataFolder, ",
            "manyToManyJoins" => "SecretDataFolder, ",
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