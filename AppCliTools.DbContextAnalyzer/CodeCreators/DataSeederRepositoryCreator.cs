using System;
using AppCliTools.CodeTools;
using AppCliTools.DbContextAnalyzer.Models;
using Microsoft.Extensions.Logging;

namespace AppCliTools.DbContextAnalyzer.CodeCreators;

public sealed class DataSeederRepositoryCreator : CodeCreator
{
    private readonly SeederCodeCreatorParameters _par;

    // ReSharper disable once ConvertToPrimaryConstructor
    public DataSeederRepositoryCreator(ILogger logger, SeederCodeCreatorParameters par) : base(logger, par.PlacePath,
        $"{par.ProjectPrefixShort}DataSeederRepository.cs")
    {
        _par = par;
    }

    public override void CreateFileStructure()
    {
        var block = new CodeBlock(string.Empty, new OneLineComment($"Created by {GetType().Name} at {DateTime.Now}"),
            "using SystemTools.DatabaseToolsShared", $"using {_par.DbProjectNamespace}",
            "using Microsoft.Extensions.Logging", $"namespace {_par.ProjectNamespace}", string.Empty,
            new CodeBlock(
                $"public sealed class {_par.ProjectPrefixShort}DataSeederRepository : DataSeederRepository, {_par.DataSeederRepositoryInterfaceName}",
                string.Empty, new OneLineComment(" ReSharper disable once ConvertToPrimaryConstructor"),
                new CodeBlock(
                    $"public {_par.ProjectPrefixShort}DataSeederRepository({_par.ProjectDbContextClassName} ctx, ILogger<{_par.ProjectPrefixShort}DataSeederRepository> logger)  : base(ctx, logger)")));
        CodeFile.AddRange(block.CodeItems);
        FinishAndSave();
    }
}
