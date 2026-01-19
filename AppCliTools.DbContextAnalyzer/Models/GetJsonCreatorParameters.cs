namespace AppCliTools.DbContextAnalyzer.Models;

public sealed class GetJsonCreatorParameters
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public GetJsonCreatorParameters(string dbContextClassName, string dbContextProjectName, string modelsFolderName,
        string placePath, string projectNamespace)
    {
        DbContextClassName = dbContextClassName;
        DbContextProjectName = dbContextProjectName;
        ModelsFolderName = modelsFolderName;
        PlacePath = placePath;
        ProjectNamespace = projectNamespace;
    }

    public string PlacePath { get; set; }
    public string ProjectNamespace { get; set; }
    public string ModelsFolderName { get; set; }
    public string DbContextProjectName { get; set; }
    public string DbContextClassName { get; set; }
}
