using System.Collections.Generic;
using System.Globalization;
using AppCliTools.CodeTools;
using AppCliTools.DbContextAnalyzer.CodeCreators;
using AppCliTools.DbContextAnalyzer.Models;
using Microsoft.Extensions.Logging;

namespace AppCliTools.LibSeedCodeCreator;

public sealed class SeedProjectDbProgramCreator(CreatorCreatorParameters par, ILogger logger)
{
    public void Go()
    {
        string repositoryClassName = $"{par.ProjectPrefixShort}DataSeederRepository"; //GmDataSeederRepository
        string repositoryInterfaceName = $"I{repositoryClassName}"; //IGmDataSeederRepository
        string repositoryObjectName =
            $"{par.ProjectPrefixShort.ToLower(CultureInfo.CurrentCulture)}DataSeederRepository"; //gmDataSeederRepository
        string projectNewDataSeedersFactory =
            $"{par.ProjectPrefixShort}NewDataSeedersFactory"; //GmNewDataSeedersFactory

        var fcbSeedProjectDbProgramCreatorUsing = new FlatCodeBlock("using CarcassDataSeeding",
            "using CarcassMasterDataDom.Models", "using Microsoft.AspNetCore.Identity", "using LibDatabaseParameters",
            $"using Seed{par.DbProjectNamespace}", $"using {par.DbProjectNamespace}DataSeeding",
            $"using {par.DbProjectNamespace}NewDataSeeding");

        var fcbGetJsonMainCommands = new FlatCodeBlock(string.Empty,
            new OneLineComment(" ReSharper disable once using"),
            "var userManager = serviceProvider.GetService<UserManager<AppUser>>()", string.Empty,
            new CodeBlock("if (userManager is null)", "StShared.WriteErrorLine(\"userManager is null\", true)",
                "return 6"), string.Empty, new OneLineComment(" ReSharper disable once using"),
            "var roleManager = serviceProvider.GetService<RoleManager<AppRole>>()", string.Empty,
            new CodeBlock("if (roleManager is null)", "StShared.WriteErrorLine(\"roleManager is null\", true)",
                "return 8"), string.Empty,
            "var carcassRepo = serviceProvider.GetService<ICarcassDataSeederRepository>()",
            new CodeBlock("if (carcassRepo is null)", "StShared.WriteErrorLine(\"carcassRepo is null\", true)",
                "return 9"), string.Empty,
            $"var {repositoryObjectName} =  serviceProvider.GetService<{repositoryInterfaceName}>()", string.Empty,
            new CodeBlock($"if ({repositoryObjectName} is null)",
                $"StShared.WriteErrorLine(\"{repositoryObjectName} is null\", true)", "return 9"), string.Empty,
            "var dataFixRepository =  serviceProvider.GetService<IDataFixRepository>()", string.Empty,
            new CodeBlock("if (dataFixRepository is null)",
                "StShared.WriteErrorLine(\"dataFixRepository is null\", true)", "return 10"), string.Empty,
            "var carcassDataSeeder = serviceProvider.GetService<ILogger<CarcassDataSeeder>>()", string.Empty,
            new CodeBlock("if (carcassDataSeeder is null)",
                "StShared.WriteErrorLine(\"carcassDataSeeder is null\", true)", "return 11"), string.Empty,
            new CodeBlock("if (string.IsNullOrWhiteSpace(par.SecretDataFolder))",
                "StShared.WriteErrorLine(\"par.SecretDataFolder is empty\", true)", "return 12"), string.Empty,
            new CodeBlock("if (string.IsNullOrWhiteSpace(par.JsonFolderName))",
                "StShared.WriteErrorLine(\"par.JsonFolderName is empty\", true)", "return 13"), string.Empty,
            string.Empty, "var checkOnly = argParser.Switches.Contains(\"--CheckOnly\")", string.Empty,
            $"var seeder = new ProjectNewDataSeeder(carcassDataSeeder, new {projectNewDataSeedersFactory}(userManager, roleManager, par.SecretDataFolder, par.JsonFolderName, carcassRepo, {repositoryObjectName}), dataFixRepository, checkOnly)",
            string.Empty, "return seeder.SeedData() ? 0 : 1", string.Empty);

        var fcbGetJsonMainServiceCreatorCodeCommands = new FlatCodeBlock(string.Empty,
            new CodeBlock("if (string.IsNullOrWhiteSpace(par.ConnectionStringSeed))",
                "StShared.WriteErrorLine(\"par.ConnectionStringSeed is empty\", true)", "return 3"), string.Empty,
            new CodeBlock("if (string.IsNullOrWhiteSpace(par.ExcludesRulesParametersFilePath))",
                "StShared.WriteErrorLine(\"par.ExcludesRulesParametersFilePath is empty\", true)", "return 13"),
            string.Empty,
            $"var servicesCreator = new SeedDbServicesCreator(par.LogFolder, null, \"{par.SeedProjectNamespace}\", par.ConnectionStringSeed, par.ExcludesRulesParametersFilePath)");

        var seedProjectDbProgramCreator = new ConsoleProgramCreator(logger, fcbSeedProjectDbProgramCreatorUsing,
            fcbGetJsonMainServiceCreatorCodeCommands, fcbGetJsonMainCommands, "SeederParameters",
            par.SeedProjectNamespace, "Seeds data in new database", par.SeedProjectPlacePath,
            new List<string> { "CheckOnly" }, "Program.cs");
        seedProjectDbProgramCreator.CreateFileStructure();

        var serviceCreatorCreator = new ServiceCreatorCreator(logger, par);
        serviceCreatorCreator.CreateFileStructure();
    }
}
