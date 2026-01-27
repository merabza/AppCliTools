using System.Collections.Generic;
using AppCliTools.CodeTools;
using AppCliTools.DbContextAnalyzer.CodeCreators;
using AppCliTools.DbContextAnalyzer.Models;
using Microsoft.Extensions.Logging;

namespace AppCliTools.LibSeedCodeCreator;

public sealed class CreateProjectSeederCodeProgramCreator(CreatorCreatorParameters par, ILogger logger)
{
    public void Go()
    {
        string scaffoldSeederDbContextProjectName = $"{par.ProjectPrefix}ScaffoldSeederDbSc";
        string dbScContextClassName = $"{par.ProjectPrefix.Replace('.', '_')}DbScContext";

        string dbContextProjectName = par.DbContextProjectName;
        string dbContextClassName = par.DbContextClassName;

        var fcbAdditionalUsing = new FlatCodeBlock($"using {scaffoldSeederDbContextProjectName}",
            $"using {dbContextProjectName}", "using AppCliTools.DbContextAnalyzer",
            "using Microsoft.EntityFrameworkCore", "using AppCliTools.DbContextAnalyzer.Models");

        var fcbMainCommands = new FlatCodeBlock(string.Empty, "var starter = new SeederCodeCreatorStarter(logger, par)",
            string.Empty, $"var optionsBuilder = new DbContextOptionsBuilder<{dbScContextClassName}>()", string.Empty,
            new CodeBlock("if (string.IsNullOrWhiteSpace(par.ConnectionStringProd))",
                "StShared.WriteErrorLine(\"ConnectionStringProd is empty\", true)", "return 3"), string.Empty,
            "optionsBuilder.UseSqlServer(par.ConnectionStringProd)", string.Empty,
            new OneLineComment(" ReSharper disable once using"),
            new OneLineComment(" ReSharper disable once DisposableConstructor"),
            $"using var context = new {dbScContextClassName}(optionsBuilder.Options)", string.Empty, string.Empty,
            $"var optionsBuilderDev = new DbContextOptionsBuilder<{dbContextClassName}>()", string.Empty,
            new CodeBlock("if (string.IsNullOrWhiteSpace(par.ConnectionStringDev))",
                "StShared.WriteErrorLine(\"ConnectionStringDev is empty\", true)", "return 3"), string.Empty,
            "optionsBuilderDev.UseSqlServer(par.ConnectionStringDev)", string.Empty,
            new OneLineComment(" ReSharper disable once using"),
            new OneLineComment(" ReSharper disable once DisposableConstructor"),
            $"using var contextDev = new {dbContextClassName}(optionsBuilderDev.Options)", string.Empty,
            "starter.Go(context, contextDev)", string.Empty, "return 0", string.Empty);

        var starterCreator = new ConsoleProgramCreator(logger, fcbAdditionalUsing, null, fcbMainCommands,
            "CreateProjectSeederCodeParameters", par.CreateSeederCodeProjectNamespace,
            "Creates Code for app CreateProjectSeederCode", par.CreateSeederCodeProjectPlacePath, new List<string>(),
            "Program.cs");
        starterCreator.CreateFileStructure();
    }
}
