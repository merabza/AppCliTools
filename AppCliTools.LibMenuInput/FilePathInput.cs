using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AppCliTools.LibMenuInput;

public sealed class FilePathInput : PathInput
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public FilePathInput(string fieldName, string? defaultValue = null, bool warningIfFileDoesNotExists = true) : base(
        fieldName, false, defaultValue, warningIfFileDoesNotExists)
    {
    }

    public string? FilePath => EnteredPath;

    protected override void AddFiles(DirectoryInfo dir, string fileName, List<string> names)
    {
        List<string> fileNames = dir.GetFiles($"{fileName}*").Select(s => s.Name).ToList();
        names.AddRange(fileNames);
    }

    protected override bool CheckExists()
    {
        return File.Exists(EnteredPath);
    }
}
