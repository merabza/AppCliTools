using CodeTools;
using DbContextAnalyzer.CodeCreators;
using DbContextAnalyzer.Models;
using Microsoft.Extensions.Logging;

namespace LibSeedCodeCreator;

public sealed class SeedProjectDbProgramCreator
{
    private readonly ILogger _logger;
    private readonly CreatorCreatorParameters _par;

    public SeedProjectDbProgramCreator(CreatorCreatorParameters par, ILogger logger)
    {
        _par = par;
        _logger = logger;
    }


    public void Go()
    {
        //var dataSeedingProjectName = $"{_par.DbProjectNamespace}DataSeeding"; //GeoModelDataSeeding
        //var dataSeedingNewProjectName = $"{_par.DbProjectNamespace}NewDataSeeding"; //GeoModelNewDataSeeding
        //string dbProjectName = _par.ProjectPrefix + "Db"; //GeoModelDb
        //string dbContextClassName = _par.ProjectPrefix + "DbContext"; //GeoModelDbContext
        var repositoryClassName = $"{_par.ProjectPrefixShort}DataSeederRepository"; //GmDataSeederRepository
        var repositoryInterfaceName = $"I{repositoryClassName}"; //IGmDataSeederRepository
        var repositoryObjectName = $"{_par.ProjectPrefixShort.ToLower()}DataSeederRepository"; //gmDataSeederRepository
        var projectNewDataSeedersFabric = $"{_par.ProjectPrefixShort}NewDataSeedersFabric"; //GmNewDataSeedersFabric


        var fcbSeedProjectDbProgramCreatorUsing = new FlatCodeBlock(
            "using CarcassDataSeeding",
            "using CarcassMasterDataDom.Models",
            "using Microsoft.AspNetCore.Identity",
            $"using Seed{_par.DbProjectNamespace}",
            $"using {_par.DbProjectNamespace}DataSeeding",
            $"using {_par.DbProjectNamespace}NewDataSeeding");

        var fcbGetJsonMainCommands = new FlatCodeBlock(
            //"",
            //$"DbContextOptionsBuilder<{dbContextClassName}> optionsBuilder = new DbContextOptionsBuilder<{dbContextClassName}>()",
            //"optionsBuilder.UseSqlServer(par.ConnectionStringSeed)",
            //"",
            //$"using {dbContextClassName} context = new {dbContextClassName}(optionsBuilder.Options, null)",
            "",
            "var userManager = serviceProvider.GetService<UserManager<AppUser>>()",
            "",
            new CodeBlock("if (userManager is null)",
                "StShared.WriteErrorLine(\"userManager is null\", true)",
                "return 3"),
            "",
            "var roleManager = serviceProvider.GetService<RoleManager<AppRole>>()",
            "",
            new CodeBlock("if (roleManager is null)",
                "StShared.WriteErrorLine(\"roleManager is null\", true)",
                "return 3"),
            "",
            $"{repositoryInterfaceName}? {repositoryObjectName} =  serviceProvider.GetService<{repositoryInterfaceName}>()",
            "",
            new CodeBlock($"if ({repositoryObjectName} is null)",
                $"StShared.WriteErrorLine(\"{repositoryObjectName} is null\", true)",
                "return 3"),
            "",
            "IDataFixRepository? dataFixRepository =  serviceProvider.GetService<IDataFixRepository>()",
            "",
            new CodeBlock("if (dataFixRepository is null)",
                "StShared.WriteErrorLine(\"dataFixRepository is null\", true)",
                "return 3"),
            "",
            "var carcassDataSeeder = serviceProvider.GetService<ILogger<CarcassDataSeeder>>()",
            "",
            new CodeBlock("if (carcassDataSeeder is null)",
                "StShared.WriteErrorLine(\"carcassDataSeeder is null\", true)",
                "return 3"),
            "",
            new CodeBlock("if (string.IsNullOrWhiteSpace(par.SecretDataFolder))",
                "StShared.WriteErrorLine(\"par.SecretDataFolder is empty\", true)",
                "return 3"),
            "",
            new CodeBlock("if (string.IsNullOrWhiteSpace(par.JsonFolderName))",
                "StShared.WriteErrorLine(\"par.JsonFolderName is empty\", true)",
                "return 3"),
            "",
            $"ProjectNewDataSeeder seeder = new ProjectNewDataSeeder(carcassDataSeeder, new {projectNewDataSeedersFabric}(userManager, roleManager, par.SecretDataFolder, par.JsonFolderName, {repositoryObjectName}), dataFixRepository)",
            new CodeBlock("if (seeder.SeedData())",
                "return 0"),
            new CodeBlock("foreach (string mes in seeder.Messages)",
                "logger.LogInformation(mes)"),
            "",
            "return 1",
            "");

        var fcbGetJsonMainServiceCreatorCodeCommands = new FlatCodeBlock("",
            new CodeBlock("if (string.IsNullOrWhiteSpace(par.ConnectionStringSeed))",
                "StShared.WriteErrorLine(\"par.ConnectionStringSeed is empty\", true)",
                "return 3"),
            "",
            "SeedDbServicesCreator servicesCreator = new SeedDbServicesCreator(par.LogFolder, null, \"SeedAppGrammarGeDb\", par.ConnectionStringSeed)"
        );

        var seedProjectDbProgramCreator = new ConsoleProgramCreator(_logger, fcbSeedProjectDbProgramCreatorUsing,
            fcbGetJsonMainServiceCreatorCodeCommands,
            fcbGetJsonMainCommands, "SeederParameters", _par.SeedProjectNamespace, "Seeds data in new database",
            _par.SeedProjectPlacePath, "Program.cs");
        seedProjectDbProgramCreator.CreateFileStructure();

        //var fakeStartUpCreator = new FakeStartUpCreator(_logger, _par);
        //fakeStartUpCreator.CreateFileStructure();

        //var projectDesignTimeDbContextFactoryCreator = new ProjectDesignTimeDbContextFactoryCreator(_logger, _par);
        //projectDesignTimeDbContextFactoryCreator.CreateFileStructure();

        var serviceCreatorCreator = new ServiceCreatorCreator(_logger, _par);
        serviceCreatorCreator.CreateFileStructure();
    }
}