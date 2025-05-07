namespace DbContextAnalyzer.Domain;

public sealed class ReplaceTableNameDomain
{
    public required string OldTableName { get; set; }
    public required string NewTableName { get; set; }
}