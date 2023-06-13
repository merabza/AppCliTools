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
    private CodeRegion? _carcassRegion;
    private CodeRegion? _projectRegion;

    public ProjectDataSeedersFabricCreator(ILogger logger, SeederCodeCreatorParameters parameters,
        bool isAnyCarcassType) : base(logger, parameters.PlacePath,
        $"{parameters.ProjectDataSeedersFabricClassName}.cs")
    {
        _parameters = parameters;
        _isAnyCarcassType = isAnyCarcassType;
    }

    public override void CreateFileStructure()
    {
        _carcassRegion = new CodeRegion("Carcass");
        _projectRegion = new CodeRegion(_parameters.ProjectPrefix);

        var block = new CodeBlock("",
            new OneLineComment($"Created by {GetType().Name} at {DateTime.Now}"),
            "using CarcassDataSeeding",
            "using CarcassDataSeeding.Seeders",
            "using CarcassMasterDataDom.Models",
            "using Microsoft.AspNetCore.Identity",
            _isAnyCarcassType ? $"using {_parameters.ProjectNamespace}.{_parameters.CarcassSeedersFolderName}" : "",
            $"using {_parameters.ProjectNamespace}.{_parameters.ProjectSeedersFolderName}",
            $"namespace {_parameters.ProjectNamespace}",
            "",
            new CodeBlock($"public /*open*/ class {_parameters.ProjectDataSeedersFabricClassName} : DataSeedersFabric",
                $"protected readonly {_parameters.DataSeederRepositoryInterfaceName} Repo",
                new CodeBlock(
                    $@"public {_parameters.ProjectDataSeedersFabricClassName}(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager, 
  string secretDataFolder, string dataSeedFolder, {_parameters.DataSeederRepositoryInterfaceName} repo) : base(userManager, roleManager, 
  secretDataFolder, dataSeedFolder, repo)",
                    "Repo = repo"),
                _carcassRegion,
                _projectRegion));
        CodeFile.AddRange(block.CodeItems);
    }

    public override void UseEntity(EntityData entityData, bool isCarcassType)
    {
        var tableName = entityData.TableName;
        var tableNameCapitalCamel = tableName.CapitalizeCamel();
        var seederClassName = tableNameCapitalCamel + "Seeder";
        var newClassName = (isCarcassType ? _parameters.ProjectPrefixShort : "") + seederClassName;

        var additionalParameters = tableName switch
        {
            "roles" => "MyRoleManager, SecretDataFolder, ",
            "users" => "MyUserManager, SecretDataFolder, ",
            //"dataRights" => "SecretDataFolder, ",
            "manyToManyJoins" => "SecretDataFolder, ",
            _ => ""
        };

        var block = new CodeBlock(
            $"public {(isCarcassType ? "override " : "virtual ")}{seederClassName} Create{seederClassName}()",
            $"return new {newClassName}({additionalParameters}DataSeedFolder, Repo)");

        if (isCarcassType)
        {
            if (_carcassRegion is null)
                throw new Exception("_carcassRegion is null");
            _carcassRegion.Add(block);
        }
        else
        {
            if (_projectRegion is null)
                throw new Exception("_projectRegion is null");
            _projectRegion.Add(block);
        }
    }
}