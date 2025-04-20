using System.Collections.Generic;
using CodeTools;
using DbContextAnalyzer.CodeCreators;
using DbContextAnalyzer.Models;
using Microsoft.Extensions.Logging;

namespace LibSeedCodeCreator;

public sealed class CreateProjectSeederCodeProgramCreator(CreatorCreatorParameters par, ILogger logger)
{
    public void Go()
    {
        var dbContextProjectName = $"{par.ProjectPrefix}ScaffoldSeederDbSc";
        var dbContextClassName = $"{par.ProjectPrefix.Replace('.', '_')}DbScContext";

        var fcbAdditionalUsing = new FlatCodeBlock($"using {dbContextProjectName}", "using DbContextAnalyzer",
            "using Microsoft.EntityFrameworkCore", "using DbContextAnalyzer.Models");

        var fcbMainCommands = new FlatCodeBlock(string.Empty, "var starter = new SeederCodeCreatorStarter(logger, par)",
            string.Empty, $"var optionsBuilder = new DbContextOptionsBuilder<{dbContextClassName}>()", string.Empty,
            new CodeBlock("if (string.IsNullOrWhiteSpace(par.ConnectionStringProd))",
                "StShared.WriteErrorLine(\"lConnectionStringProd is empty\", true)", "return 3"), string.Empty,
            "optionsBuilder.UseSqlServer(par.ConnectionStringProd)", string.Empty,
            new OneLineComment(" ReSharper disable once using"),
            new OneLineComment(" ReSharper disable once DisposableConstructor"),
            $"using var context = new {dbContextClassName}(optionsBuilder.Options)", string.Empty,
            "starter.Go(context)", string.Empty, "return 0", string.Empty);


        /*
         *
           // ReSharper disable once using
           using var context = DbContextCreator.Create<MimosiGeDbScContext>(par.ConnectionStringProd);

           if (context is null)
               return 6;


         */

        var starterCreator = new ConsoleProgramCreator(logger, fcbAdditionalUsing, null, fcbMainCommands,
            "CreateProjectSeederCodeParameters", par.CreateSeederCodeProjectNamespace,
            "Creates Code for app CreateProjectSeederCode", par.CreateSeederCodeProjectPlacePath, new List<string>(),
            "Program.cs");
        starterCreator.CreateFileStructure();
    }
}