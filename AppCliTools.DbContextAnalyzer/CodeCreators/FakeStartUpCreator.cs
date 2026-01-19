//using System;
//using CodeTools;
//using DbContextAnalyzer.Models;
//using Microsoft.Extensions.Logging;

//namespace DbContextAnalyzer.CodeCreators;

//public sealed class FakeStartUpCreator : CodeCreator
//{
//    private readonly CreatorCreatorParameters _parameters;

//    public FakeStartUpCreator(ILogger logger, CreatorCreatorParameters seederCreatorParameters) : base(logger,
//        seederCreatorParameters.SeedProjectPlacePath, "Startup.cs")
//    {
//        _parameters = seederCreatorParameters;
//    }

//    public override void CreateFileStructure()
//    {
//        var block = new CodeBlock("",
//            new OneLineComment($"Created by {GetType().Name} at {DateTime.Now}"),
//            $"namespace {_parameters.SeedProjectNamespace}",
//            "",
//            new CodeBlock("public sealed class Startup"));
//        CodeFile.AddRange(block.CodeItems);
//        FinishAndSave();
//    }

//    public override void FinishAndSave()
//    {
//        CreateFile();
//    }
//}

