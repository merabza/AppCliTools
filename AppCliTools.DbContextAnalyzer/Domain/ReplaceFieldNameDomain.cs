namespace DbContextAnalyzer.Domain;

public sealed class ReplaceFieldNameDomain
{
    public required string TableName { get; set; }
    public required string OldFieldName { get; set; }
    public required string NewFieldName { get; set; }
}