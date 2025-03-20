using System.IO;
using SystemToolsShared;

namespace CliTools.ArchiverTools;

public sealed class LinuxArchiverDetector : ArchiverDetector
{
    private const string Unzip = "unzip";

    // ReSharper disable once ConvertToPrimaryConstructor
    public LinuxArchiverDetector(bool useConsole, string fileExtension) : base(useConsole, fileExtension)
    {
    }

    public override ArchiverDetectorResults? Run()
    {
        switch (FileExtension.ToLower())
        {
            case Rar:
                var programPatchRar = CheckArchiverRunner(Rar);
                return programPatchRar is null ? null : new ArchiverDetectorResults(programPatchRar, programPatchRar);
            case Zip:
                var programPatchZip = CheckArchiverRunner(Zip);
                var programPatchUnzip = CheckArchiverRunner(Unzip);
                if (programPatchZip is null || programPatchUnzip is null)
                    return null;
                return new ArchiverDetectorResults(programPatchZip, programPatchUnzip);
            default:
                return null;
        }
    }

    private string? CheckArchiverRunner(string archiverName)
    {
        var runProcessWithOutputResult = StShared.RunProcessWithOutput(UseConsole, null, "which", archiverName);
        if (runProcessWithOutputResult.IsT1)
            return null;
        var archiverRunner = runProcessWithOutputResult.AsT0.Item1;
        if (!string.IsNullOrWhiteSpace(archiverRunner) && File.Exists(archiverRunner))
            return archiverRunner;
        return null;
    }
}