using SystemToolsShared;

namespace CliTools.ArchiverTools;

public /*open*/ class ArchiverDetector
{
    protected const string Rar = "rar";
    protected const string Zip = "zip";
    protected readonly string FileExtension;

    protected readonly bool UseConsole;


    protected ArchiverDetector(bool useConsole, string fileExtension)
    {
        UseConsole = useConsole;
        FileExtension = fileExtension;
    }

    public virtual ArchiverDetectorResults? Run()
    {
        StShared.WriteErrorLine("Archiver Detector Run method not implemented", true);
        return null;
    }
}