using System;
using CodeTools;
using DbContextAnalyzer.Models;
using Microsoft.Extensions.Logging;

namespace DbContextAnalyzer.CodeCreators;

public sealed class DataSeederBaseGenericClassCreator : CodeCreator
{
    private readonly SeederCodeCreatorParameters _parameters;

    // ReSharper disable once ConvertToPrimaryConstructor
    public DataSeederBaseGenericClassCreator(ILogger logger, SeederCodeCreatorParameters parameters) : base(logger,
        parameters.PlacePath, $"{parameters.DataSeederBaseClassName}.cs")
    {
        _parameters = parameters;
    }

    public override void CreateFileStructure()
    {
        var block = new CodeBlock("",
            new OneLineComment($"Created by {GetType().Name} at {DateTime.Now}"),
            "using CarcassDataSeeding",
            $"namespace {_parameters.ProjectNamespace}",
            "",
            new CodeBlock(
                $"public /*open*/ class {_parameters.DataSeederBaseClassName}<T> : DataSeeder<T> where T : class",
                $"protected readonly {_parameters.DataSeederRepositoryInterfaceName} {_parameters.ProjectPrefixShort}Repo",
                new CodeBlock(
                    $"protected {_parameters.DataSeederBaseClassName}(string dataSeedFolder, {_parameters.DataSeederRepositoryInterfaceName} repo) : base(dataSeedFolder, repo)",
                    $"{_parameters.ProjectPrefixShort}Repo = repo")));
        CodeFile.AddRange(block.CodeItems);
        FinishAndSave();
    }
}