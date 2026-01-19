namespace AppCliTools.DbContextAnalyzer.Models;

public sealed class ReplaceFieldName
{
    public string? TableName { get; set; }
    public string? OldFieldName { get; set; }
    public string? NewFieldName { get; set; }
}
