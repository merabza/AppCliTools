namespace CodeTools;

public sealed class BinFile
{
    public BinFile(string fileName)
    {
        FileName = fileName;
    }

    public string FileName { get; set; }
    public string? Base64String { get; set; }
}