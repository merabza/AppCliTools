using CodeTools;
using DbContextAnalyzer.CodeCreators;
using DbContextAnalyzer.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace LibSeedCodeCreator;

public sealed class GetJsonFromProjectDbProgramCreator(CreatorCreatorParameters par, ILogger logger)
{
    public void Go()
    {
        var dbContextProjectName = $"{par.ProjectPrefix}ScaffoldSeederDbSc";
        var dbContextClassName = $"{par.ProjectPrefix.Replace('.', '_')}DbScContext";

        var fcbGetJsonAdditionalUsing = new FlatCodeBlock(
            $"using {dbContextProjectName}",
            "using Microsoft.EntityFrameworkCore",
            $"using GetJsonFromScaffold{par.ProjectPrefix}Db");

        //var fcbGetJsonMainCommands = new FlatCodeBlock(
        //    "",
        //    "SeederCodeCreatorStarter starter = new SeederCodeCreatorStarter(logger, par)",
        //    "",
        //    $"DbContextOptionsBuilder<{dbContextClassName}> optionsBuilder = new DbContextOptionsBuilder<{dbContextClassName}>()",
        //    "",
        //    new CodeBlock("if (string.IsNullOrWhiteSpace(par.ConnectionStringProd))",
        //        "StShared.WriteErrorLine(\"lConnectionStringProd is empty\", true)",
        //        "return 3"),
        //    "",
        //    "optionsBuilder.UseSqlServer(par.ConnectionStringProd)",
        //    $"using {dbContextClassName} context = new {dbContextClassName}(optionsBuilder.Options)",
        //    "",
        //    "starter.Go(context)",
        //    "",
        //    "return 0",
        //    "");


        var fcbGetJsonMainCommands = new FlatCodeBlock(
            "",
            $"var optionsBuilder = new DbContextOptionsBuilder<{dbContextClassName}>()",
            "",
            new CodeBlock("if (string.IsNullOrWhiteSpace(par.ConnectionStringProd))",
                "StShared.WriteErrorLine(\"lConnectionStringProd is empty\", true)",
                "return 3"),
            "",
            "optionsBuilder.UseSqlServer(par.ConnectionStringProd)",
            "",
            $"using {dbContextClassName} context = new {dbContextClassName}(optionsBuilder.Options)",
            "",
            new CodeBlock("if (string.IsNullOrWhiteSpace(par.JsonFolderName))",
                "StShared.WriteErrorLine(\"JsonFolderName is empty\", true)",
                "return 3"),
            "",
            "JsonFilesCreator dataExtractor = new JsonFilesCreator(context, par.JsonFolderName)",
            "dataExtractor.Run()",
            "",
            "return 0",
            "");

        var getJsonCreator = new ConsoleProgramCreator(logger, fcbGetJsonAdditionalUsing, null, fcbGetJsonMainCommands,
            "GetJsonParameters", par.GetJsonProjectNamespace, "Creates Json files for seeder",
            par.GetJsonProjectPlacePath, new List<string>(), "Program.cs");

        getJsonCreator.CreateFileStructure();

        var creatorForJsonFilesCreator =
            new FakeCreatorForJsonFilesCreator(logger, par);
        creatorForJsonFilesCreator.CreateFileStructure();
        creatorForJsonFilesCreator.FinishAndSave();
    }
}