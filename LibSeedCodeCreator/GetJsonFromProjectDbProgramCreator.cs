using CodeTools;
using DbContextAnalyzer.CodeCreators;
using DbContextAnalyzer.Models;
using Microsoft.Extensions.Logging;

namespace LibSeedCodeCreator;

public sealed class GetJsonFromProjectDbProgramCreator
{
    private readonly ILogger _logger;
    private readonly CreatorCreatorParameters _par;

    public GetJsonFromProjectDbProgramCreator(CreatorCreatorParameters par, ILogger logger)
    {
        _par = par;
        _logger = logger;
    }


    public void Go()
    {
        var dbContextProjectName = $"{_par.ProjectPrefix}ScaffoldSeederDbSc";
        var dbContextClassName = $"{_par.ProjectPrefix.Replace('.', '_')}DbScContext";

        var fcbGetJsonAdditionalUsing = new FlatCodeBlock(
            $"using {dbContextProjectName}",
            "using Microsoft.EntityFrameworkCore",
            $"using GetJsonFromScaffold{_par.ProjectPrefix}Db");

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

        var getJsonCreator = new ConsoleProgramCreator(_logger, fcbGetJsonAdditionalUsing, null, fcbGetJsonMainCommands,
            "GetJsonParameters", _par.GetJsonProjectNamespace, "Creates Json files for seeder",
            _par.GetJsonProjectPlacePath, "Program.cs");
        getJsonCreator.CreateFileStructure();

        var creatorForJsonFilesCreator =
            new FakeCreatorForJsonFilesCreator(_logger, _par);
        creatorForJsonFilesCreator.CreateFileStructure();
        creatorForJsonFilesCreator.FinishAndSave();
    }
}