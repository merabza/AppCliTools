namespace DbContextAnalyzer.Domain;

public sealed class GetJsonParametersDomain
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public GetJsonParametersDomain(string jsonFolderName, string logFolder, string connectionStringProd)
    {
        JsonFolderName = jsonFolderName;
        LogFolder = logFolder;
        ConnectionStringProd = connectionStringProd;
    }

    public string JsonFolderName { get; set; }
    public string LogFolder { get; set; }
    public string ConnectionStringProd { get; set; }
}