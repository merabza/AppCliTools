using System.IO;
using SystemToolsShared;

namespace CliTools.ArchiverTools;

public sealed class LinuxArchiverDetector : ArchiverDetector
{
    private const string Unzip = "unzip";


    public LinuxArchiverDetector(bool useConsole, string fileExtension) : base(useConsole, fileExtension)
    {
    }

    public override ArchiverDetectorResults? Run()
    {
        switch (FileExtension.ToLower())
        {
            case Rar:
                var programPatchRar = CheckArchiverRunner(Rar);
                return programPatchRar is null
                    ? null
                    : new ArchiverDetectorResults(
                        programPatchRar, programPatchRar);
            case Zip:
                var programPatchZip = CheckArchiverRunner(Zip);
                var programPatchUnzip = CheckArchiverRunner(Unzip);
                if (programPatchZip is null || programPatchUnzip is null)
                    return null;
                return new ArchiverDetectorResults(programPatchZip,
                    programPatchUnzip);
            default:
                return null;
        }
    }


    private string? CheckArchiverRunner(string archiverName)
    {
        var newDotnetRunner = StShared.RunProcessWithOutput(UseConsole, null, "which", archiverName);
        if (!string.IsNullOrWhiteSpace(newDotnetRunner) && File.Exists(newDotnetRunner))
            return newDotnetRunner;
        return null;
    }
}