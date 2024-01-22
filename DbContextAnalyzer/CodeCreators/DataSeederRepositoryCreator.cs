using System;
using CodeTools;
using DbContextAnalyzer.Models;
using Microsoft.Extensions.Logging;

namespace DbContextAnalyzer.CodeCreators;

public sealed class DataSeederRepositoryCreator : CodeCreator
{
    private readonly SeederCodeCreatorParameters _par;

    // ReSharper disable once ConvertToPrimaryConstructor
    public DataSeederRepositoryCreator(ILogger logger, SeederCodeCreatorParameters par) : base(logger,
        par.PlacePath, $"{par.ProjectPrefixShort}DataSeederRepository.cs")
    {
        _par = par;
    }

    public override void CreateFileStructure()
    {
        var block = new CodeBlock("",
            new OneLineComment($"Created by {GetType().Name} at {DateTime.Now}"),
            "using CarcassDataSeeding",
            $"using {_par.DbProjectNamespace}",
            "using Microsoft.Extensions.Logging",
            $"namespace {_par.ProjectNamespace}",
            "",
            new CodeBlock(
                $"public sealed class {_par.ProjectPrefixShort}DataSeederRepository : DataSeederRepository, {_par.DataSeederRepositoryInterfaceName}",
                "",
                new OneLineComment(" ReSharper disable once ConvertToPrimaryConstructor"),
                new CodeBlock(
                    $"public {_par.ProjectPrefixShort}DataSeederRepository({_par.ProjectDbContextClassName} ctx, ILogger<{_par.ProjectPrefixShort}DataSeederRepository> logger)  : base(ctx, logger)"
                )));
        CodeFile.AddRange(block.CodeItems);
        CreateFile();
    }

    public override void FinishAndSave()
    {
        CreateFile();
    }
}