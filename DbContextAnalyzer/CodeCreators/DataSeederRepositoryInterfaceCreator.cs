using System;
using CodeTools;
using DbContextAnalyzer.Models;
using Microsoft.Extensions.Logging;

namespace DbContextAnalyzer.CodeCreators;

public sealed class DataSeederRepositoryInterfaceCreator : CodeCreator
{
    private readonly SeederCodeCreatorParameters _parameters;

    public DataSeederRepositoryInterfaceCreator(ILogger logger, SeederCodeCreatorParameters parameters) : base(logger,
        parameters.PlacePath, $"{parameters.DataSeederRepositoryInterfaceName}.cs")
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
            new CodeBlock($"public interface {_parameters.DataSeederRepositoryInterfaceName} : IDataSeederRepository"));
        CodeFile.AddRange(block.CodeItems);
        CreateFile();
    }
}