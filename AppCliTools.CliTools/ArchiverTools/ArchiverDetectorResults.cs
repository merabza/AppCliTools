namespace CliTools.ArchiverTools;

public sealed class ArchiverDetectorResults
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public ArchiverDetectorResults(string compressProgramPatch, string decompressProgramPatch)
    {
        CompressProgramPatch = compressProgramPatch;
        DecompressProgramPatch = decompressProgramPatch;
    }

    public string CompressProgramPatch { get; }
    public string DecompressProgramPatch { get; }
}