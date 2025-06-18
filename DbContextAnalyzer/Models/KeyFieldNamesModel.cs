using System.Collections.Generic;

namespace DbContextAnalyzer.Models;

public sealed class KeyFieldNamesModel
{
    public string? TableName { get; set; }
    public List<string> Keys { get; set; } = [];
}