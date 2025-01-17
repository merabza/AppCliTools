using System;
using System.Collections.Generic;
using System.Linq;
using CodeTools;
using Microsoft.Extensions.Logging;

namespace DbContextAnalyzer.CodeCreators;

public sealed class ConsoleProgramCreator : CodeCreator
{
    private readonly FlatCodeBlock _fcbAdditionalUsing;
    private readonly FlatCodeBlock _fcbMainCommands;
    private readonly string _parametersClassName;
    private readonly IReadOnlyCollection<string> _possibleSwitches;
    private readonly string _projectDescription;
    private readonly string _projectNamespace;
    private readonly FlatCodeBlock? _serviceCreatorCodeCommands;

    // ReSharper disable once ConvertToPrimaryConstructor
    public ConsoleProgramCreator(ILogger logger, FlatCodeBlock fcbAdditionalUsing,
        FlatCodeBlock? serviceCreatorCodeCommands, FlatCodeBlock fcbMainCommands, string parametersClassName,
        string projectNamespace, string projectDescription, string placePath,
        IReadOnlyCollection<string> possibleSwitches, string? codeFileName = null) : base(logger, placePath,
        codeFileName)
    {
        _fcbAdditionalUsing = fcbAdditionalUsing;
        _serviceCreatorCodeCommands = serviceCreatorCodeCommands;
        _fcbMainCommands = fcbMainCommands;
        _parametersClassName = parametersClassName;
        _projectNamespace = projectNamespace;
        _projectDescription = projectDescription;
        _possibleSwitches = possibleSwitches;
    }

    public override void CreateFileStructure()
    {
        var strPossibleSwitches = _possibleSwitches.Count == 0
            ? string.Empty
            : $", {string.Join(", ", _possibleSwitches.Select(s => $"\"--{s}\""))}";

        var finalServiceCreatorCodeCommands = _serviceCreatorCodeCommands ?? new FlatCodeBlock(
            $"var servicesCreator = new ServicesCreator(par.LogFolder, null, \"{_projectNamespace}\")");


        var block = new CodeBlock(string.Empty, new OneLineComment($"Created by {GetType().Name} at {DateTime.Now}"),
            string.Empty, "using System", "using Serilog.Events", "using CliParameters",
            "using DbContextAnalyzer.Models", "using SystemToolsShared",
            "using Microsoft.Extensions.DependencyInjection", "using Microsoft.Extensions.Logging", string.Empty,
            _fcbAdditionalUsing, string.Empty, "ILogger<Program>? logger = null",
            new CodeBlock("try", $"Console.WriteLine(\"{_projectDescription}\")",
                $"IArgumentsParser argParser = new ArgumentsParser<{_parametersClassName}>(args, \"{_projectNamespace}\", null{strPossibleSwitches})",
                new CodeBlock("switch (argParser.Analysis())", "case EParseResult.Ok: break",
                    "case EParseResult.Usage: return 1", "case EParseResult.Error: return 2",
                    "default: throw new ArgumentOutOfRangeException()"), string.Empty,
                $"var par = ({_parametersClassName}?)argParser.Par", string.Empty,
                new CodeBlock("if (par is null)",
                    "StShared.WriteErrorLine(\"CreateProjectSeederCodeParameters is null\", true)", "return 3"),
                string.Empty, finalServiceCreatorCodeCommands, new OneLineComment(" ReSharper disable once using"),
                "var serviceProvider = servicesCreator.CreateServiceProvider(LogEventLevel.Information)", string.Empty,
                new CodeBlock("if (serviceProvider is null)",
                    "StShared.WriteErrorLine(\"serviceProvider does not created\", true)", "return 4"), string.Empty,
                "logger = serviceProvider.GetService<ILogger<Program>>()", string.Empty,
                new CodeBlock("if (logger is null)", "StShared.WriteErrorLine(\"logger is null\", true)", "return 5"),
                string.Empty, _fcbMainCommands),
            new CodeBlock("catch (Exception e)", "StShared.WriteException(e, true, logger)", "return 7"));
        CodeFile.AddRange(block.CodeItems);
        FinishAndSave();
    }
}