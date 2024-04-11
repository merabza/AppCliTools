using Microsoft.Win32;

namespace CliTools.ArchiverTools;

public sealed class WindowsArchiverDetector : ArchiverDetector
{
    private const string WinRar = "WinRar";
    private const string WinZip = "WinZip";

    // ReSharper disable once ConvertToPrimaryConstructor
    public WindowsArchiverDetector(bool useConsole, string fileExtension) : base(useConsole, fileExtension)
    {
    }

    public override ArchiverDetectorResults? Run()
    {
        switch (FileExtension.ToLower())
        {
            case Rar:
                var programPatchRar = GetProgramPath(WinRar);
                return programPatchRar is null
                    ? null
                    : new ArchiverDetectorResults(programPatchRar,
                        programPatchRar);
            case Zip:
                var programPatchZip = GetProgramPath(WinZip);
                return programPatchZip is null
                    ? null
                    : new ArchiverDetectorResults(programPatchZip,
                        programPatchZip);
            default:
                return null;
        }
    }


    private string? GetProgramPath(string fileType)
    {
        char[] separators = [','];

        var subKeyName = fileType + "\\DefaultIcon";
#pragma warning disable CA1416 // Validate platform compatibility
        // ReSharper disable once using
        using var key = Registry.ClassesRoot.OpenSubKey(subKeyName);
        var subKeyVal = key?.GetValue(key.GetValueNames()[0])?.ToString();
#pragma warning restore CA1416 // Validate platform compatibility
        if (subKeyVal == null)
            return null;
        var splitWords = subKeyVal.Split(separators);
        var toRet = splitWords[0];
        if (toRet[0] == '\"')
            toRet = toRet[1..];
        if (toRet[^1] == '\"')
            toRet = toRet[..^1];
        return toRet;
    }
}