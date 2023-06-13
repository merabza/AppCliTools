namespace DbContextAnalyzer.Domain;

public sealed class TableFieldDomain
{
    public TableFieldDomain(string tableName, string fieldName)
    {
        TableName = tableName;
        FieldName = fieldName;
    }

    public string TableName { get; set; }
    public string FieldName { get; set; }
}