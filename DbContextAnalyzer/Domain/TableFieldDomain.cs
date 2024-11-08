namespace DbContextAnalyzer.Domain;

public sealed class TableFieldDomain
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public TableFieldDomain(string tableName, string fieldName)
    {
        TableName = tableName;
        FieldName = fieldName;
    }

    public string TableName { get; set; }
    public string FieldName { get; set; }
}