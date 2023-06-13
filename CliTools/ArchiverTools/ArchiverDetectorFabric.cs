using System.Runtime.InteropServices;

namespace CliTools.ArchiverTools;

public static class ArchiverDetectorFabric
{
    public static ArchiverDetector? Create(bool useConsole, string fileExtension)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return new LinuxArchiverDetector(useConsole, fileExtension);
        return RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? new WindowsArchiverDetector(useConsole, fileExtension)
            : null;
    }
}