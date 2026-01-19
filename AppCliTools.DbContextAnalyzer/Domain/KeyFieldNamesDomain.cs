using System.Collections.Generic;

namespace DbContextAnalyzer.Domain;

public sealed class KeyFieldNamesDomain
{
    public required string TableName { get; set; }
    public List<string> Keys { get; set; } = [];
}