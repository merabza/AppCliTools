using System.IO;
using SystemTools.SharedKernel;
using SystemTools.SystemToolsShared;

namespace AppCliTools.CliTools.ArchiverTools;

public sealed class LinuxArchiverDetector : ArchiverDetector
{
    private const string Unzip = "unzip";

    // ReSharper disable once ConvertToPrimaryConstructor
    public LinuxArchiverDetector(bool useConsole, string fileExtension) : base(useConsole, fileExtension)
    {
    }

    public override ArchiverDetectorResults? Run()
    {
        switch (FileExtension.ToUpperInvariant())
        {
            case Rar:
                string? programPatchRar = CheckArchiverRunner(Rar);
                return programPatchRar is null ? null : new ArchiverDetectorResults(programPatchRar, programPatchRar);
            case Zip:
                string? programPatchZip = CheckArchiverRunner(Zip);
                string? programPatchUnzip = CheckArchiverRunner(Unzip);
                if (programPatchZip is null || programPatchUnzip is null)
                {
                    return null;
                }

                return new ArchiverDetectorResults(programPatchZip, programPatchUnzip);
            default:
                return null;
        }
    }

    private string? CheckArchiverRunner(string archiverName)
    {
        Result<(string, int)> runProcessWithOutputResult =
            StShared.RunProcessWithOutput(UseConsole, null, "which", archiverName);
        if (runProcessWithOutputResult.IsFailure)
        {
            return null;
        }

        string archiverRunner = runProcessWithOutputResult.Value.Item1;
        if (!string.IsNullOrWhiteSpace(archiverRunner) && File.Exists(archiverRunner))
        {
            return archiverRunner;
        }

        return null;
    }
}
