using System.Collections.Generic;
using CodeTools;
using DbContextAnalyzer.CodeCreators;
using DbContextAnalyzer.Models;
using Microsoft.Extensions.Logging;

namespace LibSeedCodeCreator;

public sealed class SeedProjectDbProgramCreator(CreatorCreatorParameters par, ILogger logger)
{
    public void Go()
    {
        //var dataSeedingProjectName = $"{_par.DbProjectNamespace}DataSeeding"; //GeoModelDataSeeding
        //var dataSeedingNewProjectName = $"{_par.DbProjectNamespace}NewDataSeeding"; //GeoModelNewDataSeeding
        //string dbProjectName = _par.ProjectPrefix + "Db"; //GeoModelDb
        //string dbContextClassName = _par.ProjectPrefix + "DbContext"; //GeoModelDbContext
        var repositoryClassName = $"{par.ProjectPrefixShort}DataSeederRepository"; //GmDataSeederRepository
        var repositoryInterfaceName = $"I{repositoryClassName}"; //IGmDataSeederRepository
        var repositoryObjectName = $"{par.ProjectPrefixShort.ToLower()}DataSeederRepository"; //gmDataSeederRepository
        var projectNewDataSeedersFabric = $"{par.ProjectPrefixShort}NewDataSeedersFabric"; //GmNewDataSeedersFabric


        var fcbSeedProjectDbProgramCreatorUsing = new FlatCodeBlock(
            "using CarcassDataSeeding",
            "using CarcassMasterDataDom.Models",
            "using Microsoft.AspNetCore.Identity",
            $"using Seed{par.DbProjectNamespace}",
            $"using {par.DbProjectNamespace}DataSeeding",
            $"using {par.DbProjectNamespace}NewDataSeeding");

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
                "return 6"),
            "",
            "var roleManager = serviceProvider.GetService<RoleManager<AppRole>>()",
            "",
            new CodeBlock("if (roleManager is null)",
                "StShared.WriteErrorLine(\"roleManager is null\", true)",
                "return 8"),
            "",
            $"var {repositoryObjectName} =  serviceProvider.GetService<{repositoryInterfaceName}>()",
            "",
            new CodeBlock($"if ({repositoryObjectName} is null)",
                $"StShared.WriteErrorLine(\"{repositoryObjectName} is null\", true)",
                "return 9"),
            "",
            "var dataFixRepository =  serviceProvider.GetService<IDataFixRepository>()",
            "",
            new CodeBlock("if (dataFixRepository is null)",
                "StShared.WriteErrorLine(\"dataFixRepository is null\", true)",
                "return 10"),
            "",
            "var carcassDataSeeder = serviceProvider.GetService<ILogger<CarcassDataSeeder>>()",
            "",
            new CodeBlock("if (carcassDataSeeder is null)",
                "StShared.WriteErrorLine(\"carcassDataSeeder is null\", true)",
                "return 11"),
            "",
            new CodeBlock("if (string.IsNullOrWhiteSpace(par.SecretDataFolder))",
                "StShared.WriteErrorLine(\"par.SecretDataFolder is empty\", true)",
                "return 12"),
            "",
            new CodeBlock("if (string.IsNullOrWhiteSpace(par.JsonFolderName))",
                "StShared.WriteErrorLine(\"par.JsonFolderName is empty\", true)",
                "return 13"),
            "",
            "var checkOnly = argParser.Switches.Contains(\"--CheckOnly\")",
            "",
            $"var seeder = new ProjectNewDataSeeder(carcassDataSeeder, new {projectNewDataSeedersFabric}(userManager, roleManager, par.SecretDataFolder, par.JsonFolderName, {repositoryObjectName}), dataFixRepository, checkOnly)",
            "",
            "var seedDataResult = seeder.SeedData()",
            "",
            new CodeBlock("if (seedDataResult.IsSome)",
                new CodeBlock("foreach (var err in (Err[])seedDataResult)",
                    "logger.LogError(err.ErrorMessage)"),
                "return 1"),
            "",
            "return 0",
            "");

        var fcbGetJsonMainServiceCreatorCodeCommands = new FlatCodeBlock("",
            new CodeBlock("if (string.IsNullOrWhiteSpace(par.ConnectionStringSeed))",
                "StShared.WriteErrorLine(\"par.ConnectionStringSeed is empty\", true)",
                "return 3"),
            "",
            $"var servicesCreator = new SeedDbServicesCreator(par.LogFolder, null, \"{par.SeedProjectNamespace}\", par.ConnectionStringSeed)"
        );

        var seedProjectDbProgramCreator = new ConsoleProgramCreator(logger, fcbSeedProjectDbProgramCreatorUsing,
            fcbGetJsonMainServiceCreatorCodeCommands, fcbGetJsonMainCommands, "SeederParameters",
            par.SeedProjectNamespace, "Seeds data in new database", par.SeedProjectPlacePath,
            new List<string> { "CheckOnly" }, "Program.cs");
        seedProjectDbProgramCreator.CreateFileStructure();

        //var fakeStartUpCreator = new FakeStartUpCreator(_logger, _par);
        //fakeStartUpCreator.CreateFileStructure();

        //var projectDesignTimeDbContextFactoryCreator = new ProjectDesignTimeDbContextFactoryCreator(_logger, _par);
        //projectDesignTimeDbContextFactoryCreator.CreateFileStructure();

        var serviceCreatorCreator = new ServiceCreatorCreator(logger, par);
        serviceCreatorCreator.CreateFileStructure();
    }
}