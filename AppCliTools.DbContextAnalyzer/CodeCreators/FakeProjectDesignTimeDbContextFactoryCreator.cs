using System;
using AppCliTools.CodeTools;
using Microsoft.Extensions.Logging;

namespace AppCliTools.DbContextAnalyzer.CodeCreators;

public sealed class FakeProjectDesignTimeDbContextFactoryCreator : CodeCreator
{
    private readonly string _connectionStringParameterName;
    private readonly string _dbProjectNamespace;
    private readonly string _parametersFileName;
    private readonly string _projectDbContextClassName;
    private readonly string _projectNamespace;

    // ReSharper disable once ConvertToPrimaryConstructor
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
        string projectMigrationProjectName = _dbProjectNamespace + "Migration"; //GeoModelDbMigration
        string designTimeDbContextFactoryClassName =
            $"{_dbProjectNamespace}DesignTimeDbContextFactory"; //GeoModelDesignTimeDbContextFactory
        var block = new CodeBlock(string.Empty, new OneLineComment($"Created by {GetType().Name} at {DateTime.Now}"),
            $"using {_dbProjectNamespace}", "using CarcassDb", string.Empty, $"namespace {_projectNamespace}",
            string.Empty,
            new OneLineComment("ეს კლასი საჭიროა იმისათვის, რომ შესაძლებელი გახდეს მიგრაციასთან მუშაობა."),
            new OneLineComment("ანუ დეველოპერ ბაზის წაშლა და ახლიდან დაგენერირება, ან მიგრაციაში ცვლილებების გაკეთება"),
            new OneLineComment(" ReSharper disable once UnusedType.Global"),
            new CodeBlock(
                $"public sealed class {designTimeDbContextFactoryClassName} : DesignTimeDbContextFactory<{_projectDbContextClassName}>",
                new OneLineComment(" ReSharper disable once ConvertToPrimaryConstructor"),
                new CodeBlock(
                    $"public {designTimeDbContextFactoryClassName}()  : base(\"{projectMigrationProjectName}\", \"{_connectionStringParameterName}\", @\"{_parametersFileName}\")")));
        CodeFile.AddRange(block.CodeItems);
        FinishAndSave();
    }
}
