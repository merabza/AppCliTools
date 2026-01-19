namespace AppCliTools.CodeTools;

public sealed class BinFile
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public BinFile(string fileName)
    {
        FileName = fileName;
    }

    public string FileName { get; set; }
    public string? Base64String { get; set; }
}
