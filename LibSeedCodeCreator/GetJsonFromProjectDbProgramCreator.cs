using System.Collections.Generic;
using CodeTools;
using DbContextAnalyzer.CodeCreators;
using DbContextAnalyzer.Models;
using Microsoft.Extensions.Logging;

namespace LibSeedCodeCreator;

public sealed class GetJsonFromProjectDbProgramCreator(CreatorCreatorParameters par, ILogger logger)
{
    public void Go()
    {
        var dbContextProjectName = $"{par.ProjectPrefix}ScaffoldSeederDbSc";
        var dbContextClassName = $"{par.ProjectPrefix.Replace('.', '_')}DbScContext";

        var fcbGetJsonAdditionalUsing = new FlatCodeBlock($"using {dbContextProjectName}",
            "using Microsoft.EntityFrameworkCore", $"using GetJsonFromScaffold{par.ProjectPrefix}Db");

        var fcbGetJsonMainCommands = new FlatCodeBlock(string.Empty,
            $"var optionsBuilder = new DbContextOptionsBuilder<{dbContextClassName}>()", string.Empty,
            new CodeBlock("if (string.IsNullOrWhiteSpace(par.ConnectionStringProd))",
                "StShared.WriteErrorLine(\"lConnectionStringProd is empty\", true)", "return 3"), string.Empty,
            "optionsBuilder.UseSqlServer(par.ConnectionStringProd)", string.Empty,
            new OneLineComment(" ReSharper disable once using"),
            new OneLineComment(" ReSharper disable once DisposableConstructor"),
            $"using var context = new {dbContextClassName}(optionsBuilder.Options)", string.Empty,
            new CodeBlock("if (string.IsNullOrWhiteSpace(par.JsonFolderName))",
                "StShared.WriteErrorLine(\"JsonFolderName is empty\", true)", "return 3"), string.Empty,
            "var dataExtractor = new JsonFilesCreator(context, par.JsonFolderName)", "dataExtractor.Run()",
            string.Empty, "return 0", string.Empty);

        var getJsonCreator = new ConsoleProgramCreator(logger, fcbGetJsonAdditionalUsing, null, fcbGetJsonMainCommands,
            "GetJsonParameters", par.GetJsonProjectNamespace, "Creates Json files for seeder",
            par.GetJsonProjectPlacePath, new List<string>(), "Program.cs");

        getJsonCreator.CreateFileStructure();

        var creatorForJsonFilesCreator = new FakeCreatorForJsonFilesCreator(logger, par);
        creatorForJsonFilesCreator.CreateFileStructure();
        creatorForJsonFilesCreator.FinishAndSave();
    }
}