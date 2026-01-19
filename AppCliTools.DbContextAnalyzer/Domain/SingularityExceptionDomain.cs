namespace AppCliTools.DbContextAnalyzer.Domain;

public sealed class SingularityExceptionDomain
{
    public required string TableName { get; set; }
    public required string TableNameSingular { get; set; }
}
