namespace AppCliTools.DbContextAnalyzer.Domain;

public sealed class TableFieldDomain
{
    public required string TableName { get; set; }
    public required string FieldName { get; set; }
}
