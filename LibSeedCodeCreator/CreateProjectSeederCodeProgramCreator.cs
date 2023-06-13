using CodeTools;
using DbContextAnalyzer.CodeCreators;
using DbContextAnalyzer.Models;
using Microsoft.Extensions.Logging;

namespace LibSeedCodeCreator;

public sealed class CreateProjectSeederCodeProgramCreator // : CodeCreator
{
    private readonly ILogger _logger;
    private readonly CreatorCreatorParameters _par;

    public CreateProjectSeederCodeProgramCreator(CreatorCreatorParameters par, ILogger logger)
        //: base(logger, par.CreateSeederCodeProjectPlacePath, "Program.cs")
    {
        _par = par;
        _logger = logger;
    }

    //public override void CreateFileStructure()
    //{
    //    var dbContextProjectName = $"{_par.ProjectPrefix}ScaffoldSeederDbSc";
    //    var dbContextClassName = $"{_par.ProjectPrefix.Replace('.', '_')}DbScContext";
    //    var createProjectSeederCode = $"Create{_par.ProjectPrefix}SeederCode";
    //    var parametersClassName = "CreateProjectSeederCodeParameters";
    //    var serviceCreatorClassName = "ServicesCreator";
    //    var projectNamespace = _par.CreateSeederCodeProjectNamespace;
    //    string? addParametersForServiceCreator = null;
    //    var fcbAdditionalUsing = new FlatCodeBlock(
    //        "using DbContextAnalyzer",
    //        $"using {dbContextProjectName}",
    //        "using Microsoft.EntityFrameworkCore");


    //    var fcbMainCommands = new FlatCodeBlock(
    //        "",
    //        "SeederCodeCreatorStarter starter = new SeederCodeCreatorStarter(logger, par)",
    //        "",
    //        $"DbContextOptionsBuilder<{dbContextClassName}> optionsBuilder = new DbContextOptionsBuilder<{dbContextClassName}>()",
    //        "",
    //        new CodeBlock("if (string.IsNullOrWhiteSpace(par.ConnectionStringProd))",
    //            "StShared.WriteErrorLine(\"lConnectionStringProd is empty\", true)",
    //            "return 3"),
    //        "",
    //        "optionsBuilder.UseSqlServer(par.ConnectionStringProd)",
    //        $"using {dbContextClassName} context = new {dbContextClassName}(optionsBuilder.Options)",
    //        "",
    //        "starter.Go(context)",
    //        "",
    //        "return 0",
    //        "");


    //    var block = new CodeBlock("",
    //        new OneLineComment($"Created by {GetType().Name} at {DateTime.Now}"),
    //        "",
    //        "using System",
    //        "using Serilog.Events",
    //        "using CliParameters",
    //        "using DbContextAnalyzer.Models",
    //        "using SystemToolsShared",
    //        "using Microsoft.Extensions.DependencyInjection",
    //        "using Microsoft.Extensions.Logging",
    //        "",
    //        $"using {dbContextProjectName}",
    //        "",
    //        "using Microsoft.EntityFrameworkCore",
    //        "using Microsoft.Extensions.DependencyInjection",
    //        "using Microsoft.Extensions.Logging",
    //        "",
    //        "ILogger<Program>? logger = null",
    //        new CodeBlock("try",
    //            $"Console.WriteLine(\"{createProjectSeederCode} Creates Code for app CreateProjectSeederCode\")",
    //            $"IArgumentsParser argParser = new ArgumentsParser<{parametersClassName}>(args, \"{createProjectSeederCode}\", null)",
    //            new CodeBlock("switch (argParser.Analysis())",
    //                "case EParseResult.Ok: break",
    //                "case EParseResult.Usage: return 1",
    //                "case EParseResult.Error: return 2",
    //                "default: throw new ArgumentOutOfRangeException()"),
    //            "",
    //            $"var par = ({parametersClassName}?)argParser.Par",
    //            "",
    //            new CodeBlock("if (par is null)", 
    //                "StShared.WriteErrorLine(\"CreateProjectSeederCodeParameters is null\", true)",
    //                "return 3"),
    //            "",
    //            $"{serviceCreatorClassName} servicesCreator = new {serviceCreatorClassName}(par.LogFolder, null, \"{projectNamespace}\"{addParametersForServiceCreator ?? ""})",
    //            "var serviceProvider = servicesCreator.CreateServiceProvider(LogEventLevel.Information)",
    //            "",
    //            new CodeBlock("if (serviceProvider is null)",
    //                "StShared.WriteErrorLine(\"serviceProvider does not created\", true)",
    //                "return 8"),
    //            "",
    //            "logger = serviceProvider.GetService<ILogger<Program>>()",
    //            "",
    //            new CodeBlock("if (logger is null)",
    //                "StShared.WriteErrorLine(\"logger is null\", true)",
    //                "return 8"),
    //            "",
    //            fcbMainCommands),
    //        new CodeBlock("catch (Exception e)",
    //            "StShared.WriteException(e, true, logger)",
    //            "BeforeReturn()",
    //            "return 7"));
    //    CodeFile.AddRange(block.CodeItems);
    //    FinishAndSave();
    //}


    public void Go()
    {
        var dbContextProjectName = $"{_par.ProjectPrefix}ScaffoldSeederDbSc";
        var dbContextClassName = $"{_par.ProjectPrefix.Replace('.', '_')}DbScContext";

        var fcbAdditionalUsing = new FlatCodeBlock(
            $"using {dbContextProjectName}",
            "using DbContextAnalyzer",
            "using Microsoft.EntityFrameworkCore");

        var fcbMainCommands = new FlatCodeBlock(
            "",
            "SeederCodeCreatorStarter starter = new SeederCodeCreatorStarter(logger, par)",
            "",
            $"DbContextOptionsBuilder<{dbContextClassName}> optionsBuilder = new DbContextOptionsBuilder<{dbContextClassName}>()",
            "",
            new CodeBlock("if (string.IsNullOrWhiteSpace(par.ConnectionStringProd))",
                "StShared.WriteErrorLine(\"lConnectionStringProd is empty\", true)",
                "return 3"),
            "",
            "optionsBuilder.UseSqlServer(par.ConnectionStringProd)",
            $"using {dbContextClassName} context = new {dbContextClassName}(optionsBuilder.Options)",
            "",
            "starter.Go(context)",
            "",
            "return 0",
            "");

        //par.ProjectNamespace = "CreateGeoModelSeederCode",
        //par.ProjectPlacePath = "D:\\1WorkScaffoldSeeders\\GeoModel\\GeoModelScaffoldSeeder\\GeoModelScaffoldSeeder\\CreateGeoModelSeederCode",
        var starterCreator = new ConsoleProgramCreator(_logger, fcbAdditionalUsing, null, fcbMainCommands,
            "CreateProjectSeederCodeParameters", _par.CreateSeederCodeProjectNamespace,
            "Creates Code for app CreateProjectSeederCode", _par.CreateSeederCodeProjectPlacePath, "Program.cs");
        starterCreator.CreateFileStructure();
    }
}