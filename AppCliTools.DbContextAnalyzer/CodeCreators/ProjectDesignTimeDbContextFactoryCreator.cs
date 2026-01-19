//using System;
//using CodeTools;
//using DbContextAnalyzer.Models;
//using Microsoft.Extensions.Logging;

//namespace DbContextAnalyzer.CodeCreators;

//public sealed class ProjectDesignTimeDbContextFactoryCreator : CodeCreator
//{
//    private readonly CreatorCreatorParameters _par;

//    public ProjectDesignTimeDbContextFactoryCreator(ILogger logger, CreatorCreatorParameters par) : base(logger,
//        par.SeedProjectPlacePath, $"{par.DbProjectNamespace}DesignTimeDbContextFactory.cs")
//    {
//        _par = par;
//    }

//    public override void CreateFileStructure()
//    {
//        var projectMigrationProjectName = _par.DbProjectNamespace + "Migration"; //GeoModelDbMigration
//        var designTimeDbContextFactoryClassName =
//            $"{_par.DbProjectNamespace}DesignTimeDbContextFactory"; //GeoModelDesignTimeDbContextFactory
//        var block = new CodeBlock("",
//            new OneLineComment($"Created by {GetType().Name} at {DateTime.Now}"),
//            $"using {_par.DbProjectNamespace}",
//            "using CarcassDb",
//            "",
//            $"namespace {_par.SeedProjectNamespace}",
//            "",
//            new OneLineComment("ეს კლასი საჭიროა იმისათვის, რომ შესაძლებელი გახდეს მიგრაციასთან მუშაობა."),
//            new OneLineComment(
//                "ანუ დეველოპერ ბაზის წაშლა და ახლიდან დაგენერირება, ან მიგრაციაში ცვლილებების გაკეთება"),
//            new CodeBlock(
//                $"public sealed class {designTimeDbContextFactoryClassName} : DesignTimeDbContextFactory<{_par.ProjectDbContextClassName}>",
//                new CodeBlock(
//                    $"public {designTimeDbContextFactoryClassName}()  : base(\"{projectMigrationProjectName}\", \"{_par.SeedConnectionStringParameterName}\", \"{_par.SeedParametersFileName.Replace("\\", "\\\\")}\")")));
//        CodeFile.AddRange(block.CodeItems);
//        FinishAndSave();
//    }

//    public override void FinishAndSave()
//    {
//        CreateFile();
//    }
//}


