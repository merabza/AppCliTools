using System;
using System.Collections.Generic;
using System.Linq;
using CodeTools;
using Microsoft.Extensions.Logging;

namespace DbContextAnalyzer.CodeCreators;

public sealed class ConsoleProgramCreator(ILogger logger, FlatCodeBlock fcbAdditionalUsing,
    FlatCodeBlock? serviceCreatorCodeCommands, FlatCodeBlock fcbMainCommands, string parametersClassName,
    string projectNamespace, string projectDescription, string placePath, IReadOnlyCollection<string> possibleSwitches,
    string? codeFileName = null) : CodeCreator(logger, placePath, codeFileName)
{
    public override void CreateFileStructure()
    {
        var strPossibleSwitches = possibleSwitches.Count == 0
            ? string.Empty
            : string.Join(", ", possibleSwitches.Select(s => $"\"--{s}\""));

        var finalServiceCreatorCodeCommands = serviceCreatorCodeCommands ?? new FlatCodeBlock(
            $"ServicesCreator servicesCreator = new ServicesCreator(par.LogFolder, null, \"{projectNamespace}\")");


        var block = new CodeBlock("",
            new OneLineComment($"Created by {GetType().Name} at {DateTime.Now}"),
            "",
            "using System",
            "using Serilog.Events",
            "using CliParameters",
            "using DbContextAnalyzer.Models",
            "using SystemToolsShared",
            "using Microsoft.Extensions.DependencyInjection",
            "using Microsoft.Extensions.Logging",
            "",
            fcbAdditionalUsing,
            "",
            "ILogger<Program>? logger = null",
            new CodeBlock("try",
                $"Console.WriteLine(\"{projectDescription}\")",
                $"IArgumentsParser argParser = new ArgumentsParser<{parametersClassName}>(args, \"{projectNamespace}\", null, {strPossibleSwitches})",
                new CodeBlock("switch (argParser.Analysis())",
                    "case EParseResult.Ok: break",
                    "case EParseResult.Usage: return 1",
                    "case EParseResult.Error: return 2",
                    "default: throw new ArgumentOutOfRangeException()"),
                "",
                $"var par = ({parametersClassName}?)argParser.Par",
                "",
                new CodeBlock("if (par is null)",
                    "StShared.WriteErrorLine(\"CreateProjectSeederCodeParameters is null\", true)",
                    "return 3"),
                "",
                finalServiceCreatorCodeCommands,
                "var serviceProvider = servicesCreator.CreateServiceProvider(LogEventLevel.Information)",
                "",
                new CodeBlock("if (serviceProvider is null)",
                    "StShared.WriteErrorLine(\"serviceProvider does not created\", true)",
                    "return 4"),
                "",
                "logger = serviceProvider.GetService<ILogger<Program>>()",
                "",
                new CodeBlock("if (logger is null)",
                    "StShared.WriteErrorLine(\"logger is null\", true)",
                    "return 5"),
                "",
                fcbMainCommands),
            new CodeBlock("catch (Exception e)",
                "StShared.WriteException(e, true, logger)",
                "return 7"));
        CodeFile.AddRange(block.CodeItems);
        FinishAndSave();
    }
}