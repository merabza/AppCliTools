using System.IO;
using OneOf;
using SystemTools.SystemToolsShared;
using SystemTools.SystemToolsShared.Errors;

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
        OneOf<(string, int), Err[]> runProcessWithOutputResult =
            StShared.RunProcessWithOutput(UseConsole, null, "which", archiverName);
        if (runProcessWithOutputResult.IsT1)
        {
            return null;
        }

        string archiverRunner = runProcessWithOutputResult.AsT0.Item1;
        if (!string.IsNullOrWhiteSpace(archiverRunner) && File.Exists(archiverRunner))
        {
            return archiverRunner;
        }

        return null;
    }
}
