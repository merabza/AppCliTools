using System;
using AppCliTools.CodeTools;
using AppCliTools.DbContextAnalyzer.Models;
using Microsoft.Extensions.Logging;

namespace AppCliTools.DbContextAnalyzer.CodeCreators;

public sealed class DataSeederRepositoryInterfaceCreator : CodeCreator
{
    private readonly SeederCodeCreatorParameters _parameters;

    // ReSharper disable once ConvertToPrimaryConstructor
    public DataSeederRepositoryInterfaceCreator(ILogger logger, SeederCodeCreatorParameters parameters) : base(logger,
        parameters.PlacePath, $"{parameters.DataSeederRepositoryInterfaceName}.cs")
    {
        _parameters = parameters;
    }

    public override void CreateFileStructure()
    {
        var block = new CodeBlock(string.Empty, new OneLineComment($"Created by {GetType().Name} at {DateTime.Now}"),
            "using SystemTools.DatabaseToolsShared", $"namespace {_parameters.ProjectNamespace}", string.Empty,
            $"public interface {_parameters.DataSeederRepositoryInterfaceName} : IDataSeederRepository");
        CodeFile.AddRange(block.CodeItems);
        FinishAndSave();
    }
}
