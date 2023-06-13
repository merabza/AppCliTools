using System;
using CodeTools;
using Microsoft.Extensions.Logging;

namespace DbContextAnalyzer.CodeCreators;

public sealed class FakeProjectDesignTimeDbContextFactoryCreator : CodeCreator
{
    private readonly string _connectionStringParameterName;
    private readonly string _dbProjectNamespace;
    private readonly string _parametersFileName;
    private readonly string _projectDbContextClassName;
    private readonly string _projectNamespace;

    public FakeProjectDesignTimeDbContextFactoryCreator(ILogger logger, string projectPlacePath,
        string dbProjectNamespace, string projectNamespace, string projectDbContextClassName,
        string connectionStringParameterName, string parametersFileName) : base(logger, projectPlacePath,
        $"{dbProjectNamespace}DesignTimeDbContextFactory.cs")
    {
        _dbProjectNamespace = dbProjectNamespace;
        _projectNamespace = projectNamespace;
        _projectDbContextClassName = projectDbContextClassName;
        _connectionStringParameterName = connectionStringParameterName;
        _parametersFileName = parametersFileName;
    }

    public override void CreateFileStructure()
    {
        var projectMigrationProjectName = _dbProjectNamespace + "Migration"; //GeoModelDbMigration
        var designTimeDbContextFactoryClassName =
            $"{_dbProjectNamespace}DesignTimeDbContextFactory"; //GeoModelDesignTimeDbContextFactory
        var block = new CodeBlock("",
            new OneLineComment($"Created by {GetType().Name} at {DateTime.Now}"),
            $"using {_dbProjectNamespace}",
            "using CarcassDb",
            "",
            $"namespace {_projectNamespace}",
            "",
            new OneLineComment("ეს კლასი საჭიროა იმისათვის, რომ შესაძლებელი გახდეს მიგრაციასთან მუშაობა."),
            new OneLineComment(
                "ანუ დეველოპერ ბაზის წაშლა და ახლიდან დაგენერირება, ან მიგრაციაში ცვლილებების გაკეთება"),
            new CodeBlock(
                $"public sealed class {designTimeDbContextFactoryClassName} : DesignTimeDbContextFactory<{_projectDbContextClassName}>",
                new CodeBlock(
                    $"public {designTimeDbContextFactoryClassName}()  : base(\"{projectMigrationProjectName}\", \"{_connectionStringParameterName}\", \"{_parametersFileName.Replace("\\", "\\\\")}\")")));
        CodeFile.AddRange(block.CodeItems);
        FinishAndSave();
    }

    public override void FinishAndSave()
    {
        CreateFile();
    }
}