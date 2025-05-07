namespace DbContextAnalyzer.Models;

public sealed class SingularityException
{
    public string? TableName { get; set; }
    public string? TableNameSingular { get; set; }
}